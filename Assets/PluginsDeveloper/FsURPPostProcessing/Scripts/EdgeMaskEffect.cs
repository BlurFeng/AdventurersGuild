using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/EdgeMask")]
	public class EdgeMaskEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

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

	[CustomPostProcess("Able/EdgeMask", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class EdgeMaskEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private EdgeMaskEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int m_ColorProper = Shader.PropertyToID("_Color");
			internal static readonly int m_DistanceProper = Shader.PropertyToID("_Distance");
			internal static readonly int m_SharpnessProper = Shader.PropertyToID("_Sharpness");
			internal static readonly int m_RadianProper = Shader.PropertyToID("_Radian");
			internal static readonly int m_CenterProper = Shader.PropertyToID("_Center");
			internal static readonly int m_ToggleProper = Shader.PropertyToID("_Toggle");
		}

		private Vector2 m_Toggle;

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/EdgeMask");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<EdgeMaskEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

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

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}


