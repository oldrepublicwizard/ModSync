#!/usr/bin/env python3
"""JSON CLI bridge between Godot and PyKotor for KOTOR resource I/O.

Commands (stdout is always JSON on success):
  probe <path>
  read <path> [--game k1|k2]
  write <path> --payload <json-string|@file>
  extract <archive> --resref NAME --restype EXT --output <path>
  inject <archive> --resref NAME --restype EXT --source <path>
  installations
  supported-types
"""
from __future__ import annotations

import argparse
import base64
import json
import sys
import traceback
import warnings
from pathlib import Path
from typing import Any

warnings.filterwarnings("ignore")

# PyKotor import path: sibling checkout, pip install, or PYTHONPATH.
try:
    from pykotor.common.misc import Game
    from pykotor.extract.file import ResourceIdentifier
    from pykotor.resource.formats.erf.erf_auto import read_erf, write_erf
    from pykotor.resource.formats.gff.gff_auto import read_gff, write_gff
    from pykotor.resource.formats.gff.gff_data import GFF
    from pykotor.resource.formats.ncs.ncs_auto import read_ncs
    from pykotor.resource.formats.rim.rim_auto import read_rim, write_rim
    from pykotor.resource.formats.ssf.ssf_auto import read_ssf, write_ssf
    from pykotor.resource.formats.ssf.ssf_data import SSF
    from pykotor.resource.formats.tlk.tlk_auto import read_tlk, write_tlk
    from pykotor.resource.formats.tlk.tlk_data import TLK
    from pykotor.resource.formats.twoda.twoda_auto import read_2da, write_2da
    from pykotor.resource.formats.twoda.twoda_data import TwoDA
    from pykotor.resource.type import ResourceType
    from pykotor.tools.path import find_kotor_paths_from_default
except ImportError as exc:  # pragma: no cover - reported to caller via exit code
    print(
        json.dumps(
            {
                "ok": False,
                "error": f"PyKotor import failed: {exc}. Install PyKotor or set PYTHONPATH.",
            }
        ),
        file=sys.stdout,
    )
    sys.exit(2)


TEXT_EXTENSIONS = {
    "nss",
    "txt",
    "lyt",
    "vis",
    "txi",
    "tx",
    "mdlascii",
    "mdx",
}


def _emit(payload: dict[str, Any], code: int = 0) -> None:
    print(json.dumps(payload, ensure_ascii=False), file=sys.stdout)
    sys.exit(code)


def _fail(message: str, code: int = 1, **extra: Any) -> None:
    body: dict[str, Any] = {"ok": False, "error": message}
    body.update(extra)
    _emit(body, code)


def _resolve_path(path_str: str, *, must_exist: bool = True) -> Path:
    path = Path(path_str).expanduser().resolve()
    if must_exist and not path.exists():
        _fail(f"Path does not exist: {path}")
    return path


def _identify(path: Path) -> tuple[str, ResourceType]:
    try:
        resname, restype = ResourceIdentifier.from_path(path).unpack()
        return resname, restype
    except Exception:
        ext = path.suffix.lower().lstrip(".")
        try:
            return path.stem, ResourceType.from_extension(ext)
        except Exception as exc:
            _fail(f"Unknown resource type for {path}: {exc}")


def cmd_probe(path_str: str) -> None:
    path = _resolve_path(path_str)
    resname, restype = _identify(path)
    _emit(
        {
            "ok": True,
            "path": str(path),
            "resname": resname,
            "extension": restype.extension,
            "category": restype.category,
            "resource_type": restype.name,
            "size": path.stat().st_size,
        }
    )


def _read_text(path: Path) -> dict[str, Any]:
    text = path.read_text(encoding="utf-8", errors="replace")
    return {"format": "text", "encoding": "utf-8", "text": text}


def _read_binary(path: Path) -> dict[str, Any]:
    data = path.read_bytes()
    return {
        "format": "binary",
        "size": len(data),
        "base64": base64.b64encode(data).decode("ascii"),
    }


def _read_twoda(path: Path) -> dict[str, Any]:
    twoda = read_2da(path)
    return {"format": "twoda", "data": twoda.__json__()}


def _read_gff(path: Path) -> dict[str, Any]:
    gff = read_gff(path)
    return {"format": "gff", "data": gff.__json__()}


def _read_tlk(path: Path) -> dict[str, Any]:
    tlk = read_tlk(path)
    return {"format": "tlk", "data": tlk.__json__()}


def _read_ssf(path: Path) -> dict[str, Any]:
    ssf = read_ssf(path)
    return {"format": "ssf", "data": ssf.__json__()}


def _read_erf(path: Path) -> dict[str, Any]:
    erf = read_erf(path)
    resources = []
    for res in erf:
        resources.append(
            {
                "resref": str(res.resref),
                "restype": res.restype.extension,
                "size": res.size,
            }
        )
    return {"format": "erf", "resources": resources}


def _open_container(path: Path):
    ext = path.suffix.lower().lstrip(".")
    if ext == "rim":
        return read_rim(path)
    return read_erf(path)


def _write_container(archive: Path, container) -> None:
    _resname, archive_restype = _identify(archive)
    ext = archive.suffix.lower().lstrip(".")
    try:
        if ext == "rim":
            write_rim(container, archive, ResourceType.RIM)
        else:
            write_erf(container, archive, archive_restype)
    except Exception as exc:
        _fail(f"Failed to write archive {archive}: {exc}")


def cmd_extract(archive_str: str, resref: str, restype_ext: str, output_str: str) -> None:
    archive = _resolve_path(archive_str)
    try:
        container = _open_container(archive)
    except Exception as exc:
        _fail(f"Failed to open archive {archive}: {exc}")

    try:
        restype = ResourceType.from_extension(restype_ext.lower().lstrip("."))
    except Exception as exc:
        _fail(f"Unknown resource type '{restype_ext}': {exc}")

    try:
        data = container.get_data(resref, restype)
    except Exception as exc:
        _fail(f"Extract failed: {exc}")

    if not data:
        _fail(f"Resource '{resref}.{restype.extension}' not found in {archive.name}")

    output = Path(output_str).expanduser()
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(data)

    _emit(
        {
            "ok": True,
            "archive": str(archive),
            "resref": resref,
            "restype": restype.extension,
            "output": str(output.resolve()),
            "bytes": len(data),
        }
    )


def cmd_inject(archive_str: str, resref: str, restype_ext: str, source_str: str) -> None:
    archive = _resolve_path(archive_str)
    source = _resolve_path(source_str)
    try:
        container = _open_container(archive)
    except Exception as exc:
        _fail(f"Failed to open archive {archive}: {exc}")

    try:
        restype = ResourceType.from_extension(restype_ext.lower().lstrip("."))
    except Exception as exc:
        _fail(f"Unknown resource type '{restype_ext}': {exc}")

    data = source.read_bytes()
    if not data:
        _fail(f"Source file is empty: {source}")

    try:
        container.set_data(resref, restype, data)
    except Exception as exc:
        _fail(f"Inject failed: {exc}")

    _write_container(archive, container)

    _emit(
        {
            "ok": True,
            "archive": str(archive),
            "resref": resref,
            "restype": restype.extension,
            "source": str(source.resolve()),
            "bytes": len(data),
        }
    )


def _read_rim(path: Path) -> dict[str, Any]:
    rim = read_rim(path)
    resources = []
    for res in rim:
        resources.append(
            {
                "resref": str(res.resref),
                "restype": res.restype.extension,
                "size": res.size,
            }
        )
    return {"format": "rim", "resources": resources}


def _read_ncs(path: Path) -> dict[str, Any]:
    ncs = read_ncs(path)
    return {
        "format": "ncs",
        "instructions": len(ncs.instructions) if hasattr(ncs, "instructions") else 0,
        "binary": base64.b64encode(path.read_bytes()).decode("ascii"),
    }


def cmd_read(path_str: str, game: str | None) -> None:
    path = _resolve_path(path_str)
    _resname, restype = _identify(path)
    ext = restype.extension.lower()

    try:
        if ext in TEXT_EXTENSIONS:
            payload = _read_text(path)
        elif ext == "2da":
            payload = _read_twoda(path)
        elif restype.category == "GFF" or ext in {
            "utc",
            "utd",
            "ute",
            "uti",
            "utm",
            "utp",
            "uts",
            "utt",
            "utw",
            "are",
            "dlg",
            "fac",
            "git",
            "ifo",
            "jrl",
            "gui",
            "pth",
            "gff",
        }:
            payload = _read_gff(path)
        elif ext in {"tlk", "fmh", "fml"}:
            payload = _read_tlk(path)
        elif ext == "ssf":
            payload = _read_ssf(path)
        elif ext in {"erf", "mod", "sav"}:
            payload = _read_erf(path)
        elif ext == "rim":
            payload = _read_rim(path)
        elif ext == "ncs":
            payload = _read_ncs(path)
        else:
            payload = _read_binary(path)
    except Exception as exc:
        _fail(f"Read failed: {exc}", traceback=traceback.format_exc())

    _emit(
        {
            "ok": True,
            "path": str(path),
            "extension": ext,
            "resource_type": restype.name,
            "game": game,
            "payload": payload,
        }
    )


def _write_twoda(path: Path, data: dict[str, Any]) -> None:
    twoda = TwoDA.from_json(data)
    write_2da(twoda, path, ResourceType.TwoDA)


def _write_gff(path: Path, data: dict[str, Any], restype: ResourceType) -> None:
    gff = GFF.from_json(data)
    write_gff(gff, path, restype)


def _write_text(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8")


def _write_tlk(path: Path, data: dict[str, Any]) -> None:
    tlk = TLK.from_json(data)
    write_tlk(tlk, path, ResourceType.TLK)


def _write_ssf(path: Path, data: dict[str, Any]) -> None:
    ssf = SSF.from_json(data)
    write_ssf(ssf, path, ResourceType.SSF)


def cmd_write(path_str: str, payload_raw: str) -> None:
    path = Path(path_str).expanduser().resolve()
    if path.exists():
        _resname, restype = _identify(path)
    else:
        if path.suffix:
            try:
                _resname, restype = ResourceIdentifier.from_path(path).unpack()
            except Exception:
                _fail(f"Cannot infer resource type for new file: {path}")
        else:
            _fail(f"Cannot write new file without extension: {path}")
    ext = restype.extension.lower()

    try:
        payload = json.loads(payload_raw)
    except json.JSONDecodeError as exc:
        _fail(f"Invalid JSON payload: {exc}")

    fmt = payload.get("format")
    try:
        if fmt == "text" or ext in TEXT_EXTENSIONS:
            path.parent.mkdir(parents=True, exist_ok=True)
            _write_text(path, payload.get("text", ""))
        elif fmt == "twoda" or ext == "2da":
            path.parent.mkdir(parents=True, exist_ok=True)
            _write_twoda(path, payload.get("data", {}))
        elif fmt == "gff" or restype.category == "GFF":
            path.parent.mkdir(parents=True, exist_ok=True)
            _write_gff(path, payload.get("data", {}), restype)
        elif fmt == "tlk" or ext in {"tlk", "fmh", "fml"}:
            path.parent.mkdir(parents=True, exist_ok=True)
            _write_tlk(path, payload.get("data", {}))
        elif fmt == "ssf" or ext == "ssf":
            path.parent.mkdir(parents=True, exist_ok=True)
            _write_ssf(path, payload.get("data", {}))
        elif fmt == "binary" and "base64" in payload:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(base64.b64decode(payload["base64"]))
        else:
            _fail(f"Write not implemented for format '{fmt}' / extension '{ext}'")
    except Exception as exc:
        _fail(f"Write failed: {exc}", traceback=traceback.format_exc())

    _emit({"ok": True, "path": str(path), "bytes": path.stat().st_size})


def cmd_installations() -> None:
    installs: list[dict[str, Any]] = []
    try:
        paths = find_kotor_paths_from_default()
        for game, path_list in paths.items():
            game_id = "k1" if game is Game.K1 else "k2"
            for path in path_list:
                installs.append(
                    {
                        "game": game_id,
                        "path": str(path),
                        "name": path.name,
                    }
                )
    except Exception as exc:
        _fail(f"Installation discovery failed: {exc}")
    _emit({"ok": True, "installations": installs})


def cmd_supported_types() -> None:
    types = []
    for rt in ResourceType:
        types.append(
            {
                "name": rt.name,
                "extension": rt.extension,
                "category": rt.category,
            }
        )
    _emit({"ok": True, "types": types})


def main() -> None:
    parser = argparse.ArgumentParser(description="PyKotor bridge for Godot Holocron plugin")
    sub = parser.add_subparsers(dest="command", required=True)

    p_probe = sub.add_parser("probe")
    p_probe.add_argument("path")

    p_read = sub.add_parser("read")
    p_read.add_argument("path")
    p_read.add_argument("--game", choices=["k1", "k2"], default=None)

    p_write = sub.add_parser("write")
    p_write.add_argument("path")
    p_write.add_argument("--payload", required=True, help="JSON string or @file.json")

    p_extract = sub.add_parser("extract")
    p_extract.add_argument("archive")
    p_extract.add_argument("--resref", required=True)
    p_extract.add_argument("--restype", required=True, help="Resource extension, e.g. 2da, utc")
    p_extract.add_argument("--output", required=True)

    p_inject = sub.add_parser("inject")
    p_inject.add_argument("archive")
    p_inject.add_argument("--resref", required=True)
    p_inject.add_argument("--restype", required=True)
    p_inject.add_argument("--source", required=True)

    sub.add_parser("installations")
    sub.add_parser("supported-types")

    args = parser.parse_args()

    if args.command == "probe":
        cmd_probe(args.path)
    elif args.command == "read":
        cmd_read(args.path, args.game)
    elif args.command == "write":
        payload = args.payload
        if payload.startswith("@"):
            payload = Path(payload[1:]).read_text(encoding="utf-8")
        cmd_write(args.path, payload)
    elif args.command == "extract":
        cmd_extract(args.archive, args.resref, args.restype, args.output)
    elif args.command == "inject":
        cmd_inject(args.archive, args.resref, args.restype, args.source)
    elif args.command == "installations":
        cmd_installations()
    elif args.command == "supported-types":
        cmd_supported_types()
    else:
        _fail(f"Unknown command: {args.command}")


if __name__ == "__main__":
    main()
