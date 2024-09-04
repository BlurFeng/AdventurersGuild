using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
    [System.Serializable, VolumeComponentMenu("Able/CyberFault")]
    public class CyberFaultEffect : VolumeComponent
    {
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

        //主纹理偏移
        [Range(0f, 1f), Tooltip("MainTex Offest Intensity")]
        public FloatParameter m_MainTexOffestIntensity = new FloatParameter(0.01f); //偏移强度
        [Tooltip("MainTex Offest Random")]
        public FloatParameter m_MainTexOffestRandom = new FloatParameter(1f); //偏移随机

        //像素块
        [Tooltip("Pixel Size")]
        public FloatParameter m_PixelSize = new FloatParameter(20f); //像素尺寸
        [Tooltip("Pixel Ratio")]
        public Vector2Parameter m_PixelRatio = new Vector2Parameter(Vector2.one); //像素高宽比
        [Tooltip("Pixel Random")]
        public FloatParameter m_PixelRandom = new FloatParameter(1f); //像素块随机值

        //像素块剔除
        [Range(0f, 1f), Tooltip("Cull Amount")]
        public FloatParameter m_CullAmount = new FloatParameter(0.9f); //剔除数
        [Tooltip("Cull Random")]
        public FloatParameter m_CullRandom = new FloatParameter(1f); //剔除随机值

        //色彩分离
        [Range(0f, 1f), Tooltip("ColorSplit MainTexture Amount")]
        public FloatParameter m_ColorSplitMainTexAmount = new FloatParameter(0.01f); //色彩分离 强度 主纹理
        [Range(0f, 1f), Tooltip("ColorSplit Pixel Amount")]
        public FloatParameter m_ColorSplitPixelAmount = new FloatParameter(0.01f); //色彩分离 强度 像素块
        [Range(0f, 1f), Tooltip("ColorSplit Add Random")]
        public FloatParameter m_ColorSplitAddRandom = new FloatParameter(0.01f); //色彩分离 强度 随机

        //动画
        [Tooltip("Anim Enable")]
        public BoolParameter m_AnimEnable = new BoolParameter(false); //动画 开关
        [Tooltip("Anim Speed")]
        public FloatParameter m_AnimSpeed = new FloatParameter(10f); //动画 速度
        [Tooltip("Anim PixelScale Random")]
        public FloatParameter m_AnimPixelScaleRandom = new FloatParameter(0.5f); //像素块尺寸 随机变化
    }

    [CustomPostProcess("Able/CyberFault", CustomPostProcessInjectionPoint.AfterPostProcess)]
    public class CyberFaultEffectRenderer : CustomPostProcessRenderer
    {
        // A variable to hold a reference to the corresponding volume component (you can define as many as you like)
        private CyberFaultEffect m_VolumeComponent;
        // The postprocessing material (you can define as many as you like)
        private Material m_Material;
        // By default, the effect is visible in the scene view, but we can change that here.
        public override bool visibleInSceneView => true;

        // The ids of the shader variables
        static class ShaderIDs
        {
            internal static readonly int Input = Shader.PropertyToID("_MainTex");

            internal static readonly int m_MainTexOffestIntensityProper = Shader.PropertyToID("_MainTexOffestIntensity");
            internal static readonly int m_MainTexOffestRandomProper = Shader.PropertyToID("_MainTexOffestRandom");

            internal static readonly int m_PixelSizeProper = Shader.PropertyToID("_PixelSize");
            internal static readonly int m_PixelRatioProper = Shader.PropertyToID("_PixelRatio");
            internal static readonly int m_PixelRandomProper = Shader.PropertyToID("_PixelRandom");

            internal static readonly int m_CullAmountProper = Shader.PropertyToID("_CullAmount");
            internal static readonly int m_CullRandomProper = Shader.PropertyToID("_CullRandom");

            internal static readonly int m_ColorSplitMainTexAmountProper = Shader.PropertyToID("_ColorSplitMainTexAmount");
            internal static readonly int m_ColorSplitPixelAmountProper = Shader.PropertyToID("_ColorSplitPixelAmount");
            internal static readonly int m_ColorSplitAddRandomProper = Shader.PropertyToID("_ColorSplitAddRandom");

            internal static readonly int m_AnimEnableProper = Shader.PropertyToID("_AnimEnable");
            internal static readonly int m_AnimSpeedProper = Shader.PropertyToID("_AnimSpeed");
            internal static readonly int m_AnimPixelScaleRandomProper = Shader.PropertyToID("_AnimPixelScaleRandom");
        }

        // Initialized is called only once before the first render call
        // so we use it to create our material
        public override void Initialize()
        {
            m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/CyberFault");
        }

        // Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
        public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            // Get the current volume stack
            var stack = VolumeManager.instance.stack;
            // Get the corresponding volume component
            m_VolumeComponent = stack.GetComponent<CyberFaultEffect>();

            return m_VolumeComponent.m_Enable.value;
        }

        // The actual rendering execution is done here
        public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
        {
            if (m_Material != null)
            {
                m_Material.SetFloat(ShaderIDs.m_MainTexOffestIntensityProper, m_VolumeComponent.m_MainTexOffestIntensity.value);
                m_Material.SetFloat(ShaderIDs.m_MainTexOffestRandomProper, m_VolumeComponent.m_MainTexOffestRandom.value);

                m_Material.SetFloat(ShaderIDs.m_PixelSizeProper, m_VolumeComponent.m_PixelSize.value);
                m_Material.SetVector(ShaderIDs.m_PixelRatioProper, m_VolumeComponent.m_PixelRatio.value);
                m_Material.SetFloat(ShaderIDs.m_PixelRandomProper, m_VolumeComponent.m_PixelRandom.value);

                m_Material.SetFloat(ShaderIDs.m_CullAmountProper, m_VolumeComponent.m_CullAmount.value);
                m_Material.SetFloat(ShaderIDs.m_CullRandomProper, m_VolumeComponent.m_CullRandom.value);

                m_Material.SetFloat(ShaderIDs.m_ColorSplitMainTexAmountProper, m_VolumeComponent.m_ColorSplitMainTexAmount.value);
                m_Material.SetFloat(ShaderIDs.m_ColorSplitPixelAmountProper, m_VolumeComponent.m_ColorSplitPixelAmount.value);
                m_Material.SetFloat(ShaderIDs.m_ColorSplitAddRandomProper, m_VolumeComponent.m_ColorSplitAddRandom.value);

                m_Material.SetFloat(ShaderIDs.m_AnimEnableProper, m_VolumeComponent.m_AnimEnable.value ? 1f : 0f);
                m_Material.SetFloat(ShaderIDs.m_AnimSpeedProper, m_VolumeComponent.m_AnimSpeed.value);
                m_Material.SetFloat(ShaderIDs.m_AnimPixelScaleRandomProper, m_VolumeComponent.m_AnimPixelScaleRandom.value);

                // draw a fullscreen triangle to the destination
                cmd.SetGlobalTexture(ShaderIDs.Input, source);
                CoreUtils.DrawFullScreen(cmd, m_Material, destination);
            }
        }
    }
}



