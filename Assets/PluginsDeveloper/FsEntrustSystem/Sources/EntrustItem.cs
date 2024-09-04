using System;
using System.Collections.Generic;

namespace EntrustSystem
{
    /// <summary>
    /// 委托项目初始化数据结构
    /// 项目中的静态数据需要映射到此结构 用于生成委托项目
    /// </summary>
    public struct EntrustItemData
    {
        public bool isValid;

        public int nameId;
        public string title;
        public string describeSimple;
        public string describe;
        public int type;
        public int rank;
        public int roundsLimit;
        public int roundsNeedBase;
        public bool canTryMultipleTimes;
        /// <summary>
        /// 冒险者必要人数
        /// </summary>
        public int venturerNumMust;
        /// <summary>
        /// 冒险者可选人数
        /// </summary>
        public int venturerNumOptional;

        public EntrustItemData(int nameId, string title, string describeSimple, string describe, int type, int rank, int roundsLimit, int roundsNeedBase, bool canTryMultipleTimes, int venturerNumMust, int venturerNumOptional)
        {
            isValid = true;

            this.nameId = nameId;
            this.title = title;
            this.describeSimple = describeSimple;
            this.describe = describe;
            this.type = type;
            this.rank = rank;
            this.roundsLimit = roundsLimit;
            this.roundsNeedBase = roundsNeedBase;
            this.canTryMultipleTimes = canTryMultipleTimes;
            this.venturerNumMust = venturerNumMust;
            this.venturerNumOptional = venturerNumOptional;
        }
    }

    /// <summary>
    /// 委托项目
    /// 最基础 也是最核心的类型 代表一个可被分配，进行和完成的委托
    /// </summary>
    public class EntrustItem
    {
        /// <summary>
        /// EntrustItem沟通用类
        /// </summary>
        public EntrustItemHandler EntrustItemHandler { get; private set; }

        /// <summary>
        /// 委托数据 在初始化时设置
        /// </summary>
        public EntrustItemData EntrustItemData { get; private set; }

        /// <summary>
        /// 委托状态
        /// 未受理->已受理(未分配冒险者并开始执行)->进行中->完成(失败)
        /// </summary>
        public EEntrustState State { get; private set; }

        /// <summary>
        /// 生命周期回合计时器
        /// </summary>
        public int RoundsCounter { get; private set; }

        /// <summary>
        /// 执行任务花费回合计时器
        /// </summary>
        private int m_RoundsCounter_Underway;

        /// <summary>
        /// 委托进行中进度 0-1
        /// </summary>
        public float UnderwayProgress { get { return m_RoundsCounter_Underway / (GetRoundsNeed() * 1f); } }

        public EntrustItem()
        {
            EntrustItemHandler = new EntrustItemHandler(this);
            m_VenturerTeam = new List<int>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="entrustInitData">初始化数据</param>
        public void Init(EntrustItemData entrustInitData)
        {
            if (!entrustInitData.isValid) return;

            EntrustItemData = entrustInitData;
        }

        /// <summary>
        /// 开始执行委托
        /// </summary>
        /// <returns></returns>
        public bool Start(object customData)
        {
            if (EntrustCondition != null && !EntrustCondition.CheckMustCondition(customData)) return false;//必要条件未达成
            if (State != EEntrustState.WaitDistributed) return false;

            SetState(EEntrustState.Underway);

            return true;
        }

        /// <summary>
        /// 取消执行委托
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            if (State != EEntrustState.Underway) return false;

            SetState(EEntrustState.WaitDistributed);

            return true;
        }

        /// <summary>
        /// 更新回合 进行一回合
        /// </summary>
        public void RoundUpdate()
        {
            RoundsCounter++;

            //结算的委托在回合进行时自动进入销毁状态
            //一般结算和销毁应当有业务掌握时机进行
            if (State == EEntrustState.Statement)
                SetState(EEntrustState.Destroy);

            //已经进行到结束的状态
            if ((int)State > 3) return;

            //超过时间限制
            bool isTimeout = RoundsCounter > EntrustItemData.roundsLimit;

            //正在进行中
            if (State == EEntrustState.Underway)
            {
                m_RoundsCounter_Underway++;
            }

            //超过时间限制 设置状态为超时
            if (isTimeout)
            {
                switch (State)
                {
                    case EEntrustState.None:
                    case EEntrustState.Unaccepted:
                    case EEntrustState.WaitDistributed:
                    case EEntrustState.Underway:
                        //设置为超时
                        SetState(EEntrustState.Timeout);
                        break;
                    case EEntrustState.Complete:
                    case EEntrustState.Statement:
                        //完成或结算的不设置到超时
                        break;
                    default:
                        break;
                }
            }

            //进行中回合达到要求回合
            if (m_RoundsCounter_Underway == GetRoundsNeed())
            {
                SetState(EEntrustState.Complete);
            }

            m_OnRoundsChange?.Invoke(EntrustItemData.nameId, State, RoundsCounter, m_RoundsCounter_Underway, isTimeout);
        }

        /// <summary>
        /// 获取执行一次委托需要的时间
        /// </summary>
        public int GetRoundsNeed()
        {
            return EntrustItemData.roundsNeedBase;
        }

        /// <summary>
        /// 设置委托状态
        /// </summary>
        /// <param name="newState"></param>
        public void SetState(EEntrustState newState)
        {
            if (State == newState) return;

            var oldState = State;
            State = newState;

            switch (oldState)
            {
                case EEntrustState.None:
                    break;
                case EEntrustState.Unaccepted:
                    break;
                case EEntrustState.WaitDistributed:
                    break;
                case EEntrustState.Underway:
                    m_RoundsCounter_Underway = 0;//清空执行中回合计时器
                    break;
                case EEntrustState.Complete:
                    break;
                case EEntrustState.Statement:
                    break;
                case EEntrustState.Timeout:
                    break;
                case EEntrustState.Destroy:
                    break;
                default:
                    break;
            }

            switch (State)
            {
                case EEntrustState.None:
                    break;
                case EEntrustState.Unaccepted:
                    break;
                case EEntrustState.WaitDistributed:
                    break;
                case EEntrustState.Underway:
                    m_OnStart?.Invoke(EntrustItemData.nameId);
                    break;
                case EEntrustState.Complete:
                    CalculatingResult();
                    //完成后的委托需要业务层进行结算并设置到结算状态
                    m_OnComplete?.Invoke(EntrustItemData.nameId, m_RoundsCounter_Underway);
                    break;
                case EEntrustState.Statement:
                    m_OnStatement?.Invoke(EntrustItemData.nameId);
                    break;
                case EEntrustState.Timeout:
                    m_OnTimeout?.Invoke(EntrustItemData.nameId, State);
                    break;
                case EEntrustState.Destroy:
                    break;
                default:
                    break;
            }

            m_OnStateChange?.Invoke(EntrustItemData.nameId, oldState, State);
        }

        /// <summary>
        /// 确认是否能开始委托
        /// </summary>
        /// <returns></returns>
        public bool CheckCanStart()
        {
            if (State != EEntrustState.WaitDistributed) return false;

            //冒险者队伍人数少于必要人数
            int teamCount = m_VenturerTeam.Count;
            if (teamCount < EntrustItemData.venturerNumMust) return false;

            return true;
        }

        #region Venturer

        /// <summary>
        /// 冒险者队伍 记录参加此委托的冒险者Id
        /// </summary>
        private List<int> m_VenturerTeam;

        private int m_VenturerNumMax;

        /// <summary>
        /// 冒险者人数最大值 等于必须人数加可选人数
        /// </summary>
        public int VenturerNumMax
        {
            get
            {
                if (m_VenturerNumMax == 0)
                    m_VenturerNumMax = EntrustItemData.venturerNumMust + EntrustItemData.venturerNumOptional;
                return m_VenturerNumMax;
            }
        }

        /// <summary>
        /// 当前冒险者队伍人数
        /// </summary>
        public int VenturerTeamNum{get{ return m_VenturerTeam.Count; } }

        /// <summary>
        /// 冒险者队伍是否已满
        /// </summary>
        public bool VenturerTeamIsFull { get { return VenturerTeamNum >= VenturerNumMax; } }

        /// <summary>
        /// 设置冒险者队伍
        /// 如果没有在队伍中则加入队伍，已经在队伍中则移出队伍
        /// </summary>
        /// <param name="venturerId">冒险者Id</param>
        /// <param name="addOrRemove">0=自动 1=添加 2=移除</param>
        public bool SetVenturerTeam(int venturerId, int addOrRemove = 0)
        {
            if (VenturerNumMax == 0) return false;

            //确认是添加还是移除一个冒险者
            bool add;
            bool contains = false;
            int venturerIndex = 0;

            for (int i = 0; i < m_VenturerTeam.Count; i++)
            {
                if (m_VenturerTeam[i] == venturerId)
                {
                    contains = true;
                    venturerIndex = i;
                    break;
                }
            }

            //自动模式，在队伍中就执行移除，没在队伍中就执行添加
            if (addOrRemove == 0) add = !contains;
            //执行要求的行为
            else add = addOrRemove == 1;

            if (add && (State != EEntrustState.WaitDistributed || m_VenturerTeam.Count >= VenturerNumMax)) return false; //已经达到上限
            if (add && contains || !add && !contains) return false;

            //在调用事件通知前更新，因为在事件中可能有业务逻辑执行使用到次信息
            m_VenturerTeamChange = true;

            //执行冒险者的添加和移除
            if (add)
            {
                //添加冒险者 到队伍中
                m_VenturerTeam.Add(venturerId);
                m_OnVenturerAddOrRemove?.Invoke(EntrustItemData.nameId, true, m_VenturerTeam.Count - 1, venturerId);
            }
            else
            {
                //移除冒险者 从队伍中
                m_VenturerTeam.Remove(venturerId);
                m_OnVenturerAddOrRemove?.Invoke(EntrustItemData.nameId, false, venturerIndex, venturerId);
            }

            return true;
        }

        /// <summary>
        /// 清空冒险者队伍
        /// </summary>
        /// <returns></returns>
        public bool ClearVenturerTeam()
        {
            if (m_VenturerTeam.Count == 0) return false;

            //在调用事件通知前更新，因为在事件中可能有业务逻辑执行使用到次信息
            m_VenturerTeamChange = true;

            for (int i = m_VenturerTeam.Count - 1; i >= 0; i--)
            {
                SetVenturerTeam(m_VenturerTeam[i], 2);
            }
            
            return true;
        }

        /// <summary>
        /// 队伍中是否包含某个冒险者
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsVenturer(int id)
        {
            return m_VenturerTeam.Contains(id);
        }

        /// <summary>
        /// 获取冒险者队伍
        /// </summary>
        /// <returns></returns>
        public int[] GetVenturerTeam()
        {
            return m_VenturerTeam.ToArray();
        }
        #endregion

        #region Entrust Result
        //如果委托需要结果数据，需要在委托完成之前调用SetResultData设置结果数据
        //在委托完成时，会生成结果数据供项目业务使用

        private EntrustResult m_EntrustResult;

        public void SetResultData(EntrustResultInputData[] entrustResultInputDatas)
        {
            if (m_EntrustResult == null) m_EntrustResult = new EntrustResult();

            m_EntrustResult.SetData(entrustResultInputDatas);
        }

        private void CalculatingResult()
        {
            if (m_EntrustResult == null) return;

            m_EntrustResult.ComputedResult();
        }

        public bool GetEntrustResult(out EntrustResultInfo outEntrustResultInfo)
        {
            if(m_EntrustResult == null)
            {
                outEntrustResultInfo = new EntrustResultInfo();
                return false;
            }

            return m_EntrustResult.GetEntrustResult(out outEntrustResultInfo);
        }

        public bool AddEntrustResultItemWeightOffset(int inputDataIndex, int resultId, int changeWeight)
        {
            if (m_EntrustResult == null) return false;

            return m_EntrustResult.AddEntrustResultItemWeightOffset(inputDataIndex, resultId, changeWeight);
        }

        public bool SetEntrustResultItemWeightBase(int inputDataIndex, int resultId, int weightBase)
        {
            if (m_EntrustResult == null) return false;

            return m_EntrustResult.SetEntrustResultItemWeightBase(inputDataIndex, resultId, weightBase);
        }
        #endregion

        #region Entrust Condition

        public EntrustCondition EntrustCondition { get; set; }

        public bool IsHaveCondition { get { return EntrustCondition != null; } }

        /// <summary>
        /// 确认条件相关因素是否变化
        /// 每次获取后，此值将重置为false
        /// 比如冒险者队伍成员是否变化等
        /// </summary>
        public bool IsConditionFactorsChange
        {
            get
            {
                if(m_VenturerTeamChange)
                {
                    m_VenturerTeamChange = false;
                    return true;
                }

                return false;
            }
        }

        private bool m_VenturerTeamChange;

        /// <summary>
        /// 设置条件数据
        /// 我们一般在委托创建时设置条件数据
        /// </summary>
        /// <param name="entrustConditionItems"></param>
        public void SetConditionData(EntrustConditionItem[] entrustConditionItems, string customParams = "")
        {
            if (entrustConditionItems == null || entrustConditionItems.Length == 0) return;

            EntrustCondition = new EntrustCondition();
            EntrustCondition.Init(this, entrustConditionItems, customParams);
        }

        #endregion

        #region Action

        /// <summary>
        /// 当委托状态改变时
        /// 参数：Id，旧状态，新状态
        /// </summary>
        private Action<int, EEntrustState, EEntrustState> m_OnStateChange;

        /// <summary>
        /// 当回合变化时
        /// 参数：NameId，当前状态，存在回合，进行回合，是否超过限制回合
        /// </summary>
        private Action<int, EEntrustState, int, int, bool> m_OnRoundsChange;

        /// <summary>
        /// 当委托开始时
        /// 参数：NameId，
        /// </summary>
        private Action<int> m_OnStart;

        /// <summary>
        /// 当委托完成时 (进行回合达到进行一次委托需要的回合
        /// 参数：NameId，花费回合
        /// </summary>
        private Action<int, int> m_OnComplete;

        /// <summary>
        /// 当、委托结算时 (进行回合达到进行一次委托需要的回合
        /// 参数：NameId，
        /// </summary>
        private Action<int> m_OnStatement;

        /// <summary>
        /// 当超出时间限制
        /// 参数：NameId，当前状态
        /// </summary>
        private Action<int, EEntrustState> m_OnTimeout;

        /// <summary>
        /// 当冒险者添加或移除 从冒险者队伍
        /// 参数：NameId，true=添加 false=移除，冒险者所在位置下标 从0开始, 冒险者Id
        /// </summary>
        private Action<int, bool, int, int> m_OnVenturerAddOrRemove;

        /// <summary>
        /// 绑定一个委托到 OnStateChange 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStateChange(Action<int, EEntrustState, EEntrustState> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnStateChange += bindDelegate;
            }
            else
            {
                m_OnStateChange -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnRoundsChange 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithRoundsChange(Action<int, EEntrustState, int, int, bool> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnRoundsChange += bindDelegate;
            }
            else
            {
                m_OnRoundsChange -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnStart 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStart(Action<int> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnStart += bindDelegate;
            }
            else
            {
                m_OnStart -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnComplete 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithComplete(Action<int, int> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnComplete += bindDelegate;
            }
            else
            {
                m_OnComplete -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnStatement 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithStatement(Action<int> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnStatement += bindDelegate;
            }
            else
            {
                m_OnStatement -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnTimeout 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithTimeout(Action<int, EEntrustState> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnTimeout += bindDelegate;
            }
            else
            {
                m_OnTimeout -= bindDelegate;
            }
        }

        /// <summary>
        /// 绑定一个委托到 OnVenturerAddOrRemove 事件
        /// </summary>
        /// <param name="bindDelegate"></param>
        /// <param name="bind">绑定或解绑</param>
        public void BindEventWithVenturerAddOrRemove(Action<int, bool, int, int> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnVenturerAddOrRemove += bindDelegate;
            }
            else
            {
                m_OnVenturerAddOrRemove -= bindDelegate;
            }
        }
        #endregion
    }
}