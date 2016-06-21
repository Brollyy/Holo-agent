texture ScreenTexture;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 tex;
	tex = tex2D(TextureSampler, TextureCoordinate) * .6f;
	tex += tex2D(TextureSampler, TextureCoordinate + (0.005)) * .2f;
	return tex;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}