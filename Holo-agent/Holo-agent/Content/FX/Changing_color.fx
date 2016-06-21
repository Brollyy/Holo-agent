texture ScreenTexture;
float Timer;
float4 Color;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	pixelColor.r = pixelColor.r + (Color.r - pixelColor.r) * Timer;
	pixelColor.g = pixelColor.g + (Color.g - pixelColor.g) * Timer;
	pixelColor.b = pixelColor.b + (Color.b - pixelColor.b) * Timer;
	return pixelColor;
}

technique ChangeColor
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}