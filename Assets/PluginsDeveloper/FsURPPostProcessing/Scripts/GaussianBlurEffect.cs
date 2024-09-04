using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FsURPPostProcessing;

namespace FsPostProcessSystem
{
	[System.Serializable, VolumeComponentMenu("Able/GaussianBlur")]
	public class GaussianBlurEffect : VolumeComponent
	{
        //开启状态 VolumeComponent.active数据不准确
        public BoolParameter m_Enable = new BoolParameter(false);

		[Range(0f, 50f)]
		public FloatParameter BlurRadius = new FloatParameter(3f);
		[Range(1, 15)]
		public IntParameter Iteration = new IntParameter(6);
	}

	[CustomPostProcess("Able/GaussianBlur", CustomPostProcessInjectionPoint.AfterPostProcess)]
	public class GaussianBlurEffectRenderer : CustomPostProcessRenderer
	{
		// A variable to hold a reference to the corresponding volume component (you can define as many as you like)
		private GaussianBlurEffect m_VolumeComponent;
		// The postprocessing material (you can define as many as you like)
		private Material m_Material;
		// By default, the effect is visible in the scene view, but we can change that here.
		public override bool visibleInSceneView => true;

		// The ids of the shader variables
		static class ShaderIDs
		{
			internal static readonly int Input = Shader.PropertyToID("_MainTex");

			internal static readonly int BlurRadius = Shader.PropertyToID("_BlurOffset");
			internal static readonly int BufferRT1 = Shader.PropertyToID("_BufferRT1");
			internal static readonly int BufferRT2 = Shader.PropertyToID("_BufferRT2");
		}

		// Initialized is called only once before the first render call
		// so we use it to create our material
		public override void Initialize()
		{
			m_Material = CoreUtils.CreateEngineMaterial("Hidden/Custom/GaussianBlur");
		}

		// Called for each camera/injection point pair on each frame. Return true if the effect should be rendered for this camera.
		public override bool Setup(ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			// Get the current volume stack
			var stack = VolumeManager.instance.stack;
			// Get the corresponding volume component
			m_VolumeComponent = stack.GetComponent<GaussianBlurEffect>();

			return m_VolumeComponent.m_Enable.value;
		}

		// The actual rendering execution is done here
		public override void Render(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, ref RenderingData renderingData, CustomPostProcessInjectionPoint injectionPoint)
		{
			if (m_Material != null)
			{
				int RTWidth = (int)(Screen.width / 4f);
				int RTHeight = (int)(Screen.height / 4f);
				cmd.GetTemporaryRT(ShaderIDs.BufferRT1, RTWidth, RTHeight, 0, FilterMode.Bilinear);
				cmd.GetTemporaryRT(ShaderIDs.BufferRT2, RTWidth, RTHeight, 0, FilterMode.Bilinear);

				// downsample screen copy into smaller RT
				cmd.Blit(source, ShaderIDs.BufferRT1);

                for (int i = 0; i < m_VolumeComponent.Iteration.value; i++)
				{
					// horizontal blur
					cmd.SetGlobalVector(ShaderIDs.BlurRadius, new Vector4(m_VolumeComponent.BlurRadius.value / Screen.width, 0, 0, 0));
					cmd.SetGlobalTexture(ShaderIDs.Input, ShaderIDs.BufferRT1);
					CoreUtils.DrawFullScreen(cmd, m_Material, ShaderIDs.BufferRT2);

                    // vertical blur
                    cmd.SetGlobalVector(ShaderIDs.BlurRadius, new Vector4(0, m_VolumeComponent.BlurRadius.value / Screen.height, 0, 0));
                    cmd.SetGlobalTexture(ShaderIDs.Input, ShaderIDs.BufferRT2);
					CoreUtils.DrawFullScreen(cmd, m_Material, ShaderIDs.BufferRT1);
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


