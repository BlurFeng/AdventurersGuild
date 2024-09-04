using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 游戏实例 用于启动项目初始化基本的其他系统，执行整个生命周期
    /// </summary>
    public class FGameInstance : MonoBehaviour
    {
        static FGameInstance instance;

        public static FGameInstance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<FGameInstance>();

                    if (instance == null)
                    {
                        Debug.LogWarning("SingletonBehaviour: " + typeof(FGameInstance).Name + " is null");
                        GameObject singletonObject = new GameObject { name = typeof(FGameInstance).Name };
                        instance = singletonObject.AddComponent<FGameInstance>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }

        public Transform TransformGet
        {
            get
            {
                if (mCachedTransform == null && null != this)
                    mCachedTransform = transform;
                return mCachedTransform;
            }
        }
        Transform mCachedTransform;

        public GameObject GameObjectGet
        {
            get
            {
                if (mCachedGameObject == null && null != this)
                    mCachedGameObject = gameObject;
                return mCachedGameObject;
            }
        }
        GameObject mCachedGameObject;

        //ASystem的集合
        private List<USystem> m_Systems;

        /// <summary>
        /// 流程控制器
        /// </summary>
        protected ProcedureSystem m_ProcedureSystem = new ProcedureSystem();

        [Header("游戏开始默认世界")]
        public UWorldConfig m_DefaultWorldConfig;

        [Header("是否打开编辑器测试用系统")]
        public bool OpenEditorTestSystem;

        protected virtual void Awake()
        {
            DontDestroyOnLoad(this);

            //实例化成员
            m_Systems = new List<USystem>();

            //流程控制器
            m_ProcedureSystem.AddNode(new ProcedureNode((procedureSystem) =>
            {
                Init();
            }));
            m_ProcedureSystem.OnStartAnew();
        }

        protected virtual void Start()
        {
            for (int i = 0; i < m_Systems.Count; i++)
            {
                m_Systems[i].Begin();
            }

            FWorldContainer.Begin();
        }

        protected virtual void Update()
        {
            float deltaTime = Time.deltaTime;
            //执行所有的ASystem
            for (int i = 0; i < m_Systems.Count; i++)
            {
                m_Systems[i].Tick(deltaTime);
            }

            FWorldContainer.Tick(deltaTime);
        }

        protected virtual void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            //执行所有的ASystem
            for (int i = 0; i < m_Systems.Count; i++)
            {
                m_Systems[i].LateTick(deltaTime);
            }

            FWorldContainer.LateTick(deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            //执行所有的ASystem
            for (int i = 0; i < m_Systems.Count; i++)
            {
                m_Systems[i].FixedTick(fixedDeltaTime);
            }

            FWorldContainer.FixedTick(fixedDeltaTime);
        }

        protected virtual void OnDestroy()
        {
            //关闭游戏时引擎销毁对象的顺序不可控 可能导致调用销毁自身方法时 GameObject已被销毁却试图Get的问题 其实没必要在此调用DestroySelf()
            //FWorldContainer.DestroySelf();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Init()
        {
            var systemTypes = FsUtility.GetTypes(typeof(USystem));
            for (int i = 0; i < systemTypes.Length; i++)
            {
                var type = systemTypes[i];

                //是否打开编辑器测试系统
                if (!OpenEditorTestSystem && type == typeof(UEditorTestSystem)) continue;

                var system = Activator.CreateInstance(type) as USystem;
                system.Init();

                m_Systems.Add(system);
            }

            //世界容器初始化
            FWorldContainer.Init();
            if (null != m_DefaultWorldConfig)
            {
                FWorldContainer.SwitchWorld(m_DefaultWorldConfig);
            }
            else
            {
                //throw new System.Exception("GameInstance : DefaultWorldConfig is null!");
            }
        }
    }

    /// <summary>
    /// 流程控制器
    /// </summary>
    public class ProcedureSystem
    {
        private List<ProcedureNode> m_ListProcedureNode; //列表 流程节点
        private int m_NodeIndexCur; //当前流程节点下标

        public ProcedureSystem()
        {
            m_NodeIndexCur = 0;
            m_ListProcedureNode = new List<ProcedureNode>();
        }

        /// <summary>
        /// 添加 流程节点
        /// </summary>
        /// <param name="procedureNode"></param>
        public void AddNode(ProcedureNode procedureNode)
        {
            m_ListProcedureNode.Add(procedureNode);
        }

        /// <summary>
        /// 从头开始执行
        /// </summary>
        public void OnStartAnew()
        {
            m_NodeIndexCur = 0;
            OnContinueNode();
        }

        /// <summary>
        /// 执行 下一流程节点
        /// </summary>
        public void OnContinueNode()
        {
            if (m_NodeIndexCur >= m_ListProcedureNode.Count)
            {
                Debug.LogError($"ProcedureSystem.OnContinue() >> 无下一流程节点 流程控制器已执行完毕 节点下标-{m_NodeIndexCur}");
                return;
            }

            //执行 流程节点
            ProcedureNode procedureNode = m_ListProcedureNode[m_NodeIndexCur++];
            procedureNode.OnExecute(this);
        }
    }

    public class ProcedureNode
    {
        private Action<ProcedureSystem> m_Handler;

        public ProcedureNode(Action<ProcedureSystem> handler)
        {
            m_Handler = handler;
        }

        /// <summary>
        /// 执行
        /// </summary>
        public void OnExecute(ProcedureSystem procedureSystem)
        {
            m_Handler?.Invoke(procedureSystem);
        }
    }
}
