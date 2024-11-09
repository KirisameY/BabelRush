shader_type canvas_item;

uniform float rate : hint_range(0.0, 1.0) = 0.25;

//void vertex() {
//	// Called for every vertex the material is visible on.
//}

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 pos = UV*2.-vec2(1,1);
	float a = .5 - atan(pos.x,pos.y)/TAU;
	COLOR.a *= step(a, rate);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
