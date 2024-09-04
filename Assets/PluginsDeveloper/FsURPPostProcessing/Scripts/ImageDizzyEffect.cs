using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/ImageDizzy")]
	public class ImageDizzyEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0f, 1f), Tooltip("Dizzy effect Range.")]
		public FloatParameter range = new FloatParameter(0.5f);
		[Range(0f, 2f), Tooltip("Dizzy effect Speed.")]
		public FloatParameter speed = new FloatParameter(0.5f);
		[Tooltip("完整时间")]
		public FloatParameter time = new FloatParameter(1.5f);
		[Range(0f, 0.5f), Tooltip("淡入时间占比")]
		public FloatParameter fadeIn = new FloatParameter(0.2f);
		[Range(0.5f, 1f), Tooltip("淡出时间占比")]
		public FloatParameter fadeOut = new FloatParameter(0.8f);
		[Tooltip("是否循环")]
		public BoolParameter m_IsLoop = new BoolParameter(false);

		[Tooltip("开始时间")]
		public FloatParameter m_StartTime = new FloatParameter(0f);
	}

	[CustomPostProcess("Able/ImageDizzy", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class ImageDizzyEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private ImageDizzyEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int RangeProper = Shader.PropertyToID("_Range");
			internal static readonly int SpeedProper = Shader.PropertyToID("_Speed");

			internal static readonly int m_StartTimeProper = Shader.PropertyToID("_StartTime");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/ImageDizzy");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<ImageDizzyEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				cmd.SetGlobalTexture(ShaderIDs.Input, source);

				float passTime = Time.realtimeSinceStartup - m_VolumeComponent.m_StartTime.value;
				float strenth = 1;
				if (passTime > m_VolumeComponent.time.value)
				{
					if (m_VolumeComponent.m_IsLoop.value)
					{
						m_VolumeComponent.m_StartTime.value = Time.realtimeSinceStartup;
					}
					else
					{
						strenth = 0;
					}
				}
				else
				{

					float progress = passTime / m_VolumeComponent.time.value;

					if (progress < m_VolumeComponent.fadeOut.value)
					{
						if (progress < m_VolumeComponent.fadeIn.value)
						{
							strenth = progress / m_VolumeComponent.fadeIn.value;
						}
						else
						{
							strenth = 1;
						}
					}
					else
					{
						strenth = 1.0f - (progress - m_VolumeComponent.fadeOut.value) / (1.0f - m_VolumeComponent.fadeOut.value);
					}
				}

				//Debug.LogError("strenth " + strenth);
				m_Material.SetFloat(ShaderIDs.RangeProper, m_VolumeComponent.range.value * strenth);
				m_Material.SetFloat(ShaderIDs.SpeedProper, m_VolumeComponent.speed.value);

				// draw a fullscreen triangle to the destination
				CoreUtils.DrawFullScreen(cmd, m_Material, destination);
			}
		}
	}
}
