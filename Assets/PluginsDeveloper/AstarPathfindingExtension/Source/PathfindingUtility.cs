using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;
using Pathfinding.Util;

namespace AstarPathfindingExtension
{
    public class PathfindingUtility
    {
        /// <summary>
        /// 获取AstarPathfinder类
        /// 如果场景中不存在，将自动创建一个
        /// </summary>
        /// <returns></returns>
        public static AstarPath GetAstarPathfinder()
        {
            AstarPath astarPathFind = AstarPath.active;

            if (astarPathFind == null)
            {
                GameObject astarPathObj = new GameObject();
                astarPathObj.name = "AstarPathfinderManager";
                astarPathFind = astarPathObj.AddComponent<AstarPath>();
            }

            return astarPathFind;
        }

        /// <summary>
        /// 获取一个分层网格导航图
        /// 如果为空，则创建一个新的
        /// </summary>
        /// <param name="astarPath"></param>
        public static LayerGridGraph GetLayerGridGraph()
        {
            var ap = GetAstarPathfinder();
            if (ap == null) return null;

            if(ap.data.layerGridGraph == null)
                ap.data.AddGraph(typeof(LayerGridGraph));

            return ap.data.layerGridGraph;
        }

        /// <summary>
        /// 更新Collider范围内的导航图
        /// </summary>
        /// <param name="collider"></param>
        public static void UpdateGraphs(Collider collider)
        {
            if (collider == null) return;
            
            UpdateGraphs(collider.bounds);
        }

        /// <summary>
        /// 更新范围内的导航图
        /// </summary>
        /// <param name="bounds"></param>
        public static void UpdateGraphs(Bounds bounds)
        {
            var ap = GetAstarPathfinder();
            if (ap == null) return;

            ap.UpdateGraphs(bounds);

            //更新完成后最好立即要求进行SearchPath()来更新路径
        }

        /// <summary>
        /// 设置某个范围内节点是否可行走
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="walkable"></param>
        public static void SetNodeWalkableInLayerGridGraph(Bounds bounds, bool walkable)
        {
            if (AstarPath.active == null) return;

            var lgg = AstarPath.active.data.layerGridGraph;

            if (lgg == null) return;

            var inRect = lgg.GetRectFromBounds(bounds);
            var graphNodes = lgg.GetNodesInRegion(inRect);

            for (int i = 0; i < graphNodes.Count; i++)
            {
                graphNodes[i].Walkable = walkable;
            }

            for (int i = 0; i < graphNodes.Count; i++)
            {
                //更新此节点数据
                lgg.CalculateConnections((GridNodeBase)graphNodes[i]);
            }

            //更新完成后最好立即要求进行SearchPath()来更新路径
        }

        /// <summary>
        /// 设置范围内节点的惩罚值（权重值）
        /// 惩罚值越高，寻路时越不倾向选择这个节点
        /// 如果我们对寻路的惩罚值有需求，我们在生成导航图时最好给定一个默认的惩罚值
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="penalty"></param>
        public static void SetNodePenaltyInLayerGridGraph(Bounds bounds, uint penalty)
        {
            if (AstarPath.active == null) return;

            var lgg = AstarPath.active.data.layerGridGraph;

            if (lgg == null) return;

            var inRect = lgg.GetRectFromBounds(bounds);
            var graphNodes = lgg.GetNodesInRegion(inRect);

            for (int i = 0; i < graphNodes.Count; i++)
            {
                graphNodes[i].Penalty = penalty;
            }

            //更新此节点数据
            for (int i = 0; i < graphNodes.Count; i++)
            {
                lgg.CalculateConnections((GridNodeBase)graphNodes[i]);
            }

            //更新完成后最好立即要求进行SearchPath()来更新路径
        }

        /// <summary>
        /// 路径平滑
        /// 分层网格图单个Node只有四方向，当斜线行走时路径为锯齿状
        /// 此平滑将斜线行走处理为斜直线
        /// </summary>
        /// <returns></returns>
        public static void PathSmoothLayerGridDiagonal(Path path)
        {
            if (path.path.Count < 3) return;

            //优化后的路径长度必然小于或等于原有路径长度
            List<Vector3> subdivided = ListPool<Vector3>.Claim(path.path.Count);

            for (int i = 0; i < path.path.Count - 2; i++)
            {
                GraphNode n1 = path.path[i];
                GraphNode n2 = path.path[i + 1];
                GraphNode n3 = path.path[i + 2];
                Vector3 n1p = (Vector3)n1.position;
                Vector3 n2p = (Vector3)n2.position;
                Vector3 n3p = (Vector3)n3.position;

                Vector3 dir1 = n2p - n1p;
                Vector3 dir2 = n3p - n2p;

                bool isSmooth = false;
                //直角，需要平滑
                if (Vector3.Dot(dir1, dir2) == 0)
                {
                    n1.GetConnections(
                        (GraphNode node) => 
                        {
                            if(n3.ContainsConnection(node) && node != n2)
                            {
                                if(node.Walkable)
                                {
                                    //允许进行平滑
                                    isSmooth = true;

                                    subdivided.Add(n1p);
                                    subdivided.Add(n3p);
                                    i++;//略过被省略的点
                                }
                            }
                        });
                }

                //没有进行平滑
                if(!isSmooth)
                {
                    subdivided.Add(n1p);
                }
            }

            ListPool<Vector3>.Release(path.vectorPath);
            path.vectorPath = subdivided;
        }

        public static void ReleasePathV3(List<Vector3> path)
        {
            if (path == null) return;

            ListPool<Vector3>.Release(path);
        }
    }
}
