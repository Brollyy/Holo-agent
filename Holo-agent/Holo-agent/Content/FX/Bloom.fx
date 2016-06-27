float Threshold = 0.45f;
float BlurDistance = 0.0025f;
texture ScreenTexture;
float BloomIntensity = 1.25f;
float OriginalIntensity = 1.0f;
float BloomSaturation = 0.75f;
float OriginalSaturation = 0.5f;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor1 = tex2D(TextureSampler, float2(TextureCoordinate.x + BlurDistance, TextureCoordinate.y + BlurDistance));
	float4 pixelColor2 = tex2D(TextureSampler, TextureCoordinate);
	pixelColor1 += tex2D(TextureSampler, float2(TextureCoordinate.x - BlurDistance, TextureCoordinate.y - BlurDistance));
	pixelColor1 += tex2D(TextureSampler, float2(TextureCoordinate.x + BlurDistance, TextureCoordinate.y - BlurDistance));
	pixelColor1 += tex2D(TextureSampler, float2(TextureCoordinate.x - BlurDistance, TextureCoordinate.y + BlurDistance));
	pixelColor1 *= 0.25f;
	return saturate((pixelColor1 - Threshold) / (1.0f - Threshold)) + pixelColor2;
}

technique Bloom
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}