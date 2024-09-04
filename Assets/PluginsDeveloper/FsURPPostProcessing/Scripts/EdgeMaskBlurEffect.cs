using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/EdgeMaskBlur")]
	public class EdgeMaskBlurEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Header("Blur")]
		[Range(0f, 50f)]
		public FloatParameter BlurRadius = new FloatParameter(3f);
		[Range(1, 15)]
		public IntParameter Iteration = new IntParameter(6);
		[Range(1, 8)]
		public FloatParameter RTDownScaling = new FloatParameter(2f);

		[Header("EdgeMask")]
		[ColorUsage(true, true)]
		public ColorParameter m_Color = new ColorParameter(new Color(0.0f, 0.0f, 0.0f, 1));
		[Range(0.0f, 1.0f)]
		public FloatParameter m_Distance = new FloatParameter(0.4f);
		[Range(-1f, 1f)]
		public FloatParameter m_Sharpness = new FloatParameter(0.1f);
		[Range(-10f, 10f)]
		public FloatParameter m_Radian = new FloatParameter(0.2f);
		public Vector2Parameter m_Center = new Vector2Parameter(new Vector2(0.5f, 0.5f));
		public BoolParameter m_ToggleVertical = new BoolParameter(true);
		public BoolParameter m_ToggleHorizontal = new BoolParameter(true);
	}

	[CustomPostProcess("Able/EdgeMaskBlur", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class EdgeMaskBlurEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private EdgeMaskBlurEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			//高斯模糊
			internal static readonly int BlurRadius = Shader.PropertyToID("_BlurOffset");
			internal static readonly int BufferRT1 = Shader.PropertyToID("_BufferRT1");
			internal static readonly int BufferRT2 = Shader.PropertyToID("_BufferRT2");
			//边缘遮罩
			internal static readonly int m_ColorProper = Shader.PropertyToID("_Color");
			internal static readonly int m_DistanceProper = Shader.PropertyToID("_Distance");
			internal static readonly int m_SharpnessProper = Shader.PropertyToID("_Sharpness");
			internal static readonly int m_RadianProper = Shader.PropertyToID("_Radian");
			internal static readonly int m_CenterProper = Shader.PropertyToID("_Center");
			internal static readonly int m_ToggleProper = Shader.PropertyToID("_Toggle");
		}

		private  Vector2 m_Toggle;

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/EdgeMaskBlur");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<EdgeMaskBlurEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				//边缘遮罩
				m_Material.SetColor(ShaderIDs.m_ColorProper, m_VolumeComponent.m_Color.value);
				m_Material.SetFloat(ShaderIDs.m_DistanceProper, m_VolumeComponent.m_Distance.value);
				m_Material.SetFloat(ShaderIDs.m_SharpnessProper, m_VolumeComponent.m_Sharpness.value);
				m_Material.SetFloat(ShaderIDs.m_RadianProper, m_VolumeComponent.m_Radian.value);
				m_Material.SetVector(ShaderIDs.m_CenterProper, m_VolumeComponent.m_Center.value);
				if (m_VolumeComponent.m_ToggleVertical.value)
				{
					m_Toggle.y = 1;
				}
				else
				{
					m_Toggle.y = 0;
				}
				if (m_VolumeComponent.m_ToggleHorizontal.value)
				{
					m_Toggle.x = 1;
				}
				else
				{
					m_Toggle.x = 0;
				}
				m_Material.SetVector(ShaderIDs.m_ToggleProper, m_Toggle);

				//高斯模糊
				int RTWidth = (int)(Screen.width / m_VolumeComponent.RTDownScaling.value);
				int RTHeight = (int)(Screen.height / m_VolumeComponent.RTDownScaling.value);
				cmd.GetTemporaryRT(ShaderIDs.BufferRT1, RTWidth, RTHeight, 0, FilterMode.Bilinear);
				cmd.GetTemporaryRT(ShaderIDs.BufferRT2, RTWidth, RTHeight, 0, FilterMode.Bilinear);

				// downsample screen copy into smaller RT
				cmd.Blit(source, ShaderIDs.BufferRT1);

				for (int i = 0; i < m_VolumeComponent.Iteration.value; i++)
				{
					// horizontal blur
					m_Material.SetVector(ShaderIDs.BlurRadius, new Vector4(m_VolumeComponent.BlurRadius.value / Screen.width, 0, 0, 0));
					cmd.SetGlobalTexture(ShaderIDs.Input, ShaderIDs.BufferRT1);
					cmd.Blit(ShaderIDs.BufferRT1, ShaderIDs.BufferRT2, m_Material, 0);

					// vertical blur
					m_Material.SetVector(ShaderIDs.BlurRadius, new Vector4(0, m_VolumeComponent.BlurRadius.value / Screen.height, 0, 0));
					cmd.SetGlobalTexture(ShaderIDs.Input, ShaderIDs.BufferRT2);
					cmd.Blit(ShaderIDs.BufferRT2, ShaderIDs.BufferRT1, m_Material, 0);
				}

				// Render blurred texture in blend pass
				cmd.SetGlobalTexture(ShaderIDs.Input, ShaderIDs.BufferRT1);
				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination, null, 1);

				// release
				cmd.ReleaseTemporaryRT(ShaderIDs.BufferRT1);
				cmd.ReleaseTemporaryRT(ShaderIDs.BufferRT2);
			}
		}
	}
}


