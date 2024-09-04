using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace FsGridCellSystem
{
    /// <summary>
    /// 真实体积
    /// 包括整体包围盒的信息，碰撞器等
    /// </summary>
    [Serializable]
    public struct RealVolume
    {
        public RealVolume(string name, GridCoordFloat boundingBoxSize, GridCoordFloat boundingBoxLocalLocation)
        {
            this.Name = name;
            this.BoundingBoxSize = boundingBoxSize;
            this.BoundingBoxLocalLocation = boundingBoxLocalLocation;
            ColliderNode = null;
        }

        public bool EqualsPart1(RealVolume other)
        {
            if (Name != other.Name) return false;
            if (BoundingBoxSize != other.BoundingBoxSize) return false;
            if (BoundingBoxLocalLocation != other.BoundingBoxLocalLocation) return false;
            //ColliderNode为后期生成

            return true;
        }

        //此处数据为单位化数据
        //size = (0.2,0.2,0.2) = 单元格尺寸 * 0.2
        public string Name;

        /// <summary>
        /// 包围盒单位化尺寸
        /// </summary>
        public GridCoordFloat BoundingBoxSize;

        /// <summary>
        /// 包围盒单位化本地坐标
        /// 原点为GridItemSize范围的左下角点
        /// </summary>
        public GridCoordFloat BoundingBoxLocalLocation;

        /// <summary>
        /// 对应的碰撞器节点GObj
        /// 上面包含一个或多个Collider组件
        /// 可能是ColliderRoot或者此节点下的子节点
        /// </summary>
        public GameObject ColliderNode;
    }

    /// <summary>
    /// 网格项目
    /// </summary>
    [Serializable]
    public class GridItemComponent : System.Object
    {
        public GridItemComponent()
        {
            //网格数据 默认
            MainGridCoord = GridCoord.invalid;
        }

        public GridItemComponent(GridItemComponent gridItem)
        {
            //网格数据 复制
            Value = gridItem.Value;
            Direction = gridItem.Direction;
            GridItemType = gridItem.GridItemType;
            MainGridCoord = gridItem.MainGridCoord;
            GridItemSize = gridItem.GridItemSize;
            InitState(gridItem.ListState);
        }

        private MonoBehaviour m_Owner;

        /// <summary>
        /// 网格项目 数据
        /// </summary>
        public GridItemData GridItemData { get { return m_GridItemData; } set { m_GridItemData = value; } }

        [Header("网格物体数据")]
        [Tooltip("网格物体组件中保存的网格物体数据")]
        [SerializeField]
        private GridItemData m_GridItemData;

        /// <summary>
        /// 数据值
        /// </summary>
        [HideInInspector]
        public int Value { get { return m_GridItemData.Value; } set { m_GridItemData.Value = value; } }

        /// <summary>
        /// 朝向
        /// </summary>
        public EDirection Direction { get { return m_GridItemData.Direction; } set { m_GridItemData.Direction = value; } }

        /// <summary>
        /// 类型
        /// </summary>
        [HideInInspector]
        public EGridItemType GridItemType { get { return m_GridItemData.GridItemType; } set { m_GridItemData.GridItemType = value; } }

        /// <summary>
        /// 主单元格坐标
        /// </summary>
        public GridCoord MainGridCoord
        {
            get
            {
                return m_GridItemData.MainGridCoord;
            }
            set
            {
                m_GridItemData.MainGridCoord = value;
                RefreshViewObjPosition(); //刷新 显示物体位置
                ExecuteLocationCheck(); //执行 定位系统检测
            }
        }

        /// <summary>
        /// 占用单元格尺寸
        /// </summary>
        public GridCoord GridItemSize { get { return m_GridItemData.GridItemSize; } set { m_GridItemData.GridItemSize = value; } }

        /// <summary>
        /// 获取 占据网格尺寸 根据当前朝向
        /// </summary>
        public GridCoord GetGridItemSizeAtDirection
        {
            get
            {
                return m_GridItemData.GetGridItemSizeAtDirection;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init(MonoBehaviour owner)
        {
            RealVolumeInit();

            m_Owner = owner;
        }

        /// <summary>
        /// 设置 数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="gridItemType"></param>
        /// <param name="mainGridCoord"></param>
        /// <param name="gridItemSize"></param>
        /// <param name="listState"></param>
        public virtual void SetData(int value = 0, EDirection direction = EDirection.None, EGridItemType gridItemType = EGridItemType.None,
            GridCoord mainGridCoord = default, GridCoord gridItemSize = default, List<int> listState = null)
        {
            Value = value;
            Direction = direction;
            GridItemType = gridItemType;
            MainGridCoord = mainGridCoord;
            GridItemSize = gridItemSize;
            InitState(listState);
        }

        #region 状态集合
        /// <summary>
        /// 状态集合
        /// </summary>
        public List<int> ListState { get { return m_ListState; } }
        private List<int> m_ListState = new List<int>();

        /// <summary>
        /// 初始化 状态集合
        /// </summary>
        /// <param name="listState"></param>
        public void InitState(List<int> listState = null)
        {
            m_ListState.Clear();
            if (listState != null)
            {
                for (int i = 0; i < listState.Count; i++)
                {
                    m_ListState.Add(listState[i]);
                }
            }
        }

        /// <summary>
        /// 添加 状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool AddState(int state)
        {
            if (m_ListState.Contains(state)) { return false; }

            m_ListState.Add(state);

            return true;
        }

        /// <summary>
        /// 移除 状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool RemoveState(int state)
        {
            if (!m_ListState.Contains(state)) { return false; }

            m_ListState.Remove(state);

            return true;
        }

        /// <summary>
        /// 检测 是否有状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CheckState(int state)
        {
            bool hadState = m_ListState.Contains(state);

            return hadState;
        }
        #endregion

        #region 显示物体
        #region 碰撞根节点
        /// <summary>
        /// 碰撞器根节点 物体
        /// </summary>
        public GameObject ColliderRootGo
        {
            get { return m_ColliderRootGo; }
            set 
            { 
                m_ColliderRootGo = value;
                //默认为激活状态
                SetColliderRootActive(true);
            }
        }
        private GameObject m_ColliderRootGo;

        /// <summary>
        /// 设置 碰撞器根节点 激活状态
        /// </summary>
        /// <param name="isActive"></param>
        public void SetColliderRootActive(bool isActive)
        {
            if (m_ColliderRootGo == null) return;

            if (m_ColliderRootGo.activeSelf != isActive)
                m_ColliderRootGo.SetActive(isActive);
        }
        #endregion

        #region RealVolume 真实体积
        [Header("真实体积")]
        [Tooltip("真实体积数据中包括包围盒尺寸和本地位置，及碰撞器等内容")]
        [SerializeField]
        private List<RealVolume> m_RealVolumes = new List<RealVolume>();

        private string[] m_RealVolumeDicKeys;

        /// <summary>
        /// 当前使用的真实体积数据
        /// </summary>
        private string m_RealVolumeCurKey;

        public bool IsHaveMultipleCollider { get { return m_IsHaveMultipleCollider; } }
        public bool m_IsHaveMultipleCollider;

        /// <summary>
        /// 获取当前使用的真实体积数据
        /// </summary>
        public bool GetRealVolumeCur(out RealVolume outRealVolume)
        {
            if (string.IsNullOrEmpty(m_RealVolumeCurKey))
            {
                outRealVolume = new RealVolume();
                return false;
            }

            for (int i = 0; i < m_RealVolumes.Count; i++)
            {
                var realVolum = m_RealVolumes[i];
                if (realVolum.Name.Equals(m_RealVolumeCurKey))
                {
                    outRealVolume = realVolum;
                    return true;
                }
            }

            outRealVolume = new RealVolume();
            return false;
        }

        public void SetIsHaveMultipleCollider(bool isHaveMultipleCollider)
        {
            m_IsHaveMultipleCollider = isHaveMultipleCollider;
        }

        /// <summary>
        /// 真实碰撞初始化
        /// </summary>
        private void RealVolumeInit()
        {
            //关闭碰撞盒节点
            for (int i = 0; i < m_RealVolumes.Count; i++)
            {
                var realVolum = m_RealVolumes[i];
                if (realVolum.ColliderNode != null)
                {
                    realVolum.ColliderNode.SetActive(false);
                }
            }

            //默认选中 第一个碰撞节点
            if (m_RealVolumes.Count > 0)
            {
                SetRealVolumeCur(m_RealVolumes[0].Name);
            }
        }

        /// <summary>
        /// 设置当前使用的真实碰撞盒
        /// </summary>
        /// <param name="keyName"></param>
        public void SetRealVolumeCur(string keyName)
        {
            if (!string.IsNullOrEmpty(m_RealVolumeCurKey) && m_RealVolumeCurKey.Equals(keyName)) return;

            RealVolume outRealVolume;

            //处理上个使用的真实体积数据
            if (!string.IsNullOrEmpty(m_RealVolumeCurKey))
            {
                if(GetRealVolumeCur(out outRealVolume))
                    outRealVolume.ColliderNode.SetActive(false);
            }

            m_RealVolumeCurKey = keyName;

            if (GetRealVolumeCur(out outRealVolume))
            {
                if (outRealVolume.ColliderNode)
                    outRealVolume.ColliderNode.SetActive(true);
                else
                    Debug.LogWarning($"网格物体组件，真实体积数据没有有效的碰撞盒GameObject，请确认是否是真实体积数据更新但未重新生成碰撞器。网格物体名称：{(m_Owner ? m_Owner.gameObject.name : "没有拥有者")}  真实体积节点名称：{outRealVolume.Name}");
            }
            else
                Debug.LogWarning($"网格物体组件，无法获取对应KeyName : {keyName} 的RealVolume真实体积数据。确认KeyName是否正确。网格物体名称：{(m_Owner ? m_Owner.gameObject.name : "没有拥有者")}  真实体积节点名称：{outRealVolume.Name}");
        }

        /// <summary>
        /// 设置真实体积数据字典
        /// </summary>
        /// <param name="newRealVolumes"></param>
        public void SetRealVolume(RealVolume[] newRealVolumes)
        {
            if (newRealVolumes == null || newRealVolumes.Length == 0) return;

            m_RealVolumes = new List<RealVolume>(newRealVolumes);
        }

        /// <summary>
        /// 添加一个真实体积数据
        /// </summary>
        /// <param name="realVolume"></param>
        public bool AddRealVolume(RealVolume realVolume)
        {
            if (m_RealVolumes.Contains(realVolume)) { return false; }

            m_RealVolumes.Add(realVolume);
            return true;

            //Debug.Log(string.Format("GridItemComponent.SetRealVolumeDic new RealVolume Data  ---  Name:{0}  Size:{1}  LocalLocation:{2}", realVolume.name, realVolume.boundingBoxSize, realVolume.boundingBoxLocalLocation));
        }

        /// <summary>
        /// 移除一个真实体积数据
        /// </summary>
        /// <param name="realVolume"></param>
        public void RemoveRealVolume(RealVolume realVolume)
        {
            m_RealVolumes.Remove(realVolume);
        }

        public bool GetRealVolume(int index, out RealVolume outRealVolume)
        {
            if(index >= 0 && index < m_RealVolumes.Count)
            {
                outRealVolume = m_RealVolumes[index];
                return true;
            }

            outRealVolume = new RealVolume();
            return false;
        }

        /// <summary>
        /// 设置真实体积数据绑定的碰撞器节点
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="colliderNode"></param>
        public bool SetRealVolumeColliderNode(string keyName, GameObject colliderNode)
        {
            if (colliderNode == null) return false;

            for (int i = 0; i < m_RealVolumes.Count; i++)
            {
                var realVolumCur = m_RealVolumes[i];

                if (realVolumCur.Name.Equals(keyName))
                {
                    m_RealVolumes[i] = new RealVolume()
                    {
                        Name = realVolumCur.Name,
                        BoundingBoxSize = realVolumCur.BoundingBoxSize,
                        BoundingBoxLocalLocation = realVolumCur.BoundingBoxLocalLocation,
                        ColliderNode = colliderNode
                    };

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取真实体积数据所有Key
        /// </summary>
        /// <returns></returns>
        public string[] GetRealVolumeDicKeys()
        {
            m_RealVolumeDicKeys = new string[m_RealVolumes.Count];
            for (int i = 0; i < m_RealVolumes.Count; i++)
            {
                var realVolumCur = m_RealVolumes[i];
                m_RealVolumeDicKeys[i] = realVolumCur.Name;
            }

            return m_RealVolumeDicKeys;
        }

        /// <summary>
        /// 打印真实体积数据
        /// </summary>
        public void PrintRealVolumeData()
        {
            if (m_RealVolumes == null)
            {
                Debug.Log(string.Format("GridItemComponent RealVolumes is null ! "));
            }
            else
            {
                foreach (var realVolume in m_RealVolumes)
                {
                    Debug.Log($"GridItemComponent RealVolumes Datas  ---  Name:{realVolume.Name}  Size:{realVolume.BoundingBoxSize}  LocalLocation:{realVolume.BoundingBoxLocalLocation}");
                }
            }
        }
        #endregion

        /// <summary>
        /// 排序信息
        /// </summary>
        public GridItemSortInfo SortInfo
        {
            get
            {
                m_SortInfo.ViewRootTrans = ViewRootTrans; //渲染物体 根节点
                GetRealVolumeCur(out RealVolume outRealVolume);//真实体力数据
                m_SortInfo.GridCoordFloat = new GridCoordFloat(MainGridCoord) + outRealVolume.BoundingBoxLocalLocation; //网格项目世界坐标+真实体积的本地坐标
                m_SortInfo.GridItemSize = outRealVolume.BoundingBoxSize; //真实体积尺寸

                return m_SortInfo;
            }
        }
        private GridItemSortInfo m_SortInfo = new GridItemSortInfo();

        /// <summary>
        /// 显示物体的Transform
        /// </summary>
        public Transform ViewRootTrans
        {
            get
            {
                return m_ViewRootTrans;
            }
            set
            {
                m_ViewRootTrans = value;
                if (m_ViewRootTrans == null)
                    m_ViewRootGo = null;
                else
                {
                    m_ViewRootGo = m_ViewRootTrans.gameObject;
                    //m_ViewRootTrans.rotation = Quaternion.Euler(Vector3.zero); //显示物体竖立
                }
                //刷新 显示物体位置
                RefreshViewObjPosition();
            }
        }
        protected Transform m_ViewRootTrans; //显示物体Transform
        private GameObject m_ViewRootGo; //显示物体GameObject

        /// <summary>
        /// 显示物体是否可见
        /// </summary>
        public bool ViewRootIsVisible
        {
            get
            {
                if (m_ViewRootGo == null) return false;

                return m_ViewRootGo.activeInHierarchy;
            }
        }

        private bool m_EnableRefreshViewObjPosition; //开关 刷新显示物体 世界位置
        private GridCellSystemManager m_GridCellSystemManager; //网格系统管理器

        /// <summary>
        /// 设置 显示物体 是否可见
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetViewRootIsVisible(bool isVisible)
        {
            if (m_ViewRootGo == null) { return; }
            if (isVisible == m_ViewRootGo.activeSelf) { return; }

            m_ViewRootGo.SetActive(isVisible);
        }

        /// <summary>
        /// 设置 网格系统管理器
        /// </summary>
        /// <param name="gridCellSystemManager"></param>
        public void SetGridCellSystemManager(GridCellSystemManager gridCellSystemManager)
        {
            m_GridCellSystemManager = gridCellSystemManager;
        }

        /// <summary>
        /// 设置 是否更新 显示物体世界坐标
        /// </summary>
        /// <param name="isEnable"></param>
        /// <param name="refreshPosImmediate">立刻刷新 世界位置</param>
        /// <param name="gridCellSystemManager"></param>
        public void SetEnableRefreshViewObjPosition(bool isEnable)
        {
            m_EnableRefreshViewObjPosition = isEnable;

            if (m_EnableRefreshViewObjPosition)
                RefreshViewObjPosition();
        }

        /// <summary>
        /// 刷新 显示物体位置
        /// </summary>
        public void RefreshViewObjPosition()
        {
            if (MainGridCoord == GridCoord.invalid || m_ViewRootTrans == null) { return; }

            if (m_EnableRefreshViewObjPosition)
            {
                //在Aseprite中绘制并自动生成Mesh位置
                //渲染物体竖立在前面
                //float posZ = 0f;
                //if (m_GridCellSystemManager != null)
                //{
                //    var gridItemSize = GetGridItemSizeAtDirection;
                //    GetRealVolumeCur(out RealVolume outRealVolume);
                //    posZ = -(gridItemSize.Y * 0.5f - outRealVolume.BoundingBoxLocalLocation.Y) * m_GridCellSystemManager.CellUnitSizeY;
                //}
                //m_ViewRootTrans.localPosition = new Vector3(0, 0, posZ);
            }
        }

        /// <summary>
        /// 刷新 显示物体位置 ps:旧渲染排序 弃用
        /// </summary>
        /// <param name="cellCoord"></param>
        public void RefreshViewObjPosition(GridCoord gridCoord)
        {
            if (m_ViewRootTrans == null) { return; }

            var gridItemSize = GetGridItemSizeAtDirection;

            //绕中心点旋转90度后 Z轴与Y轴对调 Z轴补正
            //var posZAmendRotate = (gridItemSize.Z * GuildGridModel.Instance.CellUnitSizeZ - gridItemSize.Y * GuildGridModel.Instance.CellUnitSizeY) * 0.5f;

            //Y轴 渲染深度 根据渲染物体的世界坐标计算
            var posBottomCenter = GuildGridModel.Instance.GetGridItemSizeFrontCenterPos(gridCoord, gridItemSize);
            var posViewRoot = GuildGridModel.Instance.GetWorldPosToViewPos(posBottomCenter);

            //显示物体 世界坐标
            m_ViewRootTrans.position = new Vector3(posViewRoot.x, posViewRoot.y, posViewRoot.z);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 刷新 显示物体位置
        /// 编辑模式预览
        /// </summary>
        public void EditorRefreshViewRootPosition(GridCellSystemManager gridCellSystemManager)
        {
            if (m_ViewRootTrans == null)
            {
                Debug.Log("m_ViewObjTrans 为空，无法刷新显示结构节点位置。");
                return;
            }

            //设置 所属的网格系统管理器
            SetGridCellSystemManager(gridCellSystemManager);
            SetEnableRefreshViewObjPosition(true);
        }
#endif
        #endregion

        #region 定位功能
        #region 参数设置

        /// <summary>
        /// 开启 定位发起者
        /// </summary>
        public bool EnableLocationEmitter
        {
            set
            {
                m_EnableLocationEmitter = value;

                //添加或移除 定位发起者列表
                if (m_GridCellSystemManager != null)
                {
                    if (m_EnableLocationReceiver)
                    {
                        m_GridCellSystemManager.AddLocationEmitter(this);
                    }
                    else
                    {
                        m_GridCellSystemManager.RemoveLocationEmitter(this);
                    }
                }
            }
        }
        private bool m_EnableLocationEmitter;

        /// <summary>
        /// 开启 定位接收者
        /// </summary>
        public bool EnableLocationReceiver
        {
            set
            {
                m_EnableLocationReceiver = value;

                //添加或移除 定位接收者列表
                if (m_GridCellSystemManager != null)
                {
                    if (m_EnableLocationReceiver)
                    {
                        m_GridCellSystemManager.AddLocationReceiver(this);
                    }
                    else
                    {
                        m_GridCellSystemManager.RemoveLocationReceiver(this);
                    }
                }
            }
        }
        private bool m_EnableLocationReceiver;

        /// <summary>
        /// 定位 中心点(世界坐标)
        /// 用于相对方向计算
        /// </summary>
        public GridCoordFloat LocationCenter { get { return MainGridCoord + LocationCenterOffset; } }

        /// <summary>
        /// 定位接收者 中心点偏移(相对于网格项目的坐标点)
        /// </summary>
        public GridCoordFloat LocationCenterOffset { get; set; }

        /// <summary>
        /// 定位接收者 检测范围中心点(世界坐标)
        /// 用于检测范围计算
        /// </summary>
        public GridCoordFloat LocationReceiverRangeCenter { get { return MainGridCoord + LocationReceiverRangeCenterOffset; } }

        /// <summary>
        /// 定位接收者 检测范围中心点偏移(相对于网格项目的坐标点)
        /// </summary>
        public GridCoordFloat LocationReceiverRangeCenterOffset { get; set; }

        /// <summary>
        /// 定位接收者 检测范围
        /// </summary>
        public GridCoordFloat LocationReceiverRangeSize { get; set; }

        //字典 范围内 定位发起者信息
        private Dictionary<GridItemComponent, LocationEmitterInfo> m_DicLocationEmitterInfoInRange = new Dictionary<GridItemComponent, LocationEmitterInfo>();

        /// <summary>
        /// 设置 定位中心点
        /// </summary>
        /// <param name="centerType"></param>
        public void SetLocationCenter(ELocationCenterType centerType)
        {
            var gridCoordOffset = GridItemSize * 0.5f;
            //根据类型 修改参数
            switch (centerType)
            {
                case ELocationCenterType.UpperCenter:
                    gridCoordOffset.Z = GridItemSize.Z;
                    break;
                case ELocationCenterType.MiddleCenter:

                    break;
                case ELocationCenterType.LowerCenter:
                    gridCoordOffset.Z = 0f;
                    break;
            }

            //设置参数
            LocationCenterOffset = gridCoordOffset;
        }

        /// <summary>
        /// 设置 定位接收者 检测范围
        /// </summary>
        /// <param name="rangeType"></param>
        /// <param name="rangeLength">从定位中心点向外延伸的长度</param>
        public void SetLocationReceiverRange(ELocationReceiverRangeType rangeType, float rangeLength)
        {
            var gridCoordOffset = GridItemSize * 0.5f;
            var rangeSize = GridItemSize * 1f;

            switch (rangeType)
            {
                case ELocationReceiverRangeType.Around:
                    var lengthAmend = rangeLength * 2f;
                    rangeSize.X += lengthAmend;
                    rangeSize.Y += lengthAmend;
                    rangeSize.Z += lengthAmend;
                    break;
                case ELocationReceiverRangeType.LeftRight:
                    var lengthAmend1 = rangeLength * 2f;
                    rangeSize.X += lengthAmend1;
                    break;
                case ELocationReceiverRangeType.FrontBack:
                    var lengthAmend2 = rangeLength * 2f;
                    rangeSize.Y += lengthAmend2;
                    break;
                case ELocationReceiverRangeType.UpDown:
                    var lengthAmend3 = rangeLength * 2f;
                    rangeSize.Z += lengthAmend3;
                    break;
                case ELocationReceiverRangeType.Up:
                    gridCoordOffset.Z += rangeLength * 0.5f; //检测范围 中心点 偏移
                    rangeSize.Z = rangeLength; //检测范围 尺寸 设置长度
                    break;
                case ELocationReceiverRangeType.Down:
                    gridCoordOffset.Z += -rangeLength * 0.5f;
                    rangeSize.Z = rangeLength;
                    break;
                case ELocationReceiverRangeType.Left:
                    gridCoordOffset.X += -rangeLength * 0.5f;
                    rangeSize.X = rangeLength;
                    break;
                case ELocationReceiverRangeType.Right:
                    gridCoordOffset.X += rangeLength * 0.5f;
                    rangeSize.X = rangeLength;
                    break;
                case ELocationReceiverRangeType.Front:
                    gridCoordOffset.Y += -rangeLength * 0.5f;
                    rangeSize.Y = rangeLength;
                    break;
                case ELocationReceiverRangeType.Back:
                    gridCoordOffset.Y += rangeLength * 0.5f;
                    rangeSize.Y = rangeLength;
                    break;
            }

            //增大检测范围 误差兼容
            rangeSize += new GridCoordFloat(0.2f, 0.2f, 0.2f);

            //设置参数
            LocationReceiverRangeCenterOffset = gridCoordOffset; //检测范围 中心点
            LocationReceiverRangeSize = rangeSize; //检测范围 尺寸
        }

        #endregion

        //执行 定位系统 检测
        private void ExecuteLocationCheck()
        {
            if (m_GridCellSystemManager == null) { return; }

            //执行 定位发起者 位置检测
            if (m_EnableLocationEmitter)
                m_GridCellSystemManager.ExecuteLocationEmitterCheck(this);
        }

        /// <summary>
        /// 回调 范围内的定位发起者信息 改变
        /// </summary>
        public Action<GridItemComponent> OnLocationEmitterInfoChange { get; set; }

        /// <summary>
        /// 范围内的定位发起者信息 改变
        /// </summary>
        public void LocationEmitterInfoInRangeChange()
        {
            OnLocationEmitterInfoChange?.Invoke(this);
        }

        /// <summary>
        /// 检测 定位发起者 是否在范围内
        /// </summary>
        /// <param name="emitter"></param>
        public void CheckEmitterInRange(GridItemComponent emitter)
        {
            if (m_EnableLocationReceiver == false) { return; }

            //是否在 检测范围内
            var gridCoordRangeCenterLocal = emitter.LocationCenter - LocationReceiverRangeCenter;
            if (Math.Abs(gridCoordRangeCenterLocal.X) < LocationReceiverRangeSize.X * 0.5f &&
                Math.Abs(gridCoordRangeCenterLocal.Y) < LocationReceiverRangeSize.Y * 0.5f &&
                Math.Abs(gridCoordRangeCenterLocal.Z) < LocationReceiverRangeSize.Z * 0.5f)
            {
                //范围内
                //检测 定位发起者 在 定位接收者 的什么方向
                var gridCoordCenterLocal = emitter.LocationCenter - LocationCenter;
                var listDirection = new List<ELocationDirection>();
                listDirection.Add(gridCoordCenterLocal.X <= 0 ? ELocationDirection.Left : ELocationDirection.Right);
                listDirection.Add(gridCoordCenterLocal.Y <= 0 ? ELocationDirection.Front : ELocationDirection.Back);
                listDirection.Add(gridCoordCenterLocal.Z <= 0 ? ELocationDirection.Down : ELocationDirection.Up);

                var emitterInfo = new LocationEmitterInfo();
                emitterInfo.GridItem = emitter;
                emitterInfo.ListLocationDirection = listDirection;

                AddLocationEmitterInfo(emitterInfo);
            }
            else //范围外
                RemoveLocationEmitterInfo(emitter);
        }

        /// <summary>
        /// 添加 定位发起者信息
        /// </summary>
        /// <param name="locationEmitterInfo"></param>
        private void AddLocationEmitterInfo(LocationEmitterInfo locationEmitterInfo)
        {
            if (m_DicLocationEmitterInfoInRange.ContainsKey(locationEmitterInfo.GridItem))
                m_DicLocationEmitterInfoInRange[locationEmitterInfo.GridItem] = locationEmitterInfo;
            else
                m_DicLocationEmitterInfoInRange.Add(locationEmitterInfo.GridItem, locationEmitterInfo);

            LocationEmitterInfoInRangeChange();
        }

        /// <summary>
        /// 移除 定位发起者信息
        /// </summary>
        /// <param name="locationEmitterInfo"></param>
        private void RemoveLocationEmitterInfo(GridItemComponent gridItem)
        {
            if (m_DicLocationEmitterInfoInRange.ContainsKey(gridItem))
            {
                m_DicLocationEmitterInfoInRange.Remove(gridItem);
                LocationEmitterInfoInRangeChange();
            }
        }

        /// <summary>
        /// 检测 范围内 定位发起者信息 方向
        /// </summary>
        /// <param name="checkMode"></param>
        /// <param name="directions"></param>
        /// <returns></returns>
        public bool CheckLocationEmitterInfoDirection(ELocationCheckMode checkMode, params ELocationDirection[] directions)
        {
            bool isAgreed = false; //是否 满足条件
            foreach (var locationEmitterInfo in m_DicLocationEmitterInfoInRange.Values)
            {
                //仅有模式 方向数量不一致 直接结束
                if (checkMode == ELocationCheckMode.Only && directions.Length != locationEmitterInfo.ListLocationDirection.Count)
                {
                    isAgreed = false;
                    continue;
                }

                isAgreed = true;
                //遍历检测 所有方向
                for (int i = 0; i < directions.Length; i++)
                {
                    var direcCond = directions[i];
                    bool isContains = locationEmitterInfo.ListLocationDirection.Contains(direcCond);

                    //检测模式
                    bool checkOver = false;
                    switch (checkMode)
                    {
                        case ELocationCheckMode.Have: //有
                        case ELocationCheckMode.Only: //仅有
                            //当前检测方向不存在 直接结束
                            if (!isContains)
                            {
                                isAgreed = false;
                                checkOver = true;
                            }
                            break;
                        case ELocationCheckMode.None: //无
                            //当前检测方向存在 直接结束
                            if (isContains)
                            {
                                isAgreed = false;
                                checkOver = true;
                            }
                            break;
                        case ELocationCheckMode.Or: //或
                            //当前检测方向存在 直接结束
                            if (isContains)
                            {
                                isAgreed = true;
                                checkOver = true;
                            }
                            break;
                    }
                    if (checkOver) { break; }
                }

                //已经有定位发起者满足条件 结束遍历
                if (isAgreed)
                    break;
            }

            return isAgreed;
        }
        #endregion

        #region 区域功能

        #endregion
    }
}
