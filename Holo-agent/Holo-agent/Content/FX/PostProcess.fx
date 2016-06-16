texture ScreenTexture;
sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);

	float average = (pixelColor.r + pixelColor.g + pixelColor.b) / 3;

	if (average > 0.95) {
		average = 1.0;
	}
	else if (average > 0.5) {
		average = 0.7;
	}
	else if (average > 0.2) {
		average = 0.35;
	}
	else {
		average = 0.1;
	}

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