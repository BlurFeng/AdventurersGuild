using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/Distort")]
	public class DistortEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		public TextureParameter m_DistortTex = new TextureParameter(null);
		public Vector2Parameter m_DistortTiling = new Vector2Parameter(Vector2.one);
		[Range(0f, 1f)]
		public FloatParameter m_DistortAmount = new FloatParameter(0.1f); //扰动强度
		public Vector2Parameter m_DistortVelocity = new Vector2Parameter(new Vector2(0.1f, 0.1f)); //扰动速度
		[Range(0f, 1f)]
		public FloatParameter m_GradualLengthX = new FloatParameter(1f); //渐变长度 水平方向
		[Range(-1f, 1f)]
		public FloatParameter m_GradualAnchorX = new FloatParameter(1f); //渐变锚点 水平方向
		[Range(0f, 1f)]
		public FloatParameter m_GradualLengthY = new FloatParameter(1f); //渐变长度 垂直方向
		[Range(-1f, 1f)]
		public FloatParameter m_GradualAnchorY = new FloatParameter(1f); //渐变锚点 垂直方向

		//水面反光
		[ColorUsage(true, true)]
		public ColorParameter m_SpecularColor = new ColorParameter(new Color(0.0f, 0.0f, 0.0f, 1)); //反光颜色
		[Range(0f, 1f)]
		public FloatParameter m_SpecularAmount = new FloatParameter(1f); //反光强度
		[Range(0f, 1f)]
		public FloatParameter m_SpecularScale = new FloatParameter(1f); //反光纹理大小
	}

	[CustomPostProcess("Able/Distort", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class DistortEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private DistortEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int m_DistortTexProper = Shader.PropertyToID("_DistortTex");
			internal static readonly int m_DistortTilingProper = Shader.PropertyToID("_DistortTiling");
			internal static readonly int m_DistortAmountProper = Shader.PropertyToID("_DistortAmount");
			internal static readonly int m_DistortVelocityProper = Shader.PropertyToID("_DistortVelocity");
			internal static readonly int m_GradualLengthXProper = Shader.PropertyToID("_GradualLengthX");
			internal static readonly int m_GradualAnchorXProper = Shader.PropertyToID("_GradualAnchorX");
			internal static readonly int m_GradualLengthYProper = Shader.PropertyToID("_GradualLengthY");
			internal static readonly int m_GradualAnchorYProper = Shader.PropertyToID("_GradualAnchorY");

			internal static readonly int m_SpecularColorProper = Shader.PropertyToID("_SpecularColor");
			internal static readonly int m_SpecularAmountProper = Shader.PropertyToID("_SpecularAmount");
			internal static readonly int m_SpecularScaleProper = Shader.PropertyToID("_SpecularScale");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/Distort");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<DistortEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				m_Material.SetTexture(ShaderIDs.m_DistortTexProper, m_VolumeComponent.m_DistortTex.value);
				m_Material.SetVector(ShaderIDs.m_DistortTilingProper, m_VolumeComponent.m_DistortTiling.value);
				m_Material.SetFloat(ShaderIDs.m_DistortAmountProper, m_VolumeComponent.m_DistortAmount.value);
				m_Material.SetVector(ShaderIDs.m_DistortVelocityProper, m_VolumeComponent.m_DistortVelocity.value);
				m_Material.SetFloat(ShaderIDs.m_GradualLengthXProper, m_VolumeComponent.m_GradualLengthX.value);
				m_Material.SetFloat(ShaderIDs.m_GradualAnchorXProper, m_VolumeComponent.m_GradualAnchorX.value);
				m_Material.SetFloat(ShaderIDs.m_GradualLengthYProper, m_VolumeComponent.m_GradualLengthY.value);
				m_Material.SetFloat(ShaderIDs.m_GradualAnchorYProper, m_VolumeComponent.m_GradualAnchorY.value);
				m_Material.SetColor(ShaderIDs.m_SpecularColorProper, m_VolumeComponent.m_SpecularColor.value);
				m_Material.SetFloat(ShaderIDs.m_SpecularAmountProper, m_VolumeComponent.m_SpecularAmount.value);
				m_Material.SetFloat(ShaderIDs.m_SpecularScaleProper, m_VolumeComponent.m_SpecularScale.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}
