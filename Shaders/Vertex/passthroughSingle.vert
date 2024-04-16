#version 430 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in float aColor; 
  
out float lightness;

uniform mat4 transform;

void main()
{
    gl_Position = vec4(aPos, 1.0) * transform;
    lightness = aColor; 
}      