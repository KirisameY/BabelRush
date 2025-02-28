@tool
extends Node2D

const n: int = 4
const icon_size := Vector2(8,8)

@export
var border0: Texture2D
@export
var border1: Texture2D

var count: int = 2

var icons: Array[Texture2D] = [
	PlaceholderTexture2D.new(),
	PlaceholderTexture2D.new(),
	PlaceholderTexture2D.new(),
	PlaceholderTexture2D.new(),
]


func _draw():
	for i in range(min(n,count)):
		var border_tex := border0 if i==0 else border1
		var tex := icons[i]
		var pos_shift := Vector2(0, i*9)
		draw_texture(border_tex, pos_shift - border_tex.get_size()/2)
		draw_texture_rect(tex, Rect2(pos_shift - icon_size/2, icon_size), false)


func set_count(new_count: int) -> void:
	count = new_count
	queue_redraw()


func set_icon(index: int, icon: Texture2D) -> void:
	if index >= 4: return
	if index in range(n):
		icons[index] = icon
