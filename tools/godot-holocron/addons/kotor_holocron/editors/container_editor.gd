@tool
class_name ContainerEditor
extends KotorResourceEditorBase

signal member_open_requested(path: String, context: Dictionary)

@onready var _table: Tree = %ResourceTree
@onready var _filter_edit: LineEdit = %FilterEdit
@onready var _refresh_button: Button = %RefreshButton
@onready var _status: Label = %StatusLabel

var _format: String = "erf"
var _resources: Array = []
var _filter_query: String = ""


func _ready() -> void:
	_table.set_column_titles_visible(true)
	_table.columns = 3
	_table.set_column_title(0, "resref")
	_table.set_column_title(1, "type")
	_table.set_column_title(2, "size")
	_table.item_activated.connect(_on_item_activated)
	_table.item_selected.connect(_on_item_selected)
	var refresh_shortcut := Shortcut.new()
	var f5 := InputEventKey.new()
	f5.keycode = KEY_F5
	refresh_shortcut.events = [f5]
	_refresh_button.shortcut = refresh_shortcut


func _apply_bridge_data(data: Dictionary) -> void:
	var payload: Dictionary = data.get("payload", {})
	_format = str(payload.get("format", "erf"))
	_resources = payload.get("resources", []).duplicate(true)
	_rebuild_tree()
	_update_status_line()


func build_write_payload() -> Dictionary:
	return {}


func _sorted_resources() -> Array:
	var sorted: Array = _resources.duplicate(true)
	sorted.sort_custom(func(a: Dictionary, b: Dictionary) -> bool:
		var ref_a := str(a.get("resref", ""))
		var ref_b := str(b.get("resref", ""))
		if ref_a != ref_b:
			return ref_a.nocasecmp_to(ref_b) < 0
		return str(a.get("restype", "")).nocasecmp_to(str(b.get("restype", ""))) < 0
	)
	if _filter_query == "":
		return sorted

	var filtered: Array = []
	for res in sorted:
		var resref := str(res.get("resref", "")).to_lower()
		var restype := str(res.get("restype", "")).to_lower()
		if _filter_query in resref or _filter_query in restype:
			filtered.append(res)
	return filtered


func _update_status_line() -> void:
	var visible := _sorted_resources().size()
	if _filter_query != "":
		_status.text = "%s — showing %d of %d (double-click to open)" % [
			_format.to_upper(),
			visible,
			_resources.size(),
		]
	else:
		_status.text = "%s — %d resources (double-click to open)" % [_format.to_upper(), _resources.size()]


func _on_filter_changed(new_text: String) -> void:
	_filter_query = new_text.strip_edges().to_lower()
	_rebuild_tree()
	_update_status_line()


func _on_item_selected() -> void:
	var entry := _selected_resource_entry()
	if entry.is_empty():
		_update_status_line()
		return
	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	var size := str(entry.get("size", ""))
	if resref == "" or restype == "":
		_update_status_line()
		return
	_status.text = "%s.%s (%s bytes) — double-click to open" % [resref, restype, size]


func _rebuild_tree() -> void:
	_table.clear()
	for res in _sorted_resources():
		var item := _table.create_item()
		item.set_metadata(0, res)
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


func _on_refresh_pressed() -> void:
	if resource_path == "":
		_status.text = "No archive path loaded"
		return
	_refresh_listing()


func _on_extract_to_file_pressed() -> void:
	var entry := _selected_resource_entry()
	if entry.is_empty():
		_status.text = "Select a resource to extract"
		return
	if resource_path == "":
		_status.text = "No archive path loaded"
		return

	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid resource row"
		return

	var dialog := EditorFileDialog.new()
	dialog.file_mode = EditorFileDialog.FILE_MODE_SAVE_FILE
	dialog.access = EditorFileDialog.ACCESS_FILESYSTEM
	dialog.title = "Extract archive member to file"
	dialog.current_file = "%s.%s" % [resref, restype]
	add_child(dialog)
	dialog.popup_centered_ratio(0.6)
	dialog.file_selected.connect(func(output_path: String) -> void:
		if _extract_member_to_path(resref, restype, output_path):
			_status.text = "Extracted %s.%s to %s" % [resref, restype, output_path.get_file()]
		dialog.queue_free()
	)
	dialog.canceled.connect(func() -> void: dialog.queue_free())


func _extract_member_to_path(resref: String, restype: String, output_path: String) -> bool:
	var result := FormatBridge.extract_member(resource_path, resref, restype, output_path)
	if not result.get("ok", false):
		_status.text = "Extract failed: %s" % str(result.get("error", "unknown"))
		return false

	var extracted := str(result.get("output", output_path))
	if not FileAccess.file_exists(extracted):
		_status.text = "Extracted file missing: %s" % extracted
		return false

	return true


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


func _on_copy_listing_pressed() -> void:
	if _resources.is_empty():
		_status.text = "No resources to copy"
		return

	var lines: PackedStringArray = PackedStringArray(["resref\trestype\tsize"])
	for res in _sorted_resources():
		lines.append(
			"%s\t%s\t%s"
			% [str(res.get("resref", "")), str(res.get("restype", "")), str(res.get("size", ""))]
		)

	var text := "\n".join(lines)
	DisplayServer.clipboard_set(text)
	_status.text = "Copied %d resource(s) to clipboard" % _resources.size()


func _on_copy_member_pressed() -> void:
	var entry := _selected_resource_entry()
	if entry.is_empty():
		_status.text = "Select a resource to copy"
		return
	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid resource row"
		return
	var identity := "%s.%s" % [resref, restype]
	DisplayServer.clipboard_set(identity)
	_status.text = "Copied %s to clipboard" % identity


func _selected_resource_entry() -> Dictionary:
	var selected := _table.get_selected()
	if selected == null:
		return {}
	var entry = selected.get_metadata(0)
	if typeof(entry) != TYPE_DICTIONARY:
		return {}
	return entry


func _on_remove_member_pressed() -> void:
	var entry := _selected_resource_entry()
	if entry.is_empty():
		_status.text = "Select a resource to remove"
		return
	if resource_path == "":
		_status.text = "No archive path loaded"
		return
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
	var entry := _selected_resource_entry()
	if entry.is_empty():
		return
	if resource_path == "":
		_status.text = "No archive path loaded"
		return
	var resref := str(entry.get("resref", "")).strip_edges()
	var restype := str(entry.get("restype", "")).strip_edges().to_lower()
	if resref == "" or restype == "":
		_status.text = "Invalid resource row"
		return

	if KotorResourceTypes.kind_for_extension(restype) == KotorResourceTypes.EditorKind.UNSUPPORTED:
		_status.text = "No Holocron editor for .%s archive members yet" % restype
		return

	var safe_archive := resource_path.get_file().get_basename()
	var output := OS.get_cache_dir().path_join(
		"kotor_holocron_%s_%s.%s" % [safe_archive, resref, restype]
	)

	if not _extract_member_to_path(resref, restype, output):
		return

	_status.text = "Opened member %s.%s (Save writes back to archive)" % [resref, restype]
	member_open_requested.emit(
		output,
		{
			"archive": resource_path,
			"resref": resref,
			"restype": restype,
		}
	)
