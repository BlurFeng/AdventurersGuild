using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 演员
    /// </summary>
    public class AActor : MonoBehaviour
    {
        bool m_IsInit;

        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInit { get { return m_IsInit; } }

        private string m_SceneID;
        /// <summary>
        /// 场景Id，在添加到依存对象时生成
        /// 在游戏运行时生成，在一个World中保证唯一性。
        /// </summary>
        public string SceneID { get { return m_SceneID; } }

        private string m_NameTag;
        /// <summary>
        /// 名称Tag
        /// this.GetType().ToString()
        /// </summary>
        public string NameTag { get { return m_SceneID; } }

        private Collider m_Collider;
        /// <summary>
        /// 获取自身碰撞器组件（自身和子节点下能获取到的第一个） 没有则返回null
        /// </summary>
        public Collider GetCollider
        {
            get
            {
                if(m_Collider == null)
                    m_Collider = GetComponentInChildren<Collider>();

                return m_Collider;
            }
        }

        private Rigidbody m_Rigidbody;
        /// <summary>
        /// 获取自身碰撞器组件（自身和子节点下能获取到的第一个） 没有则返回null
        /// </summary>
        public Rigidbody GetRigidbody
        {
            get
            {
                if (m_Rigidbody == null)
                    m_Rigidbody = GetComponentInChildren<Rigidbody>();

                return m_Rigidbody;
            }
        }

        /// <summary>
        /// 获取自身范围界限的中心点 无法获取则返回根节点Transform.position
        /// </summary>
        public Vector3 GetBoundsOriginPos
        {
            get
            {
                if(GetCollider != null)
                {
                    return GetCollider.bounds.center;
                }

                return TransformGet.position;
            }
        }

        /// <summary>
        /// 拥有者
        /// 当拥有者被销毁时 自身也会被销毁
        /// </summary>
        private System.Object m_Outer;

        /// <summary>
        /// 获取此Actor的依存对象
        /// </summary>
        public System.Object GetOuter { get { return m_Outer; } }

        /// <summary>
        /// 自身所有功能组件Component的集合
        /// </summary>
        [SerializeField]
        private Dictionary<string, List<UActorComponent>> m_ComponentsDic;

        /// <summary>
        /// 注视点 用于被摄像机跟随等
        /// </summary>
        public virtual Vector3 WatchPoint { get { return TransformGet.position; } }

        /// <summary>
        /// 初始化
        /// 放置在Level场景中的Actor会在Level.Init时调用Actor.Init。
        /// 运行时实例化的Actor需要主动调用Init来进行初始化，一般没有特殊情况可以在OnAwake中调用Init
        /// </summary>
        /// <param name="outer">依存对象，当依存对象被销毁时，自身也会被销毁</param>
        /// <returns></returns>
        public virtual bool Init(System.Object outer = null)
        {
            if (m_IsInit) return false;
            m_IsInit = true;

            //自动获取当前Level作为依存对象
            if (outer == null) outer = FWorldContainer.GetLevelCur();
            if (outer == null) return false;

            m_ComponentsDic = new Dictionary<string, List<UActorComponent>>();
            m_Outer = outer;
            m_NameTag = this.GetType().ToString();

            //推送到所属者
            m_SceneID = FWorldContainer.AddActor(this, m_NameTag, outer);

            return true;
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void ResetSelf()
        {

        }

        /// <summary>
        /// 销毁自身
        /// </summary>
        public virtual void DestroySelf()
        {
            if(GameObjectGet != null)
                GameObject.Destroy(GameObjectGet);

            //从所属的容器移除自己
            FWorldContainer.RemoveActor(this, m_NameTag, m_Outer);
        }

        /// <summary>
        /// 启动 在初始化之后
        /// </summary>
        public virtual void Begin()
        {
            foreach (var components in m_ComponentsDic.Values)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Begin();
                }
            }
        }

        /// <summary>
        /// 每帧执行，意义同Update
        /// </summary>
        public virtual void Tick(float deltaTime)
        {
            foreach (var components in m_ComponentsDic.Values)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].Tick(deltaTime);
                }
            }
        }

        /// <summary>
        /// 在所有Update执行结束后执行，意义同LateUpdate
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void LateTick(float deltaTime)
        {
            foreach (var components in m_ComponentsDic.Values)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].LateTick(deltaTime);
                }
            }
        }

        /// <summary>
        /// 固定时间间隔执行，意义同FixedUpdate
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public virtual void FixedTick(float fixedDeltaTime)
        {
            foreach (var components in m_ComponentsDic.Values)
            {
                for (int i = 0; i < components.Count; i++)
                {
                    components[i].FixedTick(fixedDeltaTime);
                }
            }
        }

        //仅能重写Actor允许的生命周期函数
        //方便统一进行管理

        private void Awake()
        {
            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        private void OnDestroy()
        {
            OnDestroyThis();
        }

        private void OnEnable()
        {
            OnEnableThis();
        }

        private void OnDisable()
        {
            OnDisableThis();
        }

        private void Reset()
        {
            OnReset();
        }

        private void OnValidate()
        {
            OnValidateActor();
        }

        protected virtual void OnAwake()
        {

        }

        protected virtual void OnStart()
        {

        }

        protected virtual void OnDestroyThis()
        {

        }

        protected virtual void OnEnableThis()
        {

        }

        protected virtual void OnDisableThis()
        {

        }

        protected virtual void OnReset()
        {

        }

        protected virtual void OnValidateActor()
        {

        }

        /// <summary>
        /// 添加组件 在UActorComponent类构造是会调用 一般不需要主动调用
        /// </summary>
        /// <param name="component"></param>
        public string AddActorComponent(UActorComponent component)
        {
            string typeTag = component.GetType().ToString();

            if (!m_ComponentsDic.ContainsKey(typeTag))
            {
                //初始化数据
                m_ComponentsDic.Add(typeTag, new List<UActorComponent>());
            }

            //判断是否包含以免错误的调用add方法导致重复添加
            if (!m_ComponentsDic[typeTag].Contains(component))
            {
                //填充数据
                m_ComponentsDic[typeTag].Add(component);

                return typeTag + "_" + m_ComponentsDic[typeTag].Count;
            }

            return string.Empty;
        }

        /// <summary>
        /// 移除所有此类型的组件
        /// </summary>
        /// <param name="component"></param>
        public void RemoveActorComponentByTypeTag(UActorComponent component)
        {
            string typeTag = component.GetType().ToString();

            if (m_ComponentsDic.ContainsKey(typeTag))
            {
                m_ComponentsDic.Remove(typeTag);
            }
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="component"></param>
        public void RemoveActorComponent(UActorComponent component)
        {
            string typeTag = component.GetType().ToString();

            if (m_ComponentsDic.ContainsKey(typeTag))
            {
                m_ComponentsDic[typeTag].Remove(component);
            }
        }

        #region Unity MonoBehaviour Extension

        //这里Get放后面是为了只能索引时TransformGet和transform排在一起

        Transform mCachedTransform;
        public Transform TransformGet
        {
            get
            {
                if (mCachedTransform == null && null != this) 
                    mCachedTransform = transform;
                return mCachedTransform;
            }
        }

        GameObject mCachedGameObject;
        public GameObject GameObjectGet
        {
            get
            {
                if (mCachedGameObject == null && null != this) 
                    mCachedGameObject = gameObject;
                return mCachedGameObject;
            }
        }

        #endregion
    }
}
