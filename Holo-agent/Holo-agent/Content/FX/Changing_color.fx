texture ScreenTexture;
float Timer;
sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	float r = pixelColor.r - (pixelColor.r*Timer);
	float g = pixelColor.g - (pixelColor.g*Timer);
	float b = pixelColor.b + (pixelColor.b*Timer);
	float average = (pixelColor.r + pixelColor.g + pixelColor.b) / 3;

	pixelColor.r = r;
	pixelColor.g = g;



	return pixelColor;
}

technique Grayscale
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}