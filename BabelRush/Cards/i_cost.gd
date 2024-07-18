extends AnimatedSprite2D

@onready
var num: AnimatedSprite2D = $NumBase/num;
@onready
var points: Array = $Points.get_children();


func SetValue(value: int) -> void:
	#base
	if value<0:
		frame=1
	else:
		frame=0
	#points
	for i in range(6):
		points[i].visible = (value>i)
	#num
	if value>6:
		num.animation="x"
		num.frame=0
	elif value<0:
		num.animation="no"
		num.frame=0
	else:
		num.animation="default"
		num.frame=value
