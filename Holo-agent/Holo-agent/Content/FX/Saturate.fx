texture ScreenTexture;
float TimeFromHit;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color2;
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	float color = (pixelColor.r + pixelColor.g + pixelColor.b) / 3.0;
	color2 = dot(color, float3(0.3, 0.59, 0.11));
	pixelColor = lerp(pixelColor, color2, TimeFromHit);
	return pixelColor;
}

technique Hit
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}