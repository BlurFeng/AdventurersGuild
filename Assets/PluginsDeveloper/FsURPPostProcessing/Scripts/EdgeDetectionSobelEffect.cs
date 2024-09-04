using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/EdgeDetectionSobel")]
	public class EdgeDetectionSobelEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0.05f, 5.0f)]
		public FloatParameter edgeWidth = new FloatParameter(0.3f);
		[ColorUsage(true, true)]
		public ColorParameter edgeColor = new ColorParameter(new Color(0.0f, 0.0f, 0.0f, 1));
	}

	[CustomPostProcess("Able/EdgeDetectionSobel", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class EdgeDetectionSobelEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private EdgeDetectionSobelEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int Params = Shader.PropertyToID("_Params");
			internal static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/EdgeDetectionSobel");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<EdgeDetectionSobelEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				m_Material.SetVector(ShaderIDs.Params, new Vector2(m_VolumeComponent.edgeWidth.value, 0));
				m_Material.SetColor(ShaderIDs.EdgeColor, m_VolumeComponent.edgeColor.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}

