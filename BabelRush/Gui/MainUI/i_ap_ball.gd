extends Sprite2D

@onready var points = get_children()

func set_value(value:int):
	for i in range(6):
		points[i].visible = i<value
	if value in range(19):
		$Num.frame = value
	else:
		$Num.frame = 19

func set_rate(rate:float):
	$Recoverbar.set_instance_shader_parameter("rate",rate)
