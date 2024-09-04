Shader "C/Unlit-TexOutline"
{
    Properties 
    {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_Scale ("Scale", Range(0, 10)) = 1
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 20)) = 3
	}

	SubShader 
    {
	CGINCLUDE

		#pragma target 2.0

		#include "UnityCG.cginc"
		
		sampler2D _MainTex;
        float4 _MainTex_ST;
		half4 _MainTex_TexelSize;
        fixed4 _TextureSampleAdd;

		fixed4 _Color;
		float _Scale;
        fixed4 _OutlineColor;
        half _OutlineWidth;

		struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

		struct v2f 
        {
			float4 pos   : SV_POSITION;
			float4 color : COLOR;
			half2 uv     : TEXCOORD0;
		};

	ENDCG

		Tags
        {
            "Queue" = "Transparent"
			"IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

		LOD 100
        ZWrite Off

		Pass 
        {
			Blend One Zero

		CGPROGRAM
			  
			#pragma vertex vert
			#pragma fragment frag
		
        	fixed SampleAlpha(int pIndex, v2f i)
        	{
				//uv偏移
            	const fixed sinArray[12] = { 0, 0.5, 0.866, 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5 };
            	const fixed cosArray[12] = { 1, 0.866, 0.5, 0, -0.5, -0.866, -1, -0.866, -0.5, 0, 0.5, 0.866 };
            	float2 pos = i.uv + _MainTex_TexelSize.xy * float2(cosArray[pIndex], sinArray[pIndex]) * _OutlineWidth;
				//缩放
				const float2 cnter = float2(0.5, 0.5);
				pos = cnter + (pos - cnter) * _Scale;

				return (tex2D(_MainTex, pos) + _TextureSampleAdd).w;
        	}

			v2f vert(appdata_t v)
        	{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = float4(1, 1, 1, 1);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
        	{
            	//描边采样
            	fixed4 col = fixed4(_OutlineColor.rgb, 0);
            	col.w += SampleAlpha(0, i);
            	col.w += SampleAlpha(1, i);
            	col.w += SampleAlpha(2, i);
            	col.w += SampleAlpha(3, i);
            	col.w += SampleAlpha(4, i);
            	col.w += SampleAlpha(5, i);
            	col.w += SampleAlpha(6, i);
            	col.w += SampleAlpha(7, i);
            	col.w += SampleAlpha(8, i);
            	col.w += SampleAlpha(9, i);
            	col.w += SampleAlpha(10, i);
            	col.w += SampleAlpha(11, i);
            	col.w = clamp(col.w, 0, 1) * _OutlineColor.w;

				return col;
			}

		ENDCG
		}

		Pass 
        {
			Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
			  
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata_t v)
        	{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
            {
		    	//纹理采样
				const float2 cnter = float2(0.5, 0.5);
				i.uv = cnter + (i.uv - cnter) * _Scale;
                half4 color = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;

                return color;
            }

		ENDCG
		}
	}

	FallBack "Diffuse"
}