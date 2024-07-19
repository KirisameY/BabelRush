extends Node2D

@onready var Icon = $Icon
@onready var Num = $Num
@onready var N0 = $Num/N0
@onready var N1 = $Num/N1
@onready var Placeholder = $Placeholder


func SetIcon(icon: Texture2D) -> void:
	Icon.texture = icon


func SetValue(value: int) -> void:
	if(value<0):
		N1.visible = false
		N0.visible = false
		Icon.position.x = 4
	elif(value<10):
		N1.visible = false
		N0.visible = true
		N0.position.x = 12.5
		Icon.position.x = 0
		N0.frame = value
	else:
		N1.visible = true
		N0.visible = true
		N0.position.x = 14.5
		Icon.position.x = 0
		N0.frame = value %10
		N1.frame = (value/10) %10


func SetEmpty(empty: bool) -> void:
	Icon.visible = !empty
	Num.visible = !empty
	Placeholder.visible = empty
