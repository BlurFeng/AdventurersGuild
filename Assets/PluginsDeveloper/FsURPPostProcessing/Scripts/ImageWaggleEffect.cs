using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/ImageWaggle")]
	public class ImageWaggleEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0f, 0.5f), Tooltip("Horizontal Offset Range")]
		public FloatParameter m_HorOffsetRange = new FloatParameter(0.08f);
		[Range(0f, 80f), Tooltip("Horizontal Offset Speed")]
		public FloatParameter m_HorOffsetSpeed = new FloatParameter(7f);
		[Range(0f, 0.5f), Tooltip("Vertical Offset Range")]
		public FloatParameter m_VerOffsetRange = new FloatParameter(0.02f);
		[Range(0f, 80f), Tooltip("Vertical Offset Speed")]
		public FloatParameter m_VerOffsetSpeed = new FloatParameter(14f);

		public Vector2Parameter m_ScaCenter = new Vector2Parameter(new Vector2(0.5f, 0.5f));
		[Tooltip("Scaling Start Time")]
		public FloatParameter m_StartTime = new FloatParameter(0f);
		[Range(0f, 1f), Tooltip("Scaling Visible Start Scale")]
		public FloatParameter m_ScaStartScale = new FloatParameter(0.8f);
		[Range(0f, 1f), Tooltip("Scaling Visible End Scale")]
		public FloatParameter m_ScaEndScale = new FloatParameter(0.4f);
		[Range(0f, 30f), Tooltip("Scaling Total Time")]
		public FloatParameter m_ScaTotalTime = new FloatParameter(5f);
	}

	[CustomPostProcess("Able/ImageWaggle", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class ImageWaggleEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private ImageWaggleEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int m_HorOffsetRangeProper = Shader.PropertyToID("_HorOffsetRange");
			internal static readonly int m_HorOffsetSpeedProper = Shader.PropertyToID("_HorOffsetSpeed");
			internal static readonly int m_VerOffsetRangeProper = Shader.PropertyToID("_VerOffsetRange");
			internal static readonly int m_VerOffsetSpeedProper = Shader.PropertyToID("_VerOffsetSpeed");

			internal static readonly int m_StartTimeProper = Shader.PropertyToID("_StartTime");
			internal static readonly int m_ScaCenterProper = Shader.PropertyToID("_ScaCenter");
			internal static readonly int m_ScaStartScaleProper = Shader.PropertyToID("_ScaStartScale");
			internal static readonly int m_ScaEndScaleProper = Shader.PropertyToID("_ScaEndScale");
			internal static readonly int m_ScaTotalTimeProper = Shader.PropertyToID("_ScaTotalTime");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/ImageWaggle");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<ImageWaggleEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				//偏移参数
				m_Material.SetFloat(ShaderIDs.m_HorOffsetRangeProper, m_VolumeComponent.m_HorOffsetRange.value);
				m_Material.SetFloat(ShaderIDs.m_HorOffsetSpeedProper, m_VolumeComponent.m_HorOffsetSpeed.value);
				m_Material.SetFloat(ShaderIDs.m_VerOffsetRangeProper, m_VolumeComponent.m_VerOffsetRange.value);
				m_Material.SetFloat(ShaderIDs.m_VerOffsetSpeedProper, m_VolumeComponent.m_VerOffsetSpeed.value);
				//缩放参数
				m_Material.SetFloat(ShaderIDs.m_StartTimeProper, m_VolumeComponent.m_StartTime.value);
				m_Material.SetVector(ShaderIDs.m_ScaCenterProper, m_VolumeComponent.m_ScaCenter.value);
				m_Material.SetFloat(ShaderIDs.m_ScaStartScaleProper, m_VolumeComponent.m_ScaStartScale.value);
				m_Material.SetFloat(ShaderIDs.m_ScaEndScaleProper, m_VolumeComponent.m_ScaEndScale.value);
				m_Material.SetFloat(ShaderIDs.m_ScaTotalTimeProper, m_VolumeComponent.m_ScaTotalTime.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}



