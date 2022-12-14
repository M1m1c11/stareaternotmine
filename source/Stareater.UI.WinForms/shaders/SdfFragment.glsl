#version 140

uniform sampler2D textureSampler;
uniform vec4 color;
uniform float smoothDist;

in vec2 textureCoord;

out vec4 outputF;

void main()
{
   outputF = vec4(
      color.r, color.g, color.b, 
      smoothstep(0.5 - smoothDist, 0.5, texture2D(textureSampler, textureCoord).a) * color.a
   );
}