float BlurDistance = 0.0025f;
texture ScreenTexture;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, float2(TextureCoordinate.x + BlurDistance, TextureCoordinate.y + BlurDistance));
	pixelColor += tex2D(TextureSampler, float2(TextureCoordinate.x - BlurDistance, TextureCoordinate.y - BlurDistance));
	pixelColor += tex2D(TextureSampler, float2(TextureCoordinate.x + BlurDistance, TextureCoordinate.y - BlurDistance));
	pixelColor += tex2D(TextureSampler, float2(TextureCoordinate.x - BlurDistance, TextureCoordinate.y + BlurDistance));
	pixelColor *= 0.25f;
	float average = (pixelColor.r + pixelColor.g + pixelColor.b) * 0.33f;
	pixelColor.r = average;
	pixelColor.g = average;
	pixelColor.b = average;
	return pixelColor;
}

technique PauseMenu
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
