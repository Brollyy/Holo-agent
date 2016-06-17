texture ScreenTexture;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);

	float average = (pixelColor.r + pixelColor.g + pixelColor.b) / 3;

	pixelColor.r = average;
	pixelColor.g = average;
	pixelColor.b = average;

	return pixelColor;
}

technique Grayscale
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}