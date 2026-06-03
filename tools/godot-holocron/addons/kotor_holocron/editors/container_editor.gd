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


func _listing_has_member(resref: String, restype: String) -> bool:
	for res in _resources:
		if str(res.get("resref", "")) == resref and str(res.get("restype", "")).to_lower() == restype:
			return true
	return false


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
		_prompt_add_member(source_path)
		dialog.queue_free()
	)
	dialog.canceled.connect(func() -> void: dialog.queue_free())


func _default_resref_restype(source_path: String) -> Dictionary:
	var file_name := source_path.get_file()
	var dot := file_name.rfind(".")
	if dot <= 0:
		return {"resref": "", "restype": ""}
	return {
		"resref": file_name.substr(0, dot),
		"restype": file_name.substr(dot + 1).to_lower(),
	}


func _prompt_add_member(source_path: String) -> void:
	var defaults := _default_resref_restype(source_path)
	var default_resref := str(defaults.get("resref", ""))
	var default_restype := str(defaults.get("restype", ""))

	var win := Window.new()
	win.title = "Add archive member"
	win.size = Vector2i(360, 160)
	win.unresizable = true

	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	margin.add_theme_constant_override("margin_left", 12)
	margin.add_theme_constant_override("margin_right", 12)
	margin.add_theme_constant_override("margin_top", 12)
	margin.add_theme_constant_override("margin_bottom", 12)
	win.add_child(margin)

	var vbox := VBoxContainer.new()
	vbox.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	margin.add_child(vbox)

	var resref_row := HBoxContainer.new()
	resref_row.add_child(Label.new())
	(resref_row.get_child(0) as Label).text = "resref"
	var resref_edit := LineEdit.new()
	resref_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	resref_edit.text = default_resref
	resref_row.add_child(resref_edit)
	vbox.add_child(resref_row)

	var type_row := HBoxContainer.new()
	type_row.add_child(Label.new())
	(type_row.get_child(0) as Label).text = "type"
	var restype_edit := LineEdit.new()
	restype_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	restype_edit.text = default_restype
	type_row.add_child(restype_edit)
	vbox.add_child(type_row)

	var buttons := HBoxContainer.new()
	buttons.alignment = BoxContainer.ALIGNMENT_END
	var cancel_btn := Button.new()
	cancel_btn.text = "Cancel"
	var ok_btn := Button.new()
	ok_btn.text = "Add"
	buttons.add_child(cancel_btn)
	buttons.add_child(ok_btn)
	vbox.add_child(buttons)

	add_child(win)

	cancel_btn.pressed.connect(func() -> void: win.queue_free())
	ok_btn.pressed.connect(func() -> void:
		var resref := resref_edit.text.strip_edges()
		var restype := restype_edit.text.strip_edges().to_lower()
		win.queue_free()
		_add_member_with_identity(source_path, resref, restype)
	)
	win.close_requested.connect(func() -> void: win.queue_free())
	win.popup_centered()


func _add_member_with_identity(source_path: String, resref: String, restype: String) -> void:
	if resref == "" or restype == "":
		_status.text = "resref and type are required"
		return

	var replacing := _listing_has_member(resref, restype)
	var result := FormatBridge.add_member(resource_path, resref, restype, source_path)
	if not result.get("ok", false):
		_status.text = "Add failed: %s" % str(result.get("error", "unknown"))
		return

	if replacing:
		_status.text = "Replaced %s.%s in archive" % [resref, restype]
	else:
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

	var confirm := ConfirmationDialog.new()
	confirm.title = "Remove archive member"
	confirm.dialog_text = (
		"Remove %s.%s from %s?\nThis cannot be undone except by re-adding the file."
		% [resref, restype, resource_path.get_file()]
	)
	confirm.ok_button_text = "Remove"
	confirm.cancel_button_text = "Cancel"
	add_child(confirm)
	confirm.confirmed.connect(func() -> void:
		_execute_remove_member(resref, restype)
		confirm.queue_free()
	)
	confirm.canceled.connect(func() -> void: confirm.queue_free())
	confirm.popup_centered()


func _execute_remove_member(resref: String, restype: String) -> void:
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
