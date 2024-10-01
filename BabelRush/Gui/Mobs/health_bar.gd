extends Sprite2D

@export_color_no_alpha
var health_color: Color
@export_color_no_alpha
var shielded_health_color: Color

var shielded: bool = false:
	set(value):
		shielded = value
		$HealthbarShield.visible = value
		queue_redraw()
func set_shielded(value:bool): shielded=value

var max_health: int = 100:
	set(value):
		value = max(value,0)
		max_health = value
		queue_redraw()
var health: int:
	set(value):
		value = clamp(value,0,max_health)
		health = value
		queue_redraw()
func set_max_health(value:float): max_health=int(value)
func set_health(value:float): health=int(value)

var left_pos:float
var right_pos:float

func _ready():
	left_pos=$LeftMarker.position.x
	right_pos=$RightMarker.position.x
	$LeftMarker.queue_free()
	$RightMarker.queue_free()


func _draw():
	var h_rate:float = float(health) / float(max_health)
	var h_rect = Rect2(left_pos,-2,(right_pos-left_pos)*h_rate,4)
	var h_color = health_color if !shielded else shielded_health_color
	draw_rect(h_rect,h_color)
	
	
	
	
	
