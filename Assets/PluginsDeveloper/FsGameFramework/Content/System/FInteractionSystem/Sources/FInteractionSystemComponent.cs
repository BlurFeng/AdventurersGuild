using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;//F游戏框架 为了将UActorComponent作为基类方便管理，如果你没有此插件可将基类替换为MonoBehaviour


namespace FInteractionSystem
{
    [Serializable]
    public class FInteractionSystemComponent : UActorComponent
    {
        public FInteractionSystemComponent(AActor actor) : base(actor)
        {
            m_IgnoreColliders = new List<Collider>();
            m_OutlineObjects = new List<Component>();
        }

        /// <summary>
        /// 初始化交互功能组件
        /// </summary>
        /// <param name="centerPoint">探测圆心</param>
        /// <param name="ignoreSelf">探测忽略的对象</param>
        /// <param name="continuousDetectionTargetConditions">持续探测对象的目标条件</param>
        public void Init(Transform centerPoint, Collider ignoreSelf, float closeDis, float veryCloseDis, FDetectionConditionInfo[] continuousDetectionTargetConditions)
        {
            m_CenterPoint = centerPoint;
            m_IgnoreColliders.Add(ignoreSelf);
            m_CloseDistance = closeDis;
            m_VeryCloseDistance = veryCloseDis;

            m_ContinuousDetectionTargetConditions = continuousDetectionTargetConditions;
            m_ContinuousDetectionAllComponents = new Component[continuousDetectionTargetConditions.Length][];
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            RefreshFilteredObjects(null == m_CenterPoint? TransformGet.position : m_CenterPoint.position);
        }

        /// <summary>
        /// 刷新范围内的可交互物体缓存组 根据筛选目标条件数组ContinuousDetectionTargetConditions进行探测筛选
        /// </summary>
        /// <param name="originPosition">探测原点位置</param>
        void RefreshFilteredObjects(Vector3 originPosition)
        {
            if (m_ContinuousDetectionTargetConditions == null) return;

            //持续刷新球形范围探测
            for (int i = 0; i < m_ContinuousDetectionTargetConditions.Length; i++)
            {
                FDetectionConditionInfo info = m_ContinuousDetectionTargetConditions[i];
                FInteractionFunctionLibrary.OverlapSphereOutComponents(originPosition, info.range, info.layerMask, m_IgnoreColliders.ToArray(), out Component[] componentArray);

                m_ContinuousDetectionAllComponents[i] = componentArray;
            }

            //标记可交互接口对象为外发光功能 用于确认对象是否符合对比限制的要求
            Component veryClosestComponentTemp = null;
            IFInteractionInterface veryClosestComponentInterfaceTemp = null;
            float maxDiff = -2f;
            float minDis = m_OutlineLimitRange + 1f;

            //对获得的对象进行处理
            for (int i = 0; i < m_ContinuousDetectionAllComponents.Length; i++)
            {
                var components = m_ContinuousDetectionAllComponents[i];
                if (null == components) continue;

                for (int j = 0; j < components.Length; j++)
                {
                    var component = components[j];
                    if (null == component) continue;

                    var fInterface = component.GetComponent<IFInteractionInterface>();
                    if (null != fInterface)
                    {
                        //获取信息
                        Vector3 componentOrigin = fInterface.GetInteractionPosition();//获取中心

                        float dis = Vector3.Distance(m_CenterPoint.position, componentOrigin);

                        if (dis <= m_CloseDistance)
                        {
                            fInterface.OnDistanceClose(GetOwner(), true);
                            if (dis <= m_VeryCloseDistance)
                            {
                                fInterface.OnDistanceVeryClose(GetOwner(), true);
                            }
                            else
                            {
                                fInterface.OnDistanceVeryClose(GetOwner(), false);
                            }
                        }
                        else
                        {
                            fInterface.OnDistanceClose(GetOwner(), false);
                            fInterface.OnDistanceVeryClose(GetOwner(), false);
                        }

                        //对象对限制条件进行调整 每个对象可被发现的范围可能需求不同
                        float outlineLimitRange = m_OutlineLimitRange; float outlineLimitAngle = m_OutlineLimitAngle; float outlineToleranceRange = m_OutlineToleranceRange;
                        fInterface.AdjustOutlineLimit(ref outlineLimitRange, ref  outlineLimitAngle, ref outlineToleranceRange);

                        //OutLine 标记可交互对象 描边外发光功能
                        //比较对象是否在外发光限制要求内
                        bool goodDirOrDis, goodCompareOrLimit, conditionAllowed;
                        if (FInteractionFunctionLibrary.CompareByLimit(
                            componentOrigin, originPosition, m_CenterPoint.right,
                            outlineLimitRange, outlineLimitAngle, outlineToleranceRange, ref minDis, ref maxDiff, out goodDirOrDis, out goodCompareOrLimit))
                        {

                            if (!m_OutlineMarkedOnlyBestOne)
                            {
                                //标记所有符合条件的可交互对象时 直接打开
                                if (fInterface.OnOutline(GetOwner()/*this*/, true, out conditionAllowed))
                                {
                                    m_OutlineObjects.Add(component);
                                }
                            }

                            //通过了基本限制Limit 且通过了minDis和maxDiff的比较
                            if (goodCompareOrLimit)
                            {
                                veryClosestComponentTemp = component;//最符合的可交互对象
                                veryClosestComponentInterfaceTemp = fInterface;
                            }
                        }
                        else if (!m_OutlineMarkedOnlyBestOne)
                        {
                            //标记所有符合条件的可交互对象时 直接关闭
                            if (fInterface.OnOutline(GetOwner()/*this*/, false, out conditionAllowed))
                            {
                                m_OutlineObjects.Remove(component);
                            }
                        }
                    }
                }
            }

            //OutLine 标记可交互对象 描边外发光功能
            //最佳的标记外发光的可交互对象变化时
            if (m_OutlineBestComponentCached != veryClosestComponentTemp)
            {
                bool conditionAllowed = false;

                //外发光只标记最合适的一个交互对象时 切换外发光状态
                if (m_OutlineMarkedOnlyBestOne)
                {
                    if (null != m_OutlineBestComponentInterfaceCached)
                    {
                        if (m_OutlineBestComponentInterfaceCached.OnOutline(GetOwner()/*this*/, false, out conditionAllowed))
                        {
                            m_OutlineObjects.Remove(m_OutlineBestComponentCached);
                        }
                    }
                    if (null != veryClosestComponentInterfaceTemp)
                    {
                        if (veryClosestComponentInterfaceTemp.OnOutline(GetOwner()/*this*/, true, out conditionAllowed))
                        {
                            m_OutlineObjects.Add(veryClosestComponentTemp);
                        }
                    }
                }

                //刷新当前高亮标记对象缓存
                //当新的对象为空 或者新的对象不为空且通过了条件允许打开描边（联网时通过了条件但可能不会真的打开描边 因为描边只为玩家自己的本地角色打开）
                if(veryClosestComponentTemp == null || conditionAllowed)
                {
                    OnOutlineBestComponentChange?.Invoke(veryClosestComponentTemp, m_OutlineBestComponentCached);
                    m_OutlineBestComponentCached = veryClosestComponentTemp;
                    m_OutlineBestComponentInterfaceCached = veryClosestComponentInterfaceTemp;
                }
            }

            //某些情况下外发光被暂时关闭 又允许打开后需要打开
            if (m_OutlineMarkedOnlyBestOne && m_OutlineBestComponentInterfaceCached != null && !m_OutlineBestComponentInterfaceCached.IsOutline())
            {
                m_OutlineBestComponentInterfaceCached.OnOutline(GetOwner()/*this*/, true, out bool conditionAllowed);
            }
        }

        #region Outline 描边 外发光 标记最佳可交互对象的功能
        /// <summary>
        /// 事件 当最佳描边对象发生改变时
        /// 参数一：新的描边对象 参数二：旧的描边对象
        /// </summary>
        public Action<Component, Component> OnOutlineBestComponentChange;

        [SerializeField]
        bool m_OutlineMarkedOnlyBestOne = true;

        float m_OutlineLimitRange = 0.5f;
        float m_OutlineLimitAngle = 180f;
        float m_OutlineToleranceRange = 0.1f;

        Component m_OutlineBestComponentCached;
        IFInteractionInterface m_OutlineBestComponentInterfaceCached;

        List<Component> m_OutlineObjects;
        #endregion

        #region Detection 探测可交互对象功能
        /// <summary>
        /// 中心点方位信息
        /// </summary>
        Transform m_CenterPoint;

        /// <summary>
        /// 持续筛选时忽略的对象碰撞体
        /// </summary>
        List<Collider> m_IgnoreColliders;

        /// <summary>
        /// 持续探测目标条件组
        /// 可以设置多组探测目标条件 之后会缓存到第一次探测数组中
        /// </summary>
        FDetectionConditionInfo[] m_ContinuousDetectionTargetConditions;

        bool m_IsNoEmptyUClassInContinuousDetectionTargetConditions;

        /// <summary>
        /// 第一次探测对象缓存数组
        /// </summary>
        Component[][] m_ContinuousDetectionAllComponents;

        /// <summary>
        /// GetDetectionFilteredTArray 获得第一次筛选数组方法使用
        /// 用于缓存筛选目标类 和ContinuousDetectionTargetConditions以及ContinuousDetectionAllComponents中的index对应
        /// </summary>
        Dictionary<Type, int> m_DetectionFilteredClassMap;

        /// <summary>
        /// 对近距离范围定义 当可交互对象在此范围内时调用OnClose接口方法
        /// </summary>
        float m_CloseDistance = 1.4f;

        /// <summary>
        /// 对非常近距离范围定义 当可交互对象在此范围内时调用OnVeryClose接口方法
        /// </summary>
        float m_VeryCloseDistance = 3.2f;

        /// <summary>
        /// 是否在Editor场景中绘制探测范围
        /// </summary>
        [SerializeField]
        bool m_EnableDebugDrawing;

        /// <summary>
        /// 在所有初步筛选目标中 探测并返回距离自己最近的符合条件的目标 需要提供多个方位信息参数
	    ///（初步筛选目标在组件的ContinuousDetectionTargetConditions数组中添加）
        /// </summary>
        /// <param name="findComponent">找到并返回的目标Component</param>
        /// <param name="fetectionOriginPoint">探测中心源点类型</param>
        /// <param name="radiusLimit">探测范围半径</param>
        /// <param name="angleLimit">探测范围角度限制</param>
        /// <param name="toleranceRange">容许范围 当两个对象的距离差小于此值时，优先获取更接近参照方向的对象</param>
        /// <param name="originDirectionType">探测参照方向类型</param>
        /// <param name="filteredClass">探测源点位置偏移 偏移会根据源点的坐标系</param>
        /// <param name="containsSubclass">目标类型筛选 如果为空则不筛选</param>
        /// <param name="getTargetPositionFunc">筛选时包含此类及其子类</param>
        /// <returns>是否找到了有效的目标</returns>
        public bool DetectionObject(
            out Component findComponent, DetectionOriginPoint fetectionOriginPoint = DetectionOriginPoint.CenterPoint, float radiusLimit = 4f, float angleLimit = 1.8f, float toleranceRange = 10f, 
            EDirection originDirectionType = EDirection.Forward, System.Type filteredClass = null, bool containsSubclass = true,
            GetTargetPosition getTargetPositionFunc = null)
        {
            findComponent = null;

            //获取第一次持续探测缓存的对象数组
            if (!GetContinuousDetectionActors(filteredClass, out Component[] outFilteredActors)) return false;

            //获取探测原点方位信息
            Vector3 originPosition = TransformGet.position, originReferenceDir = Vector3.forward;
            Transform origin = TransformGet;
            switch (fetectionOriginPoint)
            {
                case DetectionOriginPoint.CenterPoint:
                    if(null != m_CenterPoint)
                    {
                        originPosition = m_CenterPoint.position;
                        origin = m_CenterPoint;
                    }
                    break;
                case DetectionOriginPoint.Root:
                    originPosition = TransformGet.position;
                    origin = TransformGet;
                    break;
                case DetectionOriginPoint.PlayerCamera:
                    //TODO:获得当前主相机的方位信息
                    break;
            }

            switch (originDirectionType)
            {
                case EDirection.Forward:
                    originReferenceDir = origin.forward;
                    break;
                case EDirection.Back:
                    originReferenceDir = -origin.forward;
                    break;
                case EDirection.Right:
                    originReferenceDir = origin.right;
                    break;
                case EDirection.Left:
                    originReferenceDir = -origin.right;
                    break;
                case EDirection.Up:
                    originReferenceDir = origin.up;
                    break;
                case EDirection.Down:
                    originReferenceDir = -origin.up;
                    break;
                default:
                    break;
            }

            return FInteractionFunctionLibrary.DetectionObjectByObjectArray(
                outFilteredActors, out findComponent,
                originPosition, originReferenceDir, radiusLimit, angleLimit, toleranceRange, filteredClass, containsSubclass, getTargetPositionFunc, m_EnableDebugDrawing);
        }

        bool GetContinuousDetectionActors(Type filteredClass, out Component[] outFilteredActors)
        {
            //有目标筛选类型
            if (null != filteredClass)
            {
                //此部分代码用于优化 在筛选条件中有条件有筛选UClass但没有ObjectTypes标签，那么有存在保存了所有目标类对象的探测对象数组
                //可以直接只获得此数组用于第二次探测

                //映射字典中不存在 在筛选条件数组中寻找
                if (!m_DetectionFilteredClassMap.ContainsKey(filteredClass))
                {

                    //筛选数组中不存在对应的类或其父类 则index=-1
                    m_DetectionFilteredClassMap.Add(filteredClass, -1);

                    for (int i = 0; i < m_ContinuousDetectionTargetConditions.Length; i++)
                    {
                        FDetectionConditionInfo info = m_ContinuousDetectionTargetConditions[i];

                        //存在此类及其父类的所有第一次探测缓存对象数组
                        if (info.classFilter != null && info.layerMask.value == 0
                            && filteredClass.IsSubclassOf(info.classFilter))
                        {
                            //获得第一次筛选对象缓存数组对应的index
                            m_DetectionFilteredClassMap[filteredClass] = i;
                            break;
                        }
                    }
                }

                if (m_DetectionFilteredClassMap[filteredClass] >= 0)
                {
                    outFilteredActors = m_ContinuousDetectionAllComponents[m_DetectionFilteredClassMap[filteredClass]];
                    return outFilteredActors.Length > 0;
                }

            }
            //整合所有的第一次探测对象缓存数组 用于第二次探测
            //TODO: O(n) 效率需要优化
            List<AActor> actors = new List<AActor>();
            for (int i = 0; i < m_ContinuousDetectionAllComponents.Length; i++)
            {
                actors.AddRange(m_ContinuousDetectionAllComponents[i]);
            }
            outFilteredActors = actors.ToArray();

            return outFilteredActors.Length != 0;
        }
        #endregion

        #region Unity MonoBehaviour Extension

        Transform mCachedTransform;
        public Transform TransformGet
        {
            get
            {
                if (mCachedTransform == null) mCachedTransform = GetOwner().TransformGet;//transform;
                return mCachedTransform;
            }
        }

        GameObject mCachedGameObject;
        public GameObject GameObjectGet
        {
            get
            {
                if (mCachedGameObject == null) mCachedGameObject = GetOwner().GameObjectGet;//gameObject;
                return mCachedGameObject;
            }
        }

        #endregion
    }
}