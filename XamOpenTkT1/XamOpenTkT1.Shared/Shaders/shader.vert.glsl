attribute vec3 aPosition;
attribute vec3 aOffset;
attribute float aColor;
varying vec3 ourColor;
//uniform mat4 model;
//uniform mat4 view;
//uniform mat4 projection;
uniform mat4 transform;
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
	gl_PointSize = 0.1;
	gl_Position = transform * vec4(aPosition + aOffset, 1.0);
	//gl_Position = model * vec4(aPosition + aOffset, 1.0);
	//gl_Position = model * view * vec4(aPosition + aOffset, 1.0);
	//gl_Position = model * view * projection * vec4(aPosition + aOffset, 1.0);
	//gl_Position = transform * vec4(aPosition + aOffset, 1.0);
	//gl_Position = transform * vec4(aPosition, 1.0);
	//gl_Position = vec4(aPosition, 1.0);
	ourColor = unpackColor(aColor);
}

