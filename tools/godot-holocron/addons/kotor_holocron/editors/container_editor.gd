@tool
extends KotorResourceEditorBase

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


func _apply_bridge_data(data: Dictionary) -> void:
	var payload: Dictionary = data.get("payload", {})
	_format = str(payload.get("format", "erf"))
	_resources = payload.get("resources", []).duplicate(true)
	_rebuild_tree()
	_status.text = "%s — %d resources (read-only)" % [_format.to_upper(), _resources.size()]


func build_write_payload() -> Dictionary:
	return {}


func _rebuild_tree() -> void:
	_table.clear()
	for res in _resources:
		var item := _table.create_item()
		item.set_text(0, str(res.get("resref", "")))
		item.set_text(1, str(res.get("restype", "")))
		item.set_text(2, str(res.get("size", "")))
