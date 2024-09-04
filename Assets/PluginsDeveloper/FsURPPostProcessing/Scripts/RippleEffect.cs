using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/Ripple")]
	public class RippleEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		public Vector4Parameter m_Center1 = new Vector4Parameter(Vector4.zero);
		public Vector4Parameter m_Center2 = new Vector4Parameter(Vector4.zero);
		public Vector4Parameter m_Center3 = new Vector4Parameter(Vector4.zero);
		[Range(0f, 0.5f)]
		public FloatParameter m_Height = new FloatParameter(0.05f);
		[Range(0f, 0.5f)]
		public FloatParameter m_Width = new FloatParameter(0.1f);
		[Range(0f, 1f)]
		public FloatParameter m_Speed = new FloatParameter(0.5f);
		[Range(0f, 10f)]
		public FloatParameter m_HeightAttenuation = new FloatParameter(2f);
		[Range(0f, 10f)]
		public FloatParameter m_WidthAttenuation = new FloatParameter(2f);
	}

	[CustomPostProcess("Able/Ripple", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class RippleEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private RippleEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal readonly static int Input = Shader.PropertyToID("_MainTex");
			internal static readonly int m_Center1Proper = Shader.PropertyToID("_Center1");
			internal static readonly int m_Center2Proper = Shader.PropertyToID("_Center2");
			internal static readonly int m_Center3Proper = Shader.PropertyToID("_Center3");
			internal static readonly int m_HeightProper = Shader.PropertyToID("_Height");
			internal static readonly int m_WidthProper = Shader.PropertyToID("_Width");
			internal static readonly int m_SpeedProper = Shader.PropertyToID("_Speed");
			internal static readonly int m_HeightAttenProper = Shader.PropertyToID("_HeightAtten");
			internal static readonly int m_WidthAttenProper = Shader.PropertyToID("_WidthAtten");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/Ripple");
// #if UNITY_EDITOR
// 			if (!Application.isPlaying)
// 			{
// 				m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/Ripple");
// 			}
// 			else
// 			{
// 				m_Material = CoreUtils.CreateEngineMaterial(ConfigModel.Instance.GetShader("Hidden/Custom/Ripple"));
// 			}
// #else
// 			m_Material = CoreUtils.CreateEngineMaterial(ConfigModel.Instance.GetShader("Hidden/Custom/Ripple"));
// #endif
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<RippleEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);
				m_Material.SetVector(ShaderIDs.m_Center1Proper, m_VolumeComponent.m_Center1.value);
				m_Material.SetVector(ShaderIDs.m_Center2Proper, m_VolumeComponent.m_Center2.value);
				m_Material.SetVector(ShaderIDs.m_Center3Proper, m_VolumeComponent.m_Center3.value);
				m_Material.SetFloat(ShaderIDs.m_HeightProper, m_VolumeComponent.m_Height.value);
				m_Material.SetFloat(ShaderIDs.m_WidthProper, m_VolumeComponent.m_Width.value);
				m_Material.SetFloat(ShaderIDs.m_SpeedProper, m_VolumeComponent.m_Speed.value);
				m_Material.SetFloat(ShaderIDs.m_HeightAttenProper, m_VolumeComponent.m_HeightAttenuation.value);
				m_Material.SetFloat(ShaderIDs.m_WidthAttenProper, m_VolumeComponent.m_WidthAttenuation.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}
