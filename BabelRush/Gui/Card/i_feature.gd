extends Sprite2D

const n: int = 4
var Features: Array[Sprite2D]
var Icons: Array[TextureRect]


func _ready():
	Features.push_back(self)
	for i in range(1, n):
		Features.push_back(get_node("Feature%d"%i))
	for i in range(n):
		Icons.push_back(get_node("Icon%d"%i))


func SetCount(count: int) -> void:
	for i in range(Features.size()):
		Features[i].visible = count>i
		Icons[i].visible = count>i


func SetIcon(index: int, icon: Texture2D) -> void:
	if index in range(Icons.size()):
		Icons[index].texture = icon
