texture ScreenTexture;
float Health;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	float z = (TextureCoordinate.x - 0.5f) * (TextureCoordinate.x - 0.5f) + (TextureCoordinate.y - 0.5f) * (TextureCoordinate.y - 0.5f);
	pixelColor.r *= z - (Health * 0.01f);
	pixelColor.g = 0.0f;
	pixelColor.b = 0.0f;
	pixelColor.a *= z - (Health * 0.01f);
	return pixelColor;
}

technique ChangeColor
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}