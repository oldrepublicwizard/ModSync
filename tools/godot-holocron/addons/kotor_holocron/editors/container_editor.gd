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
