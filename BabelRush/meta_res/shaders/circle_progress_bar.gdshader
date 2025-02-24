shader_type canvas_item;

instance uniform float rate : hint_range(0.0, 1.0) = 0.25;

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 pos = UV*2.-vec2(1,1);
	float a = .5 - atan(pos.x,pos.y)/TAU;
	COLOR.a *= step(a, rate);
}
