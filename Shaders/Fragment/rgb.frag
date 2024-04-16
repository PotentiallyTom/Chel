#version 430
out vec4 FragColor;  
in vec3 rgb;
  
void main()
{
    FragColor = vec4(rgb.x, rgb.y, rgb.z, 1.0f);
}