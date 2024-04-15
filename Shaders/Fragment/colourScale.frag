#version 430
out vec4 FragColor;  
in float lightness;
  
void main()
{
    // FragColor = vec4(1.0f * lightness, 1.0f * lightness, 0.2f * lightness, 1.0f);
    FragColor = vec4(1.0f * lightness, 0.0f, 0.0f, 1.0f);
}