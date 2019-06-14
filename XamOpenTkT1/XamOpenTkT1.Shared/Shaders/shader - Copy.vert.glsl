#version 300 es
in vec3 aPosition;
in float aColor;
out vec3 ourColor;
vec3 unpackColor(float fColor) 
{
	vec3 color;
	uint mask = uint(0x000000FF);
	uint ucolor = uint(fColor);
	float scale = float(255);
	color.r = float(mask & (ucolor >> 24)) / scale;
	color.g = float(mask & (ucolor >> 16)) / scale;
	color.b = float(mask & (ucolor >> 8)) / scale;
	// now we have a vec3 with the 3 components in range [0..255]. Let's normalize it!
	return color;
}
void main(void)
{
    gl_Position = vec4(aPosition, 1.0);
	ourColor = unpackColor(aColor);
}

