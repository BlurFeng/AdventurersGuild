Shader "Able/Unlit/FrameAnim"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {} //纹理
        _SheetCount ("Sheet Count", Float) = 8 //图集切片数
        _RowCount ("Row Count", Float) = 1 //图集行数
        _SecondFrame ("Second Frame", Float) = 10 //每秒帧数
        _TimeStart ("Time Start", Float) = 0 //开始时间 秒
    }

    SubShader 
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
        }
        
        LOD 100

        Pass 
        {
        CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t 
            {                   
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _SheetCount; //图集切片数
            float _RowCount; //图集行数
            float _SecondFrame; //每秒帧数
            float _TimeStart; //开始时间 秒

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //总帧数 取整数
                float frameTotal = floor((_Time.y - _TimeStart) * _SecondFrame);
                float frameCur = fmod(frameTotal, _SheetCount); //当前帧数

                float2 uv = i.texcoord;
                float rowSheetCount = _SheetCount / _RowCount; //单行帧数
                //uv 横向缩放
                uv.x /= rowSheetCount;
                //uv 横向偏移
                uv.x += fmod(frameCur, rowSheetCount) / rowSheetCount;
                //uv 纵向缩放
                uv.y /= _RowCount;
                //uv 纵向偏移
                uv.y += floor(frameCur / rowSheetCount) * (1 / _RowCount);
                fixed4 color = tex2D(_MainTex, uv);
                
                clip(color.a - 0.004);

                return color;
            }

            ENDCG
        }
    }
}
