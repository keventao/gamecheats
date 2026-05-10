#!/usr/bin/env python3
"""Extract elementaryId -> {EN,CN} name map from spacehaven.jar.

Run once after game updates. Writes resource_names.json next to this script.
"""
import json
import os
import re
import sys
import zipfile
from pathlib import Path
from xml.etree import ElementTree as ET

OUT = Path(__file__).parent / "resource_names.json"


def find_jar() -> Path:
    if len(sys.argv) > 1:
        p = Path(sys.argv[1])
        if p.is_file():
            return p
    env_path = os.environ.get("SPACEHAVEN_JAR")
    if env_path:
        p = Path(env_path)
        if p.is_file():
            return p
    raise SystemExit("spacehaven.jar not found; pass path as arg or set SPACEHAVEN_JAR")


def read_text_from_jar(jar: Path, name: str) -> str:
    with zipfile.ZipFile(jar) as z:
        return z.read(name).decode("utf-8")


def main() -> None:
    jar = find_jar()
    print(f"Using jar: {jar}")

    haven_xml = read_text_from_jar(jar, "library/haven")
    texts_xml = read_text_from_jar(jar, "library/texts")

    # game's XML isn't strictly valid — use regex-only extraction
    tid_map: dict[str, dict[str, str]] = {}
    t_block = re.compile(r'<t\s+id="(\d+)"[^>]*>(.*?)</t>', re.DOTALL)
    lang_tag = re.compile(r"<(EN|CN|PTBR|DE|JA|FR|KO|ES)>(.*?)</\1>", re.DOTALL)
    for m in t_block.finditer(texts_xml):
        tid = m.group(1)
        body = m.group(2)
        entry = {}
        for lm in lang_tag.finditer(body):
            entry[lm.group(1)] = lm.group(2).strip()
        tid_map[tid] = entry
    haven_xml_for_regex = haven_xml

    # scan haven for <product eid="X" type="Elementary" ...> <name tid="Y"/>
    # single-pass regex; XML too large and has mixed content
    name_pattern = re.compile(r'<name\s+tid="(\d+)"')
    patterns = [
        (re.compile(r'<product\s+eid="(\d+)"[^>]*>(.*?)</product>', re.DOTALL), "product"),
        (re.compile(r'<item\s+mid="(\d+)"[^>]*>(.*?)</item>', re.DOTALL), "item"),
    ]

    out: dict[str, dict[str, str]] = {}
    for pat, kind in patterns:
        for m in pat.finditer(haven_xml_for_regex):
            eid, body = m.group(1), m.group(2)
            nm = name_pattern.search(body)
            if not nm:
                continue
            tid = nm.group(1)
            texts = tid_map.get(tid, {})
            en = texts.get("EN", "")
            cn = texts.get("CN", "") or en
            if eid in out and out[eid].get("en"):
                continue  # product definitions win over items
            out[eid] = {"en": en, "cn": cn, "tid": tid, "kind": kind}

    OUT.write_text(json.dumps(out, ensure_ascii=False, indent=2, sort_keys=True))
    print(f"Wrote {len(out)} entries -> {OUT}")
    # preview a few
    for eid in list(out.keys())[:10]:
        print(f"  {eid}: EN={out[eid]['en']!r}  CN={out[eid]['cn']!r}")


if __name__ == "__main__":
    main()
