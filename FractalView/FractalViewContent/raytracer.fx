float4x4 View;
float4x4 Projection;

float3 camPos;
float3 camDir;

int Iterations;
float Bailout;
int Power;
int MarchSteps;

float Scale;

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

float orbit_trap(float3 p)
{
	float3 w = p;
	float dist = 1e10;
	for(int i=0; i<Iterations; ++i)
	{
		float x = w.x; float x2 = x*x; float x4 = x2*x2;
		float y = w.y; float y2 = y*y; float y4 = y2*y2;
		float z = w.z; float z2 = z*z; float z4 = z2*z2;

		float k3 = x2 + z2;
		float k2 = 1 / sqrt( k3*k3*k3*k3*k3*k3*k3 );
		float k1 = x4 + y4 + z4 - 6.0*y2*z2 - 6.0*x2*y2 + 2.0*z2*x2;
		float k4 = x2 - y2 + z2;

		w.x =  64.0*x*y*z*(x2-z2)*k4*(x4-6.0*x2*z2+z4)*k1*k2;
		w.y = -16.0*y2*k3*k4*k4 + k1*k1;
		w.z = -8.0*y*k4*(x4*x4 - 28.0*x4*x2*z2 + 70.0*x4*z4 - 28.0*x2*z2*z4 + z4*z4)*k1*k2;
		
		dist = min(dist, length(w));

	}
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

	float3 curr = camPos;

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

VertexShaderOutput IterateVS(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

    output.WorldPos = input.Position * Scale + camPos;

    return output;
}

float4 IteratePS(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.WorldPos;

	float x,y,x0,y0;

	x = x0 = pos.x;
	y = y0 = pos.y;

	float xx = x * x;
	float yy = y * y;

	int it = Iterations;

	while(it && (xx + yy < 4))
	{
		y = 2 * x * y + y0;
		x = xx - yy + x0;

		xx = x * x;
		yy = y * y;
		it--;
	}

	float4 color = float4(0, 0, 0, 1);
	
	if(it)
		color.xyz = float3(0.1, 0.25, 0.9) * (1.2f - it / float(Iterations));

	return color;

}

technique Iterate
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 IterateVS();
		PixelShader = compile ps_3_0 IteratePS();
	}
}