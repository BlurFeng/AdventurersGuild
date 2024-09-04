using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace FsSearchPathSystem
{
    /// <summary>
    /// Edge weight digraph Search path
    /// 加权边有向图 寻路系统
    /// 
    /// author WinhooFeng
    /// date 20210711
    /// </summary>
    public class EWDSearchPath
    {
        //特性：DSP为惰性加载
        //支持：动态添加新的边 动态修改边权重值
        //不支持：动态添加新的顶点

        //如果想要达到某些某些路线一开始不可用，之后可用的需求，可以先将边的权重设置为float.max

        /// <summary>
        /// 加权边有向图
        /// </summary>
        private Digraph m_Digraph;

        //所有顶点的DSP项目 Key=VertexNum Vaule=DSP
        private Dictionary<int, DspItem> m_VertexDSPDic;

        //所有边项目 Key=ID Vaule=EdgeItem
        private Dictionary<int, EdgeItem> EdgeItemDic;
        private int EdgeItemIDCounter;//边项目ID计数器 从0开始 只增

        //Dsp只保存了使用到的DirectedEdge类 此字典专门为Dsp服务
        private Dictionary<DirectedEdge, EdgeItem> m_EdgeItemDicForDSP;//Key=DirectedEdge Vaule=EdgeItem

        /// <summary>
        /// 顶点数量
        /// </summary>
        public int VertexCount { get { return m_Digraph.VertexCount; } }

        /// <summary>
        /// 初始化 顶点数量
        /// </summary>
        /// <param name="vertexCount"></param>
        public void Init(int vertexCount)
        {
            m_VertexDSPDic = new Dictionary<int, DspItem>();//Key=VertexNum Vaule=DSP
            m_EdgeItemDicForDSP = new Dictionary<DirectedEdge, EdgeItem>();
            EdgeItemDic = new Dictionary<int, EdgeItem>();
            EdgeItemIDCounter = 0;

            m_Digraph = new Digraph(vertexCount);
        }

        /// <summary>
        /// 添加一条有向边
        /// from和to不能超过有向图顶点数量的上限 否则将添加失败
        /// </summary>
        /// <param name="from">从</param>
        /// <param name="to">到</param>
        /// <param name="weight">权重</param>
        /// <returns>边对应的Index</returns>
        public int AddEdge(int from, int to, float weight)
        {
            if (m_Digraph == null) return -1;

            //生成边项目ID
            EdgeItemIDCounter++;

            //生成边和边项目
            var edge = m_Digraph.AddEdge(from, to, weight);
            var item = new EdgeItem()
            {
                Id = EdgeItemIDCounter,
                edge = edge
            };

            //添加到集合
            EdgeItemDic.Add(EdgeItemIDCounter, item);
            m_EdgeItemDicForDSP.Add(edge, item);

            //如果在生成dsp数据后添加了新的边 所有的dsp都需要标记为旧数据
            //那么下次使用此dsp时就需要重新计算
            if (m_VertexDSPDic.Count > 0)
            {
                foreach (var dspItem in m_VertexDSPDic.Values)
                {
                    dspItem.isDataOld = true;
                }
            }

            return EdgeItemIDCounter;
        }

        /// <summary>
        /// 刷新某条路对应的有向边权重
        /// </summary>
        /// <param name="edgeItemId"></param>
        public void ChangeEdgeWeight(int edgeItemId, int newWeight)
        {
            if (!EdgeItemDic.ContainsKey(edgeItemId)) return;

            var item = EdgeItemDic[edgeItemId];
            if (item.edge.Weight == newWeight) return;

            item.edge.SetWeight(newWeight);//设置边权重
            item.onWeightChange?.Invoke();
        }

        /// <summary>
        /// 获取路径 顶点
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="outVertexs">途经的顶点（不包括起点 包括终点）</param>
        /// <returns>是否有可到达的路径</returns>
        public bool GetPathVertexs(int start, int end, out int[] outVertexs)
        {
            outVertexs = null;
            if (!GetPathEdges(start, end, out int[] outEdgeIds)) return false;

            outVertexs = new int[outEdgeIds.Length];
            int index = 0;
            foreach (var e in outEdgeIds)
            {
                outVertexs[index] = EdgeItemDic[e].edge.To;//添加途经的顶点
                index++;
            }

            return true;
        }

        /// <summary>
        /// 获取路径 边
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="outEdgeIds">途经的顶点（不包括起点 包括终点）</param>
        /// <returns>是否有可到达的路径</returns>
        public bool GetPathEdges(int start, int end, out int[] outEdgeIds)
        {
            outEdgeIds = null;
            if (start == end) return false;//没有到达自己的路径
            if (!GetDspItem(start, out DspItem dspItem)) return false;//获取dsp
            if (!dspItem.dsp.HasPathTo(end)) return false;//是否有到达的路径

            var edges = dspItem.dsp.PathTo(end);//获取路径经过的边

            outEdgeIds = new int[edges.Count];
            int index = 0;
            foreach (var e in edges)
            {
                outEdgeIds[index] = m_EdgeItemDicForDSP[e].Id;//添加途经的顶点
                index++;
            }

            return true;
        }

        /// <summary>
        /// 刷新所有的DSP
        /// 请在有向图的数据添加完毕后调用 否则之后添加的边并不在dsp的计算范围内 那么之后添加一条边 所有的DspItem都会被标记为旧数据
        /// </summary>
        /// <param name="forceCreateDsp">强制重新生成Dsp</param>
        public void RefreshAllDijkstraSP(bool forceCreateDsp = false)
        {
            //生成所有点对应的dsp
            for (int v = 0; v < m_Digraph.VertexCount; v++)
            {
                CreateDspItem(v, forceCreateDsp);
            }
        }

        /// <summary>
        /// 创建某个顶点对应的Dsp
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="forceCreate"></param>
        private void CreateDspItem(int vertex, bool forceCreate = false)
        {
            GetDspItem(vertex, out DspItem dspItem, forceCreate);
        }

        /// <summary>
        /// 获取DspItem
        /// 会进行数据确认(初次生成或 刷新数据)
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="outDspItem"></param>
        /// <param name="forceCreate"></param>
        /// <returns></returns>
        private bool GetDspItem(int vertex, out DspItem outDspItem, bool forceCreate = false)
        {
            outDspItem = null;
            if (vertex > VertexCount) return false;

            //初次获取 生成
            if (!m_VertexDSPDic.ContainsKey(vertex) || forceCreate)
            {
                var dspItem = 
                    new DspItem()
                    {
                        dsp = new DijkstraSP(m_Digraph, vertex)
                    };

                //已经包含对应的dsp
                if (m_VertexDSPDic.ContainsKey(vertex))
                {
                    m_VertexDSPDic[vertex].UnbindAllAction();//解绑所有事件
                    m_VertexDSPDic[vertex] = dspItem;//替换dsp
                }
                else
                    m_VertexDSPDic.Add(vertex, dspItem);

                //绑定数据变化事件到dsp关联的边
                foreach (var e in dspItem.dsp.Edges)
                {
                    dspItem.BindToActionDataChange(m_EdgeItemDicForDSP[e].onWeightChange);
                }
            }

            outDspItem = m_VertexDSPDic[vertex];

            //使用的数据过时时 重新计算路径数据(使用到的边权重变化了)
            if (outDspItem.isDataOld)
            {
                outDspItem.dsp.CalculatePath();//重新计算路径

                //解绑之前的数据变化事件 重新绑定到新关联的边
                outDspItem.UnbindAllAction();
                foreach (var e in outDspItem.dsp.Edges)
                {
                    outDspItem.BindToActionDataChange(m_EdgeItemDicForDSP[e].onWeightChange);
                }
            }

            return true;
        }

#if DEBUG
        /// <summary>
        /// 打印某个顶点可以到达的所有目的地及路径
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="printPathVertexMap">打印路径顶点映射Map 将顶点数替换为Map中对应的内容</param>
        public void PrintPlacePath(int start, Dictionary<int, int> printPathVertexMap)
        {
            if (start > VertexCount)
            {
                Debug.Log("No vertex!");
                return;
            }

            GetDspItem(start, out DspItem dspItem);
            var dsp = dspItem.dsp;

            bool haveAnypath = false;

            for (int v = 0; v < VertexCount; v++)
            {
                if (dsp.HasPathTo(v))
                {
                    haveAnypath = true;

                    string startStr = start.ToString();
                    if (printPathVertexMap != null && printPathVertexMap.ContainsKey(start))
                        startStr = printPathVertexMap[start].ToString();

                    string endStr = v.ToString();
                    if (printPathVertexMap != null && printPathVertexMap.ContainsKey(v))
                        endStr = printPathVertexMap[v].ToString();

                    StringBuilder sb = new StringBuilder(startStr + " To " + endStr + " Path: " + startStr);
                    var edges = dsp.PathTo(v);
                    foreach (var e in edges)
                    {
                        string toStr = e.To.ToString();
                        if (printPathVertexMap != null && printPathVertexMap.ContainsKey(e.To))
                            toStr = printPathVertexMap[e.To].ToString();

                        sb.Append(" -> " + toStr);
                    }

                    sb.Append(" |WeightTotal: " + dsp.DistTo(v));
                    Debug.Log(sb);
                }
            }

            if (!haveAnypath)
                Debug.Log("Fail to reach anyvertex!");
        }
#endif
    }

    /// <summary>
    /// 边项目
    /// </summary>
    public class EdgeItem
    {
        public int Id;
        public DirectedEdge edge;
        public Action onWeightChange;//当边的权重发生变化时
    }

    /// <summary>
    /// 寻路项目
    /// </summary>
    public class DspItem
    {
        public DijkstraSP dsp;
        public bool isDataOld;//数据需要更新

        //自身绑定到的事件
        private readonly List<Action> m_BindAction = new List<Action>();

        /// <summary>
        /// 绑定到数据变化事件
        /// </summary>
        /// <param name="action"></param>
        public void BindToActionDataChange(Action action)
        {
            action += OnEdgeWeightChange;
            m_BindAction.Add(action);
        }

        /// <summary>
        /// 解绑所有事件
        /// </summary>
        public void UnbindAllAction()
        {
            for (int i = 0; i < m_BindAction.Count; i++)
            {
                m_BindAction[i] -= OnEdgeWeightChange;
            }

            m_BindAction.Clear();
        }

        /// <summary>
        /// 当dsp使用到的边的权重变化时 标记自身数据为旧
        /// 那么在下次使用到此dsp时 需要刷新路线数据
        /// </summary>
        private void OnEdgeWeightChange()
        {
            isDataOld = true;
        }
    }
}