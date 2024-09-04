using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/DistortRipple")]
	public class DistortRippleEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		//水面扰动
		public TextureParameter m_DistortTex = new TextureParameter(null);
		public Vector2Parameter m_DistortTiling = new Vector2Parameter(Vector2.one);
		[Range(0f, 1f)]
		public FloatParameter m_DistortAmount = new FloatParameter(0.1f); //扰动强度
		public Vector2Parameter m_DistortVelocity = new Vector2Parameter(new Vector2(0.1f, 0.1f)); //扰动速度
		[Range(0f, 1f)]
		public FloatParameter m_GradualLengthX = new FloatParameter(1f); //渐变长度 水平方向
		[Range(-1f, 1f)]
		public FloatParameter m_GradualAnchorX = new FloatParameter(0f); //渐变锚点 水平方向
		[Range(0f, 1f)]
		public FloatParameter m_GradualLengthY = new FloatParameter(1f); //渐变长度 垂直方向
		[Range(-1f, 1f)]
		public FloatParameter m_GradualAnchorY = new FloatParameter(0f); //渐变锚点 垂直方向

		//水波纹
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

	[CustomPostProcess("Able/DistortRipple", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class DistortRippleEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private DistortRippleEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");
			//水面扰动
			internal static readonly int m_DistortTexProper = Shader.PropertyToID("_DistortTex");
			internal static readonly int m_DistortTilingProper = Shader.PropertyToID("_DistortTiling");
			internal static readonly int m_DistortAmountProper = Shader.PropertyToID("_DistortAmount");
			internal static readonly int m_DistortVelocityProper = Shader.PropertyToID("_DistortVelocity");
			internal static readonly int m_GradualLengthXProper = Shader.PropertyToID("_GradualLengthX");
			internal static readonly int m_GradualAnchorXProper = Shader.PropertyToID("_GradualAnchorX");
			internal static readonly int m_GradualLengthYProper = Shader.PropertyToID("_GradualLengthY");
			internal static readonly int m_GradualAnchorYProper = Shader.PropertyToID("_GradualAnchorY");
			//水波纹
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
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/DistortRipple");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<DistortRippleEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				//水面扰动
				m_Material.SetTexture(ShaderIDs.m_DistortTexProper, m_VolumeComponent.m_DistortTex.value);
				m_Material.SetVector(ShaderIDs.m_DistortTilingProper, m_VolumeComponent.m_DistortTiling.value);
				m_Material.SetFloat(ShaderIDs.m_DistortAmountProper, m_VolumeComponent.m_DistortAmount.value);
				m_Material.SetVector(ShaderIDs.m_DistortVelocityProper, m_VolumeComponent.m_DistortVelocity.value);
				m_Material.SetFloat(ShaderIDs.m_GradualLengthXProper, m_VolumeComponent.m_GradualLengthX.value);
				m_Material.SetFloat(ShaderIDs.m_GradualAnchorXProper, m_VolumeComponent.m_GradualAnchorX.value);
				m_Material.SetFloat(ShaderIDs.m_GradualLengthYProper, m_VolumeComponent.m_GradualLengthY.value);
				m_Material.SetFloat(ShaderIDs.m_GradualAnchorYProper, m_VolumeComponent.m_GradualAnchorY.value);
				//水波纹
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
