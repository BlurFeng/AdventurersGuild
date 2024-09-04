using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/Mosaic")]
	public class MosaicEffect : VolumeComponent
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
	}

	[CustomPostProcess("Able/Mosaic", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class MosaicEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private MosaicEffect m_VolumeComponent;
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
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/Mosaic");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<MosaicEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				m_Material.SetFloat(ShaderIDs.m_MainTexOffestIntensityProper, m_VolumeComponent.m_MainTexOffestIntensity.value);
				m_Material.SetFloat(ShaderIDs.m_MainTexOffestRandomProper, m_VolumeComponent.m_MainTexOffestRandom.value);

				m_Material.SetFloat(ShaderIDs.m_PixelSizeProper, m_VolumeComponent.m_PixelSize.value);
				m_Material.SetVector(ShaderIDs.m_PixelRatioProper, m_VolumeComponent.m_PixelRatio.value);
				m_Material.SetFloat(ShaderIDs.m_PixelRandomProper, m_VolumeComponent.m_PixelRandom.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}



