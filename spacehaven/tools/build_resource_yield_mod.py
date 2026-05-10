#!/usr/bin/env python3
"""Build a Space Haven XML test mod for resource and production tuning.

The generated mod copies only Product definitions from the local game jar and
applies safe XML-only tweaks. Generated files are not intended for git; they are
derived from the user's local Space Haven install.
"""
from __future__ import annotations

import argparse
import copy
import os
import shutil
import sys
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

MOD_NAME_PREFIX = "KK Resource Tuning"
MOD_FOLDER_PREFIX = "kk-resource-tuning"
MOD_ID_BASE = 2026050500
MOD_VERSION = "0.2.0"
DEFAULT_MULTIPLIER = 2
DEFAULT_CROP_TIME_DIVISOR = 2
DEFAULT_NEED_INTERVAL_MULTIPLIER = 2


def candidate_game_roots() -> list[Path]:
    roots: list[Path] = []
    if env_root := os.environ.get("SPACEHAVEN_GAME_ROOT"):
        roots.append(Path(env_root))
    return roots


def find_game_root(explicit: str | None) -> Path:
    if explicit:
        root = Path(explicit)
        if (root / "spacehaven.jar").is_file():
            return root
        raise SystemExit(f"spacehaven.jar not found under {root}")

    for root in candidate_game_roots():
        if (root / "spacehaven.jar").is_file():
            return root
    raise SystemExit("spacehaven.jar not found; pass --game-root or set SPACEHAVEN_GAME_ROOT")


def indent_xml(element: ET.Element) -> None:
    try:
        ET.indent(element, space="    ")
    except AttributeError:
        pass


def multiply_outputs(product: ET.Element, multiplier: int) -> int:
    changed = 0
    for output in product.findall(".//products/l"):
        raw = output.get("howMuch")
        if raw is None:
            continue
        try:
            value = int(raw)
        except ValueError:
            continue
        new_value = max(1, value * multiplier)
        if new_value == value:
            continue
        output.set("howMuch", str(new_value))
        changed += 1
    return changed


def divide_int_attribute(element: ET.Element, attribute: str, divisor: int) -> bool:
    raw = element.get(attribute)
    if raw is None:
        return False
    try:
        value = int(raw)
    except ValueError:
        return False
    new_value = max(1, value // divisor)
    if new_value == value:
        return False
    element.set(attribute, str(new_value))
    return True


def multiply_int_attribute(element: ET.Element, attribute: str, multiplier: int) -> bool:
    raw = element.get(attribute)
    if raw is None:
        return False
    try:
        value = int(raw)
    except ValueError:
        return False
    new_value = max(1, value * multiplier)
    if new_value == value:
        return False
    element.set(attribute, str(new_value))
    return True


def speed_up_crop_stages(product: ET.Element, divisor: int) -> int:
    if product.get("type") != "Crop":
        return 0
    changed = 0
    for stage in product.findall(".//stages/l"):
        if divide_int_attribute(stage, "time", divisor):
            changed += 1
    return changed


def increase_need_intervals(product: ET.Element, multiplier: int) -> int:
    changed = 0
    for need in product.findall(".//needs/l"):
        if multiply_int_attribute(need, "consumeEvery", multiplier):
            changed += 1
    return changed


def mod_name(multiplier: int) -> str:
    return f"{MOD_NAME_PREFIX} x{multiplier}"


def mod_folder(multiplier: int) -> str:
    return f"{MOD_FOLDER_PREFIX}-x{multiplier}"


def mod_id(multiplier: int) -> str:
    return str(MOD_ID_BASE + multiplier)


def build_haven_xml(
    jar_path: Path,
    multiplier: int,
    crop_time_divisor: int,
    crop_need_interval_multiplier: int,
    process_need_interval_multiplier: int,
    output_boost: bool,
    crop_speed: bool,
    crop_input_saver: bool,
    process_input_saver: bool,
) -> tuple[ET.Element, dict[str, int]]:
    with zipfile.ZipFile(jar_path) as jar:
        root = ET.fromstring(jar.read("library/haven"))

    source_products = root.find("Product")
    if source_products is None:
        raise SystemExit("library/haven has no Product section")

    out_root = ET.Element("data")
    out_products = ET.SubElement(out_root, "Product")
    stats = {
        "products": 0,
        "outputs": 0,
        "crop_stage_times": 0,
        "crop_need_intervals": 0,
        "process_need_intervals": 0,
    }

    for product in source_products:
        if product.tag != "product":
            continue
        product_type = product.get("type")
        if product_type not in {"Crop", "Process"}:
            continue

        patched = copy.deepcopy(product)
        changed = 0
        if output_boost:
            output_count = multiply_outputs(patched, multiplier)
            stats["outputs"] += output_count
            changed += output_count
        if crop_speed and product_type == "Crop":
            crop_time_count = speed_up_crop_stages(patched, crop_time_divisor)
            stats["crop_stage_times"] += crop_time_count
            changed += crop_time_count
        if crop_input_saver and product_type == "Crop":
            crop_need_count = increase_need_intervals(patched, crop_need_interval_multiplier)
            stats["crop_need_intervals"] += crop_need_count
            changed += crop_need_count
        if process_input_saver and product_type == "Process":
            process_need_count = increase_need_intervals(patched, process_need_interval_multiplier)
            stats["process_need_intervals"] += process_need_count
            changed += process_need_count

        if changed == 0:
            continue
        out_products.append(patched)
        stats["products"] += 1

    indent_xml(out_root)
    return out_root, stats


def write_info_xml(mod_dir: Path, multiplier: int) -> None:
    info = ET.Element("info")
    ET.SubElement(info, "name").text = mod_name(multiplier)
    ET.SubElement(info, "version").text = MOD_VERSION
    ET.SubElement(info, "author").text = "kk"
    ET.SubElement(info, "website").text = "https://github.com/"
    ET.SubElement(info, "modid").text = mod_id(multiplier)
    ET.SubElement(info, "minimumLoaderVersion").text = "0.12.0"
    ET.SubElement(info, "description").text = (
        f"XML-only test mod: resource outputs x{multiplier}, faster crops, lower input use. "
        "Generated from the local Space Haven library."
    )
    indent_xml(info)
    ET.ElementTree(info).write(mod_dir / "info.xml", encoding="utf-8", xml_declaration=False)


def build_mod(
    game_root: Path,
    output_root: Path,
    multiplier: int,
    clean: bool,
    crop_time_divisor: int,
    crop_need_interval_multiplier: int,
    process_need_interval_multiplier: int,
    output_boost: bool,
    crop_speed: bool,
    crop_input_saver: bool,
    process_input_saver: bool,
) -> Path:
    name = mod_name(multiplier)
    mod_dir = output_root / mod_folder(multiplier)
    if clean and mod_dir.exists():
        shutil.rmtree(mod_dir)
    (mod_dir / "library").mkdir(parents=True, exist_ok=True)

    haven_xml, stats = build_haven_xml(
        game_root / "spacehaven.jar",
        multiplier,
        crop_time_divisor,
        crop_need_interval_multiplier,
        process_need_interval_multiplier,
        output_boost,
        crop_speed,
        crop_input_saver,
        process_input_saver,
    )
    ET.ElementTree(haven_xml).write(mod_dir / "library" / "haven", encoding="utf-8", xml_declaration=False)
    write_info_xml(mod_dir, multiplier)

    readme = mod_dir / "README.txt"
    readme.write_text(
        "\n".join(
            [
                name,
                "=" * len(name),
                "",
                "Features:",
                f"- Resource output multiplier: x{multiplier}" if output_boost else "- Resource output multiplier: disabled",
                f"- Crop stage time divisor: /{crop_time_divisor}" if crop_speed else "- Crop speed: disabled",
                f"- Crop need consumeEvery multiplier: x{crop_need_interval_multiplier}" if crop_input_saver else "- Crop need saver: disabled",
                f"- Process need consumeEvery multiplier: x{process_need_interval_multiplier}" if process_input_saver else "- Process need saver: disabled",
                "",
                f"Patched Product definitions: {stats['products']}",
                f"Patched output entries: {stats['outputs']}",
                f"Patched crop stage times: {stats['crop_stage_times']}",
                f"Patched crop need intervals: {stats['crop_need_intervals']}",
                f"Patched process need intervals: {stats['process_need_intervals']}",
                "",
                "Install:",
                "1. Copy this folder into SpaceHaven/mods/.",
                "2. Open Space Haven Mod Loader.",
                "3. Clear QuickLaunch file.",
                "4. Launch Space Haven from the modloader.",
                "",
                "This generated folder is derived from the local game library.",
            ]
        ),
        encoding="utf-8",
    )
    print(f"built {mod_dir}")
    print(
        " ".join(
            [
                f"products={stats['products']}",
                f"outputs={stats['outputs']}",
                f"crop_times={stats['crop_stage_times']}",
                f"crop_needs={stats['crop_need_intervals']}",
                f"process_needs={stats['process_need_intervals']}",
                f"multiplier=x{multiplier}",
            ]
        )
    )
    return mod_dir


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-root", help="Space Haven install folder containing spacehaven.jar")
    parser.add_argument(
        "--output",
        default=str(Path(__file__).resolve().parents[1] / "generated"),
        help="Output folder for generated mods",
    )
    parser.add_argument("--multiplier", type=int, default=DEFAULT_MULTIPLIER)
    parser.add_argument("--crop-time-divisor", type=int, default=DEFAULT_CROP_TIME_DIVISOR)
    parser.add_argument("--crop-need-interval-multiplier", type=int, default=DEFAULT_NEED_INTERVAL_MULTIPLIER)
    parser.add_argument("--process-need-interval-multiplier", type=int, default=DEFAULT_NEED_INTERVAL_MULTIPLIER)
    parser.add_argument("--no-output-boost", action="store_true", help="Do not multiply product outputs")
    parser.add_argument("--no-crop-speed", action="store_true", help="Do not reduce Crop stage time values")
    parser.add_argument("--no-crop-input-saver", action="store_true", help="Do not increase Crop need consumeEvery intervals")
    parser.add_argument("--no-process-input-saver", action="store_true", help="Do not increase Process need consumeEvery intervals")
    parser.add_argument("--no-clean", action="store_true", help="Do not delete the old generated mod first")
    args = parser.parse_args(argv)

    if args.multiplier < 1 or args.multiplier > 10:
        raise SystemExit("--multiplier must be between 1 and 10")
    if args.crop_time_divisor < 1 or args.crop_time_divisor > 10:
        raise SystemExit("--crop-time-divisor must be between 1 and 10")
    if args.crop_need_interval_multiplier < 1 or args.crop_need_interval_multiplier > 10:
        raise SystemExit("--crop-need-interval-multiplier must be between 1 and 10")
    if args.process_need_interval_multiplier < 1 or args.process_need_interval_multiplier > 10:
        raise SystemExit("--process-need-interval-multiplier must be between 1 and 10")

    game_root = find_game_root(args.game_root)
    output_root = Path(args.output)
    build_mod(
        game_root,
        output_root,
        args.multiplier,
        clean=not args.no_clean,
        crop_time_divisor=args.crop_time_divisor,
        crop_need_interval_multiplier=args.crop_need_interval_multiplier,
        process_need_interval_multiplier=args.process_need_interval_multiplier,
        output_boost=not args.no_output_boost,
        crop_speed=not args.no_crop_speed,
        crop_input_saver=not args.no_crop_input_saver,
        process_input_saver=not args.no_process_input_saver,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
