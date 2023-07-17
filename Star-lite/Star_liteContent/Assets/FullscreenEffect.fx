float3 Tint;

sampler TextureSampler: register(s0);
float4 PixelShaderFunction(float2 texcoord: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, texcoord);

    // Calculate the distance of the current pixel from the center of the screen
    float2 center = float2(0.5, 0.5);
    float distance = length(texcoord - center);

    // Apply a gradient falloff function to the distance value
    float falloff = smoothstep(0.1, 1, distance);

    falloff = 1 - falloff;

    // Multiply the input color by the falloff value
    color.rgb *= falloff;

    color.rbg += (1 - falloff) * Tint;

    return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
