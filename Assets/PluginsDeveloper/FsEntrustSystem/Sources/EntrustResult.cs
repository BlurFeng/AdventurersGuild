using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntrustSystem
{
    /// <summary>
    /// 输入数据
    /// </summary>
    public class EntrustResultInputData
    {
        /// <summary>
        /// 需要获取的结果数量
        /// </summary>
        public int resultNum;

        public List<EntrustResultItem> entrustResultItems;
    }

    /// <summary>
    /// 委托结果数据项目
    /// </summary>
    public struct EntrustResultItem
    {
        public EntrustResultItem(int resultId, int weightBase)
        {
            this.resultId = resultId;
            this.weightBase = weightBase;
            weightOffset = 0;
        }

        /// <summary>
        /// 委托结果Id，含义有项目业务定义
        /// </summary>
        public int resultId;

        /// <summary>
        /// 基础权重
        /// </summary>
        private int weightBase;

        /// <summary>
        /// 权重调整
        /// </summary>
        private int weightOffset;

        /// <summary>
        /// 权重
        /// </summary>
        public int Weight { get { return weightBase + weightOffset; } }

        /// <summary>
        /// 增加权重调整值
        /// </summary>
        /// <param name="addWeight"></param>
        public void WeightOffsetAdd(int addWeight)
        {
            weightOffset += addWeight;
        }

        /// <summary>
        /// 设置基础权重
        /// </summary>
        /// <param name="newWeightBase"></param>
        public void SetWeightBase(int newWeightBase)
        {
            weightBase = newWeightBase;
        }
    }

    /// <summary>
    /// 结果信息，单个项目
    /// </summary>
    public struct EntrustResultInfoItem
    {
        public EntrustResultItem[] entrustResultItems;
    }

    /// <summary>
    /// 结果信息数据
    /// </summary>
    public struct EntrustResultInfo
    {
        public bool isValid;

        /// <summary>
        /// 获取到的结果信息
        /// index = 传入entrustResultInputDatas的对应下标
        /// </summary>
        public EntrustResultInfoItem[] entrustResultInfoItems;

        public bool GetFirstResultId(out int resultId)
        {
            if(entrustResultInfoItems != null && entrustResultInfoItems.Length > 0 && entrustResultInfoItems[0].entrustResultItems != null && entrustResultInfoItems[0].entrustResultItems.Length > 0)
            {
                resultId = entrustResultInfoItems[0].entrustResultItems[0].resultId;
                return true;
            }

            resultId = -1;
            return false;
        }
    }

    /// <summary>
    /// 委托结果
    /// 允许在委托开始时传入结果数据，在委托完成时会计算结果
    /// 项目业务可按需传入，并在结束时根据结果执行业务逻辑
    /// </summary>
    public class EntrustResult
    {
        //委托结果类只按照输入数据，在委托结束时按照权重和其他要求给出结果。结果的具体效果由项目业务决定。
        //如果我们需要一个EntrustItem在完成时提供结果，我们需要按照一下步骤操作。注意，我们无法直接获取EntrustItem，需要通过EntrustSystemManager提供的方法进行操作。
        //1.在委托创建后，调用EntrustSystemManager.SetResultData来设置某个委托的输入数据。
        //需要传入EntrustResultInputData数组，这样我们可以同时得到多个不同分类的结果
        //输入数据中需要初始化entrustResultItems结果项目数组和需要获取的数量，entrustResultItems中主要分配结果Id和权重。结果Id应当由使用者保证唯一。
        //2.在委托完成后，我们就可以调用EntrustSystemManager.GetEntrustResult来获取结果信息了
        //结果信息中的entrustResultInfoItems顺序和输入数据entrustResultInputDatas一致。

        //other其他操作
        //SetEntrustResultItemWeight改变结果项目权重，在委托完成进行结果计算前，我们都可以改变结果项目数据来改变最终结果。
        //比如，委托进行中触发了一些事件，这让某项结果更容易出现了。

        //委托结果输入数据
        private EntrustResultInputData[] m_EntrustResultInputDatas;

        /// <summary>
        /// 计算结果信息
        /// </summary>
        private EntrustResultInfo m_EntrustResultInfo;

        private static List<EntrustResultItem> m_EntrustResultItemsCached = new List<EntrustResultItem>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="entrustResultInputDatas">委托输入数据数组，每个输入数据的结果将在委托完成时分开计算</param>
        public void SetData(EntrustResultInputData[] entrustResultInputDatas)
        {
            m_EntrustResultInputDatas = entrustResultInputDatas;
        }

        /// <summary>
        /// 计算结果
        /// </summary>
        public void ComputedResult()
        {
            if (m_EntrustResultInputDatas == null || m_EntrustResultInputDatas.Length == 0)
            {
                if(m_EntrustResultInfo.isValid)
                    m_EntrustResultInfo = new EntrustResultInfo();
                return;
            }

            EntrustResultInfoItem[] entrustResultInfoItems = new EntrustResultInfoItem[m_EntrustResultInputDatas.Length];

            Random.InitState(Mathf.CeilToInt(Time.realtimeSinceStartup * 10000f));

            //--开始计算--//
            for (int i = 0; i < m_EntrustResultInputDatas.Length; i++)
            {
                EntrustResultInputData entrustResultInputData = m_EntrustResultInputDatas[i];

                m_EntrustResultItemsCached.Clear();
                for (int j = 0; j < entrustResultInputData.entrustResultItems.Count; j++)
                {
                    m_EntrustResultItemsCached.Add(entrustResultInputData.entrustResultItems[j]);
                }

                EntrustResultItem[] entrustResultItems = new EntrustResultItem[entrustResultInputData.resultNum];

                //获取要求数量的结果
                for (int j = 0; j < entrustResultInputData.resultNum; j++)
                {
                    //计算总权重
                    int weightTotal = 0;
                    for (int k = 0; k < m_EntrustResultItemsCached.Count; k++)
                    {
                        EntrustResultItem entrustResultItem = m_EntrustResultItemsCached[k];
                        weightTotal += entrustResultItem.Weight;
                    }

                    float randomNum = RandomChangeSeed(0f, 1f);//随机数

                    int getIndex = 0;
                    float left;
                    float right = 0;
                    //获取随机命中的权重区段对应结果
                    for (int k = 0; k < m_EntrustResultItemsCached.Count; k++)
                    {
                        left = right;
                        right = left + GetWeightPoint(m_EntrustResultItemsCached[k].Weight, weightTotal);

                        //随机数条件有重合部分 但可以忽略
                        if (randomNum >= left && randomNum <= right)
                        {
                            //获取结果数据
                            getIndex = k;
                            entrustResultItems[j] = m_EntrustResultItemsCached[k];
                            break;
                        }
                    }

                    //将获取过的结果从随机项目中屏蔽
                    m_EntrustResultItemsCached.RemoveAt(getIndex);
                }

                entrustResultInfoItems[i] = new EntrustResultInfoItem()
                {
                    entrustResultItems = entrustResultItems,
                };
            }

            m_EntrustResultInfo = new EntrustResultInfo()
            {
                isValid = true,
                entrustResultInfoItems = entrustResultInfoItems,
            };
        }

        private float RandomChangeSeed(float minInclusive, float maxInclusive)
        {
            float seedFloat = Random.Range(minInclusive, maxInclusive);
            Random.InitState(Mathf.CeilToInt(seedFloat * 10000f));

            return seedFloat;
        }

        private float GetWeightPoint(int weight, int weightTotal)
        {
            return weight * (1f / weightTotal);
        }

        /// <summary>
        /// 获取结果
        /// 保证获取前先调用一次ComputedResult来计算结果
        /// </summary>
        /// <returns></returns>
        public bool GetEntrustResult(out EntrustResultInfo outEntrustResultInfo)
        {
            outEntrustResultInfo = m_EntrustResultInfo;
            return m_EntrustResultInfo.isValid;
        }

        private bool GetEntrustResultItem(int inputDataIndex, int resultId, out EntrustResultItem entrustResultItem)
        {
            entrustResultItem = new EntrustResultItem();
            if (m_EntrustResultInputDatas == null || m_EntrustResultInputDatas.Length == 0) return false;
            if (inputDataIndex < 0 && inputDataIndex >= m_EntrustResultInputDatas.Length) return false;

            for (int i = 0; i < m_EntrustResultInputDatas[inputDataIndex].entrustResultItems.Count; i++)
            {
                EntrustResultItem item = m_EntrustResultInputDatas[inputDataIndex].entrustResultItems[i];
                if (item.resultId == resultId)
                {
                    entrustResultItem = m_EntrustResultInputDatas[inputDataIndex].entrustResultItems[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 改变一个委托结果项目的权重
        /// 我们可以在委托进行时因为一些项目业务逻辑原因对某个结果权重进行调整
        /// </summary>
        /// <param name="inputDataIndex">委托输入数据下标</param>
        /// <param name="resultId">结果Id</param>
        /// <param name="changeWeight">改变的权重值，新权重等于旧权重加上改变的权重值</param>
        /// <returns></returns>
        public bool AddEntrustResultItemWeightOffset(int inputDataIndex, int resultId, int changeWeight)
        {
            if (!GetEntrustResultItem(inputDataIndex, resultId, out EntrustResultItem entrustResultItem)) return false;

            entrustResultItem.WeightOffsetAdd(changeWeight);

            return true;
        }

        /// <summary>
        /// 设置委托基础权重值
        /// </summary>
        /// <param name="inputDataIndex">委托输入数据下标</param>
        /// <param name="resultId">结果Id</param>
        /// <param name="weightBase">基础权重值</param>
        /// <returns></returns>
        public bool SetEntrustResultItemWeightBase(int inputDataIndex, int resultId, int weightBase)
        {
            if (!GetEntrustResultItem(inputDataIndex, resultId, out EntrustResultItem entrustResultItem)) return false;

            entrustResultItem.SetWeightBase(weightBase);

            return true;
        }
    }
}