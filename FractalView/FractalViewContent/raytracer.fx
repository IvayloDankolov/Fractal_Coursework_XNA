//��������� �� ������, ������ ���� � ���������� �� ���������
float4x4 View;

//������������ ��-���� ������ �� ������ ���� � ����������, ������� �� ��������, ������ � �.�.
float3 camPos;
float3 camDir;

int Iterations;
float Bailout;
int Power;
int MarchSteps;

float Scale;


// ��������� ������������� ��������� �� ������������ �� ��������,
// ��������� �������� 
float distance_estimate(float3 pos) {
	float3 z = pos;
	float dr = 1.0;
	float r = 0.0;
	for (int i = 0; i < Iterations ; i++) {
		
		//��������� �� ��������� �� �������. ���� � ����������� �� "�����" ��������.
		r = length(z);
		if (r>Bailout) break;

		// ������� � ������� ����������
		float theta = acos(z.z/r);
		float phi = atan2(z.y,z.x);

		// ���������� ������������ �� ����������� �������
		dr =  pow( r, Power-1.0)*Power*dr + 1.0;

		// ��������� � ������ �������, ������ ����������� �� ������� �� ������� �� ������ �� n-�� ������
		float zr = pow( r,Power);
		theta = theta*Power;
		phi = phi*Power;

		// ������� �� � ��������� ����������, �� �� �������� ����������
		z = zr*float3(sin(theta)*cos(phi), sin(phi)*sin(theta), cos(theta));
		z+=pos;
	}

	// �������������� �� �� ���������� �� �������� ���� �� ��������� �� �����.
	// ������ � ��������������
	return 0.5*log(r)*r/dr;
}

//����� �� ������� �� �������. ���� ������� � ����� ������
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

//������ �� ������� �� �������, ������ �� ���������� ��������� �� ������ � �������� �������
struct VertexShaderOutput
{
    float4 Position : POSITION0;

	float3 WorldPos : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

	//��������, �� ������ � ������ �� ���������� 1 �� �������� (camDir)
	//������� � ����
    output.WorldPos = mul(input.Position, View) + camPos + camDir;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// ������� �������� �� ���� ���� ����������� ����� �� ������
	// � ���������, �� �� ��� ������� 1
    float3 dir = input.WorldPos - camPos;
	dir = normalize(dir);


	// �������� �����, ����� �����������
	float3 curr = camPos;

	// �������� �� �������
	int s = 0;

	// ��������� ���������� �� ��������
	float dist = 0;

	while( s < MarchSteps)
	{
		// ������� ��������������� ���������� �� ��������
		float next_step = distance_estimate(curr);
		
		//��� ��� ���������� ������, �� �������� �� ��� ��������� �� "����������", ������.
		if(next_step < 0.0001)
		{
			break;
		}

		//�����, �� ������ ������ � ������� ��������� �� ���� � �������� ������.
		curr += next_step * dir;
		dist += next_step;
		s++;
	}

	// ������������ � ������� ������ ������ �� ��������, ����� �� �� �������� �� �������.
	float mult = (MarchSteps - s) / float(MarchSteps);
	if(mult < 0 || dist < 0)
		mult = 0;

	float4 outColor = float4(1,1,1,1);
	outColor.xyz *= mult;

    return outColor;
}

//�������� �� ��������� �� ����������, ��� ����� �� shading �������.
//����������, �� �� ���� ����������� ����� ������.
technique Raymarch
{
    pass Cast
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

//���������� �� ��������� �� 2D �������. �� �� ��������� �� ���� �� 3D.
VertexShaderOutput IterateVS(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

    output.WorldPos = input.Position * Scale + camPos;

    return output;
}

//���������� �� ������� �� 2D �������. ��� ������� ����������.
float4 IteratePS(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.WorldPos;

	// ������������ � ��������� ���-�������� ������������, ����� N ������, � ��� ���� �������� �� � ���������� �� �������.
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

//� �������� � ���� �������, �� �� ���� ����������� �� ���.
technique Iterate
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 IterateVS();
		PixelShader = compile ps_3_0 IteratePS();
	}
}