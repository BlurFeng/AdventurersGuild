using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FsSearchPathSystem
{
    /// <summary>
    /// 迪杰斯特拉 最短路径算法
    /// </summary>
    public class DijkstraSP
    {
        private Digraph m_Digraph;

        private int start;

        /// <summary>
        /// m_EdgeTo[v] 从s到v最短路径上的最后一条边
        /// 可能有null值
        /// </summary>
        private DirectedEdge[] m_EdgeTo;

        bool isOldDataEdges;//需要刷新所有在使用的边的数据
        private DirectedEdge[] m_Edges;
        /// <summary>
        /// 所有路线包含的
        /// </summary>
        public DirectedEdge[] Edges
        {
            get
            {
                if(isOldDataEdges)
                {
                    isOldDataEdges = false;

                    List<DirectedEdge> edgeList = new List<DirectedEdge>();
                    for (int i = 0; i < m_EdgeTo.Length; i++)
                    {
                        var e = m_EdgeTo[i];
                        if (e != null)
                            edgeList.Add(e);
                    }

                    m_Edges = edgeList.ToArray();
                }

                return m_Edges;
            }
        }

        /// <summary>
        /// m_DisTo[v] 从s到v最短路径的长度
        /// </summary>
        private float[] m_DistTo;

        private IndexMinPriorityQueue<float> m_PQ;

        /// <summary>
        /// 初始化一个顶点的最短路径算法结构
        /// </summary>
        /// <param name="g">加权有向图结构</param>
        /// <param name="s">起始点</param>
        public DijkstraSP(Digraph g, int s)
        {
            m_Digraph = g;
            start = s;

            m_EdgeTo = new DirectedEdge[m_Digraph.VertexCount];
            m_DistTo = new float[m_Digraph.VertexCount];
            m_PQ = new IndexMinPriorityQueue<float>(m_Digraph.VertexCount);

            CalculatePath();
        }

        /// <summary>
        /// 计算路径
        /// </summary>
        public void CalculatePath()
        {
            //重置数据
            isOldDataEdges = true;
            for (int i = 0; i < m_Digraph.VertexCount; i++)
            {
                //重置边和路径长度数据
                m_EdgeTo[i] = null;
                m_DistTo[i] = float.MaxValue;
            }
            m_DistTo[start] = 0f;//起点到自身距离为0
            m_PQ.insert(start, 0f);//入队

            while (!m_PQ.isEmpty())
                Visit(m_Digraph, m_PQ.delMin());
        }

        private void Visit(Digraph g, int v)
        {
            foreach (var e in g.GetAdjacency(v))
            {
                int w = e.To;

                if(w > m_DistTo.Length)
                {
                    Debug.LogError("IndexOutOfRangeException: vertex Num > g.VertexNum");
                    return;
                }

                //如果经由v点到w点 权重更小则选择v点
                float weightTemp = m_DistTo[v] + e.Weight;//起点到达v点的距离 加e的长度(权重
                float distToW = m_DistTo[w];
                if (distToW > weightTemp)
                {
                    //更新最短路径长度和对应路径
                    m_DistTo[w] = distToW = weightTemp;
                    m_EdgeTo[w] = e;

                    if(m_PQ.contains(w))
                    {
                        m_PQ.changeItem(w, distToW);//更新索引w对应值
                    }
                    else
                    {
                        m_PQ.insert(w, distToW);//插入
                    }
                }
            }
        }

        /// <summary>
        /// 获取从自身到达v点的最小长度
        /// </summary>
        /// <param name="v">目标点</param>
        /// <returns></returns>
        public float DistTo(int v)
        {
            return m_DistTo[v];
        }

        /// <summary>
        /// 自身是否有前往v点的路线
        /// </summary>
        /// <param name="v">目标点</param>
        /// <returns></returns>
        public bool HasPathTo(int v)
        {
            return m_DistTo[v] < float.MaxValue;
        }

        /// <summary>
        /// 自身前往v点的路径
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Stack<DirectedEdge> PathTo(int v)
        {
            //没有到达v点的路径
            if (!HasPathTo(v)) return null;

            Stack<DirectedEdge> path = new Stack<DirectedEdge>();
            for (DirectedEdge e = m_EdgeTo[v]; e != null; e = m_EdgeTo[e.From])
            {
                path.Push(e);
            }

            return path;
        }
    }

    public class DijkstraSPDemo
    {
        public static void DemoPrint()
        {
            Digraph g = new Digraph(4);
            g.AddEdges(0, 1, 10, 3, 20);
            g.AddEdge(1, 2, 10);
            g.AddEdge(2, 3, 10);

            int start = 0;
            DijkstraSP dsp = new DijkstraSP(g, start);
            for (int i = 0; i < g.VertexCount; i++)
            {
                if (dsp.HasPathTo(i))
                {
                    StringBuilder sb = new StringBuilder(start + " To " + i + " Path: " + start);
                    var edges = dsp.PathTo(i);
                    foreach (var e in edges)
                    {
                        sb.Append(" -> " + e.To);
                    }

                    sb.Append(" |WeightTotal: " + dsp.DistTo(i));
                    Debug.Log(sb);
                }
            }
        }
    }
}