#!/usr/bin/env python3
"""Space Haven save editor.

Loads savegames/*/save/game XML, exposes bank + crew stats in a Tk GUI,
auto-backs up original before writing.
"""
from __future__ import annotations

import json
import os
import re
import shutil
import sys
import time
import tkinter as tk
from pathlib import Path
from tkinter import filedialog, messagebox, ttk
from xml.etree import ElementTree as ET

GAME_ROOT = Path(__file__).resolve().parent.parent
HOME = Path.home()

CANDIDATE_SAVE_DIRS = [
    HOME / "<APP_SUPPORT>/Spacehaven/savegames",
    HOME / "<APP_SUPPORT>/SpaceHaven/savegames",
    GAME_ROOT / "savegames",
    HOME / "<APP_SUPPORT>/compatibility layer/Bottles/Steam/drive_c/Program Files (x86)/Steam/steamapps/common/SpaceHaven/savegames",
]
if env_dir := os.environ.get("SPACEHAVEN_SAVES"):
    CANDIDATE_SAVE_DIRS.insert(0, Path(env_dir))
if len(sys.argv) > 1:
    CANDIDATE_SAVE_DIRS.insert(0, Path(sys.argv[1]))

SAVES_DIR = next((p for p in CANDIDATE_SAVE_DIRS if p.is_dir()), GAME_ROOT / "savegames")

SKILL_NAMES = {
    2: "Piloting", 3: "Gunnery", 4: "Mining", 5: "Botany", 6: "Industry",
    7: "Medical", 8: "Shields", 9: "Weapons", 10: "Navigation", 12: "Diplomacy",
    13: "Research", 14: "Construction", 16: "Maintenance", 22: "Operations",
}
ATTR_NAMES = {210: "Bravery", 212: "Intelligence", 213: "Perception", 214: "Zest"}

_names_path = Path(__file__).parent / "resource_names.json"
if _names_path.is_file():
    _raw = json.loads(_names_path.read_text())
    RESOURCE_NAMES = {eid: f'{v.get("cn") or v.get("en")} ({v.get("en")})'.strip() for eid, v in _raw.items()}
else:
    RESOURCE_NAMES = {}


def find_save_files() -> list[Path]:
    if not SAVES_DIR.exists():
        return []
    results = []
    for save_dir in SAVES_DIR.iterdir():
        if not save_dir.is_dir():
            continue
        for sub in ("save", "autosave1", "autosave2", "autosave3", "autosave4"):
            game_file = save_dir / sub / "game"
            if game_file.is_file():
                results.append(game_file)
    return sorted(results)


class SaveModel:
    def __init__(self, path: Path):
        self.path = path
        self.tree = ET.parse(path)
        self.root = self.tree.getroot()

    def bank(self) -> ET.Element | None:
        return self.root.find(".//playerBank")

    def player_characters(self) -> list[ET.Element]:
        out = []
        for char in self.root.iter("c"):
            if char.get("side") == "Player" and char.get("name"):
                out.append(char)
        return out

    def player_ship(self) -> ET.Element | None:
        player_ship_ids = set()
        for f in self.root.iter("f"):
            if f.get("isPlayer") == "true":
                for l in f.findall("./createdShips/l"):
                    sid = l.get("createdShipId")
                    if sid and sid != "0":
                        player_ship_ids.add(sid)
        for ship in self.root.iter("ship"):
            if ship.get("sid") in player_ship_ids:
                return ship
        for ship in self.root.iter("ship"):
            if ship.get("sname") and not ship.get("sname", "").startswith("HSS ") is False:
                return ship
        return None

    def resource_slots(self, ship: ET.Element) -> list[ET.Element]:
        slots = []
        for s in ship.iter("s"):
            if s.get("elementaryId") is not None and s.get("inStorage") is not None:
                slots.append(s)
        return slots

    def backup(self) -> Path:
        stamp = time.strftime("%Y%m%d-%H%M%S")
        bak = self.path.with_name(f"game.bak-{stamp}")
        shutil.copy2(self.path, bak)
        return bak

    def save(self) -> None:
        self.tree.write(self.path, encoding="utf-8", xml_declaration=False)


class EditorApp(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Space Haven Save Editor")
        self.geometry("880x640")
        self.model: SaveModel | None = None
        self.bank_vars: dict[str, tk.StringVar] = {}
        self.char_frames: list[ttk.Frame] = []
        self._build_ui()
        self._refresh_save_list()

    def _build_ui(self) -> None:
        top = ttk.Frame(self, padding=8)
        top.pack(fill="x")
        ttk.Label(top, text="Savegame:").pack(side="left")
        self.save_combo = ttk.Combobox(top, state="readonly")
        self.save_combo.pack(side="left", padx=6, fill="x", expand=True)

        btns = ttk.Frame(self, padding=(8, 0))
        btns.pack(fill="x")
        ttk.Button(btns, text="Load", command=self._load).pack(side="left", padx=2)
        ttk.Button(btns, text="Refresh", command=self._refresh_save_list).pack(side="left", padx=2)
        ttk.Button(btns, text="Browse...", command=self._browse).pack(side="left", padx=2)

        self.status = tk.StringVar(value="No save loaded.")
        ttk.Label(self, textvariable=self.status, padding=4, foreground="#555").pack(fill="x")

        self.nb = ttk.Notebook(self)
        self.nb.pack(fill="both", expand=True, padx=8, pady=4)

        self.bank_tab = ttk.Frame(self.nb, padding=12)
        self.crew_tab = ttk.Frame(self.nb, padding=8)
        self.res_tab = ttk.Frame(self.nb, padding=8)
        self.nb.add(self.bank_tab, text="Bank")
        self.nb.add(self.crew_tab, text="Crew")
        self.nb.add(self.res_tab, text="Resources")

        btn_bar = ttk.Frame(self, padding=8)
        btn_bar.pack(fill="x")
        ttk.Button(btn_bar, text="Save (with backup)", command=self._do_save).pack(side="right")

    def _refresh_save_list(self) -> None:
        self.saves = find_save_files()
        display = [str(p.relative_to(SAVES_DIR)) for p in self.saves] if self.saves else []
        self.save_combo["values"] = display
        if display:
            self.save_combo.current(0)

    def _browse(self) -> None:
        init = str(SAVES_DIR if SAVES_DIR.exists() else GAME_ROOT)
        path = filedialog.askopenfilename(
            title="Choose game XML", initialdir=init,
            filetypes=[("Save file", "game"), ("All files", "*.*")],
        )
        if path:
            self._load_path(Path(path))

    def _load(self) -> None:
        idx = self.save_combo.current()
        if idx < 0 or idx >= len(self.saves):
            messagebox.showwarning("No selection", "Pick a save first.")
            return
        self._load_path(self.saves[idx])

    def _load_path(self, path: Path) -> None:
        try:
            self.model = SaveModel(path)
        except Exception as exc:
            messagebox.showerror("Load failed", f"{exc}")
            return
        self.status.set(f"Loaded: {path}")
        self._render_bank()
        self._render_crew()
        self._render_resources()

    def _render_bank(self) -> None:
        for w in self.bank_tab.winfo_children():
            w.destroy()
        self.bank_vars.clear()
        bank = self.model.bank() if self.model else None
        if bank is None:
            ttk.Label(self.bank_tab, text="playerBank not found.").pack()
            return
        rows = [
            ("ca", "Credits"),
            ("cr", "Credits reserved"),
            ("slp", "Science points"),
            ("blp", "Build points"),
        ]
        for i, (key, label) in enumerate(rows):
            ttk.Label(self.bank_tab, text=label, width=20, anchor="w").grid(row=i, column=0, sticky="w", pady=4)
            var = tk.StringVar(value=bank.get(key, "0"))
            ttk.Entry(self.bank_tab, textvariable=var, width=20).grid(row=i, column=1, sticky="w")
            self.bank_vars[key] = var

        ttk.Button(self.bank_tab, text="Max all (9,999,999)",
                   command=self._max_bank).grid(row=len(rows), column=0, columnspan=2, pady=10, sticky="w")

    def _max_bank(self) -> None:
        for key in ("ca", "slp", "blp"):
            if key in self.bank_vars:
                self.bank_vars[key].set("9999999")

    def _render_crew(self) -> None:
        for w in self.crew_tab.winfo_children():
            w.destroy()
        self.char_frames.clear()
        if not self.model:
            return
        chars = self.model.player_characters()
        if not chars:
            ttk.Label(self.crew_tab, text="No player crew found.").pack()
            return

        canvas = tk.Canvas(self.crew_tab, highlightthickness=0)
        vbar = ttk.Scrollbar(self.crew_tab, orient="vertical", command=canvas.yview)
        inner = ttk.Frame(canvas)
        inner.bind("<Configure>", lambda e: canvas.configure(scrollregion=canvas.bbox("all")))
        canvas.create_window((0, 0), window=inner, anchor="nw")
        canvas.configure(yscrollcommand=vbar.set)
        canvas.pack(side="left", fill="both", expand=True)
        vbar.pack(side="right", fill="y")

        for char in chars:
            self._build_char_card(inner, char)

    def _build_char_card(self, parent: ttk.Frame, char: ET.Element) -> None:
        name = char.get("name", "?")
        frame = ttk.LabelFrame(parent, text=f"{name}  (cid={char.get('cid')})", padding=8)
        frame.pack(fill="x", padx=6, pady=6)
        state: dict[str, tk.StringVar] = {}

        props_row = ttk.Frame(frame)
        props_row.pack(fill="x")
        for i, tag in enumerate(("Health", "Food", "Rest", "Comfort", "Mood")):
            el = char.find(f"./props/{tag}")
            if el is None:
                continue
            ttk.Label(props_row, text=f"{tag}:").grid(row=0, column=i * 2, padx=(0, 2))
            var = tk.StringVar(value=el.get("v", "0"))
            ttk.Entry(props_row, textvariable=var, width=6).grid(row=0, column=i * 2 + 1, padx=(0, 8))
            state[f"prop:{tag}"] = var

        ttk.Label(frame, text="Attributes:").pack(anchor="w", pady=(6, 0))
        attr_row = ttk.Frame(frame)
        attr_row.pack(fill="x")
        for i, a in enumerate(char.findall("./pers/attr/a")):
            aid = int(a.get("id", 0))
            label = ATTR_NAMES.get(aid, f"id {aid}")
            ttk.Label(attr_row, text=label).grid(row=0, column=i * 2, padx=(0, 2))
            var = tk.StringVar(value=a.get("points", "0"))
            ttk.Entry(attr_row, textvariable=var, width=4).grid(row=0, column=i * 2 + 1, padx=(0, 8))
            state[f"attr:{aid}"] = var

        ttk.Label(frame, text="Skills (level):").pack(anchor="w", pady=(6, 0))
        skill_row = ttk.Frame(frame)
        skill_row.pack(fill="x")
        skills = char.findall("./pers/skills/s")
        for i, s in enumerate(skills):
            sk = int(s.get("sk", 0))
            label = SKILL_NAMES.get(sk, f"sk{sk}")
            r, c = divmod(i, 7)
            ttk.Label(skill_row, text=label).grid(row=r * 2, column=c, padx=2)
            var = tk.StringVar(value=s.get("level", "0"))
            ttk.Entry(skill_row, textvariable=var, width=4).grid(row=r * 2 + 1, column=c, padx=2)
            state[f"skill:{sk}"] = var

        quick = ttk.Frame(frame)
        quick.pack(fill="x", pady=(6, 0))
        ttk.Button(quick, text="Max skills (this crew)",
                   command=lambda st=state: self._max_char_skills(st)).pack(side="left")
        ttk.Button(quick, text="Max attrs (10 each)",
                   command=lambda st=state: self._max_char_attrs(st)).pack(side="left", padx=6)
        ttk.Button(quick, text="Heal & feed",
                   command=lambda st=state: self._heal_char(st)).pack(side="left")

        self.char_frames.append((char, state))

    def _max_char_skills(self, state: dict) -> None:
        for key, var in state.items():
            if key.startswith("skill:"):
                var.set("10")

    def _max_char_attrs(self, state: dict) -> None:
        for key, var in state.items():
            if key.startswith("attr:"):
                var.set("10")

    def _heal_char(self, state: dict) -> None:
        for tag, val in (("Health", "120"), ("Food", "100"), ("Rest", "100"), ("Comfort", "100"), ("Mood", "100")):
            key = f"prop:{tag}"
            if key in state:
                state[key].set(val)

    def _render_resources(self) -> None:
        for w in self.res_tab.winfo_children():
            w.destroy()
        self.res_targets: dict[str, tk.StringVar] = {}
        self.res_slot_map: dict[str, list[ET.Element]] = {}
        if not self.model:
            return
        ship = self.model.player_ship()
        if ship is None:
            ttk.Label(self.res_tab, text="Player ship not found.").pack()
            return

        slots = self.model.resource_slots(ship)
        for s in slots:
            eid = s.get("elementaryId")
            self.res_slot_map.setdefault(eid, []).append(s)

        header = ttk.Frame(self.res_tab)
        header.pack(fill="x", pady=(0, 6))
        ttk.Label(header, text=f"Player ship: {ship.get('sname','?')}  |  {len(slots)} storage slots across {len(self.res_slot_map)} resources").pack(side="left")

        actions = ttk.Frame(self.res_tab)
        actions.pack(fill="x", pady=(0, 6))
        ttk.Button(actions, text="Set all totals = 50", command=lambda: self._fill_all("50")).pack(side="left")
        ttk.Button(actions, text="Set all totals = 99", command=lambda: self._fill_all("99")).pack(side="left", padx=4)
        ttk.Button(actions, text="Set all totals = 999", command=lambda: self._fill_all("999")).pack(side="left", padx=4)
        ttk.Label(actions, text="(target total; distributed across slots, game caps at slot capacity)").pack(side="left", padx=8)

        canvas = tk.Canvas(self.res_tab, highlightthickness=0)
        vbar = ttk.Scrollbar(self.res_tab, orient="vertical", command=canvas.yview)
        inner = ttk.Frame(canvas)
        inner.bind("<Configure>", lambda e: canvas.configure(scrollregion=canvas.bbox("all")))
        canvas.create_window((0, 0), window=inner, anchor="nw")
        canvas.configure(yscrollcommand=vbar.set)
        canvas.pack(side="left", fill="both", expand=True)
        vbar.pack(side="right", fill="y")

        hdr = ttk.Frame(inner)
        hdr.pack(fill="x")
        for i, (txt, w) in enumerate([("ID", 6), ("Name", 42), ("Slots", 6), ("Total", 8), ("Set total to", 12)]):
            ttk.Label(hdr, text=txt, width=w, anchor="w", foreground="#666").grid(row=0, column=i, sticky="w")

        def sort_key(e):
            known = e in RESOURCE_NAMES
            return (0 if known else 1, RESOURCE_NAMES.get(e, ""), int(e))

        for eid in sorted(self.res_slot_map.keys(), key=sort_key):
            group = self.res_slot_map[eid]
            total = sum(int(s.get("inStorage", 0) or 0) for s in group)
            row = ttk.Frame(inner)
            row.pack(fill="x", pady=1)
            ttk.Label(row, text=eid, width=6, anchor="w").grid(row=0, column=0, sticky="w")
            ttk.Label(row, text=RESOURCE_NAMES.get(eid, f"(id {eid})"), width=42, anchor="w").grid(row=0, column=1, sticky="w")
            ttk.Label(row, text=str(len(group)), width=6, anchor="w").grid(row=0, column=2, sticky="w")
            ttk.Label(row, text=str(total), width=8, anchor="w").grid(row=0, column=3, sticky="w")
            var = tk.StringVar(value="")
            ttk.Entry(row, textvariable=var, width=10).grid(row=0, column=4, sticky="w")
            self.res_targets[eid] = var

    def _fill_all(self, val: str) -> None:
        for var in self.res_targets.values():
            var.set(val)

    def _apply_to_tree(self) -> list[str]:
        errors = []
        bank = self.model.bank()
        if bank is not None:
            for key, var in self.bank_vars.items():
                val = var.get().strip()
                if not re.fullmatch(r"-?\d+", val):
                    errors.append(f"Bank {key}: '{val}' not integer")
                    continue
                bank.set(key, val)

        for char, state in self.char_frames:
            for key, var in state.items():
                val = var.get().strip()
                if not re.fullmatch(r"-?\d+", val):
                    errors.append(f"{char.get('name')} {key}: '{val}' not integer")
                    continue
                kind, ident = key.split(":", 1)
                if kind == "prop":
                    el = char.find(f"./props/{ident}")
                    if el is not None:
                        el.set("v", val)
                        if "ltv" in el.attrib:
                            el.set("ltv", val)
                elif kind == "attr":
                    for a in char.findall("./pers/attr/a"):
                        if a.get("id") == ident:
                            a.set("points", val)
                elif kind == "skill":
                    for s in char.findall("./pers/skills/s"):
                        if s.get("sk") == ident:
                            s.set("level", val)
                            cur_mx = int(s.get("mxn", "0") or 0)
                            if int(val) > cur_mx:
                                s.set("mxn", val)

        for eid, var in getattr(self, "res_targets", {}).items():
            val = var.get().strip()
            if val == "":
                continue
            if not re.fullmatch(r"\d+", val):
                errors.append(f"Resource {eid}: '{val}' not non-negative integer")
                continue
            slots = self.res_slot_map.get(eid, [])
            if not slots:
                continue
            total = int(val)
            n = len(slots)
            base, rem = divmod(total, n)
            for i, s in enumerate(slots):
                s.set("inStorage", str(base + 1 if i < rem else base))
                if "onTheWayIn" in s.attrib:
                    s.set("onTheWayIn", "0")
                if "onTheWayOut" in s.attrib:
                    s.set("onTheWayOut", "0")
        return errors

    def _do_save(self) -> None:
        if not self.model:
            messagebox.showwarning("No save", "Load a save first.")
            return
        errs = self._apply_to_tree()
        if errs:
            messagebox.showerror("Invalid input", "\n".join(errs[:12]))
            return
        try:
            bak = self.model.backup()
            self.model.save()
        except Exception as exc:
            messagebox.showerror("Save failed", f"{exc}")
            return
        self.status.set(f"Saved. Backup at {bak.name}")
        messagebox.showinfo("Saved", f"Save written.\nBackup: {bak}")


if __name__ == "__main__":
    EditorApp().mainloop()
