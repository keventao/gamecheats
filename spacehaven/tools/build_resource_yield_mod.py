#!/usr/bin/env python3
"""Build a minimal Space Haven XML mod that boosts resource output.

The generated mod copies only Product definitions from the local game jar and
multiplies output `howMuch` values under `<products>`. Generated files are not
intended for git; they are derived from the user's local Space Haven install.
"""
from __future__ import annotations

import argparse
import os
import shutil
import sys
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

MOD_NAME = "KK Resource Yield x2"
MOD_FOLDER = "kk-resource-yield-x2"
MOD_ID = "2026050501"
MOD_VERSION = "0.1.0"
DEFAULT_MULTIPLIER = 2


def candidate_game_roots() -> list[Path]:
    roots: list[Path] = []
    if env_root := os.environ.get("SPACEHAVEN_GAME_ROOT"):
        roots.append(Path(env_root))
    roots.extend(
        [
            Path(r"<SPACEHAVEN_GAME_ROOT>"),
            Path(r"<SPACEHAVEN_GAME_ROOT>"),
            Path(r"<SPACEHAVEN_GAME_ROOT>"),
        ]
    )
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
        output.set("howMuch", str(max(1, value * multiplier)))
        changed += 1
    return changed


def build_haven_xml(jar_path: Path, multiplier: int) -> tuple[ET.Element, int, int]:
    with zipfile.ZipFile(jar_path) as jar:
        root = ET.fromstring(jar.read("library/haven"))

    source_products = root.find("Product")
    if source_products is None:
        raise SystemExit("library/haven has no Product section")

    out_root = ET.Element("data")
    out_products = ET.SubElement(out_root, "Product")
    product_count = 0
    output_count = 0

    for product in source_products:
        if product.tag != "product":
            continue
        if product.get("type") not in {"Crop", "Process"}:
            continue
        changed = multiply_outputs(product, multiplier)
        if changed == 0:
            continue
        out_products.append(product)
        product_count += 1
        output_count += changed

    indent_xml(out_root)
    return out_root, product_count, output_count


def write_info_xml(mod_dir: Path, multiplier: int) -> None:
    info = ET.Element("info")
    ET.SubElement(info, "name").text = MOD_NAME
    ET.SubElement(info, "version").text = MOD_VERSION
    ET.SubElement(info, "author").text = "kk"
    ET.SubElement(info, "website").text = "https://github.com/"
    ET.SubElement(info, "modid").text = MOD_ID
    ET.SubElement(info, "minimumLoaderVersion").text = "0.12.0"
    ET.SubElement(info, "description").text = (
        f"Multiplies Crop and Process resource outputs by x{multiplier}. "
        "Generated from the local Space Haven library."
    )
    indent_xml(info)
    ET.ElementTree(info).write(mod_dir / "info.xml", encoding="utf-8", xml_declaration=False)


def build_mod(game_root: Path, output_root: Path, multiplier: int, clean: bool) -> Path:
    mod_dir = output_root / MOD_FOLDER
    if clean and mod_dir.exists():
        shutil.rmtree(mod_dir)
    (mod_dir / "library").mkdir(parents=True, exist_ok=True)

    haven_xml, product_count, output_count = build_haven_xml(game_root / "spacehaven.jar", multiplier)
    ET.ElementTree(haven_xml).write(mod_dir / "library" / "haven", encoding="utf-8", xml_declaration=False)
    write_info_xml(mod_dir, multiplier)

    readme = mod_dir / "README.txt"
    readme.write_text(
        "\n".join(
            [
                MOD_NAME,
                "=" * len(MOD_NAME),
                "",
                f"Multiplier: x{multiplier}",
                f"Patched Product definitions: {product_count}",
                f"Patched output entries: {output_count}",
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
    print(f"products={product_count} outputs={output_count} multiplier=x{multiplier}")
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
    parser.add_argument("--no-clean", action="store_true", help="Do not delete the old generated mod first")
    args = parser.parse_args(argv)

    if args.multiplier < 1 or args.multiplier > 10:
        raise SystemExit("--multiplier must be between 1 and 10")

    game_root = find_game_root(args.game_root)
    output_root = Path(args.output)
    build_mod(game_root, output_root, args.multiplier, clean=not args.no_clean)
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
