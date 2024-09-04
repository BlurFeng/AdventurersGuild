using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/RadialBlur")]
	public class RadialBlurEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0f, 1f)]
		public FloatParameter BlurRadius = new FloatParameter(0.6f);
		[Range(2, 30)]
		public IntParameter Iteration = new IntParameter(10);
		[Range(0f, 1f)]
		public FloatParameter TransparentAmount = new FloatParameter(1f);
		[Range(0f, 1f)]
		public FloatParameter RadialCenterX = new FloatParameter(0.5f);
		[Range(0f, 1f)]
		public FloatParameter RadialCenterY = new FloatParameter(0.5f);
		[ColorUsage(true, true)]
		public ColorParameter GrayColor = new ColorParameter(new Color(0.0f, 0.0f, 0.0f, 1));
	}

	[CustomPostProcess("Able/RadialBlur", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class RadialBlurEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private RadialBlurEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int Params = Shader.PropertyToID("_Params");
			internal static readonly int GrayColor = Shader.PropertyToID("_GrayColor");
			internal static readonly int TransparentAmount = Shader.PropertyToID("_TransparentAmount");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/RadialBlur");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<RadialBlurEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				m_Material.SetVector(ShaderIDs.Params, new Vector4(m_VolumeComponent.BlurRadius.value * 0.02f, m_VolumeComponent.Iteration.value, m_VolumeComponent.RadialCenterX.value, m_VolumeComponent.RadialCenterY.value));
				m_Material.SetFloat(ShaderIDs.TransparentAmount, m_VolumeComponent.TransparentAmount.value);
				m_Material.SetVector(ShaderIDs.GrayColor, m_VolumeComponent.GrayColor.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}
