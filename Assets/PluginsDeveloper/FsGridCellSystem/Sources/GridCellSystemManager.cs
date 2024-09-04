using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 网格单元系统
/// </summary>
namespace FsGridCellSystem
{
    #region 网格项目 类型定义
    /// <summary>
    /// 网格项目 数据
    /// </summary>
    [Serializable]
    public struct GridItemData
    {
        public GridItemData(GridItemData gridItemData)
        {
            Value = gridItemData.Value;
            GridItemType = gridItemData.GridItemType;
            MainGridCoord = gridItemData.MainGridCoord;
            GridItemSize = gridItemData.GridItemSize;
            Direction = gridItemData.Direction;
        }

        /// <summary>
        /// 根据朝向 获取占用单元格尺寸
        /// </summary>
        public GridCoord GetGridItemSizeAtDirection
        {
            get
            {
                var gridItemSize = GridItemSize;
                //家具纵向时 单元格尺寸翻转
                if (Direction == EDirection.Left || Direction == EDirection.Right)
                {
                    var sizeX = gridItemSize.X;
                    gridItemSize.X = gridItemSize.Y;
                    gridItemSize.Y = sizeX;
                }

                return gridItemSize;
            }
        }

        /// <summary>
        /// 数据值
        /// </summary>
        public int Value;

        /// <summary>
        /// 类型
        /// </summary>
        public EGridItemType GridItemType;

        /// <summary>
        /// 主单元格坐标
        /// </summary>
        public GridCoord MainGridCoord;

        /// <summary>
        /// 占用单元格尺寸
        /// </summary>
        public GridCoord GridItemSize;

        /// <summary>
        /// 朝向
        /// </summary>
        public EDirection Direction;
    }

    /// <summary>
    /// 网格坐标 精确到小数
    /// </summary>
    [Serializable]
    public struct GridCoordFloat
    {
        public float X;
        public float Y;
        public float Z;

        public GridCoordFloat(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public GridCoordFloat(float x, float y)
        {
            X = x;
            Y = y;
            Z = 0f;
        }

        public GridCoordFloat(GridCoord gridCoord)
        {
            X = gridCoord.X;
            Y = gridCoord.Y;
            Z = gridCoord.Z;
        }

        public static implicit operator string(GridCoordFloat a)
        {
            return string.Format("GridCoordFloat({0},{1},{2})", a.X, a.Y, a.Z);
        }

        public override string ToString()
        {
            return string.Format("GridCoordFloat({0},{1},{2})", X, Y, Z);
        }

        public static implicit operator Vector3(GridCoordFloat a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }

        public static GridCoordFloat operator +(GridCoordFloat posA, GridCoordFloat posB)
        {
            return new GridCoordFloat(posA.X + posB.X, posA.Y + posB.Y, posA.Z + posB.Z);
        }

        public static GridCoordFloat operator +(GridCoordFloat posA, float value)
        {
            return new GridCoordFloat(posA.X + value, posA.Y + value, posA.Z + value); ;
        }

        public static GridCoordFloat operator -(GridCoordFloat posA, GridCoordFloat posB)
        {
            return new GridCoordFloat(posA.X - posB.X, posA.Y - posB.Y, posA.Z - posB.Z);
        }

        public static GridCoordFloat operator *(GridCoordFloat pos, float b)
        {
            return new GridCoordFloat(pos.X * b, pos.Y * b, pos.Z * b);
        }

        public static GridCoordFloat operator /(GridCoordFloat a, GridCoord b)
        {
            return new GridCoordFloat(a.X / b.X, a.Y / b.X, a.Z / b.Z);
        }

        public static GridCoordFloat zero { get { return new GridCoordFloat(0, 0, 0); } }
        public static GridCoordFloat one { get { return new GridCoordFloat(1, 1, 1); } }

        /// <summary>
        /// 长度 
        /// </summary>
        public float Magnitude
        {
            get
            {
                return Mathf.Abs((float)Math.Sqrt(Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2)) + Math.Pow(Z, 2)));
            }
        }

        /// <summary>
        /// 是否 负向量 (XYZ其中一个为负数)
        /// </summary>
        public bool IsMinusVector
        {
            get
            {
                return X < 0 || Y < 0 || Z < 0;
            }
        }

        /// <summary>
        /// 是否 正向量 (XYZ全部为正数)
        /// </summary>
        public bool IsPositiveVector
        {
            get
            {
                return !(X <= 0 || Y <= 0 || Z <= 0);
            }
        }
    }

    /// <summary>
    /// 网格坐标
    /// </summary>
    [Serializable]
    public struct GridCoord
    {
        public GridCoord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public GridCoord(GridCoordFloat gridCoordFloat)
        {
            X = (int)(gridCoordFloat.X + 0.001f);
            Y = (int)(gridCoordFloat.Y + 0.001f);
            Z = (int)(gridCoordFloat.Z + 0.001f);
        }

        /// <summary>
        /// 横向坐标
        /// </summary>
        public int X;
        /// <summary>
        /// 纵向坐标
        /// </summary>
        public int Y;
        /// <summary>
        /// 高度坐标
        /// </summary>
        public int Z;

        public static implicit operator string(GridCoord a)
        {
            return string.Format("GridCoord({0},{1},{2})", a.X, a.Y, a.Z);
        }

        public override string ToString()
        {
            return string.Format("GridCoord({0},{1},{2})", X, Y, Z);
        }

        public static GridCoord operator +(GridCoord a, GridCoord b)
        {
            return new GridCoord(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static GridCoord operator +(GridCoord a, int value)
        {
            return new GridCoord(a.X + value, a.Y + value, a.Z + value);
        }

        public static GridCoord operator -(GridCoord a, GridCoord b)
        {
            return new GridCoord(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static GridCoord operator *(GridCoord a, GridCoord b)
        {
            return new GridCoord(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static implicit operator Vector3(GridCoord a)
        {
            return new Vector3(a.X, a.Y, a.Z);
        }

        public static implicit operator GridCoordFloat(GridCoord a)
        {
            return new GridCoordFloat(a.X, a.Y, a.Z);
        }

        public static GridCoordFloat operator *(GridCoord a, float b)
        {
            return new GridCoordFloat(a.X * b, a.Y * b, a.Z * b);
        }

        public static GridCoordFloat operator *(GridCoord a, GridCoordFloat b)
        {
            return new GridCoordFloat(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static GridCoordFloat operator *(GridCoordFloat a, GridCoord b)
        {
            return new GridCoordFloat(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static GridCoordFloat operator /(GridCoord a, GridCoord b)
        {
            return new GridCoordFloat(a.X * 1f / b.X, a.Y * 1f / b.X, a.Z * 1f / b.Z);
        }

        public static bool operator ==(GridCoord a, GridCoord b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(GridCoord a, GridCoord b)
        {
            return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// 没有体积
        /// 当X，Y，Z任何一个值为0时，返回true
        /// </summary>
        /// <returns></returns>
        public bool isNoVolume()
        {
            return X == 0 || Y == 0 || Z == 0;
        }

        public static GridCoord zero { get { return new GridCoord(0, 0, 0); } }
        public static GridCoord one { get { return new GridCoord(1, 1, 1); } }
        public static GridCoord invalid { get { return new GridCoord(-1, -1, -1); } }
    }

    /// <summary>
    /// 网格项目类型
    /// </summary>
    public enum EGridItemType
    {
        None = 0,
        /// <summary>
        /// 主单元格
        /// </summary>
        Main = 1,
        /// <summary>
        /// 子单元格
        /// </summary>
        Sub = 2,
    }

    /// <summary>
    /// 朝向
    /// </summary>
    public enum EDirection
    {
        None = 0,
        /// <summary>
        /// 向下
        /// </summary>
        Down = 1,
        /// <summary>
        /// 向左
        /// </summary>
        Left = 2,
        /// <summary>
        /// 向上
        /// </summary>
        Up = 3,
        /// <summary>
        /// 向右
        /// </summary>
        Right = 4,
    }

    /// <summary>
    /// 检查类型
    /// </summary>
    public enum ECheckStateType
    { 
        /// <summary>
        /// 所有
        /// </summary>
        All,
        /// <summary>
        /// 任意一个
        /// </summary>
        Anyone,
    }
    #endregion

    #region 定位系统 类型定义
    /// <summary>
    /// 定位 方向
    /// </summary>
    public enum ELocationDirection
    {
        None,
        /// <summary>
        /// 上
        /// </summary>
        Up,
        /// <summary>
        /// 下
        /// </summary>
        Down,
        /// <summary>
        /// 左
        /// </summary>
        Left,
        /// <summary>
        /// 右
        /// </summary>
        Right,
        /// <summary>
        /// 前
        /// </summary>
        Front,
        /// <summary>
        /// 后
        /// </summary>
        Back,
    }

    /// <summary>
    /// 方向检测模式
    /// </summary>
    public enum ELocationCheckMode
    {
        /// <summary>
        /// 有
        /// </summary>
        Have,
        /// <summary>
        /// 没有
        /// </summary>
        None,
        /// <summary>
        /// 仅有 完全匹配
        /// </summary>
        Only,
        /// <summary>
        /// 或 有其中之一
        /// </summary>
        Or,
    }

    /// <summary>
    /// 定位发起者 信息
    /// </summary>
    public struct LocationEmitterInfo
    {
        /// <summary>
        /// 网格项目
        /// </summary>
        public GridItemComponent GridItem;

        /// <summary>
        /// 方向
        /// </summary>
        public List<ELocationDirection> ListLocationDirection;
    }

    /// <summary>
    /// 定位中心 类型
    /// </summary>
    public enum ELocationCenterType
    {
        /// <summary>
        /// 顶部中心
        /// </summary>
        UpperCenter,
        /// <summary>
        /// 中间中心
        /// </summary>
        MiddleCenter,
        /// <summary>
        /// 底部中心
        /// </summary>
        LowerCenter
    }

    /// <summary>
    /// 定位接收者 检测范围类型
    /// </summary>
    public enum ELocationReceiverRangeType
    {
        /// <summary>
        /// 环绕 全方向
        /// </summary>
        Around,
        /// <summary>
        /// 左右 X轴向 
        /// </summary>
        LeftRight,
        /// <summary>
        /// 前后 Y轴向
        /// </summary>
        FrontBack,
        /// <summary>
        /// 上下 Z轴向
        /// </summary>
        UpDown,
        /// <summary>
        /// 上
        /// </summary>
        Up,
        /// <summary>
        /// 下
        /// </summary>
        Down,
        /// <summary>
        /// 左
        /// </summary>
        Left,
        /// <summary>
        /// 右
        /// </summary>
        Right,
        /// <summary>
        /// 前
        /// </summary>
        Front,
        /// <summary>
        /// 后
        /// </summary>
        Back,
    }
    #endregion

    #region 区域功能 类型定义
    /// <summary>
    /// 区域组 信息
    /// </summary>
    public class AreaGroupInfo
    {
        public AreaGroupInfo(int id)
        {
            AreaGroupId = id;
            m_DicAreaInfo = new Dictionary<string, AreaInfo>();
        }

        /// <summary>
        /// 区域组 ID
        /// </summary>
        public int AreaGroupId;

        /// <summary>
        /// 边界坐标
        /// </summary>
        public GridCoordFloat BoundGridCoord;

        /// <summary>
        /// 边界尺寸
        /// </summary>
        public GridCoordFloat BoundSize;

        /// <summary>
        /// 检查 在边界内
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public bool CheckInBound(GridCoordFloat gridCoord)
        {
            //是否处于边界坐标内
            var gridCoordDiff = gridCoord - BoundGridCoord;
            if (gridCoordDiff.IsMinusVector)
                return false;

            //是否处于边界尺寸内
            var sizeDiff = BoundSize - gridCoordDiff;
            if (!sizeDiff.IsPositiveVector)
                return false;

            return true;
        }

        /// <summary>
        /// 列表 区域信息
        /// </summary>
        public Dictionary<string, AreaInfo> DicAreaInfo { get { return m_DicAreaInfo; } }
        private Dictionary<string, AreaInfo> m_DicAreaInfo;

        /// <summary>
        /// 添加 区域信息
        /// </summary>
        /// <param name="areaInfoKey"></param>
        /// <param name="areaInfo"></param>
        /// <param name="isInherits"></param>
        public void AddAreaInfo(string areaInfoKey, AreaInfo areaInfo, bool isInherits = true)
        {
            bool isContains = m_DicAreaInfo.ContainsKey(areaInfoKey);
            if (!isInherits && isContains) { return; }

            //继承数据
            AreaInfo areaInfoCur = null;
            if (isInherits && m_DicAreaInfo.TryGetValue(areaInfoKey, out areaInfoCur))
            {
                areaInfo.Value = areaInfoCur.Value;
                areaInfo.DicIntraGridItem = areaInfoCur.DicIntraGridItem;
            }

            RemoveAreaInfo(areaInfoKey); //移除 当前区域信息
            m_DicAreaInfo.Add(areaInfoKey, areaInfo); //记录 新区域信息

            //更新 区域组边界
            if (m_DicAreaInfo.Count == 1)
            {
                BoundGridCoord = areaInfo.GridCoord;
                BoundSize = areaInfo.Size;
            }
            else
                AddAreaGroupBound(areaInfo);
        }

        /// <summary>
        /// 移除 区域信息
        /// </summary>
        /// <param name="areaInfoKey"></param>
        public void RemoveAreaInfo(string areaInfoKey)
        {
            AreaInfo areaInfo = null;
            if (!m_DicAreaInfo.TryGetValue(areaInfoKey, out areaInfo)) { return; }
            m_DicAreaInfo.Remove(areaInfoKey);

            //更新 区域组边界
            if (m_DicAreaInfo.Count == 0)
            {
                BoundGridCoord = new GridCoordFloat();
                BoundSize = new GridCoordFloat();
            }
            else
                RemoveAreaGroupBound(areaInfo);
        }

        /// <summary>
        /// 检查 在哪个区域内
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public AreaInfo CheckInAreaBound(GridCoordFloat gridCoord)
        {
            foreach (var areaInfo in m_DicAreaInfo.Values)
            {
                //是否处于边界坐标内
                if (areaInfo.CheckInBound(gridCoord))
                    return areaInfo;
            }

            return null;
        }

        /// <summary>
        /// 添加 区域组边界
        /// </summary>
        /// <param name="areaInfo"></param>
        private void AddAreaGroupBound(AreaInfo areaInfo)
        {
            //更新边界坐标
            var gridCoordDiff = areaInfo.GridCoord - BoundGridCoord;
            if (gridCoordDiff.IsMinusVector)
            {
                if (gridCoordDiff.X < 0)
                    BoundGridCoord.X += gridCoordDiff.X;
                if (gridCoordDiff.Y < 0)
                    BoundGridCoord.Y += gridCoordDiff.Y;
                if (gridCoordDiff.Z < 0)
                    BoundGridCoord.Z += gridCoordDiff.Z;
            }
            //更新 边界尺寸
            var sizeDiff = BoundSize - (gridCoordDiff + areaInfo.Size);
            if (!sizeDiff.IsPositiveVector)
            {
                if (sizeDiff.X < 0)
                    BoundSize.X -= sizeDiff.X;
                if (sizeDiff.Y < 0)
                    BoundSize.Y -= sizeDiff.Y;
                if (sizeDiff.Z < 0)
                    BoundSize.Z -= sizeDiff.Z;
            }
        }

        /// <summary>
        /// 移除 区域组边界
        /// </summary>
        /// <param name="areaInfo"></param>
        private void RemoveAreaGroupBound(AreaInfo areaInfo)
        {
            bool needInitRefresh = false;
            //检查边界坐标
            var gridCoordDiff = areaInfo.GridCoord - BoundGridCoord;
            if (gridCoordDiff.X == 0 || gridCoordDiff.Y == 0 || gridCoordDiff.Z == 0)
            {
                needInitRefresh = true;
            }
            //检查边界尺寸
            if (!needInitRefresh)
            {
                var sizeDiff = BoundSize - (gridCoordDiff + areaInfo.Size);
                if (sizeDiff.X == 0 || sizeDiff.Y == 0 || sizeDiff.Z == 0)
                {
                    needInitRefresh = true;
                }
            }

            //重更新 边界坐标与尺寸
            if (needInitRefresh)
            {
                int index = 0;
                foreach (var areaInfoCur in m_DicAreaInfo.Values)
                {
                    if (index == 0)
                    {
                        BoundGridCoord = areaInfoCur.GridCoord;
                        BoundSize = areaInfoCur.Size;
                        continue;
                    }

                    AddAreaGroupBound(areaInfoCur);
                    index++;
                }
            }
        }
    }

    /// <summary>
    /// 区域信息
    /// </summary>
    public class AreaInfo
    {
        public AreaInfo(string keyName, GridCoord gridCoord, GridCoord size)
        {
            KeyName = keyName;
            GridCoord = gridCoord;
            Size = size;
        }

        /// <summary>
        /// 所在区域组的ID
        /// </summary>
        public int AreaGroupId;

        /// <summary>
        /// 名称 唯一标识
        /// </summary>
        public string KeyName;

        /// <summary>
        /// 值（配置表ID）
        /// </summary>
        public int Value 
        {
            get { return m_Value; }
            set
            {
                if (m_Value == value) { return; }

                m_Value = value;
                OnValueChange?.Invoke(m_Value);
            }
        }
        private int m_Value;

        /// <summary>
        /// 坐标
        /// </summary>
        public GridCoord GridCoord;

        /// <summary>
        /// 占用尺寸
        /// </summary>
        public GridCoord Size;

        /// <summary>
        /// 回调 值改变
        /// </summary>
        public Action<int> OnValueChange { get; set; }

        /// <summary>
        /// 检查 是否在区域内
        /// </summary>
        /// <param name="gridCoord"></param>
        public bool CheckInBound(GridCoordFloat gridCoord)
        {
            //是否处于边界坐标内
            var gridCoordDiff = gridCoord - GridCoord;
            if (gridCoordDiff.IsMinusVector)
                return false;

            //是否处于边界尺寸内
            var sizeDiff = Size - gridCoordDiff;
            if (!sizeDiff.IsPositiveVector)
                return false;

            return true;
        }

        #region 内部网格项目
        /// <summary>
        /// 内部网格项目
        /// </summary>
        public Dictionary<int, List<GridItemComponent>> DicIntraGridItem { get { return m_DicIntraGridItem; } set { m_DicIntraGridItem = value; } }
        private Dictionary<int, List<GridItemComponent>> m_DicIntraGridItem = new Dictionary<int, List<GridItemComponent>>();

        /// <summary>
        /// 回调 内部网格项目改变
        /// </summary>
        public Action<int> OnListIntraGridItemChange { get; set; }

        /// <summary>
        /// 添加 内部网格项目
        /// </summary>
        /// <param name="gridItem"></param>
        public void AddIntraGridItem(GridItemComponent gridItem, int key = -1)
        {
            var list = GetDicIntraGridItemList(key);
            if (list.Contains(gridItem)) { return; }

            //检查 是否处于区域内
            if (!CheckInBound(gridItem.MainGridCoord)) return;

            //记录 网格项目
            list.Add(gridItem);
            //设置网格项目显示状态
            gridItem.SetViewRootIsVisible(m_IsVisible);

            OnListIntraGridItemChange?.Invoke(key);
        }

        /// <summary>
        /// 移除 内部网格项目
        /// </summary>
        /// <param name="gridItem"></param>
        public void RemoveIntraGridItem(GridItemComponent gridItem, int key = -1)
        {
            var list = GetDicIntraGridItemList(key);
            if (!list.Contains(gridItem)) { return; }

            //移除 网格项目
            list.Remove(gridItem);
            //设置网格项目显示状态
            gridItem.SetViewRootIsVisible(true);

            OnListIntraGridItemChange?.Invoke(key);
        }

        /// <summary>
        /// 获取 内部网格项目列表 数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetIntraGridItemListCount(int key)
        {
            int count = 0;

            List<GridItemComponent> list = null;
            if (m_DicIntraGridItem.TryGetValue(key, out list))
                count = list.Count;

            return count;
        }

        //获取 网格项目列表
        private List<GridItemComponent> GetDicIntraGridItemList(int key)
        {
            List<GridItemComponent> list = null;
            if (!m_DicIntraGridItem.TryGetValue(key, out list))
            {
                list = new List<GridItemComponent>();
                m_DicIntraGridItem.Add(key, list);
            }

            return list;
        }

        private bool m_IsVisible = true; //可见状态

        /// <summary>
        /// 设置 可见状态
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetVisibleState(bool isVisible)
        {
            if (m_IsVisible == isVisible) { return; }
            m_IsVisible = isVisible;

            foreach (var list in m_DicIntraGridItem.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetViewRootIsVisible(isVisible);
                }
            }
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// 网格项目 排序信息
    /// </summary>
    [Serializable]
    public class GridItemSortInfo
    {
        /// <summary>
        /// 渲染物体 根节点
        /// </summary>
        public Transform ViewRootTrans;

        /// <summary>
        /// 网格坐标 精确至小数
        /// </summary>
        public GridCoordFloat GridCoordFloat;

        /// <summary>
        /// 占地尺寸 精确至小数
        /// </summary>
        public GridCoordFloat GridItemSize;
    }

    public class GridCellSystemManager
    {
        private GridItemComponent[][,,] m_GridItemArray; //网格 层组

        /// <summary>
        /// 单元格世界坐标尺寸 X
        /// </summary>
        public float CellUnitSizeX{ get { return m_CellUnitSizeX; } }
        private float m_CellUnitSizeX; //单格尺寸
        /// <summary>
        /// 单元格世界坐标尺寸 Y
        /// </summary>
        public float CellUnitSizeY { get { return m_CellUnitSizeY; } }
        private float m_CellUnitSizeY; //单格尺寸
        /// <summary>
        /// 单元格世界坐标尺寸 Z
        /// </summary>
        public float CellUnitSizeZ { get { return m_CellUnitSizeZ; } }
        private float m_CellUnitSizeZ; //单格高度

        private int m_GridCellCountX; //X轴 网格数量
        private int m_GridCellCountY; //Y轴 网格数量
        private int m_GridCellCountZ; //Z轴 网格数量
        private int m_LayerCount; //网格 层数

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cellUnitSizeX">单元格尺寸 X长度</param>
        /// <param name="cellUnitSizeY">单元格尺寸 Y长度</param>
        /// <param name="cellUnitSizeZ">单元格尺寸 Z长度</param>
        /// <param name="gridCellCountX">网格的单元格数量 X轴</param>
        /// <param name="gridCellCountY">网格的单元格数量 Y轴</param>
        /// <param name="gridCellCountZ">网格的单元格数量 Z轴</param>
        /// <param name="layerCount">网格层数</param>
        public void Init(float cellUnitSizeX = 0.1f, float cellUnitSizeY = 0.1f, float cellUnitSizeZ = 0.1f, int gridCellCountX = 1, int gridCellCountY = 1, int gridCellCountZ = 1, int layerCount = 1)
        {
            m_CellUnitSizeX = cellUnitSizeX;
            m_CellUnitSizeY = cellUnitSizeY;
            m_CellUnitSizeZ = cellUnitSizeZ;
            m_GridCellCountX = gridCellCountX;
            m_GridCellCountY = gridCellCountY;
            m_GridCellCountZ = gridCellCountZ;
            m_LayerCount = layerCount;

            //初始化 网格 层组
            m_GridItemArray = new GridItemComponent[m_LayerCount][,,];
            for (int i = 0; i < m_LayerCount; i++)
            {
                var gridItemArray = new GridItemComponent[m_GridCellCountX, m_GridCellCountY, m_GridCellCountZ];
                m_GridItemArray[i] = gridItemArray;
                //for (int x = 0; x < gridItemArray.GetLength(0); x++)
                //{
                //    for (int y = 0; y < gridItemArray.GetLength(1); y++)
                //    {
                //        for (int z = 0; z < gridItemArray.GetLength(2); z++)
                //        {
                //            gridItemArray[x, y, z] = new GridItemComponent();
                //        }
                //    }
                //}
            }
        }

        #region 网格操作
        /// <summary>
        /// 获取 网格项目组
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public GridItemComponent[,,] GetGridItemArray(int layer)
        {
            if (layer < m_GridItemArray.Length)
                return m_GridItemArray[layer];
            else
                return null;
        }

        /// <summary>
        /// 获取 网格项目
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="isNullCreateNew"></param>
        /// <returns></returns>
        private GridItemComponent OnGetGridItem(int layer, GridCoord gridCoord, bool isNullCreateNew = true)
        {
            var gridItem = m_GridItemArray[layer][gridCoord.X, gridCoord.Y, gridCoord.Z];
            if (isNullCreateNew && gridItem == null)
            {
                gridItem = new GridItemComponent();
                m_GridItemArray[layer][gridCoord.X, gridCoord.Y, gridCoord.Z] = gridItem;
            }

            return gridItem;
        }

        /// <summary>
        /// 获取 单元格
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="isNullCreateNew"></param>
        /// <returns></returns>
        public GridItemComponent GetGridItem(int layer, GridCoord gridCoord, bool isNullCreateNew = false)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return null; }
            var gridItem = OnGetGridItem(layer, gridCoord, isNullCreateNew);

            return gridItem;
        }

        /// <summary>
        /// 设置 单元格
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="gridItemData"></param>
        /// <param name="isReplace">是否替换</param>
        /// <returns>是否 设置成功</returns>
        public bool SetGridItem(int layer, GridCoord gridCoord, GridItemData gridItemData)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return false; }

            var gridItemCur = OnGetGridItem(layer, gridCoord);
            gridItemCur.Value = gridItemData.Value;
            gridItemCur.Direction = gridItemData.Direction;
            gridItemCur.GridItemType = gridItemData.GridItemType;
            gridItemCur.MainGridCoord = gridItemData.MainGridCoord;
            gridItemCur.GridItemSize = gridItemData.GridItemSize;

            return true;
        }

        /// <summary>
        /// 设置 单元格状态
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public bool SetGridItemState(int layer, GridCoord gridCoord, int state, bool isTrue)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return false; }

            var gridItemCur = OnGetGridItem(layer, gridCoord);
            if (isTrue)
                gridItemCur.AddState(state); //增加状态
            else
                gridItemCur.RemoveState(state); //移除状态

            return true;
        }

        /// <summary>
        /// 设置 单元格状态
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="state"></param>
        /// <param name="isTrue"></param>
        /// <param name="gridItemSize"></param>
        /// <returns></returns>
        public bool SetGridItemState(int layer, GridCoord gridCoord, GridCoord gridItemSize, int state, bool isTrue)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return false; }

            for (int gridCoordXadd = 0; gridCoordXadd < gridItemSize.X; gridCoordXadd++)
            {
                for (int gridCoordYadd = 0; gridCoordYadd < gridItemSize.Y; gridCoordYadd++)
                {
                    for (int gridCoordZadd = 0; gridCoordZadd < gridItemSize.Z; gridCoordZadd++)
                    {
                        SetGridItemState(layer, gridCoord + new GridCoord(gridCoordXadd, gridCoordYadd, gridCoordZadd), state, isTrue);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取 主单元格 数值
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public int GetMainGridItemValue(int layer, GridCoord gridCoord)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return 0; }

            var gridItem = GetMainGridItem(layer, gridCoord);
            if (gridItem == null)
            {
                return 0;
            }
            else
            {
                return gridItem.Value;
            }
        }

        /// <summary>
        /// 弹出 主单元格 数值
        /// 会移除网格主单元格上的数值
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public GridItemComponent PopMainGridItemValue(int layer, GridCoord gridCoord)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return new GridItemComponent(); }

            var gridItem = GetMainGridItem(layer, gridCoord);
            if (gridItem == null || gridItem.Value == 0)
            {
                return new GridItemComponent();
            }
            else
            {
                var gridItemReturn = new GridItemComponent(gridItem);
                RemoveMainGridItemData(layer, gridCoord);
                return gridItemReturn;
            }
        }

        /// <summary>
        /// 压入 主单元格 数值
        /// 如果当前单元格已有数值 不会修改单元格数值
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridItemData"></param>
        /// <returns>是否 压入成功</returns>
        public bool PushMainGridItemData(int layer, GridItemData gridItemData)
        {
            if (CheckGridCoordIsInvalid(layer, gridItemData.MainGridCoord)) { return false; }

            //检查 物体尺寸单元格内 是否无冲突
            if (CheckGridItemSizeHasGirdItem(layer, gridItemData.MainGridCoord, gridItemData.GetGridItemSizeAtDirection)) { return false; }

            //设置 主单元格 数值
            SetMainGridItemValue(layer, gridItemData);
            return true;
        }

        /// <summary>
        /// 设置 主单元格 数值
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridItemData"></param>
        /// <returns>是否 设置成功</returns>
        public bool SetMainGridItemValue(int layer, GridItemData gridItemData)
        {
            if (CheckGridCoordIsInvalid(layer, gridItemData.MainGridCoord)) { return false; }

            var gridItemCur = OnGetGridItem(layer, gridItemData.MainGridCoord);
            if (gridItemCur.Value == gridItemData.Value) { return false; }

            //移除 旧数值
            RemoveMainGridItemData(layer, gridItemData.MainGridCoord);

            //根据物体尺寸 设置附属的子单元格
            var gridItemSize = gridItemData.GridItemSize;
            var gridItemDataSub = new GridItemData(gridItemData); //子单元格数据
            gridItemDataSub.Value = gridItemData.Value;
            gridItemDataSub.GridItemType = EGridItemType.Sub;
            gridItemDataSub.Direction = EDirection.None;
            
            for (int gridCoordXadd = 0; gridCoordXadd < gridItemSize.X; gridCoordXadd++)
            {
                for (int gridCoordYadd = 0; gridCoordYadd < gridItemSize.Y; gridCoordYadd++)
                {
                    for (int gridCoordZadd = 0; gridCoordZadd < gridItemSize.Z; gridCoordZadd++)
                    {
                        var gridCoordSub = gridItemData.MainGridCoord + new GridCoord(gridCoordXadd, gridCoordYadd, gridCoordZadd);
                        SetGridItem(layer, gridCoordSub, gridItemDataSub);
                    }
                }
            }

            //设置主单元格
            SetGridItem(layer, gridItemData.MainGridCoord, gridItemData);

            return true;
        }

        /// <summary>
        /// 移除 主单元格 数值
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public bool RemoveMainGridItemData(int layer, GridCoord gridCoord)
        {
            if (CheckGridCoordIsInvalid(layer, gridCoord)) { return false; }

            var gridItemCur = GetMainGridItem(layer, gridCoord);
            if (gridItemCur == null || gridItemCur.Value == 0) { return false; }

            //根据尺寸 移除主单元格与所有子单元格
            var mainGridCoord = gridItemCur.MainGridCoord;
            var gridItemSize = gridItemCur.GridItemSize;
            gridItemCur.GridItemData = new GridItemData();

            for (int gridCoordXadd = 0; gridCoordXadd < gridItemSize.X; gridCoordXadd++)
            {
                for (int gridCoordYadd = 0; gridCoordYadd < gridItemSize.Y; gridCoordYadd++)
                {
                    for (int gridCoordZadd = 0; gridCoordZadd < gridItemSize.Z; gridCoordZadd++)
                    {
                        var gridCoordSub = mainGridCoord + new GridCoord(gridCoordXadd, gridCoordYadd, gridCoordZadd);
                        SetGridItem(layer, gridCoordSub, gridItemCur.GridItemData);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取 主单元格
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public GridItemComponent GetMainGridItem(int layer, GridCoord gridCoord)
        {
            var gridItem = GetGridItem(layer, gridCoord);
            if (gridItem != null && gridItem.GridItemType == EGridItemType.Sub)
            {
                //单元格为子单元格时 获取隶属的主单元格
                gridItem = GetGridItem(layer, gridItem.MainGridCoord);
            }

            return gridItem;
        }

        /// <summary>
        /// 检查 网格物体尺寸单元格内 是否有网格物体
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="gridItemSize"></param>
        /// <returns></returns>
        public bool CheckGridItemSizeHasGirdItem(int layer, GridCoord gridCoord, GridCoord gridItemSize)
        {
            for (int gridCoordXadd = 0; gridCoordXadd < gridItemSize.X; gridCoordXadd++)
            {
                for (int gridCoordYadd = 0; gridCoordYadd < gridItemSize.Y; gridCoordYadd++)
                {
                    for (int gridCoordZadd = 0; gridCoordZadd < gridItemSize.Z; gridCoordZadd++)
                    {
                        var gridItem = GetGridItem(layer, gridCoord + new GridCoord(gridCoordXadd, gridCoordYadd, gridCoordZadd));
                        if (gridItem != null && gridItem.Value != 0) return true;
                    }
                }
            }

            return false;
        }

        //检查 单元格坐标是否无效
        private bool CheckGridCoordIsInvalid(int layer, GridCoord gridCoord)
        {
            bool invalid = layer >= m_GridItemArray.Length || gridCoord.X >= m_GridCellCountX || gridCoord.Y >= m_GridCellCountY || gridCoord.Z >= m_GridCellCountZ || gridCoord.X < 0 || gridCoord.Y < 0 || gridCoord.Z < 0;
            return invalid;
        }
        #endregion

        #region 状态检查
        /// <summary>
        /// 检查 网格项目尺寸单元格内 状态
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="gridCoord"></param>
        /// <param name="gridItemSize"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool CheckGridItemSizeState(int layer, GridCoord gridCoord, GridCoord gridItemSize, int state, ECheckStateType checkType)
        {
            switch (checkType)
            {
                case ECheckStateType.All:
                    for (int xAdd = 0; xAdd < gridItemSize.X; xAdd++)
                    {
                        for (int yAdd = 0; yAdd < gridItemSize.Y; yAdd++)
                        {
                            for (int zAdd = 0; zAdd < gridItemSize.Z; zAdd++)
                            {
                                var gridItem = GetGridItem(layer, gridCoord + new GridCoord(xAdd, yAdd, zAdd));
                                bool hasState = gridItem != null && gridItem.CheckState(state);
                                if (hasState == false) return false;
                            }
                        }
                    }
                    return true;
                case ECheckStateType.Anyone:
                    for (int xAdd = 0; xAdd < gridItemSize.X; xAdd++)
                    {
                        for (int yAdd = 0; yAdd < gridItemSize.Y; yAdd++)
                        {
                            for (int zAdd = 0; zAdd < gridItemSize.Z; zAdd++)
                            {
                                var gridItem = GetGridItem(layer, gridCoord + new GridCoord(xAdd, yAdd, zAdd));
                                bool hasState = gridItem != null && gridItem.CheckState(state);
                                if (hasState == true) return true;
                            }
                        }
                    }
                    return false;
            }

            return false;
        }
        #endregion

        #region 坐标转换
        /// <summary>
        /// 获取 单元格的世界坐标
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public GridCoordFloat GetWorldPosition(GridCoord gridCoord)
        {
            GridCoordFloat position = new GridCoordFloat();

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            position.X = gridCoord.X * m_CellUnitSizeX;
            position.Y = gridCoord.Y * m_CellUnitSizeY;
            position.Z = gridCoord.Z * m_CellUnitSizeZ;

            return position;
        }

        /// <summary>
        /// 获取 单元格的世界坐标
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public GridCoordFloat GetWorldPosition(GridCoordFloat gridCoord)
        {
            GridCoordFloat position = new GridCoordFloat();

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            position.X = gridCoord.X * m_CellUnitSizeX;
            position.Y = gridCoord.Y * m_CellUnitSizeY;
            position.Z = gridCoord.Z * m_CellUnitSizeZ;

            return position;
        }

        /// <summary>
        /// 获取 世界坐标所在单元格坐标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public GridCoord GetGridCoord(GridCoordFloat worldPosition)
        {
            //float值不稳定 增加偏移 防止因处于临界值造成的浮动偏差
            worldPosition += new GridCoordFloat(0.0001f, 0.0001f, 0.0001f);

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            int x = (int)Math.Floor(worldPosition.X / m_CellUnitSizeX);
            int y = (int)Math.Floor(worldPosition.Y / m_CellUnitSizeY);
            int z = (int)Math.Floor(worldPosition.Z / m_CellUnitSizeZ);

            return new GridCoord(x, y, z);
        }

        /// <summary>
        /// 获取 世界坐标所在单元格坐标 精确到小数
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public GridCoordFloat GetGridCoordFloat(GridCoordFloat worldPosition)
        {
            //float值不稳定 增加偏移 防止因处于临界值造成的浮动偏差
            worldPosition += new GridCoordFloat(0.0001f, 0.0001f, 0.0001f);

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            float x = worldPosition.X / m_CellUnitSizeX;
            float y = worldPosition.Y / m_CellUnitSizeY;
            float z = worldPosition.Z / m_CellUnitSizeZ;

            return new GridCoordFloat(x, y, z);
        }

        /// <summary>
        /// 获取 三维网格坐标 转 二维显示坐标
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public GridCoordFloat GetGridCoordToViewPos(GridCoord gridCoord)
        {
            GridCoordFloat position = new GridCoordFloat();

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            position.X = gridCoord.X * m_CellUnitSizeX;
            position.Y = gridCoord.Y * m_CellUnitSizeY;
            var posZ = gridCoord.Z * m_CellUnitSizeZ;
            position.Y += posZ; //渲染空间只有二维 高度在Y值上进行增加

            //Z值 用于区分渲染顺序 Y值越大越靠后
            position.Z = -gridCoord.Y * m_CellUnitSizeY + posZ; //加上Z轴高度 高的遮住低的

            return position;
        }

        /// <summary>
        /// 获取 三维世界坐标 转 二维显示坐标
        /// </summary>
        /// <param name="gridPos"></param>
        /// <returns></returns>
        public GridCoordFloat GetWorldPosToViewPos(GridCoordFloat gridPos)
        {
            GridCoordFloat position = new GridCoordFloat();

            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            position.X = gridPos.X;
            position.Y = gridPos.Y + gridPos.Z; //渲染空间只有二维 高度在Y值上进行增加

            //Z值 用于区分渲染顺序
            position.Z = -gridPos.Y;// + gridPos.Z / (m_CellUnitSizeZ * m_GridCellCountZ);

            return position;
        }

        /// <summary>
        /// 获取 二维显示坐标 转 三维世界坐标
        /// </summary>
        /// <param name="gridPos">Z值为给定的目标高度</param>
        /// <returns></returns>
        public GridCoordFloat GetViewPosToWorldPos(GridCoordFloat gridPos)
        {
            GridCoordFloat position = new GridCoordFloat();
            
            //网格系统中 高度是Z轴。Unity中 高度是Y轴
            position.X = gridPos.X;
            position.Y = gridPos.Y - gridPos.Z; //渲染空间只有二维 Y轴是 高度与宽度的和
            position.Z = gridPos.Z;

            return position;
        }
        #endregion

        #region 渲染层级排序 ps:弃用的场景渲染方案
        private List<GridItemSortInfo> m_ListViewSortGridItem = new List<GridItemSortInfo>();

        /// <summary>
        /// 添加 网格项目的渲染排序信息
        /// </summary>
        /// <param name="sortInfo"></param>
        public void AddGridItemSortInfo(GridItemSortInfo sortInfo, bool checkRepeatLog = false)
        {
            if (sortInfo.ViewRootTrans == null) { return; }
            //移除 重复的网格项目
            RemoveGridItemSortInfo(sortInfo);

            GridItemSortInfo sortInfoLast = sortInfo; //上一个 队列中的 网格项目
            GridItemSortInfo sortInfoCur = sortInfo; //下一个 队列中的 网格项目
            GridCoordFloat gridCoordTar = sortInfo.GridCoordFloat; //需要排序的 网格项目坐标
            GridCoordFloat gridCoordCur = GridCoordFloat.zero; //队列中的 网格项目坐标
            int indexInsert = m_ListViewSortGridItem.Count; //需要排序的 网格项目 在队列中的位置
            float posSortFront = -sortInfo.GridCoordFloat.Z * CellUnitSizeZ; //前方物体的 排序坐标值
            float posSortBack = posSortFront; //前方物体的 排序坐标值
            bool isFront = true;

            //设置默认的深度值
            var posOriSortInfoCur = sortInfoCur.ViewRootTrans.position;
            sortInfoCur.ViewRootTrans.position = new Vector3(posOriSortInfoCur.x, posSortFront, posOriSortInfoCur.z);

            //比较排序
            for (int i = m_ListViewSortGridItem.Count - 1; i >= 0; i--)
            {
                var gridItemCheck = m_ListViewSortGridItem[i]; //获取 当前的 网格项目
                
                //移除 无效的网格项目
                if (gridItemCheck == sortInfo || gridItemCheck.ViewRootTrans == null)
                {
                    m_ListViewSortGridItem.RemoveAt(i);
                    indexInsert--;
                    continue;
                }

                sortInfoLast = sortInfoCur; //记录 上一个 网格项目
                sortInfoCur = gridItemCheck; //记录 当前的 网格项目
                gridCoordCur = sortInfoCur.GridCoordFloat;

                //检查 主坐标重复
                if (checkRepeatLog)
                {
                    if ((gridCoordTar - gridCoordCur).Magnitude < 0.01f)
                    {
                        Debug.LogError($"GridCellSystemManager.AddViewSortGridItem() Error! >> 重叠的主坐标网格项目！GameObjectName1-{sortInfoCur.ViewRootTrans.parent.name} GameObjectName2-{sortInfo.ViewRootTrans.parent.name}");
                    }
                }

                //根据坐标与尺寸 判断前后关系
                if(gridCoordTar.Y == gridCoordCur.Y && gridCoordTar.Z == gridCoordCur.Z)
                {
                    if (sortInfo.GridItemSize.Z >= sortInfoCur.GridItemSize.Z) //占用尺寸更高
                        isFront = true; //在前方
                    else
                        isFront = false; //在后方
                }
                else if (gridCoordTar.Y <= gridCoordCur.Y && gridCoordTar.Z >= gridCoordCur.Z)
                {
                    if ((gridCoordTar.X >= gridCoordCur.X && gridCoordCur.X + sortInfoCur.GridItemSize.X - gridCoordTar.X > 0.2f) ||
                        (gridCoordTar.X <= gridCoordCur.X && gridCoordTar.X + sortInfo.GridItemSize.X - gridCoordCur.X > 0.2f))
                        isFront = true; //占用尺寸重叠 重叠长度大于0.2个单元格
                    else if (sortInfo.GridItemSize.Z >= sortInfoCur.GridItemSize.Z)
                        isFront = true; //占用尺寸更高
                    else
                        isFront = false; //在后方
                }
                else if (gridCoordTar.Y >= gridCoordCur.Y && gridCoordTar.Z < gridCoordCur.Z)
                {
                    if (gridCoordTar.Y <= gridCoordCur.Y + sortInfoCur.GridItemSize.Y &&
                        gridCoordTar.Z <= gridCoordCur.Z && gridCoordTar.Z + sortInfo.GridItemSize.Z >= gridCoordCur.Z + sortInfoCur.GridItemSize.Z)
                        isFront = true; //当前的网格项目 内嵌至 需要排序的网格项目
                    else
                        isFront = false;
                }
                else if (gridCoordTar.Y < gridCoordCur.Y && gridCoordTar.Z < gridCoordCur.Z)
                {
                    if (gridCoordTar.Y + sortInfo.GridItemSize.Y <= gridCoordCur.Y)
                        isFront = true;
                    else if(gridCoordTar.Z <= gridCoordCur.Z && gridCoordTar.Z + sortInfo.GridItemSize.Z >= gridCoordCur.Z + sortInfoCur.GridItemSize.Z)
                        isFront = true; //当前的网格项目 内嵌至 需要排序的网格项目
                    else
                        isFront = false;
                }
                else //if(coordTar.Y > coordCur.Y && coordTar.Z > coordCur.Z)
                {
                    if (gridCoordTar.Y >= gridCoordCur.Y + sortInfoCur.GridItemSize.Y)
                        isFront = false;
                    else if(gridCoordTar.Z >= gridCoordCur.Z && gridCoordTar.Z + sortInfo.GridItemSize.Z < gridCoordCur.Z + sortInfoCur.GridItemSize.Z)
                        isFront = false;  //需要排序的网格项目 内嵌至 当前的网格项目
                    else
                        isFront = true;
                }

                //记录 前后网格项目的排序坐标值
                posSortBack = sortInfoCur.ViewRootTrans.position.y;
                posSortFront = sortInfoLast.ViewRootTrans.position.y;

                //比较排序结束
                if (isFront)
                {
                    break;
                }

                indexInsert--; //插入下标 前移
            }

            //边界情况 最前或最后
            //距离小于0.001f
            if (indexInsert == 0 && (posSortBack - posSortFront) < 0.04f)
            {
                //排在最后
                posSortFront = posSortBack - 0.2f;
            }
            else if (indexInsert >= m_ListViewSortGridItem.Count && (posSortFront - posSortBack) < 0.04f)
            {
                //排在最前
                posSortFront = posSortBack + 0.2f;
            }

            //设置显示物体的渲染深度坐标 插到前后网格项目的中间
            var posOri = sortInfo.ViewRootTrans.position;
            sortInfo.ViewRootTrans.position = new Vector3(posOri.x, (posSortFront + posSortBack) * 0.5f, posOri.z);

            //插入队列
            m_ListViewSortGridItem.Insert(indexInsert, sortInfo);

            //渲染物体高度大于0 || 队列中世界位置过于密集时 重新等距排序
            if (sortInfo.ViewRootTrans.position.y > 0f || Math.Abs(posSortFront - posSortBack) < 0.001f)
            {
                for (int i = m_ListViewSortGridItem.Count - 1; i >= 0; i--)
                {
                    var viewObjTrans = m_ListViewSortGridItem[i].ViewRootTrans;
                    posOri = viewObjTrans.position;
                    viewObjTrans.position = new Vector3(posOri.x, (m_ListViewSortGridItem.Count - i) * -0.2f - 10f, posOri.z);
                }
            }
        }

        /// <summary>
        /// 移除 网格项目的渲染排序信息
        /// </summary>
        /// <param name="sortInfo"></param>
        public void RemoveGridItemSortInfo(GridItemSortInfo sortInfo)
        {
            m_ListViewSortGridItem.Remove(sortInfo);
        }

        /// <summary>
        /// 清除 所有渲染层级排序项目
        /// </summary>
        public void ClearAllViewSortGridItem()
        {
            m_ListViewSortGridItem.Clear();
        }
        #endregion

        #region 定位功能
        private List<GridItemComponent> m_ListLocationEmitter = new List<GridItemComponent>(); //列表 定位发起者
        private List<GridItemComponent> m_ListLocationReceiver = new List<GridItemComponent>(); //列表 定位接收者

        /// <summary>
        /// 添加 定位发起者
        /// </summary>
        /// <param name="gridItem"></param>
        public void AddLocationEmitter(GridItemComponent gridItem)
        {
            if (m_ListLocationEmitter.Contains(gridItem)) { return; }

            m_ListLocationEmitter.Add(gridItem);

            //立刻执行一次 定位检查
            ExecuteLocationEmitterCheck(gridItem);
        }

        /// <summary>
        /// 移除 定位发起者
        /// </summary>
        /// <param name="gridItem"></param>
        public void RemoveLocationEmitter(GridItemComponent gridItem)
        {
            if (!m_ListLocationEmitter.Contains(gridItem)) { return; }

            m_ListLocationEmitter.Remove(gridItem);

            //立刻执行一次 定位检查
            ExecuteLocationEmitterCheck(gridItem);
        }

        /// <summary>
        /// 执行 定位发起者 定位检查
        /// </summary>
        /// <param name="emitter"></param>
        public void ExecuteLocationEmitterCheck(GridItemComponent emitter)
        {
            for (int i = 0; i < m_ListLocationReceiver.Count; i++)
            {
                var receiver = m_ListLocationReceiver[i];
                receiver.CheckEmitterInRange(emitter);
            }
        }

        /// <summary>
        /// 添加 定位接收者
        /// </summary>
        /// <param name="receiver"></param>
        public void AddLocationReceiver(GridItemComponent receiver)
        {
            if (m_ListLocationReceiver.Contains(receiver)) { return; }

            m_ListLocationReceiver.Add(receiver);

            //立刻执行一次 定位检查
            ExecuteLocationReceiverCheck(receiver);
        }

        /// <summary>
        /// 移除 定位接收者
        /// </summary>
        /// <param name="receiver"></param>
        public void RemoveLocationReceiver(GridItemComponent receiver)
        {
            m_ListLocationReceiver.Remove(receiver);

            //立刻执行一次 定位检查
            ExecuteLocationReceiverCheck(receiver);
        }

        /// <summary>
        /// 执行 定位接收者 位置检查
        /// </summary>
        /// <param name="receiver"></param>
        public void ExecuteLocationReceiverCheck(GridItemComponent receiver)
        {
            for (int i = 0; i < m_ListLocationEmitter.Count; i++)
            {
                var emitter = m_ListLocationReceiver[i];
                receiver.CheckEmitterInRange(emitter);
            }
        }
        #endregion

        #region 区域功能
        public Dictionary<int, AreaGroupInfo> DicAreaGroupInfo { get { return m_DicAreaGroupInfo; } }
        private Dictionary<int, AreaGroupInfo> m_DicAreaGroupInfo = new Dictionary<int, AreaGroupInfo>(); //字典 区域组信息

        /// <summary>
        /// 添加 区域信息
        /// </summary>
        /// <param name="areaGroupId"></param>
        /// <param name="areaInfo"></param>
        /// <param name="isInherits">继承已存在的区域信息的数据</param>
        public void AddAreaInfo(int areaGroupId, AreaInfo areaInfo, bool isInherits = true)
        {
            var areaGroupInfo = GetAreaGroupInfo(areaGroupId);
            areaInfo.AreaGroupId = areaGroupId;
            areaGroupInfo.AddAreaInfo(areaInfo.KeyName, areaInfo, isInherits);
        }

        /// <summary>
        /// 移除 区域信息
        /// </summary>
        /// <param name="areaGroupId"></param>
        /// <param name="areaInfoKey"></param>
        public void RemoveAreaInfo(int areaGroupId, string areaInfoKey)
        {
            var areaGroupInfo = GetAreaGroupInfo(areaGroupId);
            areaGroupInfo.RemoveAreaInfo(areaInfoKey);
        }

        /// <summary>
        /// 移除 区域组信息
        /// </summary>
        /// <param name="areaGroupId"></param>
        public void RemoveAreaGroupInfo(int areaGroupId)
        {
            m_DicAreaGroupInfo.Remove(areaGroupId);
        }

        /// <summary>
        /// 获取 区域组信息
        /// </summary>
        /// <param name="areaGroupId"></param>
        /// <returns></returns>
        private AreaGroupInfo GetAreaGroupInfo(int areaGroupId)
        {
            AreaGroupInfo areaGroupInfo;
            if (!m_DicAreaGroupInfo.TryGetValue(areaGroupId, out areaGroupInfo))
            {
                areaGroupInfo = new AreaGroupInfo(areaGroupId);
                m_DicAreaGroupInfo.Add(areaGroupId, areaGroupInfo);
            }

            return areaGroupInfo;
        }

        /// <summary>
        /// 检查 是否在区域内
        /// </summary>
        public AreaInfo CheckInAreaInfo(GridCoordFloat gridCoord)
        {
            AreaInfo areaInfo = null;
            foreach (var areaGroupInfo in m_DicAreaGroupInfo.Values)
            {
                if (areaGroupInfo.CheckInBound(gridCoord))
                    areaInfo = areaGroupInfo.CheckInAreaBound(gridCoord);
            }

            return areaInfo;
        }

        #region 焦点区域

        private AreaGroupInfo m_FocusAreaGroupInfoCur; //区域组信息 当前焦点

        /// <summary>
        /// 设置 焦点区域信息
        /// </summary>
        /// <param name="areaInfo">为null时 所有区域内物体都可见</param>
        public void SetFocusAreaInfo(AreaInfo areaInfo)
        {
            if (areaInfo == null && m_FocusAreaGroupInfoCur != null)
            {
                //当前焦点区域组 全部还原为可见
                foreach (var areaInfoCur in m_FocusAreaGroupInfoCur.DicAreaInfo.Values)
                {
                    areaInfoCur.SetVisibleState(true);
                }
                m_FocusAreaGroupInfoCur = null;
            }
            else if (m_DicAreaGroupInfo.TryGetValue(areaInfo.AreaGroupId, out m_FocusAreaGroupInfoCur))
            {
                //设置新的焦点区域组
                var gridCoordCheck = areaInfo.GridCoord + areaInfo.Size; //坐标 检查临界点
                foreach (var areaInfoCur in m_FocusAreaGroupInfoCur.DicAreaInfo.Values)
                {
                    //检查 当前区域是否高于焦点区域
                    if (areaInfoCur.GridCoord.Z < gridCoordCheck.Z)
                    {
                        areaInfoCur.SetVisibleState(true);
                    }
                    else
                    {
                        areaInfoCur.SetVisibleState(false);
                    }
                }
            }
        }

        #endregion
        #endregion
    }
}
