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
	return pixelColor * float4(1.0f, 0.0f, 0.0f, 1.0f);
}

technique GameOver
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
