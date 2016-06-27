int Colorlines = 1;
int Shadowlines = 1;
float RecordingTime;
float RecordingTimeLimit;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : SV_TARGET0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	float pixelRing = (TextureCoordinate.x - 0.5f) * (TextureCoordinate.x - 0.5f) + (TextureCoordinate.y - 0.5f) * (TextureCoordinate.y - 0.5f);
	pixelColor.r /= 1.0f - ((RecordingTime * (RecordingTimeLimit * 0.075f)) + pixelRing);
	pixelColor.g /= 1.0f - ((RecordingTime * (RecordingTimeLimit * 0.075f)) + pixelRing);
	pixelColor.b /= 1.0f - ((RecordingTime * (RecordingTimeLimit * 0.075f)) + pixelRing);
	pixelColor.r = 1.0f - pixelColor.r;
	pixelColor.g = 1.0f - pixelColor.g;
	pixelColor.b = 1.0f - pixelColor.b;
	float y = Position.y;
	float scanline = (y) % (Colorlines + Shadowlines);
	float intensity = scanline < Colorlines ? 1.0f : 0.2f;
	return (pixelColor * intensity);
}

technique Postprocessing
{
	pass Pass1
	{
		PixelShader = compile ps_4_0  PixelShaderFunction();
	}
}