using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/ViewportTrans")]
	public class ViewportTransEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

        [Range(0f, 1f), Tooltip("Start Point UV")]
        public Vector2Parameter m_RectStartPoint = new Vector2Parameter(new Vector2(0f, 0f));
        [Range(0f, 1f), Tooltip("Rect Width Height")]
        public Vector2Parameter m_RectWH = new Vector2Parameter(new Vector2(0f, 0f));
    }

	[CustomPostProcess("Able/ViewportTrans", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class ViewportTransEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private ViewportTransEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");
			internal static readonly int m_ViewportRect = Shader.PropertyToID("_ViewportRect");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/ViewportTrans");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<ViewportTransEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				//设置参数
				var uv = m_VolumeComponent.m_RectStartPoint.value;
				var wh = m_VolumeComponent.m_RectWH.value;
				m_Material.SetVector(ShaderIDs.m_ViewportRect, new Vector4(uv.x, uv.y, wh.x, wh.y));

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}



