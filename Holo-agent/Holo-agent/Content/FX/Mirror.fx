﻿float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 TintColor = float4(1, 1, 1, 1);
float3 CameraPosition;

sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Mirror;
	AddressV = Mirror;
};

struct VertexShaderInput
{
	float4 Position : SV_POSITION;
	float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Reflection : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);

	float4 VertexPosition = mul(input.Position, World);
	float3 ViewDirection = CameraPosition - VertexPosition.xyz;

	float3 Normal = normalize(mul(input.Normal, WorldInverseTranspose)).xyz;
	output.Reflection = reflect(-normalize(ViewDirection), normalize(Normal));

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return TintColor;
}

technique Reflection
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}