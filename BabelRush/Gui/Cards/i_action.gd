@tool
extends Node2D

@export
var nums: SpriteFrames
@export
var place_holder: Texture2D

@export_color_no_alpha
var num_color: Color = 0x404040_ff

var icon : Texture2D = PlaceholderTexture2D.new()
var n : int = 0
var empty := false

func _draw():
	if empty:
		draw_texture(place_holder, Vector2(8,1), num_color)
		return
	
	# icon
	var icon_x := 4 if n<0 else 0
	var icon_rect := Rect2(icon_x, -4, 8, 8)
	draw_texture_rect(icon, icon_rect, false)
	
	# num
	if n<0: pass
	elif n<10:
		var tex := nums.get_frame_texture("default", n)
		draw_texture(tex, Vector2(11,-3), num_color)
	else:
		var tex := nums.get_frame_texture("default", n%10)
		draw_texture(tex, Vector2(13,-3), num_color)
		tex = nums.get_frame_texture("default", (n/10)%10)
		draw_texture(tex, Vector2(9,-3), num_color)


func set_icon(newicon: Texture2D) -> void:
	icon = newicon
	queue_redraw()


func set_value(value: int) -> void:
	n = value
	queue_redraw()


func set_empty(e: bool) -> void:
	empty = e
	queue_redraw()
