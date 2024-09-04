using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FsURPPostProcessing
{
    /// <summary>
    /// 自定义后处理
    /// <list type="bullet">
    /// <item>
    /// <description> 桥接URP的PostProcessing，使用相同的方式在Volume中进行添加与设置 </description>
    /// </item>
    /// </list>
    /// </summary>
    [Serializable]
    public class CustomPostProcess : ScriptableRendererFeature
    {
        /// <summary>
        /// The settings for the custom post processing render feature.
        /// </summary>
        [Serializable]
        public class CustomPostProcessSettings 
        {
            /// <summary>
            /// Three list (one for each injection point) that holds the custom post processing renderers in order of execution during the render pass
            /// </summary>
            [SerializeField]
            public List<string> renderersAfterOpaqueAndSky, renderersBeforePostProcess, renderersAfterPostProcess;

            public CustomPostProcessSettings()
            {
                renderersAfterOpaqueAndSky = new List<string>();
                renderersBeforePostProcess = new List<string>();
                renderersAfterPostProcess = new List<string>();
            }
        }

        /// <summary>
        /// The settings of the render feature
        /// </summary>
        [SerializeField] public CustomPostProcessSettings settings = new CustomPostProcessSettings();

        /// <summary>
        /// The render passes at each inject point
        /// </summary>
        private CustomPostProcessRenderPass m_AfterOpaqueAndSky, m_BeforePostProcess, m_AfterPostProcess;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Create()
        {
            //创建 自定义后处理 渲染通道
            Dictionary<string, CustomPostProcessRenderer> shared = new Dictionary<string, CustomPostProcessRenderer>();
            m_AfterOpaqueAndSky = new CustomPostProcessRenderPass(CustomPostProcessInjectionPoint.AfterOpaqueAndSky, InstantiateRenderers(settings.renderersAfterOpaqueAndSky, shared));
            m_BeforePostProcess = new CustomPostProcessRenderPass(CustomPostProcessInjectionPoint.BeforePostProcess, InstantiateRenderers(settings.renderersBeforePostProcess, shared));
            m_AfterPostProcess = new CustomPostProcessRenderPass(CustomPostProcessInjectionPoint.AfterPostProcess, InstantiateRenderers(settings.renderersAfterPostProcess, shared));
        }
        
        /// <summary>
        /// Converts the class name (AssemblyQualifiedName) to an instance. Filters out types that don't exist or don't match the requirements.
        /// </summary>
        /// <param name="names">The list of assembly-qualified class names</param>
        /// <param name="shared">Dictionary of shared instances keyed by class name</param>
        /// <returns>List of renderers</returns>
        private List<CustomPostProcessRenderer> InstantiateRenderers(List<String> names, Dictionary<string, CustomPostProcessRenderer> shared)
        {
            var renderers = new List<CustomPostProcessRenderer>(names.Count);
            foreach (var name in names)
            {
                if(shared.TryGetValue(name, out var renderer))
                    renderers.Add(renderer);
                else
                {
                    var type = Type.GetType(name);
                    if (type == null || !type.IsSubclassOf(typeof(CustomPostProcessRenderer))) continue;
                    var attribute = CustomPostProcessAttribute.GetAttribute(type);
                    if(attribute == null) continue;

                    renderer = Activator.CreateInstance(type) as CustomPostProcessRenderer;
                    renderers.Add(renderer);
                    
                    if(attribute.ShareInstance)
                        shared.Add(name, renderer);
                }
            }

            return renderers;
        }

        /// <summary>
        /// 插入 自定义后处理渲染通道
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="renderingData"></param>
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //摄像机未开启后处理
            if (renderingData.cameraData.postProcessEnabled == false) return;

            //检查后处理是否开启 并插入渲染通道
            if (m_AfterOpaqueAndSky.HasPostProcessRenderers && m_AfterOpaqueAndSky.PrepareRenderers(ref renderingData))
                renderer.EnqueuePass(m_AfterOpaqueAndSky);

            if (m_BeforePostProcess.HasPostProcessRenderers && m_BeforePostProcess.PrepareRenderers(ref renderingData))
                renderer.EnqueuePass(m_BeforePostProcess);

            if (m_AfterPostProcess.HasPostProcessRenderers && m_AfterPostProcess.PrepareRenderers(ref renderingData))
                renderer.EnqueuePass(m_AfterPostProcess);
        }
    }

    /// <summary>
    /// 自定义后处理渲染通道
    /// </summary>
    public class CustomPostProcessRenderPass : ScriptableRenderPass
    {
        /// <summary>
        /// The injection point of the pass
        /// </summary>
        private CustomPostProcessInjectionPoint injectionPoint;

        /// <summary>
        /// Pass名称 用于在Profiler分析器上分组显示
        /// </summary>
        private string m_PassName;

        /// <summary>
        /// List of all post process renderer instances.
        /// </summary>
        private List<CustomPostProcessRenderer> m_PostProcessRenderers;

        /// <summary>
        /// List of all post process renderer instances that are active for the current camera.
        /// </summary>
        private List<int> m_ActivePostProcessRenderers;

        /// <summary>
        /// A list of profiling samplers, one for each post process renderer
        /// </summary>
        private List<ProfilingSampler> m_ProfilingSamplers;

        /// <summary>
        /// Gets whether this render pass has any post process renderers to execute
        /// </summary>
        public bool HasPostProcessRenderers => m_PostProcessRenderers.Count != 0;

        /// <summary>
        /// Construct the custom post-processing render pass
        /// </summary>
        /// <param name="injectionPoint">The post processing injection point</param>
        /// <param name="classes">The list of classes for the renderers to be executed by this render pass</param>
        public CustomPostProcessRenderPass(CustomPostProcessInjectionPoint injectionPoint, List<CustomPostProcessRenderer> renderers)
        {
            this.injectionPoint = injectionPoint;
            this.m_ProfilingSamplers = new List<ProfilingSampler>(renderers.Count);
            this.m_PostProcessRenderers = renderers;

            foreach (var renderer in renderers)
            {
                // Get renderer name and add it to the names list
                var attribute = CustomPostProcessAttribute.GetAttribute(renderer.GetType());
                m_ProfilingSamplers.Add(new ProfilingSampler(attribute?.Name));
            }

            // Pre-allocate a list for active renderers
            this.m_ActivePostProcessRenderers = new List<int>(renderers.Count);

            //设置 渲染事件 插入的节点
            switch(injectionPoint)
            {
                case CustomPostProcessInjectionPoint.AfterOpaqueAndSky: 
                    renderPassEvent = RenderPassEvent.AfterRenderingSkybox; 
                    m_PassName = "Custom PostProcess after Opaque & Sky";
                    break;
                case CustomPostProcessInjectionPoint.BeforePostProcess: 
                    renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                    m_PassName = "Custom PostProcess before PostProcess";
                    break;
                case CustomPostProcessInjectionPoint.AfterPostProcess:
                    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
                    m_PassName = "Custom PostProcess after PostProcess";
                    break;
            }

            //初始化 临时绘制纹理
            InitTempRenderTexture();
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
        }

        /// <summary>
        /// Prepares the renderer for executing on this frame and checks if any of them actually requires rendering
        /// </summary>
        /// <param name="renderingData">Current rendering data</param>
        /// <returns>True if any renderer will be executed for the given camera. False Otherwise.</returns>
        public bool PrepareRenderers(ref RenderingData renderingData)
        {
            // See if current camera is a scene view camera to skip renderers with "visibleInSceneView" = false.
            bool isSceneView = renderingData.cameraData.cameraType == CameraType.SceneView;

            // Here, we will collect the inputs needed by all the custom post processing effects
            ScriptableRenderPassInput passInput = ScriptableRenderPassInput.None;

            // Collect the active renderers
            m_ActivePostProcessRenderers.Clear();
            for(int index = 0; index < m_PostProcessRenderers.Count; index++)
            {
                var ppRenderer = m_PostProcessRenderers[index];
                // Skips current renderer if "visibleInSceneView" = false and the current camera is a scene view camera. 
                if(isSceneView && !ppRenderer.visibleInSceneView) continue;

                // Setup the camera for the renderer and if it will render anything, add to active renderers and get its required inputs
                if (ppRenderer.Setup(ref renderingData, injectionPoint))
                {
                    m_ActivePostProcessRenderers.Add(index);
                    passInput |= ppRenderer.input;
                }
            }

            // Configure the pass to tell the renderer what inputs we need
            ConfigureInput(passInput);

            // return if no renderers are active
            return m_ActivePostProcessRenderers.Count != 0;
        }

        #region 中间RenderTexture
        /// <summary>
        /// 临时RT 绘制目标句柄
        /// </summary>
        private RenderTargetHandle[] m_TempRenderTextureHandle;

        /// <summary>
        /// 临时RT 分配状态
        /// </summary>
        private bool[] m_TempRenderTextureIsAllocated;

        /// <summary>
        /// RT描述参数 当前Renderer
        /// </summary>
        private RenderTextureDescriptor m_TempRenderTextureDesc;

        //RT数量
        private int m_TempRenderTextureCount = 2;

        /// <summary>
        /// 初始化 临时绘制纹理
        /// </summary>
        private void InitTempRenderTexture()
        {
            m_TempRenderTextureHandle = new RenderTargetHandle[m_TempRenderTextureCount];
            m_TempRenderTextureIsAllocated = new bool[m_TempRenderTextureCount];

            for (int i = 0; i < m_TempRenderTextureHandle.Length; i++)
            {
                m_TempRenderTextureHandle[i].Init($"_TempRenderTexture{i}");
                m_TempRenderTextureIsAllocated[i] = false;
            }
        }

        /// <summary>
        /// 获取 临时RT
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private RenderTargetIdentifier GetIntermediate(CommandBuffer cmd, int index)
        {
            //未分配时 进行RT分配
            if (!m_TempRenderTextureIsAllocated[index])
            {
                cmd.GetTemporaryRT(m_TempRenderTextureHandle[index].id, m_TempRenderTextureDesc);
                m_TempRenderTextureIsAllocated[index] = true;
            }

            return m_TempRenderTextureHandle[index].Identifier();
        }

        /// <summary>
        /// 释放已经分配的RT
        /// </summary>
        /// <param name="cmd">The command buffer to use for deallocation</param>
        private void ReleaseAllTempRenderTexture(CommandBuffer cmd)
        {
            for (int i = 0; i < m_TempRenderTextureCount; i++)
            {
                if (m_TempRenderTextureIsAllocated[i] == false) continue;

                cmd.ReleaseTemporaryRT(m_TempRenderTextureHandle[i].id);
                m_TempRenderTextureIsAllocated[i] = false;
            }
        }
        #endregion

        /// <summary>
        /// 执行 后处理特效绘制
        /// </summary>
        /// <param name="context">SRP的上下文数据</param>
        /// <param name="renderingData">当前绘制数据</param>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            RenderTargetIdentifier target = renderingData.cameraData.renderer.cameraColorTarget;

            //相机描述参数 用于设置RT
            m_TempRenderTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            m_TempRenderTextureDesc.msaaSamples = 1; //多重采样 等级1
            m_TempRenderTextureDesc.depthBufferBits = 0; //深度缓冲 关闭

            CommandBuffer cmd = CommandBufferPool.Get(m_PassName);
            //context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            int width = m_TempRenderTextureDesc.width;
            int height = m_TempRenderTextureDesc.height;
            cmd.SetGlobalVector("_ScreenSize", new Vector4(width, height, 1.0f / width, 1.0f / height));

            int intermediateIndex = 0; //当前使用的 临时RT 下标
            //将帧缓冲先绘制进临时RT
            //直接用于后处理绘制 会导致Shader的_MainTex无法正常传入
            cmd.Blit(target, GetIntermediate(cmd, intermediateIndex));
            //绘制所有后处理Renderer
            for (int index = 0; index < m_ActivePostProcessRenderers.Count; ++index)
            {
                var rendererIndex = m_ActivePostProcessRenderers[index];
                var renderer = m_PostProcessRenderers[rendererIndex];
                
                RenderTargetIdentifier source, destination;

                //获取 源贴图
                source = GetIntermediate(cmd, intermediateIndex);
                //获取 绘制的目标贴图
                if (index >= m_ActivePostProcessRenderers.Count - 1)
                    destination = target; //最后一次绘制 直接绘制进帧缓冲贴图
                else
                {
                    //更换RT 反复绘制后处理效果
                    intermediateIndex = 1 - intermediateIndex;
                    destination = GetIntermediate(cmd, intermediateIndex);
                }

                using (new ProfilingScope(cmd, m_ProfilingSamplers[rendererIndex]))
                {
                    //初始化后处理Renderer
                    if (!renderer.Initialized)
                        renderer.InitializeInternal();
                    //执行后处理绘制
                    renderer.Render(cmd, source, destination, ref renderingData, injectionPoint);
                }
            } 

            //执行CommandBuffer 并释放
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            //释放所有临时RT
            ReleaseAllTempRenderTexture(cmd);
        }
    }
}