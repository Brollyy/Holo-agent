texture ScreenTexture;
float Health;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	pixelColor.r = 1.0f - (Health * 0.01f);
	pixelColor.g = 0.0f;
	pixelColor.b = 0.0f;
	pixelColor.a = 0.0f;
	return pixelColor;
}

technique ChangeColor
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}