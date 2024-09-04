using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathFindTestSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject m_DynamicTerrian;
    [SerializeField] private AstarPath m_AstarPath;
    [SerializeField] private PathFindTestCharacter m_PathFindTestCharacter;

    //导航图形数据存储地址
    string NavGraphDataPath;

    private NavGraph m_NavGraph0Cached;

    private void Awake()
    {
        NavGraphDataPath = Application.dataPath + "/Scenes/Test/PathFind/graph.bytes";
    }

    // Start is called before the first frame update
    void Start()
    {
        //加载保存的导航图数据
        byte[] bytes = Pathfinding.Serialization.AstarSerializer.LoadFromFile(NavGraphDataPath);
        AstarPath.active.data.DeserializeGraphs(bytes);
    }

    // Update is called once per frame
    void Update()
    {
        //测试，动态改变地形后重新扫描导航图，并保存到本地文件。
        if (Input.GetKeyDown(KeyCode.G))
        {
            m_DynamicTerrian.SetActive(!m_DynamicTerrian.activeSelf);

            ////设置尺寸
            //var graphs = m_AstarPath.graphs;
            //foreach (var graph in graphs)
            //{
            //    var gridGraph = graph as GridGraph;
            //    gridGraph.SetDimensions(100, 100, 1);
            //}

            //更新某个范围内的图
            //Bounds bounds = GetComponent<Collider>().bounds;
            //AstarPath.active.UpdateGraphs(bounds);

            //扫描导航图
            m_AstarPath.Scan();

            //重新要求单位绘制路径
            m_PathFindTestCharacter.SearchPath();

            Save();
        }

        //随机改变所有节点的惩罚点
        if (Input.GetKeyDown(KeyCode.H))
        {
            bool leftC = Input.GetKey(KeyCode.LeftControl);

            //添加一个异步工作
            //在这里进行Node的修改是安全的，因为寻路的线程可能在运行。此处能保证在寻路线程停止时进行Graph数据的更新
            //AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
            //{

            //}));

            //随机改变Node的权重
            //此处做测试，直接运行

            //获取某个范围内的Nodes进行修改
            //Collider co = new Collider();
            //var inRect = gg.GetRectFromBounds(co.bounds);
            //gg.GetNodesInRegion(inRect);

            var lgg = m_AstarPath.data.layerGridGraph;

            for (int z = 0; z < lgg.depth; z++)
            {
                for (int x = 0; x < lgg.width; x++)
                {
                    for (int lay = 0; lay < lgg.LayerCount; lay++)
                    {
                        var node = lgg.GetNode(x, z, lay);

                        if (node == null) continue;

                        //Random.InitState((int)Time.realtimeSinceStartup * 1000);
                        node.Penalty = leftC ? 1000 : (uint)Random.Range(0, 2000);

                        // This example uses perlin noise to generate the map
                        //node.Walkable = Mathf.PerlinNoise(x * 0.087f, z * 0.087f) > 0.4f;
                    }
                }
            }

            // Recalculate all grid connections
            // This is required because we have updated the walkability of some nodes
            lgg.GetNodes(node => lgg.CalculateConnections((GridNodeBase)node));

            // If you are only updating one or a few nodes you may want to use
            // gg.CalculateConnectionsForCellAndNeighbours only on those nodes instead for performance.

            //重新要求单位绘制路径
            m_PathFindTestCharacter.SearchPath();

            Save();

            //获取距离某个位置最近的Node
            //var node = AstarPath.active.GetNearest(transform.position).node;
            //node.Walkable = false;
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            if(Pathfinding.AstarData.active.data.graphs[0] != null)
            {
                m_NavGraph0Cached = Pathfinding.AstarData.active.data.graphs[0];
                Pathfinding.AstarData.active.data.graphs[0] = null;
            }
            else
            {
                Pathfinding.AstarData.active.data.graphs[0] = m_NavGraph0Cached;
            }
            
            //if (Pathfinding.AstarData.active.data.layerGridGraph.active)
            //    Pathfinding.AstarData.active.data.layerGridGraph.active = null;
            //else
            //    Pathfinding.AstarData.active.data.layerGridGraph.active = Pathfinding.AstarData.active;
        }
    }

    public void Save()
    {
        var serializationSettings = Pathfinding.Serialization.SerializeSettings.Settings;
        serializationSettings.nodes = true;
        byte[] bytes = AstarPath.active.data.SerializeGraphs(serializationSettings);
        Pathfinding.Serialization.AstarSerializer.SaveToFile(NavGraphDataPath, bytes);
    }
}
