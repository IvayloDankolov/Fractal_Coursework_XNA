//Матрицата на екрана, същата като в програмата на процесора
float4x4 View;

//Променливите по-долу отново са същите като в програмата, позиции на камерата, мащаби и т.н.
float3 camPos;
float3 camDir;

int Iterations;
float Bailout;
int Power;
int MarchSteps;

float Scale;


// Изчислява приблизителна дистанция до повърхността на фрактала,
// Използващ скаларна 
float distance_estimate(float3 pos) {
	float3 z = pos;
	float dr = 1.0;
	float r = 0.0;
	for (int i = 0; i < Iterations ; i++) {
		
		//Записваме си дължината на вектора. Това е измислената ни "модул" операция.
		r = length(z);
		if (r>Bailout) break;

		// Отиваме в полярни координати
		float theta = acos(z.z/r);
		float phi = atan2(z.y,z.x);

		// Натрупваме производната на итериращата функция
		dr =  pow( r, Power-1.0)*Power*dr + 1.0;

		// Скалираме и въртим точката, според измисленото ни правило за вдигане на вектор на n-та степен
		float zr = pow( r,Power);
		theta = theta*Power;
		phi = phi*Power;

		// Връщаме се в декартови координати, за да довършим итерацията
		z = zr*float3(sin(theta)*cos(phi), sin(phi)*sin(theta), cos(theta));
		z+=pos;
	}

	// Апроксимацията ни за разстояние до фрактала идва от функцията на Грийн.
	// Повече в документацията
	return 0.5*log(r)*r/dr;
}

//Входа на шейдъра по върхове. Нищо особено в нашия случай
struct VertexShaderInput
{
    float4 Position : POSITION0;
};

//Изхода от шейдъра по върхове, трябва ни единствено позицията на екрана и реалната позиция
struct VertexShaderOutput
{
    float4 Position : POSITION0;

	float3 WorldPos : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

	//Приемаме, че екрана е винаги на разстояние 1 от камерата (camDir)
	//Другото е ясно
    output.WorldPos = mul(input.Position, View) + camPos + camDir;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Взимаме посоката на лъча през съответната точка от екрана
	// И нормираме, за да има дължина 1
    float3 dir = input.WorldPos - camPos;
	dir = normalize(dir);


	// Текущата точка, която проверяваме
	float3 curr = camPos;

	// Итерации до момента
	int s = 0;

	// Извървяно разстояние от началото
	float dist = 0;

	while( s < MarchSteps)
	{
		// Взимаме приблизителното разстояние до фрактала
		float next_step = distance_estimate(curr);
		
		//Ако сме достатъчно близко, за харесана от нас дефиниция на "достатъчно", готово.
		if(next_step < 0.0001)
		{
			break;
		}

		//Иначе, се движим наивно с толкова дистанция по лъча и опитваме отново.
		curr += next_step * dir;
		dist += next_step;
		s++;
	}

	// Оцветяването в мометна просто зависи от стъпките, които са ни трябвали да стигнем.
	float mult = (MarchSteps - s) / float(MarchSteps);
	if(mult < 0 || dist < 0)
		mult = 0;

	float4 outColor = float4(1,1,1,1);
	outColor.xyz *= mult;

    return outColor;
}

//Описание на техниката за оцветяване, със двете си shading функции.
//Необходимо, за да знае компилатора какво искаме.
technique Raymarch
{
    pass Cast
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

//Оцветяване по върховете на 2D шейдъра. Не се различава от това на 3D.
VertexShaderOutput IterateVS(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = input.Position;

    output.WorldPos = input.Position * Scale + camPos;

    return output;
}

//Оцветяване по пиксели на 2D шейдъра. Тук смятаме Манделброт.
float4 IteratePS(VertexShaderOutput input) : COLOR0
{
	float2 pos = input.WorldPos;

	// Реализацията е абсолютно най-простата апроксимация, върви N стъпки, и виж дали редицата ще е ограничена до момента.
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

//И описваме и тази техника, за да знае компилатора за нея.
technique Iterate
{
	pass Pass1
	{
		VertexShader = compile vs_3_0 IterateVS();
		PixelShader = compile ps_3_0 IteratePS();
	}
}