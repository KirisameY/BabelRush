extends RichTextLabel

@export var feature_color: Color
@export var action_color: Color
@export var detail_color: Color

var title_font: Font
var title_size: int    = 19
var subtitle_font: Font
var subtitle_size: int = 12
var detail_font: Font
var detail_size: int   = 8
var any_field: bool    = false


func reset():
	text=""
	any_field=false


func append_title(title: String, _value: int):
	#head space
	append_empty_line(4)
	#title
	push_outline_size(4)
	push_font(title_font, title_size)
	push_paragraph(HORIZONTAL_ALIGNMENT_CENTER)
	add_text(title)
	pop_all()
	newline()
	#seperator
	append_empty_line(8)


func append_feature(icon: Texture2D, title: String, value: int, desc: String):
	append_field(icon, title, feature_color, value, desc)
	
	
func append_action(icon: Texture2D, title: String, value: int, desc: String):
	append_field(icon, title, action_color, value, desc)	


func append_field(icon: Texture2D, title: String, color: Color, value: int, desc: String):
	#seperator
	if(any_field): append_empty_line(4)
	#title
	push_outline_size(2)
	push_font(subtitle_font, subtitle_size)
	add_image(icon, 10, 10)#icon
	add_text(' ')#title text
	push_color(color)
	add_text(title)
	pop()
	add_text(' ')
	add_text(str(value))#value
	pop_all()
	newline()
	#description
	push_outline_size(1)
	push_font(detail_font, detail_size)
	push_color(detail_color)
	append_text(desc)
	pop_all()
	newline()
	
	
func append_footer():
	append_empty_line(8)


func append_empty_line(height: int):
	push_font_size(height)
	add_text(' ')
	pop()
	newline()
