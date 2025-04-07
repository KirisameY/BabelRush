@tool
extends AnimatedSprite2D

@export
var nums: SpriteFrames;
@export
var num_base: Texture2D;
@export
var point0: Texture2D;
@export
var point1: Texture2D;

@export_color_no_alpha
var num_color: Color = 0x404040_ff
@export_color_no_alpha
var p0_color: Color = 0xbfbfbf_ff
@export_color_no_alpha
var p1_color: Color = 0x808080_ff

var n: int = 0

const points: Array[Vector2]= [
	Vector2( 1, 1),
	Vector2( 1, 1),
	Vector2( 1,-1),
	Vector2(-1,-1),
	Vector2(-1, 1),
	Vector2(-1, 1),
]


func _draw():
	# base
	draw_texture(num_base, -num_base.get_size()/2)
	
	# points
	for i in range(min(n,6)):
		var tex_ : Texture2D = point1 if i%3 == 1 else point0
		var size := tex_.get_size()
		var dir : Vector2 = points[i]
		var color := p1_color if i%2 else p0_color
		draw_set_transform(Vector2.ZERO, 0, dir)
		draw_texture(tex_, -size/2, color)
	draw_set_transform(Vector2.ZERO)
	
	# nums
	var tex: Texture2D
	if n>6: tex = nums.get_frame_texture("x",0)
	elif n<0: tex = nums.get_frame_texture("no", 0)
	else: tex = nums.get_frame_texture("default", n)
	draw_texture(tex, -tex.get_size()/2, num_color)
	

func set_value(value: int) -> void:
	n = value
	frame = 1 if value<0 else 0
	queue_redraw()
