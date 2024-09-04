using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/Zoom")]
	public class ZoomEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);
		// 放大强度
		[Range(-2.0f, 2.0f)]
		public FloatParameter ZoomFactor = new FloatParameter(0.4f);
		// 放大镜大小
		[Range(0.0f, 0.2f)]
		public FloatParameter Size = new FloatParameter(0.15f);
		// 遮罩中心位置
		public Vector2Parameter Pos = new Vector2Parameter(Vector2.one * 0.5f);

	}
	[CustomPostProcess("Able/Zoom", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class ZoomEffectRenderer : CustomPostProcessRenderer
	{
		private ZoomEffect m_VolumeComponent;
		private Material m_Material;
		public override bool visibleInSceneView => true;
		// shader
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");
			internal static readonly int ZoomFactor = Shader.PropertyToID("_ZoomFactor");
			internal static readonly int EdgeFactor = Shader.PropertyToID("_EdgeFactor");
			internal static readonly int Size = Shader.PropertyToID("_Size");
			internal static readonly int Pos = Shader.PropertyToID("_Pos");
		}
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/Zoom");
		}

		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<ZoomEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);
				m_Material.SetFloat(ShaderIDs.ZoomFactor, m_VolumeComponent.ZoomFactor.value);
				m_Material.SetFloat(ShaderIDs.Size, m_VolumeComponent.Size.value);
				m_Material.SetVector(ShaderIDs.Pos, m_VolumeComponent.Pos.value);
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}

