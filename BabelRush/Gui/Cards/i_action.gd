extends Node2D

@onready var icon: TextureRect = $Icon
@onready var num: Node2D = $Num
@onready var n0: AnimatedSprite2D = $Num/N0
@onready var n1: AnimatedSprite2D = $Num/N1
@onready var placeholder: Sprite2D = $Placeholder


func set_icon(newicon: Texture2D) -> void:
	icon.texture = newicon


func set_value(value: int) -> void:
	if(value<0):
		n1.visible = false
		n0.visible = false
		icon.position.x = 4
	elif(value<10):
		n1.visible = false
		n0.visible = true
		n0.position.x = 11
		icon.position.x = 0
		n0.frame = value
	else:
		n1.visible = true
		n0.visible = true
		n0.position.x = 13
		icon.position.x = 0
		n0.frame = value %10
		n1.frame = (value/10) %10


func set_empty(empty: bool) -> void:
	icon.visible = !empty
	num.visible = !empty
	placeholder.visible = empty
