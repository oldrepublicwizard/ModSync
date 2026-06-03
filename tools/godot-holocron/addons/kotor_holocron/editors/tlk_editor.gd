@tool
extends KotorResourceEditorBase

@onready var _table: Tree = %DataTree
@onready var _status: Label = %StatusLabel

var _strings: Array = []


func _ready() -> void:
	_table.set_column_titles_visible(true)
	_table.columns = 3
	_table.set_column_title(0, "index")
	_table.set_column_title(1, "text")
	_table.set_column_title(2, "sound")


func _apply_bridge_data(data: Dictionary) -> void:
	var payload: Dictionary = data.get("payload", {})
	var body: Dictionary = payload.get("data", {})
	_strings = body.get("strings", []).duplicate(true)
	_rebuild_tree()
	_status.text = "%d strings" % _strings.size()


func build_write_payload() -> Dictionary:
	return {"format": "tlk", "data": {"strings": _strings}}


func _rebuild_tree() -> void:
	_table.clear()
	for entry in _strings:
		var item := _table.create_item()
		item.set_text(0, str(entry.get("_index", entry.get("index", ""))))
		item.set_text(1, str(entry.get("text", "")))
		item.set_text(2, str(entry.get("soundResRef", entry.get("sound_resref", ""))))
		item.set_editable(1, true)
		item.set_editable(2, true)


func _on_item_edited() -> void:
	var edited := _table.get_edited()
	if edited == null:
		return
	var row := edited.get_index()
	if row < 0 or row >= _strings.size():
		return
	var entry: Dictionary = _strings[row]
	entry["text"] = edited.get_text(1)
	entry["soundResRef"] = edited.get_text(2)
	mark_dirty()


func _on_add_string_pressed() -> void:
	var next_index := _strings.size()
	_strings.append({
		"_index": str(next_index),
		"text": "",
		"soundResRef": "",
	})
	mark_dirty()
	_rebuild_tree()


func _on_save_pressed() -> void:
	var result := FormatBridge.write_file(resource_path, build_write_payload())
	if result.get("ok", false):
		_dirty = false
		_status.text = "Saved (%d bytes)" % int(result.get("bytes", 0))
		saved.emit(resource_path)
	else:
		_status.text = "Save failed: %s" % str(result.get("error", "unknown"))
