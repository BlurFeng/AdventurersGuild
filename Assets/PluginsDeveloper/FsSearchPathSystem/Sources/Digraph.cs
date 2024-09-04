using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FsSearchPathSystem
{
    /// <summary>
    /// 加权有向边
    /// </summary>
    public class DirectedEdge
    {
        public DirectedEdge(int from, int to, float weight)
        {
            this.m_From = from;
            this.m_To = to;
            this.m_Weight = weight;
        }

        //起点
        private readonly int m_From;

        //终点
        private readonly int m_To;

        //权重
        private float m_Weight;

        /// <summary>
        /// 起点
        /// </summary>
        public int From { get { return m_From; } }

        /// <summary>
        /// 终点
        /// </summary>
        public int To { get { return m_To; } }

        /// <summary>
        /// 权重值
        /// </summary>
        public float Weight { get { return m_Weight; } }

        public void SetWeight(float newWeight)
        {
            m_Weight = newWeight;
        }
    }

    /// <summary>
    /// 有向图
    /// </summary>
    public class Digraph
    {
        public Digraph(int vertexNum)
        {
            this.m_VertexCount = vertexNum;
            this.m_EdgeNum = 0;
            m_AdjacencyArray = new Queue<DirectedEdge>[vertexNum];
            for (int i = 0; i < vertexNum; i++)
            {
                m_AdjacencyArray[i] = new Queue<DirectedEdge>();
            }
        }

        private int m_EdgeNum;
        /// <summary>
        /// 边数量
        /// </summary>
        public int EdgeNum { get { return m_EdgeNum; } }

        private int m_VertexCount;
        /// <summary>
        /// 顶点数量
        /// </summary>
        public int VertexCount { get { return m_VertexCount; } }

        //邻接表
        private Queue<DirectedEdge>[] m_AdjacencyArray;

        /// <summary>
        /// 添加一条边
        /// </summary>
        /// <param name="edge"></param>
        public void AddEdge(DirectedEdge edge)
        {
            m_AdjacencyArray[edge.From].Enqueue(edge);
            m_EdgeNum++;
        }

        /// <summary>
        /// 添加一条边
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="weight"></param>
        public DirectedEdge AddEdge(int from, int to, float weight)
        {
            if(from > VertexCount || to > VertexCount)
            {
                Debug.LogError("from or to out of vertexCount!");
                return null;
            }

            DirectedEdge edge = new DirectedEdge(from, to, weight);
            AddEdge(edge);

            return edge;
        }

        /// <summary>
        /// 获取某个点的邻接表
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public Queue<DirectedEdge> GetAdjacency(int vertex)
        {
            return m_AdjacencyArray[vertex];
        }

        public Queue<DirectedEdge> GetAllEdges()
        {
            Queue<DirectedEdge> edges = new Queue<DirectedEdge>();

            for (int i = 0; i < m_AdjacencyArray.Length; i++)
            {
                foreach (var e in m_AdjacencyArray[i])
                {
                    edges.Enqueue(e);
                }
            }

            return edges;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("( Vertices: " + m_VertexCount + "," + "Edges: " + m_EdgeNum + "\n");

            for (int i = 0; i < m_VertexCount; i++)
            {
                sb.Append(i + ": ");
                foreach (var e in GetAdjacency(i))
                {
                    sb.Append(e + " ");
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        #region AddEdge More

        /// <summary>
        /// 添加一个顶点和两条边
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to1"></param>
        /// <param name="weight1"></param>
        /// <param name="to2"></param>
        /// <param name="weight2"></param>
        public void AddEdges(int from, int to1, float weight1, int to2, float weight2)
        {
            AddEdge(from, to1, weight1);
            AddEdge(from, to2, weight2);
        }

        /// <summary>
        /// 添加一个顶点和三条边
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to1"></param>
        /// <param name="weight1"></param>
        /// <param name="to2"></param>
        /// <param name="weight2"></param>
        /// <param name="to3"></param>
        /// <param name="weight3"></param>
        public void AddEdges(int from, int to1, float weight1, int to2, float weight2, int to3, float weight3)
        {
            AddEdge(from, to1, weight1);
            AddEdge(from, to2, weight2);
            AddEdge(from, to3, weight3);
        }

        /// <summary>
        /// 添加一个顶点和四条边
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to1"></param>
        /// <param name="weight1"></param>
        /// <param name="to2"></param>
        /// <param name="weight2"></param>
        /// <param name="to3"></param>
        /// <param name="weight3"></param>
        /// <param name="to4"></param>
        /// <param name="weight4"></param>
        public void AddEdges(int from, int to1, float weight1, int to2, float weight2, int to3, float weight3, int to4, float weight4)
        {
            AddEdge(from, to1, weight1);
            AddEdge(from, to2, weight2);
            AddEdge(from, to3, weight3);
            AddEdge(from, to4, weight4);
        }

        /// <summary>
        /// 添加一个顶点和五条边
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to1"></param>
        /// <param name="weight1"></param>
        /// <param name="to2"></param>
        /// <param name="weight2"></param>
        /// <param name="to3"></param>
        /// <param name="weight3"></param>
        /// <param name="to4"></param>
        /// <param name="weight4"></param>
        /// <param name="to5"></param>
        /// <param name="weight5"></param>
        public void AddEdges(int from, int to1, float weight1, int to2, float weight2, int to3, float weight3, int to4, float weight4, int to5, float weight5)
        {
            AddEdge(from, to1, weight1);
            AddEdge(from, to2, weight2);
            AddEdge(from, to3, weight3);
            AddEdge(from, to4, weight4);
            AddEdge(from, to5, weight5);
        }
        #endregion
    }
}