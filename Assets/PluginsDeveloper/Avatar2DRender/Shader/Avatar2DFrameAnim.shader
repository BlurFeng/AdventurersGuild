Shader "Avatar2DRender/Unlit/Avatar2DFrameAnim"
{
    Properties
    {
        _SheetCount ("Sheet Count", Float) = 8 //图集切片数
        _SecondFrame ("Second Frame", Range(1, 100)) = 10 //每秒帧数
        _TimeStart ("Time Start", Float) = 0 //开始时间 秒

        _BodyHeadTex ("BodyHead Tex", 2D) = "white" {} //身体头部 纹理图集
        _BodyHeadUVAnchors ("BodyHead UV Anchors", Vector) = (0, 0, 1, 1) //uv渲染范围
        _BodyTrunkTex ("BodyTrunk Tex", 2D) = "white" {} //身体躯干 纹理图集
        _BodyTrunkUVAnchors ("BodyTrunk UV Anchors", Vector) = (0, 0, 1, 1)
        _BodyEyeTex ("BodyEye Tex", 2D) = "white" {} //身体眼睛 纹理图集
        _BodyEyeUVAnchors ("BodyEye UV Anchors", Vector) = (0, 0, 1, 1)
        _BodyBrowTex ("BodyBrow Tex", 2D) = "white" {} //身体眉毛 纹理图集
        _BodyBrowUVAnchors ("BodyBrow UV Anchors", Vector) = (0, 0, 1, 1)
        _BodyHairFrontTex ("BodyHairFront Tex", 2D) = "white" {} //身体头发前 纹理图集
        _BodyHairFrontUVAnchors ("BodyHairFront UV Anchors", Vector) = (0, 0, 1, 1)
        _BodyHairBackTex ("BodyHairBack Tex", 2D) = "white" {} //身体头发后 纹理图集
        _BodyHairBackUVAnchors ("BodyHairBack UV Anchors", Vector) = (0, 0, 1, 1)
        _BodyOtherTex ("BodyOther Tex", 2D) = "white" {} //身体其他 纹理图集
        _BodyOtherUVAnchors ("BodyOther UV Anchors", Vector) = (0, 0, 1, 1)

        _EquipHelmetTex ("EquipHelmet Tex", 2D) = "white" {} //装备头部 纹理图集
        _EquipHelmetUVAnchors ("EquipHelmet UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipArmourTex ("EquipArmour Tex", 2D) = "white" {} //装备胸部 纹理图集
        _EquipArmourUVAnchors ("EquipArmour UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipGlovesTex ("EquipGloves Tex", 2D) = "white" {} //装备手部 纹理图集
        _EquipGlovesUVAnchors ("EquipGloves UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipLeggingsTex ("EquipLeggings Tex", 2D) = "white" {} //装备腿部 纹理图集
        _EquipLeggingsUVAnchors ("EquipLeggings UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipBootsTex ("EquipBoots Tex", 2D) = "white" {} //装备脚部 纹理图集
        _EquipBootsUVAnchors ("EquipBoots UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipMainhandTex ("EquipMainhand Tex", 2D) = "white" {} //装备主手 纹理图集
        _EquipMainhandUVAnchors ("EquipMainhand UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipOffhandTex ("EquipOffhand Tex", 2D) = "white" {} //装备副手 纹理图集
        _EquipOffhandUVAnchors ("EquipOffhand UV Anchors", Vector) = (0, 0, 1, 1)
        _EquipOtherTex ("EquipOther Tex", 2D) = "white" {} //装备其他 纹理图集
        _EquipOtherUVAnchors ("EquipOther UV Anchors", Vector) = (0, 0, 1, 1)
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

            float _SheetCount; //图集切片数
            float _SecondFrame; //每秒帧数
            float _TimeStart; //开始时间 秒

            sampler2D _BodyHeadTex; //身体头部 纹理图集
            float4 _BodyHeadTex_ST;
            float4 _BodyHeadUVAnchors; //身体头部 uv渲染范围
            sampler2D _BodyTrunkTex; //身体躯干 纹理图集
            float4 _BodyTrunkTex_ST;
            float4 _BodyTrunkUVAnchors;
            sampler2D _BodyEyeTex; //身体眼睛 纹理图集
            float4 _BodyEyeTex_ST;
            float4 _BodyEyeUVAnchors;
            sampler2D _BodyBrowTex; //身体眉毛 纹理图集
            float4 _BodyBrowTex_ST;
            float4 _BodyBrowUVAnchors;
            sampler2D _BodyHairFrontTex; //身体头发前 纹理图集
            float4 _BodyHairFrontTex_ST;
            float4 _BodyHairFrontUVAnchors;
            sampler2D _BodyHairBackTex; //身体头发后 纹理图集
            float4 _BodyHairBackTex_ST;
            float4 _BodyHairBackUVAnchors;
            sampler2D _BodyOtherTex; //身体其他 纹理图集
            float4 _BodyOtherTex_ST;
            float4 _BodyOtherUVAnchors;

            sampler2D _EquipHelmetTex; //装备头部 纹理图集
            float4 _EquipHelmetTex_ST;
            float4 _EquipHelmetUVAnchors;
            sampler2D _EquipArmourTex; //装备胸部 纹理图集
            float4 _EquipArmourTex_ST;
            float4 _EquipArmourUVAnchors;
            sampler2D _EquipGlovesTex; //装备手部 纹理图集
            float4 _EquipGlovesTex_ST;
            float4 _EquipGlovesUVAnchors;
            sampler2D _EquipLeggingsTex; //装备腿部 纹理图集
            float4 _EquipLeggingsTex_ST;
            float4 _EquipLeggingsUVAnchors;
            sampler2D _EquipBootsTex; //装备脚部 纹理图集
            float4 _EquipBootsTex_ST;
            float4 _EquipBootsUVAnchors;
            sampler2D _EquipMainhandTex; //装备主手 纹理图集
            float4 _EquipMainhandTex_ST;
            float4 _EquipMainhandUVAnchors;
            sampler2D _EquipOffhandTex; //装备副手 纹理图集
            float4 _EquipOffhandTex_ST;
            float4 _EquipOffhandUVAnchors;
            sampler2D _EquipOtherTex; //装备其他 纹理图集
            float4 _EquipOtherTex_ST;
            float4 _EquipOtherUVAnchors;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _BodyHeadTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = fixed4(0, 0, 0, 0);
                float2 uv = i.texcoord;
                //总帧数 取整数
                float frameTotal = floor((_Time.y - _TimeStart) * _SecondFrame);
                //当前帧数
                float frameCur = fmod(frameTotal, _SheetCount);
                
                //纹理采样 身体头发后
                float2 uvMin = _BodyHairBackUVAnchors.xy;
                float2 uvMax = _BodyHairBackUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyHairBackTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体头部
                uvMin = _BodyHeadUVAnchors.xy;
                uvMax = _BodyHeadUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyHeadTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体躯干
                uvMin = _BodyTrunkUVAnchors.xy;
                uvMax = _BodyTrunkUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyTrunkTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体眼睛
                uvMin = _BodyEyeUVAnchors.xy;
                uvMax = _BodyEyeUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyEyeTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体眉毛
                uvMin = _BodyBrowUVAnchors.xy;
                uvMax = _BodyBrowUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyBrowTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体头发前
                uvMin = _BodyHairFrontUVAnchors.xy;
                uvMax = _BodyHairFrontUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyHairFrontTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 身体其他
                uvMin = _BodyOtherUVAnchors.xy;
                uvMax = _BodyOtherUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_BodyOtherTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }

                //纹理采样 装备头部
                uvMin = _EquipHelmetUVAnchors.xy;
                uvMax = _EquipHelmetUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipHelmetTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备胸部
                uvMin = _EquipArmourUVAnchors.xy;
                uvMax = _EquipArmourUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipArmourTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备手部
                uvMin = _EquipGlovesUVAnchors.xy;
                uvMax = _EquipGlovesUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipGlovesTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备腿部
                uvMin = _EquipLeggingsUVAnchors.xy;
                uvMax = _EquipLeggingsUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipLeggingsTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备脚部
                uvMin = _EquipBootsUVAnchors.xy;
                uvMax = _EquipBootsUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipBootsTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备主手
                uvMin = _EquipMainhandUVAnchors.xy;
                uvMax = _EquipMainhandUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipMainhandTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备副手
                uvMin = _EquipOffhandUVAnchors.xy;
                uvMax = _EquipOffhandUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipOffhandTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }
                //纹理采样 装备其他
                uvMin = _EquipOtherUVAnchors.xy;
                uvMax = _EquipOtherUVAnchors.zw;
                if(uv.x - uvMin.x > 0 && uv.y - uvMin.y > 0 && uvMax.x - uv.x > 0 && uvMax.y - uv.y > 0)
                {
                    //uv 坐标缩放
                    float2 uvScale = uv;
                    uvScale.x = (uvScale.x - uvMin.x) / (uvMax.x - uvMin.x);
                    uvScale.y = (uvScale.y - uvMin.y) / (uvMax.y - uvMin.y);
                    //uv 横向缩放
                    uvScale.x /= _SheetCount;
                    //uv 横向偏移
                    uvScale.x += frameCur / _SheetCount;
                    fixed4 col = tex2D(_EquipOtherTex, uvScale);
                    if(col.a > 0.004)
                    {
                        color = col;
                    }
                }

                clip(color.a - 0.004);

                return color;
            }

            ENDCG
        }
    }
}
