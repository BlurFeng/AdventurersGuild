Shader "Hidden/Custom/EdgeMaskBlur"
{
HLSLINCLUDE
    // This file contains the "Luminance" which we use to get the grayscale value
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
	#include "Assets/PluginsDeveloper/FsURPPostProcessing/ShaderLibrary/Core.hlsl"

	TEXTURE2D_X(_MainTex);
	float4 _MainTex_TexelSize;

	//Blur
	half4 _BlurOffset;
	//EdgeMask
	half4 _Color;
    half _Distance;
	half _Sharpness;
    half _Radian;
	half2 _Center;
    half2 _Toggle;

	struct v2f
	{
		float4 pos: POSITION;
		float2 uv: TEXCOORD0;
		float4 uv01: TEXCOORD1;
		float4 uv23: TEXCOORD2;
		float4 uv45: TEXCOORD3;
	};

	v2f VertGaussianBlur(FullScreenTrianglePostProcessAttributes v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
    	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.pos = GetFullScreenTriangleVertexPosition(v.vertexID);
		o.pos = float4(o.pos.xy, 0, 1);
		o.uv = GetFullScreenTriangleTexCoord(v.vertexID);

//Y轴翻转
//#if UNITY_UV_STARTS_AT_TOP
		//o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
//#endif

		//o.uv = TransformStereoScreenSpaceTex(o.uv, 1.0);

		o.uv01 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1);
		o.uv23 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1) * 2.0;
		o.uv45 = o.uv.xyxy + _BlurOffset.xyxy * float4(1, 1, -1, -1) * 6.0;

		return o;
	}

	float4 FragGaussianBlur(v2f i) : SV_Target
	{
		half4 color = float4(0, 0, 0, 0);

		color += 0.40 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv);
		color += 0.15 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv01.xy);
		color += 0.15 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv01.zw);
		color += 0.10 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv23.xy);
		color += 0.10 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv23.zw);
		color += 0.05 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv45.xy);
		color += 0.05 * SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.uv45.zw);

		return color;
	}

	float4 FragCombine(PostProcessVaryings i) : SV_Target
	{
		float4 srcColor = SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, i.texcoord);
		
		//边缘遮罩
        //计算距离中心点长度
		half disY = abs(i.texcoord.y - _Center.y);
        half disX = abs(i.texcoord.x - _Center.x);
        half disMask = max(0.0001, _Distance);
        //Y轴方向
        half percentY = (disY + disX * sin(disX) * _Radian) / disMask * _Toggle.y;
        //X轴方向
        half percentX = (disX + disY * sin(disY) * _Radian) / disMask * _Toggle.x;
        //插值混合颜色
        half indensity = smoothstep((_Distance - _Sharpness) / disMask * 0.999, 1, max(percentX, percentY));
		half3 finalColor = lerp(srcColor.rgb, _Color.rgb, indensity * _Color.a);
		
		return float4(finalColor.rgb, 1);
	}
ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertGaussianBlur
			#pragma fragment FragGaussianBlur

			ENDHLSL
		}

		Pass
		{
			HLSLPROGRAM

			#pragma vertex FullScreenTrianglePostProcessVertexProgram
			#pragma fragment FragCombine

			ENDHLSL
		}
	}
}