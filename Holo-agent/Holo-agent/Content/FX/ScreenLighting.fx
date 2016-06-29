texture ScreenTexture;
float Gamma;
float Brightness;
float Contrast;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	
	pixelColor.rgb /= pixelColor.a;

	// Apply contrast.
	pixelColor.rgb = ((pixelColor.rgb - 0.5f) * (Contrast + 1)) + 0.5f;

	// Apply brightness.
	pixelColor.rgb += Brightness;

	// Apply gamma correction
	pixelColor.rgb = pow(abs(pixelColor.rgb), 1.0 / Gamma);

	// Return final pixel color.
	pixelColor.rgb *= pixelColor.a;
	return pixelColor;
}

technique ScreenLightingShader
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
