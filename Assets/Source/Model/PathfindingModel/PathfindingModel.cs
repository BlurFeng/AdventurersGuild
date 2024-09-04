using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AstarPathfindingExtension;
using Pathfinding;
using FsGameFramework;
using Deploy;
using System;

/// <summary>
/// 寻路模块
/// 功能基于AstarPathfindingProject插件实现
/// </summary>
public class PathfindingModel : Singleton<PathfindingModel>, IDestroy, ISaveData
{
    #region Event

    /// <summary>
    /// 当导航图更新时
    /// </summary>
    private Action OnGraphUpdate;
    public void BindEventWithOnGraphUpdate(Action action, bool bind)
    {
        if (bind)
            OnGraphUpdate += action;
        else
            OnGraphUpdate -= action;
    }

    #endregion

    /// <summary>
    /// 寻路插件管理器类
    /// </summary>
    private AstarPath m_AstarPath;

    private Dictionary<string, Pathfinding_LevelGraph> m_Pathfinding_LevelGraphDic;

    public override void Init()
    {
        base.Init();

        m_AstarPath = PathfindingUtility.GetAstarPathfinder();

#if !UNITY_EDITOR
        //在非编辑器时设置
        m_AstarPath.logPathResults = PathLog.None;
#endif

        //初始化导航地图数据
        m_Pathfinding_LevelGraphDic = new Dictionary<string, Pathfinding_LevelGraph>();
        var pflgMap = ConfigSystem.Instance.GetConfigMap<Pathfinding_LevelGraph>();
        foreach (var item in pflgMap.Values)
        {
            if (!m_Pathfinding_LevelGraphDic.ContainsKey(item.MapName))
                m_Pathfinding_LevelGraphDic.Add(item.MapName, item);
            else
                Debug.LogWarning($"Pathfinding_LevelGraph data have repetition MapName! Id : {item.Id}  MapName : {item.MapName}");
        }

        BindAction(true);
    }

    public override void Destroy()
    {
        base.Destroy();

        BindAction(false);
    }

    private void BindAction(bool bind)
    {
        FWorldContainer.BindActionWithOnLoadLevel(OnLoadLevel, bind);
    }

    #region SaveData

    public class AstarPathfindingModelSaveData
    {
        public AstarPathfindingModelSaveData()
        {
            serializeGraphsDic = new Dictionary<string, byte[]>();
        }

        public Dictionary<string, byte[]> serializeGraphsDic;
    }

    private const string m_AstarPathfindingModelSaveDataKey = "AstarPathfindingModelSaveDataKey";

    public void LoadData(ES3File saveData)
    {
        AstarPathfindingModelSaveData astarPathfindingModelSaveData;
        if (saveData.KeyExists(m_AstarPathfindingModelSaveDataKey))
        {
            astarPathfindingModelSaveData = saveData.Load<AstarPathfindingModelSaveData>(m_AstarPathfindingModelSaveDataKey);
        }
        else
        {
            astarPathfindingModelSaveData = new AstarPathfindingModelSaveData();
        }

        InitLevelMapNavGraph(astarPathfindingModelSaveData);
    }

    public void SaveData(ES3File saveData)
    {
        AstarPathfindingModelSaveData astarPathfindingModelSaveData = new AstarPathfindingModelSaveData();

        foreach (var item in m_LayerGridGraphDic)
        {
            var serializationSettings = Pathfinding.Serialization.SerializeSettings.Settings;
            serializationSettings.nodes = true;
            m_AstarPath.data.graphs[0] = item.Value;
            byte[] bytes = m_AstarPath.data.SerializeGraphs(serializationSettings);
            astarPathfindingModelSaveData.serializeGraphsDic.Add(item.Key, bytes);
        }
        
        //Pathfinding.Serialization.AstarSerializer.SaveToFile(NavGraphDataPath, bytes);

        saveData.Save(m_AstarPathfindingModelSaveDataKey, astarPathfindingModelSaveData);
    }

    #endregion

    #region LevelMapNavGraph

    //此游戏使用layerGridGraph类型的导航图作为寻路方案
    //业务上同时仅存在一个有效的Graph。切换Level时我们替换AstarPath.data.graphs来改变当前的导航图

    /// <summary>
    /// 分层网格导航图字典
    /// key=LevelName
    /// </summary>
    private Dictionary<string, NavGraph> m_LayerGridGraphDic;

    private void InitLevelMapNavGraph(AstarPathfindingModelSaveData astarPathfindingModelSaveData)
    {
        m_LayerGridGraphDic = new Dictionary<string, NavGraph>();

        //反序列化所有存储的导航图数据并缓存
        if(astarPathfindingModelSaveData != null && astarPathfindingModelSaveData.serializeGraphsDic.Count > 0)
        {
            foreach (var item in astarPathfindingModelSaveData.serializeGraphsDic)
            {
                m_AstarPath.data.DeserializeGraphs(item.Value);
                m_LayerGridGraphDic.Add(item.Key, m_AstarPath.data.graphs[0]);
            }
        }

        //加载当前导航图
        OnLoadLevel(FWorldContainer.GetLevelCur());
    }

    public void OnLoadLevel(ULevel level)
    {
        if (level == null) return;

        string levelName = level.Name;

        if (m_LayerGridGraphDic.ContainsKey(levelName))
        {
            m_AstarPath.data.graphs[0] = m_LayerGridGraphDic[levelName];
        }
        else if(!string.IsNullOrEmpty(levelName) && m_Pathfinding_LevelGraphDic.ContainsKey(levelName))
        {
            //获取配置的地图数据
            Pathfinding_LevelGraph pathfinding_LevelGraph = m_Pathfinding_LevelGraphDic[levelName];

            //初始化AstarPath的数据
            LayerGridGraph layerGridGraph = PathfindingUtility.GetLayerGridGraph();
            layerGridGraph.center = new Vector3(pathfinding_LevelGraph.Center[0], pathfinding_LevelGraph.Center[1], pathfinding_LevelGraph.Center[2]);
            layerGridGraph.SetDimensions(pathfinding_LevelGraph.Width, pathfinding_LevelGraph.Depth, pathfinding_LevelGraph.NodeSize * 0.1f);

            //分层网格导航图通用设置
            layerGridGraph.characterHeight = 0.35f;
            layerGridGraph.maxClimb = 0.3f;
            layerGridGraph.maxSlope = 50f;
            layerGridGraph.erodeIterations = 0;
            layerGridGraph.mergeSpanRange = 0.5f;
            GraphCollision graphCollision = new GraphCollision()
            {
                collisionCheck = true,
                type = ColliderType.Capsule,
                diameter = 1.2f,
                height = 0.35f,
                collisionOffset = 0f,
                mask = LayerMask.GetMask("PathGraphObstacles"),
                heightCheck = true,
                fromHeight = 100f,
                heightMask = LayerMask.GetMask("Ground"),
                unwalkableWhenNoGround = true
            };
            layerGridGraph.collision = graphCollision;

            //默认惩罚值（权重
            layerGridGraph.initialPenalty = 10000;

            //扫描生成整个导航图
            layerGridGraph.Scan();

            //根据场景中的GridItem物品设置Node属性 TODO：


            m_LayerGridGraphDic.Add(levelName, layerGridGraph);
        }
    }

    /// <summary>
    /// 更新Collider范围内的导航图
    /// 当创建或销毁建筑等，改变可移动地块时调用
    /// </summary>
    /// <param name="bounds">范围，可由collider.bounds等方式获取</param>
    public void UpdateGraphs(Bounds bounds, bool updateSearchPath = false)
    {
        PathfindingUtility.UpdateGraphs(bounds);

        if (OnGraphUpdate != null) OnGraphUpdate.Invoke();
    }

    /// <summary>
    /// 设置某个范围内节点是否可行走
    /// 当创建或销毁家具等，影响节点是否可行走时调用
    /// </summary>
    /// <param name="bounds">范围，可由collider.bounds等方式获取</param>
    /// <param name="walkable">是否可行走</param>
    public void SetNodeWalkableInLayerGridGraph(Bounds bounds, bool walkable, bool updateSearchPath = false)
    {
        PathfindingUtility.SetNodeWalkableInLayerGridGraph(bounds, walkable);

        if (OnGraphUpdate != null) OnGraphUpdate.Invoke();
    }

    /// <summary>
    /// 设置范围内节点的惩罚值（权重值）
    /// 惩罚值越高，寻路时越不倾向选择这个节点
    /// 如果我们对寻路的惩罚值有需求，我们在生成导航图时最好给定一个默认的惩罚值
    /// </summary>
    /// <param name="bounds"></param>
    /// <param name="penalty"></param>
    public void SetNodePenaltyInLayerGridGraph(Bounds bounds, uint penalty, bool updateSearchPath = false)
    {
        PathfindingUtility.SetNodePenaltyInLayerGridGraph(bounds, penalty);

        if (OnGraphUpdate != null) OnGraphUpdate.Invoke();
    }

    #endregion
}
