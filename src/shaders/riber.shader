shader_type spatial;

uniform float aspect_radio = 0.5f;
uniform float tilefactor = 1f;
uniform vec4 colortint : hint_color;
uniform sampler2D noise;
uniform float speed = 0.5f;


void fragment() {
	vec2 coord = UV * tilefactor;

	float t = TIME * speed;
	coord.y *= aspect_radio; 
	coord.y += cos(t)*0.1f;
	
	vec4 txtpos =  texture(noise,coord.xy);
	vec3 result = mix(txtpos.rgb,colortint.rgb,colortint.a);
	ALBEDO = result;
}