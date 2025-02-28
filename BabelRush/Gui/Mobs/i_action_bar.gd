extends Node2D

@export_color_no_alpha
var progress_color: Color:
	set(value): 
		progress_color = value
		call_deferred("update_color", value)

@export
var nums: SpriteFrames

@onready var progress_node: Sprite2D = $Progress

var action_value: int = -1:
	set(value):
		if action_value == value: return
		action_value = value
		queue_redraw()


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func _draw():
	var pos := Vector2(7,-3)
	match action_value:
		-1: return
		0 : draw_texture(num_tex(0), pos); return
	
	var n: int = action_value
	pos.x += floori(log(n)/log(10)) * 5
	while n!=0:
		draw_texture(num_tex(n%10), pos)
		pos.x -= 5
		n = n/10


func set_progress(value: float):
	progress_node.set_instance_shader_parameter("rate", value)

func set_icon(icon: Texture2D):
	($Sprite2D as TextureRect).texture = icon

func set_action_value(value: int):
	action_value = value


func update_color(color: Color):
	progress_node.self_modulate = color

func num_tex(n: int) -> Texture2D:
	return nums.get_frame_texture("default", n)
