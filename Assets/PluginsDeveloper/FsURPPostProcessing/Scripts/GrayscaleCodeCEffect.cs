using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/Grayscale")]
	public class GrayscaleAbleEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0f, 1f), Tooltip("Grayscale effect intensity.")]
		public FloatParameter blend = new FloatParameter(0.5f);
	}

	[CustomPostProcess("Able/Grayscale", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class GrayscaleEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private GrayscaleAbleEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int BlendProper = Shader.PropertyToID("_Blend");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/ImageGrayscale");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<GrayscaleAbleEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				m_Material.SetFloat(ShaderIDs.BlendProper, m_VolumeComponent.blend.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}


