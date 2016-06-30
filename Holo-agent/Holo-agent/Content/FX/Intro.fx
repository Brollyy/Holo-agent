float IntroTime;
float IntroTimeLimit;
texture ScreenTexture;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 Position : SV_POSITION, float4 Color : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 pixelColor = tex2D(TextureSampler, TextureCoordinate);
	pixelColor *= saturate(-(IntroTime / IntroTimeLimit) * (IntroTime / IntroTimeLimit) + (IntroTime / IntroTimeLimit)) * IntroTimeLimit;
	return pixelColor;
}

technique Intro
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}

