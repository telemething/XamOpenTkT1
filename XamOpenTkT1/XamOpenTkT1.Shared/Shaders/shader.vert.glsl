attribute vec3 aPosition;
attribute float aColor;
varying vec3 ourColor;
vec3 unpackColor(float f)
{
	vec3 color;
	color.b = floor(f / 16777216.0);
	color.g = floor((f - color.b * 16777216.0) / 65536.0);
	color.r = floor((f - color.b * 16777216.0 - color.g * 65536.0) / 256.0);
	return color / 255.0;
}
void main(void)
{
    gl_Position = vec4(aPosition, 1.0);
	ourColor = unpackColor(aColor);
}

