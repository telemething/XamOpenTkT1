#version 300 es
precision mediump float;
in vec3 ourColor;
out vec4 outputColor;
void main()
{
    outputColor = vec4(ourColor, 1.0f);
}