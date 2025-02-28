extends Sprite2D

const n: int = 4
var features: Array[Sprite2D]
var icons: Array[TextureRect]


func _ready():
	features.push_back(self)
	for i in range(1, n):
		features.push_back(get_node("Feature%d"%i))
	for i in range(n):
		icons.push_back(get_node("Icon%d"%i))


func set_count(count: int) -> void:
	for i in range(features.size()):
		features[i].visible = count>i
		icons[i].visible = count>i


func set_icon(index: int, icon: Texture2D) -> void:
	if index in range(icons.size()):
		icons[index].texture = icon
