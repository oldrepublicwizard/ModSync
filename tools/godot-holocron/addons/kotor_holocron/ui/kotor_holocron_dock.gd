@tool
extends Control

@onready var _path_edit: LineEdit = %PathEdit
@onready var _status: Label = %StatusLabel
@onready var _editor_host: Control = %EditorHost
@onready var _install_list: ItemList = %InstallList

var _current_editor: KotorResourceEditorBase
var _archive_inject_context: Dictionary = {}


func _ready() -> void:
	_refresh_installations()


func _on_install_activated(index: int) -> void:
	var result := FormatBridge.list_installations()
	if not result.get("ok", false):
		return
	var installs: Array = result.get("installations", [])
	if index < 0 or index >= installs.size():
		return
	var base := str(installs[index].get("path", "")).strip_edges()
	if base == "":
		return
	var candidates := [
		base.path_join("dialog.tlk"),
		base.path_join("dialog_f.tlk"),
	]
	for candidate in candidates:
		if FileAccess.file_exists(candidate):
			_path_edit.text = candidate
			_open_path(candidate)
			return
	_path_edit.text = base
	_status.text = "Install: %s — pick a resource file" % base.get_file()


func _refresh_installations() -> void:
	_install_list.clear()
	var result := FormatBridge.list_installations()
	if not result.get("ok", false):
		_status.text = "Bridge: %s" % str(result.get("error", "unavailable"))
		return
	for item in result.get("installations", []):
		var label := "%s — %s" % [item.get("game", "?"), item.get("path", "")]
		_install_list.add_item(label)


func _on_browse_pressed() -> void:
	var dialog := EditorFileDialog.new()
	dialog.file_mode = EditorFileDialog.FILE_MODE_OPEN_FILE
	dialog.access = EditorFileDialog.ACCESS_FILESYSTEM
	dialog.title = "Open KOTOR Resource"
	add_child(dialog)
	dialog.popup_centered_ratio(0.6)
	dialog.file_selected.connect(func(path: String) -> void:
		_path_edit.text = path
		_open_path(path)
		dialog.queue_free()
	)
	dialog.canceled.connect(func() -> void: dialog.queue_free())


func _on_open_pressed() -> void:
	var path := _path_edit.text.strip_edges()
	if path == "":
		_status.text = "Enter a file path"
		return
	_open_path(path)


func _open_path(path: String) -> void:
	var probe := FormatBridge.probe(path)
	if not probe.get("ok", false):
		_status.text = "Probe failed: %s" % str(probe.get("error", ""))
		return
	var ext := str(probe.get("extension", "")).to_lower()
	var kind := KotorResourceTypes.kind_for_extension(ext)
	var read_result := FormatBridge.read_file(path)
	if not read_result.get("ok", false):
		_status.text = "Read failed: %s" % str(read_result.get("error", ""))
		return
	_clear_editor()
	_current_editor = EditorRegistry.create_editor(kind)
	if _current_editor == null:
		_status.text = "No editor for .%s" % ext
		return
	_editor_host.add_child(_current_editor)
	_current_editor.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_current_editor.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_current_editor.size_flags_vertical = Control.SIZE_EXPAND_FILL
	_current_editor.load_resource(path, read_result)
	_wire_nested_open(_current_editor)
	_connect_editor_saved(_current_editor)
	_status.text = "%s — %s" % [
		KotorResourceTypes.kind_label(kind),
		path.get_file(),
	]


func _wire_nested_open(editor: KotorResourceEditorBase) -> void:
	if editor is ContainerEditor:
		var container := editor as ContainerEditor
		if not container.member_open_requested.is_connected(_on_member_open_requested):
			container.member_open_requested.connect(_on_member_open_requested)


func _on_member_open_requested(member_path: String, context: Dictionary) -> void:
	_archive_inject_context = context.duplicate(true)
	_open_path(member_path)


func _connect_editor_saved(editor: KotorResourceEditorBase) -> void:
	if not editor.saved.is_connected(_on_editor_saved):
		editor.saved.connect(_on_editor_saved)


func _on_editor_saved(saved_path: String) -> void:
	if _archive_inject_context.is_empty():
		return

	var archive := str(_archive_inject_context.get("archive", "")).strip_edges()
	var resref := str(_archive_inject_context.get("resref", "")).strip_edges()
	var restype := str(_archive_inject_context.get("restype", "")).strip_edges()
	if archive == "" or resref == "" or restype == "":
		_archive_inject_context = {}
		return

	var result := FormatBridge.inject_member(archive, resref, restype, saved_path)
	if result.get("ok", false):
		_status.text = "Saved %s.%s into %s" % [resref, restype, archive.get_file()]
		_archive_inject_context = {}
		_open_path(archive)
		return
	_status.text = "Archive inject failed: %s" % str(result.get("error", "unknown"))
	_archive_inject_context = {}


func _clear_editor() -> void:
	_archive_inject_context = {}
	if _current_editor and _current_editor.saved.is_connected(_on_editor_saved):
		_current_editor.saved.disconnect(_on_editor_saved)
	if _current_editor:
		_current_editor.queue_free()
		_current_editor = null
	for child in _editor_host.get_children():
		child.queue_free()
