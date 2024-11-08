#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SourceTex;

sampler2D TexSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct ImageData
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

bool IsSkinTone(float4 colour)
{
    return (colour.r > 0.3 && colour.g > 0.2 && colour.b < 0.2);
}

float4 ProcessColour(ImageData input) : COLOR
{
     // Sample the texture color at the given coordinates
    float4 colour = tex2D(TexSampler, input.TextureCoordinates);
    
    // Check if it's a skin tone, and leave it untouched
    if (IsSkinTone(colour))
    {
        return colour;
    }

    // Otherwise, check if the color is blue and convert it to yellow (mix of red and green)
    if (colour.b > colour.r && colour.b > colour.g)
    {
        // Mix red and green to create a more balanced yellow
        colour.r = 0.9; // Slightly reduced red value (not full saturation)
        colour.g = 0.9; // Slightly reduced green value (to avoid brightness overload)
        colour.b = 0.1; // Keep the blue value the same (we're turning blue to yellow)
    }

    return colour;
}

technique
{
    pass
    {
        PixelShader = compile PS_SHADERMODEL ProcessColour();
    }
};