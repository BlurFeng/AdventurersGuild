using System;
using System.Collections.Generic;

namespace EntrustSystem
{
    /// <summary>
    /// 外部不允许直接获取EntrustItem，通过EntrustItemHandle来进行沟通
    /// </summary>
    /// 
    [ES3Serializable]
    public class EntrustItemHandler
    {
        public EntrustItemHandler(EntrustItem entrustItem)
        {
            m_EntrustItem = entrustItem;
        }

        private EntrustItem m_EntrustItem;

        #region Venturer

        //一个委托需要冒险者加入队伍且满足人数条件后才能开始
        //VenturerNumMust + VenturerNumOptional为队伍的最大人数
        //VenturerNumMust为必须满足的人数
        //VenturerNumOptional为可选择的人数 也可以不分配冒险者

        /// <summary>
        /// 冒险者人数 必须要的人数
        /// </summary>
        public int VenturerNumMust { get { return m_EntrustItem.EntrustItemData.venturerNumMust; } }

        /// <summary>
        /// 冒险者人数 可选人数
        /// </summary>
        public int VenturerNumOptional { get { return m_EntrustItem.EntrustItemData.venturerNumOptional; } }

        /// <summary>
        /// 冒险者人数最大值 等于必须人数加可选人数
        /// </summary>
        public int VenturerNumMax { get { return m_EntrustItem.VenturerNumMax; } }

        /// <summary>
        /// 当前冒险者队伍人数
        /// </summary>
        public int VenturerTeamNum { get { return m_EntrustItem.VenturerTeamNum; } }

        /// <summary>
        /// 冒险者队伍是否已满
        /// </summary>
        public bool VenturerTeamIsFull { get { return m_EntrustItem.VenturerTeamIsFull; } }

        /// <summary>
        /// 获取冒险者队伍
        /// </summary>
        /// <returns></returns>
        public int[] GetVenturerTeam()
        {
            return m_EntrustItem.GetVenturerTeam();
        }

        /// <summary>
        /// 队伍中是否包含某个冒险者
        /// </summary>
        /// <param name="venturerId"></param>
        /// <returns></returns>
        public bool ContainsVenturer(int venturerId)
        {
            return m_EntrustItem.ContainsVenturer(venturerId);
        }

        #endregion

        #region Result

        /// <summary>
        /// 获取委托结果
        /// </summary>
        /// <param name="outEntrustResultInfo"></param>
        /// <returns></returns>
        public bool GetEntrustResult(out EntrustResultInfo outEntrustResultInfo)
        {
            return m_EntrustItem.GetEntrustResult(out outEntrustResultInfo);
        }

        #endregion

        #region Condition

        /// <summary>
        /// 拥有委托条件
        /// </summary>
        /// <returns></returns>
        public bool IsHaveCondition { get { return m_EntrustItem.IsHaveCondition; } }

        /// <summary>
        /// 获取条件自定义参数
        /// 此参数在生成委托项目EntrustItem并调用SetConditionData时传入
        /// </summary>
        /// <returns></returns>
        public string GetConditionCustomParams()
        {
            if (m_EntrustItem.EntrustCondition == null) return string.Empty;
            return m_EntrustItem.EntrustCondition.CustomParams;
        }

        /// <summary>
        /// 确认必须条件是否达成
        /// </summary>
        /// <returns></returns>
        public bool CheckMustCondition(object customData = null)
        {
            if (m_EntrustItem.EntrustCondition == null) return false;
            return m_EntrustItem.EntrustCondition.CheckMustCondition(customData);
        }

        /// <summary>
        /// 遍历条件项目，并按需执行操作方法
        /// </summary>
        /// <param name="func"></param>
        public void ForeachCondition(EntrustCondition.EntrustConditionItemHandlerFuc func)
        {
            if (m_EntrustItem.EntrustCondition == null) return;
            m_EntrustItem.EntrustCondition.ForeachCondition(func);
        }

        /// <summary>
        /// 所有条件项目进行一次计算
        /// </summary>
        /// <param name="weightTotal">总权重值</param>
        /// <param name="achievingRateWeightNumTotal">达成率累加值</param>
        /// <param name="checkEntrustItemConditionFactorsChange">是否确认条件因素变化，仅在条件因素变化时进行计算。</param>
        /// <returns>有有效的条件可执行计算</returns>
        public bool ConditionItemsAchievingRateCalculate(out float weightTotal, out float achievingRateWeightNumTotal, object customData = null, bool checkEntrustItemConditionFactorsChange = true)
        {
            if (m_EntrustItem.EntrustCondition == null)
            {
                weightTotal = 0f; achievingRateWeightNumTotal = 0f;
                return false;
            }

            return m_EntrustItem.EntrustCondition.ConditionItemsAchievingRateCalculate(customData, out weightTotal, out achievingRateWeightNumTotal, checkEntrustItemConditionFactorsChange);
        }

        /// <summary>
        /// 处理某一个条件
        /// </summary>
        /// <param name="conditionNameId"></param>
        /// <param name="func"></param>
        public void HandlerCondition(int conditionNameId, EntrustCondition.EntrustConditionItemHandlerFuc func)
        {
            if (m_EntrustItem.EntrustCondition == null) return;
            m_EntrustItem.EntrustCondition.HandlerCondition(conditionNameId, func);
        }

        /// <summary>
        /// 获得所有条件的NameId数组
        /// </summary>
        /// <param name="nameIds"></param>
        /// <returns></returns>
        public bool GetConditionNameIds(out int[] nameIds)
        {
            if (m_EntrustItem.EntrustCondition == null)
            {
                nameIds = null;
                return false;
            }
            return m_EntrustItem.EntrustCondition.GetConditionNameIds(out nameIds);
        }
        #endregion

        /// <summary>
        /// 委托状态
        /// </summary>
        public EEntrustState State { get { return m_EntrustItem.State; } }

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get { return m_EntrustItem.EntrustItemData.nameId; } }

        /// <summary>
        /// 委托类型 由项目定义
        /// 比如 调查 护卫 讨伐 捕捉 获取 杂务 等类型的委托
        /// </summary>
        public int Type { get { return m_EntrustItem.EntrustItemData.type; } }

        /// <summary>
        /// 等级
        /// 用于定义这个委托的难度
        /// </summary>
        public int Rank { get { return m_EntrustItem.EntrustItemData.rank; } }

        //时间用int回合来计数 具体对回合的定义由项目决定

        /// <summary>
        /// 委托的时间限制
        /// </summary>
        public int RoundsLimit { get { return m_EntrustItem.EntrustItemData.roundsLimit; } }

        /// <summary>
        /// 剩余回合数
        /// 不会小于0
        /// </summary>
        public int RoundsRemaining
        {
            get
            {
                int round = m_EntrustItem.EntrustItemData.roundsLimit - m_EntrustItem.RoundsCounter;
                return round >= 0 ? round : 0;
            }
        }

        /// <summary>
        /// 完成委托需要的回合时间
        /// </summary>
        public int RoundsNeedBase { get { return m_EntrustItem.EntrustItemData.roundsNeedBase; } }

        /// <summary>
        /// 是否允许进行多次尝试
        /// 如果执行失败了 但是时间限制还没到 那么允许再次进行
        /// </summary>
        public bool CanTryMultipleTimes { get { return m_EntrustItem.EntrustItemData.canTryMultipleTimes; } }

        /// <summary>
        /// 标题名称
        /// 标题内容 或者是标题的多语言Key
        /// </summary>
        public string Title { get { return m_EntrustItem.EntrustItemData.title; } }

        /// <summary>
        /// 简单描述
        /// 描述内容 或者是描述的多语言Key
        /// </summary>
        public string DescribeSimple { get { return m_EntrustItem.EntrustItemData.describeSimple; } }

        /// <summary>
        /// 描述
        /// 描述内容 或者是描述的多语言Key
        /// </summary>
        public string Describe { get { return m_EntrustItem.EntrustItemData.describe; } }

        /// <summary>
        /// 委托进行中进度 0-1
        /// </summary>
        public float UnderwayProgress { get { return m_EntrustItem.UnderwayProgress; } }

        /// <summary>
        /// 确认是否能开始委托
        /// </summary>
        /// <returns></returns>
        public bool CheckCanStart()
        {
            if (State != EEntrustState.WaitDistributed) return false;

            //冒险者队伍人数少于必要人数
            if (GetVenturerTeam().Length < VenturerNumMust) return false;

            return true;
        }


        #region Action

        /// <summary>
        /// 绑定一个委托到 OnStateChange 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStateChange(Action<int, EEntrustState, EEntrustState> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithStateChange(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnRoundsChange 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithRoundsChange(Action<int, EEntrustState, int, int, bool> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithRoundsChange(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnStart 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStart(Action<int> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithStart(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnComplete 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithComplete(Action<int, int> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithComplete(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnStatement 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStatement(Action<int> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithStatement(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnTimeout 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithTimeout(Action<int, EEntrustState> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithTimeout(bindDelegate, bind);
        }

        /// <summary>
        /// 绑定一个委托到 OnVenturerAddOrRemove 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithVenturerAddOrRemove(Action<int, bool, int, int> bindDelegate, bool bind = true)
        {
            m_EntrustItem.BindEventWithVenturerAddOrRemove(bindDelegate, bind);
        }

        #endregion
    }
}