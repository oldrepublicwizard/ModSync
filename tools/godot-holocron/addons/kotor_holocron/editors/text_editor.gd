@tool
extends KotorResourceEditorBase

@onready var _editor: CodeEdit = %CodeEdit
@onready var _save_button: Button = %SaveButton
@onready var _status: Label = %StatusLabel

var _read_only: bool = false


func _apply_bridge_data(data: Dictionary) -> void:
	_read_only = false
	_editor.editable = true
	_save_button.disabled = false

	var payload: Dictionary = data.get("payload", {})
	if str(payload.get("format", "")) == "binary":
		_read_only = true
		_editor.text = _binary_preview_text(payload)
		_editor.editable = false
		_save_button.disabled = true
		_status.text = "%s — %d bytes (read-only)" % [
			resource_path.get_file(),
			int(payload.get("size", 0)),
		]
		return

	if payload.get("format") == "text":
		_editor.text = str(payload.get("text", ""))
	elif payload.has("data"):
		_editor.text = JSON.stringify(payload.get("data"), "\t")
	else:
		_editor.text = JSON.stringify(payload, "\t")
	_status.text = resource_path.get_file()


func _binary_preview_text(payload: Dictionary) -> String:
	var size := int(payload.get("size", 0))
	var b64 := str(payload.get("base64", ""))
	var raw := PackedByteArray()
	if b64 != "":
		raw = Marshalls.base64_to_raw(b64)

	var max_bytes := mini(raw.size(), 2048)
	var lines: PackedStringArray = PackedStringArray()
	lines.append("Binary preview (read-only). Dedicated MDL/TPC/WAV editors are planned for a later phase.")
	lines.append("Size: %d bytes" % size)
	if raw.size() > max_bytes:
		lines.append("Showing first %d bytes:" % max_bytes)
	lines.append("")

	var offset := 0
	while offset < max_bytes:
		var row_end := mini(offset + 16, max_bytes)
		var hex_parts: PackedStringArray = PackedStringArray()
		var ascii := ""
		for index in range(offset, row_end):
			var byte := raw[index]
			hex_parts.append("%02x" % byte)
			if byte >= 32 and byte < 127:
				ascii += char(byte)
			else:
				ascii += "."
		var hex_line := " ".join(hex_parts)
		while hex_line.length() < 47:
			hex_line += " "
		lines.append("%08x  %s  %s" % [offset, hex_line, ascii])
		offset += 16

	return "\n".join(lines)


func build_write_payload() -> Dictionary:
	return {"format": "text", "text": _editor.text}


func _on_save_pressed() -> void:
	if _read_only:
		_status.text = "Binary preview is read-only"
		return

	var result := FormatBridge.write_file(resource_path, build_write_payload())
	if result.get("ok", false):
		_dirty = false
		_status.text = "Saved"
		saved.emit(resource_path)
	else:
		_status.text = "Save failed: %s" % str(result.get("error", "unknown"))


func _on_text_changed() -> void:
	if _read_only:
		return
	mark_dirty()
