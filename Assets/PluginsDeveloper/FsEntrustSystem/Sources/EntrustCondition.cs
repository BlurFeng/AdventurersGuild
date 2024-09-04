using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace EntrustSystem
{
    /// <summary>
    /// 委托条件项目
    /// 一个委托条件会有一个或多个条件项目
    /// </summary>
    public class EntrustConditionItem
    {
        private static Dictionary<EntrustConditionKey, IEntrustCondition> mIEntrustConditionDic = new Dictionary<EntrustConditionKey, IEntrustCondition>();

        private EntrustItem mEntrustItem;

        /// <summary>
        /// Id可用于查询和获取，定义一个Item。
        /// 此Id一般是静态的，是静态配置表中的唯一Id
        /// Id的唯一性由创建者保证
        /// </summary>
        public int NameId { get; protected set; }

        /// <summary>
        /// 委托条件接口
        /// 由项目实现具体内容
        /// </summary>
        public IEntrustCondition condition;

        /// <summary>
        /// 权重
        /// </summary>
        public int weight;

        /// <summary>
        /// 是必须达成的条件
        /// </summary>
        public bool isMust;

        /// <summary>
        /// 是隐藏条件
        /// 隐藏条件和其他条件在功能上没有实际区别，但项目可按需让玩家不可见这些条件，或达成一些其他的业务需求
        /// </summary>
        public bool isHidden;

        public EntrustConditionItem(EntrustItem entrustItem, string entrustConditionClassName, string entrustConditionParams, int entrustConditionNameId, int weight, bool isMust = false, bool isHidden = false)
        {
            this.mEntrustItem = entrustItem;
            this.NameId = entrustConditionNameId;
            this.weight = weight;
            this.isMust = isMust;
            this.isHidden = isHidden;

            //获取条件接口实例
            Type conditionType = Type.GetType(entrustConditionClassName);
            if (conditionType != null)
            {
                EntrustConditionKey key = new EntrustConditionKey(conditionType, entrustConditionParams);
                if (mIEntrustConditionDic.ContainsKey(key))
                {
                    this.condition = mIEntrustConditionDic[key];
                }
                else
                {
                    IEntrustCondition entrustConditionBase = Activator.CreateInstance(conditionType) as IEntrustCondition;
                    if (entrustConditionBase != null)
                    {
                        entrustConditionBase.Parse(entrustConditionParams);
                        this.condition = entrustConditionBase;
                    }
                }
            }
        }

        public bool CheckCondition(object customData)
        {
            return condition.CheckCondition(mEntrustItem.EntrustItemHandler, customData);
        }

        public float GetAchievingRate(object customData)
        {
            return condition.GetAchievingRate(mEntrustItem.EntrustItemHandler, customData);
        }
    }

    /// <summary>
    /// 委托条件，允许给委托配置条件
    /// 条件可用于限制委托能否开始，或者用于计算成功率等
    /// </summary>
    public class EntrustCondition
    {
        public delegate void EntrustConditionItemHandlerFuc(EntrustConditionItem entrustConditionItem);

        //委托条件属于委托项目，一个委托项目会有委托条件（也可能没有）
        //多个委托条件的完成系数和权重等参数，影响委托最终的成功率或结果（这由项目决定）
        //我们也可以在委托进行中，根据触发的一些事件，来动态调整一些条件的内容

        //一般我们在静态配置表中配置一个委托项目的条件Map（条件逻辑子类和参数），在运行时读取配置来初始化委托项目的EntrustCondition内容

        private EntrustItem m_EntrustItem;

        private EntrustConditionItem[] m_EntrustConditionItems;

        private float m_WeightTotal = 0f;
        private float m_AchievingRateWeightNumTotal = 0f;

        /// <summary>
        /// 自定义的参数
        /// </summary>
        public string CustomParams { get; private set; }

        public void Init(EntrustItem entrustItem, EntrustConditionItem[] entrustConditionItems, string customParams = "")
        {
            m_EntrustItem = entrustItem;

            m_EntrustConditionItems = entrustConditionItems;
            CustomParams = customParams;
        }

        /// <summary>
        /// 确认必须条件是否都达成
        /// </summary>
        /// <returns></returns>
        public bool CheckMustCondition(object customData)
        {
            if (m_EntrustItem == null) return false;
            if (m_EntrustConditionItems == null || m_EntrustConditionItems.Length == 0) return true;

            for (int i = 0; i < m_EntrustConditionItems.Length; i++)
            {
                var item = m_EntrustConditionItems[i];
                if (!item.isMust) continue;
                if (!item.CheckCondition(customData))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 遍历条件项目，并按需执行操作方法
        /// </summary>
        /// <param name="func"></param>
        public void ForeachCondition(EntrustConditionItemHandlerFuc func)
        {
            for (int i = 0; i < m_EntrustConditionItems.Length; i++)
            {
                func(m_EntrustConditionItems[i]);
            }
        }

        /// <summary>
        /// 处理某一个条件
        /// </summary>
        /// <param name="conditionNameId"></param>
        /// <param name="func"></param>
        public void HandlerCondition(int conditionNameId, EntrustConditionItemHandlerFuc func)
        {
            if (!GetConditonItem(conditionNameId, out EntrustConditionItem conditionItem)) return;
            func(conditionItem);
        }

        /// <summary>
        /// 处理某一个条件
        /// </summary>
        /// <param name="index"></param>
        /// <param name="func"></param>
        public void HandlerConditionByIndex(int index, EntrustConditionItemHandlerFuc func)
        {
            if (index < 0 || index >= m_EntrustConditionItems.Length) return;
            func(m_EntrustConditionItems[index]);
        }

        /// <summary>
        /// 所有条件项目进行一次计算
        /// </summary>
        /// <param name="weightTotal">总权重值</param>
        /// <param name="achievingRateWeightNumTotal">达成率累加值</param>
        /// <param name="checkEntrustItemConditionFactorsChange">是否确认条件因素变化，仅在条件因素变化时进行计算。</param>
        /// <returns>有有效的条件可执行计算</returns>
        public bool ConditionItemsAchievingRateCalculate(object customData, out float weightTotal, out float achievingRateWeightNumTotal, bool checkEntrustItemConditionFactorsChange = true)
        {
            if(m_EntrustConditionItems == null || m_EntrustConditionItems.Length == 0)
            {
                weightTotal = 0f; achievingRateWeightNumTotal = 0f;
                return false;
            }

            bool execute = !checkEntrustItemConditionFactorsChange || (m_EntrustItem != null && m_EntrustItem.IsConditionFactorsChange);

            //执行条件达成率计算
            if(execute)
            {
                m_WeightTotal = 0f; m_AchievingRateWeightNumTotal = 0f;

                for (int i = 0; i < m_EntrustConditionItems.Length; i++)
                {
                    EntrustConditionItem item = m_EntrustConditionItems[i];
                    m_WeightTotal += item.weight;
                    m_AchievingRateWeightNumTotal += item.GetAchievingRate(customData) * item.weight;
                }
            }

            weightTotal = m_WeightTotal; achievingRateWeightNumTotal = m_AchievingRateWeightNumTotal;
            return true;
        }

        /// <summary>
        /// 通过条件NameId获取条件实例
        /// </summary>
        /// <param name="conditionNameId"></param>
        /// <param name="conditionItem"></param>
        /// <returns></returns>
        public bool GetConditonItem(int conditionNameId, out EntrustConditionItem conditionItem)
        {
            if (m_EntrustConditionItems == null || m_EntrustConditionItems.Length == 0)
            {
                conditionItem = null;
                return false;
            }

            for (int i = 0; i < m_EntrustConditionItems.Length; i++)
            {
                if(m_EntrustConditionItems[i].NameId == conditionNameId)
                {
                    conditionItem = m_EntrustConditionItems[i];
                    return true;
                }
            }

            conditionItem = null;
            return false;
        }

        /// <summary>
        /// 获得所有条件的NameId数组
        /// </summary>
        /// <param name="nameIds"></param>
        /// <returns></returns>
        public bool GetConditionNameIds(out int[] nameIds)
        {
            if (m_EntrustConditionItems == null || m_EntrustConditionItems.Length == 0)
            {
                nameIds = null;
                return false;
            }

            nameIds = new int[m_EntrustConditionItems.Length];
            for (int i = 0; i < m_EntrustConditionItems.Length; i++)
            {
                nameIds[i] = m_EntrustConditionItems[i].NameId;
            }

            return true;
        }
    }
}