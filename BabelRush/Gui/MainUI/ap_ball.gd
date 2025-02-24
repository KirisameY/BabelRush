extends Sprite2D

@onready var Points = get_children()

func SetValue(value:int):
	for i in range(6):
		Points[i].visible = i<value
	if value in range(19):
		$Num.frame = value
	else:
		$Num.frame = 19

func SetRate(rate:float):
	$Recoverbar.set_instance_shader_parameter("rate",rate)
