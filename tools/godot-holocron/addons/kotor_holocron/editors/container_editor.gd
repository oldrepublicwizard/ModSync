@tool
class_name ContainerEditor
extends KotorResourceEditorBase

signal member_open_requested(path: String, context: Dictionary)

@onready var _table: Tree = %ResourceTree
@onready var _status: Label = %StatusLabel

var _format: String = "erf"
var _resources: Array = []


func _ready() -> void:
	_table.set_column_titles_visible(true)
	_table.columns = 3
	_table.set_column_title(0, "resref")
	_table.set_column_title(1, "type")
	_table.set_column_title(2, "size")
	_table.item_activated.connect(_on_item_activated)


func _apply_bridge_data(data: Dictionary) -> void:
	var payload: Dictionary = data.get("payload", {})
	_format = str(payload.get("format", "erf"))
	_resources = payload.get("resources", []).duplicate(true)
	_rebuild_tree()
	_status.text = "%s — %d resources (double-click to open)" % [_format.to_upper(), _resources.size()]


func build_write_payload() -> Dictionary:
	return {}


func _rebuild_tree() -> void:
	_table.clear()
	for res in _resources:
		var item := _table.create_item()
		item.set_text(0, str(res.get("resref", "")))
		item.set_text(1, str(res.get("restype", "")))
		item.set_text(2, str(res.get("size", "")))


func _refresh_listing() -> void:
	if resource_path == "":
		return
	var read_result := FormatBridge.read_file(resource_path)
	if read_result.get("ok", false):
		_apply_bridge_data(read_result)
	else:
		_status.text = "Refresh failed: %s" % str(read_result.get("error", "unknown"))


func _on_add_member_pressed() -> void:
	if resource_path == "":
		_status.text = "No archive path loaded"
		return

	var dialog := EditorFileDialog.new()
	dialog.file_mode = EditorFileDialog.FILE_MODE_OPEN_FILE
	dialog.access = EditorFileDialog.ACCESS_FILESYSTEM
	dialog.title = "Add archive member from file"
	add_child(dialog)
	dialog.popup_centered_ratio(0.6)
	dialog.file_selected.connect(func(source_path: String) -> void:
		_add_member_from_file(source_path)
		dialog.queue_free()
	)
	dialog.canceled.connect(func() -> void: dialog.queue_free())


func _add_member_from_file(source_path: String) -> void:
	var file_name := source_path.get_file()
	var dot := file_name.rfind(".")
	if dot <= 0:
		_status.text = "Could not parse resref/restype from filename"
		return

	var resref := file_name.substr(0, dot)
	var restype := file_name.substr(dot + 1).to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid filename for KOTOR resource"
		return

	var result := FormatBridge.add_member(resource_path, resref, restype, source_path)
	if not result.get("ok", false):
		_status.text = "Add failed: %s" % str(result.get("error", "unknown"))
		return

	_status.text = "Added %s.%s to archive" % [resref, restype]
	_refresh_listing()


func _on_remove_member_pressed() -> void:
	var selected := _table.get_selected()
	if selected == null:
		_status.text = "Select a resource to remove"
		return
	var row := selected.get_index()
	if row < 0 or row >= _resources.size():
		_status.text = "Invalid selection"
		return
	if resource_path == "":
		_status.text = "No archive path loaded"
		return

	var entry: Dictionary = _resources[row]
	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid resource row"
		return

	var result := FormatBridge.remove_member(resource_path, resref, restype)
	if not result.get("ok", false):
		_status.text = "Remove failed: %s" % str(result.get("error", "unknown"))
		return

	_status.text = "Removed %s.%s from archive" % [resref, restype]
	_refresh_listing()


func _on_item_activated() -> void:
	var selected := _table.get_selected()
	if selected == null:
		return
	var row := selected.get_index()
	if row < 0 or row >= _resources.size():
		return
	if resource_path == "":
		_status.text = "No archive path loaded"
		return

	var entry: Dictionary = _resources[row]
	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid resource row"
		return

	var safe_archive := resource_path.get_file().get_basename()
	var output := OS.get_cache_dir().path_join(
		"kotor_holocron_%s_%s.%s" % [safe_archive, resref, restype]
	)

	var result := FormatBridge.extract_member(resource_path, resref, restype, output)
	if not result.get("ok", false):
		_status.text = "Extract failed: %s" % str(result.get("error", "unknown"))
		return

	var extracted := str(result.get("output", output))
	if not FileAccess.file_exists(extracted):
		_status.text = "Extracted file missing: %s" % extracted
		return

	_status.text = "Opened member %s.%s (Save writes back to archive)" % [resref, restype]
	member_open_requested.emit(
		extracted,
		{
			"archive": resource_path,
			"resref": resref,
			"restype": restype,
		}
	)
