float4x4 View;
float4x4 Projection;

float3 camPos;
float3 camDir;

int Iterations;
float Bailout;
int Power;
int MarchSteps;

float distance_estimate(float3 pos) {
	float3 z = pos;
	float dr = 1.0;
	float r = 0.0;
	for (int i = 0; i < Iterations ; i++) {
		r = length(z);
		if (r>Bailout) break;

		// convert to polar coordinates
		float theta = acos(z.z/r);
		float phi = atan2(z.y,z.x);
		dr =  pow( r, Power-1.0)*Power*dr + 1.0;

		// scale and rotate the point
		float zr = pow( r,Power);
		theta = theta*Power;
		phi = phi*Power;

		// convert back to cartesian coordinates
		z = zr*float3(sin(theta)*cos(phi), sin(phi)*sin(theta), cos(theta));
		z+=pos;
	}
	return 0.5*log(r)*r/dr;
}



struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

	float3 WorldPos : TEXCOORD0;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

    output.WorldPos = mul(input.Position, View) + camPos + camDir;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

	float3 dir = input.WorldPos - camPos;
	dir = normalize(dir);

	float3 curr = input.WorldPos;

	int s = 0;

	float dist = 0;

	while( s < MarchSteps)
	{
		float next_step = distance_estimate(curr);
		if(next_step < 0.001)
		{
			break;
		}
		curr += next_step * dir;
		dist += next_step;
		s++;
	}

	float mult = (MarchSteps - s) / float(MarchSteps);
	if(mult < 0 || dist < 0)
		mult = 0;

	float4 outColor = float4(1,1,1,1);
	outColor.xyz *= mult;

    return outColor;
}

technique Raymarch
{
    pass Cast
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
