using System.Collections.Generic;
using UnityEngine;

namespace EntrustSystem
{
    /// <summary>
    /// 委托系统管理器
    /// 按需求添加到项目的单例或静态管理器中
    /// </summary>
    public class EntrustSystemManager
    {
        /// <summary>
        /// 委托系统管理器配置
        /// </summary>
        EntrustSystemManagerConfig m_EntrustSystemManagerConfig = new EntrustSystemManagerConfig()
        {
            venturerWorkingOnlyOneEntrust = true,
            abortEntrust_ReturnUnacceptedOrDestroy = true,
        };

        /// <summary>
        /// 所有的委托项目
        /// </summary>
        private Dictionary<int, EntrustItem> m_EntrustItemDic;

        /// <summary>
        /// 所有委托项目Dic
        /// Key=Rank, Vaule=委托Id数组
        /// </summary>
        private Dictionary<int, List<int>> m_EntrustItemDic_Rank;

        /// <summary>
        /// 所有委托项目数组
        /// </summary>
        List<EntrustItem> m_EntrustItems_UnacceptedAndAcceptedCached;

        /// <summary>
        /// 未受理委托 世界池中的委托
        /// 可以选择进行受理
        /// </summary>
        private EntrustItemContent m_EntrustItemContent_Unaccepted;

        /// <summary>
        /// 受理的委托
        /// </summary>
        private EntrustItemContent m_EntrustItemContent_Accepted;

        /// <summary>
        /// 冒险者加入委托字典
        /// 用于记录冒险者当前加入了哪些队伍的信息
        /// Key=冒险者Id Vaule=委托Id
        /// </summary>
        private Dictionary<int, List<int>> m_VenturerJoinEntrustDic;

        /// <summary>
        /// 需要销毁的委托
        /// 当委托失败或者超出时限后 会被添加到此列表
        /// 在每次回合更新时会被销毁
        /// </summary>
        private List<int> m_EntrustItem_Destroy;

        /// <summary>
        /// 委托数量 包括在世界池和受理的委托
        /// </summary>
        public int EntrustNum { get { return m_EntrustItemDic.Count; } }

        /// <summary>
        /// 委托数量 在世界池中的
        /// </summary>
        public int EntrustNum_Unaccepted { get { return m_EntrustItemContent_Unaccepted.Count; } }

        /// <summary>
        /// 委托数量 受理的
        /// </summary>
        public int EntrustNum_Accepted { get { return m_EntrustItemContent_Accepted.Count; } }

        #region Public

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            m_EntrustItemDic = new Dictionary<int, EntrustItem>();
            m_EntrustItemDic_Rank = new Dictionary<int, List<int>>();
            m_EntrustItemContent_Unaccepted = new EntrustItemContent();
            m_EntrustItemContent_Accepted = new EntrustItemContent();
            m_EntrustItem_Destroy = new List<int>();
            m_VenturerJoinEntrustDic = new Dictionary<int, List<int>>();
            m_EntrustItems_UnacceptedAndAcceptedCached = new List<EntrustItem>();

            EntrustRandomSpawnInit();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="entrustSystemManagerConfig">委托系统配置</param>
        public void Init(EntrustSystemManagerConfig entrustSystemManagerConfig)
        {
            m_EntrustSystemManagerConfig = entrustSystemManagerConfig;

            Init();
        }

        /// <summary>
        /// 回合更新时
        /// </summary>
        public void RoundUpdate()
        {
            //回合更新，不直接使用Foreach因为我们无法保证业务逻辑不在RoundUpdate的各种广播中直接执行DestroyEntrustItem来改变m_EntrustItemDic
            int[] keys = new int[m_EntrustItemDic.Count];
            m_EntrustItemDic.Keys.CopyTo(keys, 0);
            for (int i = 0; i < keys.Length; i++)
            {
                m_EntrustItemDic[keys[i]].RoundUpdate();
            }

            if (m_EntrustItem_Destroy.Count > 0)
            {
                for (int i = 0; i < m_EntrustItem_Destroy.Count; i++)
                {
                    DestroyEntrustItem(m_EntrustItem_Destroy[i]);
                }

                m_EntrustItem_Destroy.Clear();
            }
        }

        /// <summary>
        /// 受理一个委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool AcceptedEntrust(int entrustId)
        {
            if(!GetEntrustItem(entrustId, out EntrustItem item)) return false;
            item.SetState(EEntrustState.WaitDistributed);
            return AddToAccepted(entrustId);
        }

        /// <summary>
        /// 开始一个委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool StartEntrust(int entrustId, object customData = null)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
                return false;

            //确认委托是否能开始
            if (!item.CheckCanStart()) return false;

            bool succeed = item.Start(customData);

            //配置冒险者仅允许在一个委托中工作时，将冒险者从其他委托中移除
            if (succeed && m_EntrustSystemManagerConfig.venturerWorkingOnlyOneEntrust)
            {
                var venturerTeam = item.GetVenturerTeam();
                if (venturerTeam != null)
                {
                    for (int i = 0; i < venturerTeam.Length; i++)
                    {
                        int ventureId = venturerTeam[i];
                        if (!m_VenturerJoinEntrustDic.ContainsKey(ventureId)) continue;

                        var joinEntrustIds = m_VenturerJoinEntrustDic[ventureId];
                        if (joinEntrustIds.Count <= 1) continue;

                        //冒险者从其他委托中离开
                        var joinEntrustIdsArray = joinEntrustIds.ToArray();
                        for (int j = 0; j < joinEntrustIdsArray.Length; j++)
                        {
                            if (joinEntrustIdsArray[j] == entrustId) continue;

                            RemoveVenturerFromEntrust(joinEntrustIdsArray[j], ventureId);
                        }
                    }
                }
            }

            return succeed;
        }

        /// <summary>
        /// 取消执行一个委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool CancelEntrust(int entrustId)
        {
            if (!ContainsEntrust(entrustId))
                return false;

            //进行中的委托才能取消
            if (m_EntrustItemContent_Accepted.Get(entrustId).State != EEntrustState.Underway) return false;

            return m_EntrustItemContent_Accepted.Get(entrustId).Cancel();
        }

        /// <summary>
        /// 结算委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool StatementEntrust(int entrustId)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem))
                return false;

            if (entrustItem.State != EEntrustState.Complete) return false;

            entrustItem.SetState(EEntrustState.Statement);
            return true;
        }

        /// <summary>
        /// 放弃一个委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool AbortEntrust(int entrustId)
        {
            //只能放弃已经受理的委托
            if (!ContainsEntrustInAccepted(entrustId))
                return false;

            //放弃的委托返回未受理池
            m_EntrustItemDic[entrustId].SetState(EEntrustState.Unaccepted);
            return RemoveFromAccepted(entrustId) && AddToUnaccepted(entrustId);
        }

        /// <summary>
        /// 销毁一个委托
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool DestroyEntrust(int entrustId)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
                return false;

            //将此委托中的冒险者移除出队伍
            var venturerTeam = item.GetVenturerTeam();
            if (venturerTeam.Length > 0)
            {
                for (int i = 0; i < venturerTeam.Length; i++)
                {
                    RemoveVenturerFromEntrust(entrustId, venturerTeam[i]);
                }
            }

            item.SetState(EEntrustState.Destroy);

            //主动放弃的委托直接销毁
            DestroyEntrustItem(entrustId);
            return true;
        }

        /// <summary>
        /// 添加一个冒险者到已受理委托队伍
        /// </summary>
        /// <param name="entrustId">委托Id</param>
        /// <param name="venturerId">冒险者Id</param>
        public bool AddVenturerToEntrust(int entrustId, int venturerId)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
                return false;

            if(!item.SetVenturerTeam(venturerId, 1)) return false;

            //记录冒险者和委托关系信息
            if (!m_VenturerJoinEntrustDic.ContainsKey(venturerId))
                m_VenturerJoinEntrustDic.Add(venturerId, new List<int>());
            m_VenturerJoinEntrustDic[venturerId].Add(entrustId);

            return true;
        }

        /// <summary>
        /// 移除一个冒险者从已受理委托队伍
        /// </summary>
        /// <param name="entrustId">委托Id</param>
        /// <param name="venturerId">冒险者Id</param>
        public bool RemoveVenturerFromEntrust(int entrustId, int venturerId)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
                return false;

            if (!item.SetVenturerTeam(venturerId, 2)) return false;

            //处理冒险者和委托关系信息
            if (m_VenturerJoinEntrustDic.ContainsKey(venturerId))
                m_VenturerJoinEntrustDic[venturerId].Remove(entrustId);

            return true;
        }

        /// <summary>
        /// 从所有的委托队伍中移除此冒险者Id
        /// </summary>
        /// <param name="venturerId"></param>
        /// <returns>如果有从任何队伍中移除则为true</returns>
        public bool RemoveVenturerInAllEntrust(int venturerId)
        {
            if (!m_VenturerJoinEntrustDic.ContainsKey(venturerId)) return false;

            var list = m_VenturerJoinEntrustDic[venturerId];

            if (list.Count == 0) return false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                RemoveVenturerFromEntrust(list[i], venturerId);
            }

            return true;
        }

        /// <summary>
        /// 某个冒险者Id是否在某个受理的委托的队伍中
        /// </summary>
        /// <param name="entrustId">委托Id</param>
        /// <param name="venturerId">冒险者Id</param>
        /// <returns></returns>
        public bool ContainsVenturerInEntrust(int entrustId, int venturerId)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
                return false;

            return item.ContainsVenturer(venturerId);
        }

        /// <summary>
        /// 确认冒险者是否正在工作中
        /// 冒险者处于Underway，Complete的委托时，认为冒险者正在工作中
        /// 此时冒险者一般被锁定在此委托
        /// </summary>
        /// <param name="venturerId"></param>
        /// <returns>是否处在工作中</returns>
        public bool CheckVenturerIsWorking(int venturerId)
        {
            if (!m_VenturerJoinEntrustDic.ContainsKey(venturerId)) return false;
            var list = m_VenturerJoinEntrustDic[venturerId];
            if (list.Count == 0) return false;

            for (int i = 0; i < list.Count; i++)
            {
                if (!GetEntrustItem(list[i], out EntrustItem item)) continue;
                if (item.State == EEntrustState.Underway || item.State == EEntrustState.Complete)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取委托Id，根据冒险者Id。
        /// 获取冒险者加入了那些队伍的信息
        /// </summary>
        /// <param name="venturerId"></param>
        /// <param name="entrustIds"></param>
        /// <returns></returns>
        public bool GetEntrustItemIdsByVenturerId(int venturerId, out int[] entrustIds)
        {
            if (!m_VenturerJoinEntrustDic.ContainsKey(venturerId))
            {
                entrustIds = null;
                return false;
            }

            entrustIds = m_VenturerJoinEntrustDic[venturerId].ToArray();

            return entrustIds.Length > 0;
        }


        /// <summary>
        /// 获取某个委托项目数据
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool GetEntrustItemData(int entrustId, out EntrustItemData entrustItemData)
        {
            if (!m_EntrustItemDic.ContainsKey(entrustId))
            {
                entrustItemData = new EntrustItemData();
                return false;
            }

            entrustItemData = m_EntrustItemDic[entrustId].EntrustItemData;
            return true;
        }

        /// <summary>
        /// 获取委托ID数组
        /// </summary>
        /// <param name="entrustPoolType">获取类型，需要获取那些委托</param>
        /// <returns></returns>
        public EntrustItemHandler[] GetEntrustHandlers(EEntrustPoolType entrustPoolType, out bool isDirty)
        {
            isDirty = false;
            EntrustItemHandler[] itemHandlers = null;

            switch (entrustPoolType)
            {
                case EEntrustPoolType.UnacceptedPool:
                    itemHandlers = m_EntrustItemContent_Unaccepted.GetEntrustItemHandlers(out isDirty);
                    break;
                case EEntrustPoolType.AcceptedPool:
                    itemHandlers = m_EntrustItemContent_Accepted.GetEntrustItemHandlers(out isDirty);
                    break;
                case EEntrustPoolType.UnacceptedAndAcceptedPool:
                    var itemsU = m_EntrustItemContent_Unaccepted.GetEntrustItems(out bool isDirty_U);
                    var itemsA = m_EntrustItemContent_Accepted.GetEntrustItems(out bool isDirty_A);
                    if (isDirty_U || isDirty_A)
                    {
                        isDirty = true;
                        m_EntrustItems_UnacceptedAndAcceptedCached.Clear();
                        if (itemsU != null && itemsU.Length > 0)
                            m_EntrustItems_UnacceptedAndAcceptedCached.AddRange(itemsU);
                        if (itemsA != null && itemsA.Length > 0)
                            m_EntrustItems_UnacceptedAndAcceptedCached.AddRange(itemsA);
                    }

                    if(m_EntrustItems_UnacceptedAndAcceptedCached.Count > 0)
                    {
                        itemHandlers = new EntrustItemHandler[m_EntrustItems_UnacceptedAndAcceptedCached.Count];
                        for (int i = 0; i < m_EntrustItems_UnacceptedAndAcceptedCached.Count; i++)
                        {
                            itemHandlers[i] = m_EntrustItems_UnacceptedAndAcceptedCached[i].EntrustItemHandler;
                        }
                    }
                    break;
            }

            return itemHandlers;
        }

        /// <summary>
        /// 获取某个委托项目
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        public bool GetEntrustItemHandle(int entrustId, out EntrustItemHandler entrustItemHandler)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem item))
            {
                entrustItemHandler = null;
                return false;
            }

            entrustItemHandler = item.EntrustItemHandler;

            return true;
        }

        /// <summary>
        /// 是否包含某个委托 在任何位置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsEntrust(int id)
        {
            return m_EntrustItemDic.ContainsKey(id);
        }

        /// <summary>
        /// 是否包含某个委托 在受理的委托中
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ContainsEntrustInAccepted(int id)
        {
            return m_EntrustItemContent_Accepted.Contains(id);
        }

        /// <summary>
        /// 获取冒险者队伍 根据委托Id
        /// </summary>
        /// <param name="entrustId">委托Id</param>
        /// <returns></returns>
        public int[] GetVenturerTeam(int entrustId)
        {
            if (!m_EntrustItemContent_Accepted.Contains(entrustId) || m_EntrustItemContent_Accepted.Get(entrustId).GetVenturerTeam().Length == 0)
                return null;

            return m_EntrustItemContent_Accepted.Get(entrustId).GetVenturerTeam();
        }

        /// <summary>
        /// 获取某个RankKey的委托数量
        /// </summary>
        /// <param name="rankKey"></param>
        /// <returns></returns>
        public int GetEntrustNumByRank(int rankKey)
        {
            if (!m_EntrustItemDic_Rank.ContainsKey(rankKey)) return 0;

            return m_EntrustItemDic_Rank[rankKey].Count;
        }

        #endregion

        #region Private

        /// <summary>
        /// 创建一个委托
        /// </summary>
        /// <param name="entrust_ItemConfig"></param>
        public void CreateEntrustItem(EntrustItem entrustItem)
        {
            if (entrustItem == null)
            {
                Debug.LogWarning("CreateEntrustItem  ---  entrustItem is null !!");
                return;
            }

            //不能添加多个相同Id的委托到池中
            if (ContainsEntrust(entrustItem.EntrustItemHandler.Id))
                return;

            entrustItem.BindEventWithStateChange(OnStateChange);
            entrustItem.BindEventWithRoundsChange(OnRoundsChange);
            entrustItem.BindEventWithComplete(OnEnd);
            entrustItem.BindEventWithTimeout(OnTimeout);

            m_EntrustItemDic.Add(entrustItem.EntrustItemHandler.Id, entrustItem);

            if (!m_EntrustItemDic_Rank.ContainsKey(entrustItem.EntrustItemHandler.Rank))
                m_EntrustItemDic_Rank.Add(entrustItem.EntrustItemHandler.Rank, new List<int>());
            m_EntrustItemDic_Rank[entrustItem.EntrustItemHandler.Rank].Add(entrustItem.EntrustItemHandler.Id);

            //创建的委托会先添加到世界池中
            switch (entrustItem.State)
            {
                case EEntrustState.None:
                case EEntrustState.Unaccepted:
                    //添加到世界委托池的委托 设置为未受理状态
                    entrustItem.SetState(EEntrustState.Unaccepted);
                    AddToUnaccepted(entrustItem.EntrustItemHandler.Id);
                    break;
                case EEntrustState.WaitDistributed:
                case EEntrustState.Underway:
                case EEntrustState.Complete:
                case EEntrustState.Statement:
                case EEntrustState.Timeout:
                case EEntrustState.Destroy:
                    AddToAccepted(entrustItem.EntrustItemHandler.Id);
                    break;
            }
        }

        /// <summary>
        /// 销毁一个委托
        /// </summary>
        /// <param name="id"></param>
        private bool DestroyEntrustItem(int id)
        {
            if (!ContainsEntrust(id))
                return false;

            if (m_EntrustItemDic_Rank.ContainsKey(m_EntrustItemDic[id].EntrustItemHandler.Rank))
                m_EntrustItemDic_Rank[m_EntrustItemDic[id].EntrustItemHandler.Rank].Remove(id);
            m_EntrustItemDic.Remove(id);

            RemoveFromUnaccepted(id);
            RemoveFromAccepted(id);

            return true;
        }

        /// <summary>
        /// 添加委托到世界池
        /// </summary>
        /// <param name="entrustItem"></param>
        private bool AddToUnaccepted(int id)
        {
            if (!ContainsEntrust(id))
                return false;

            return m_EntrustItemContent_Unaccepted.Add(id, m_EntrustItemDic[id]);
        }

        /// <summary>
        /// 移除委托从世界池
        /// </summary>
        /// <param name="id"></param>
        private bool RemoveFromUnaccepted(int id)
        {
            return m_EntrustItemContent_Unaccepted.Remove(id);
        }

        /// <summary>
        /// 添加委托到受理字典
        /// </summary>
        /// <param name="entrustItem"></param>
        private bool AddToAccepted(int entrustItem)
        {
            if (!ContainsEntrust(entrustItem))
                return false;

            m_EntrustItemContent_Accepted.Add(entrustItem, m_EntrustItemDic[entrustItem]);

            RemoveFromUnaccepted(entrustItem);//从未受理移除

            return true;
        }

        /// <summary>
        /// 移除委托从受理字典
        /// </summary>
        /// <param name="id"></param>
        private bool RemoveFromAccepted(int id)
        {
            return m_EntrustItemContent_Accepted.Remove(id);
        }

        /// <summary>
        /// 获取某个委托项目
        /// </summary>
        /// <param name="entrustId"></param>
        /// <returns></returns>
        private bool GetEntrustItem(int entrustId, out EntrustItem entrustItem)
        {
            if (!m_EntrustItemDic.ContainsKey(entrustId))
            {
                entrustItem = null;
                return false;
            }

            entrustItem = m_EntrustItemDic[entrustId];
            return true;
        }

        public EntrustItem[] GetEntrustItems()
        {
            EntrustItem[] entrustItems = new EntrustItem[m_EntrustItemDic.Values.Count];
            m_EntrustItemDic.Values.CopyTo(entrustItems, 0);
            return entrustItems;
        }

        /// <summary>
        /// 获取委托ID数组
        /// </summary>
        /// <param name="entrustPoolType">获取类型，需要获取那些委托</param>
        /// <returns></returns>
        private EntrustItem[] GetEntrusts(EEntrustPoolType entrustPoolType, out bool isDirty)
        {
            isDirty = false;

            switch (entrustPoolType)
            {
                case EEntrustPoolType.UnacceptedPool:
                    return m_EntrustItemContent_Unaccepted.GetEntrustItems(out isDirty);
                case EEntrustPoolType.AcceptedPool:
                    return m_EntrustItemContent_Accepted.GetEntrustItems(out isDirty);
                case EEntrustPoolType.UnacceptedAndAcceptedPool:
                    var itemsU = m_EntrustItemContent_Unaccepted.GetEntrustItems(out bool isDirty_U);
                    var itemsA = m_EntrustItemContent_Accepted.GetEntrustItems(out bool isDirty_A);
                    if (isDirty_U || isDirty_A)
                    {
                        isDirty = true;
                        m_EntrustItems_UnacceptedAndAcceptedCached.Clear();
                        if (itemsU != null && itemsU.Length > 0)
                            m_EntrustItems_UnacceptedAndAcceptedCached.AddRange(itemsU);
                        if (itemsA != null && itemsA.Length > 0)
                            m_EntrustItems_UnacceptedAndAcceptedCached.AddRange(itemsA);
                    }
                    return m_EntrustItems_UnacceptedAndAcceptedCached.ToArray();
            }

            return null;
        }

        #endregion

        #region Entrust Result

        public void SetResultData(int entrustId, EntrustResultInputData entrustResultInputData)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem)) return;

            EntrustResultInputData[] entrustResultInputDatas = new EntrustResultInputData[] { entrustResultInputData };
            entrustItem.SetResultData(entrustResultInputDatas);
        }

        public void SetResultData(int entrustId, EntrustResultInputData[] entrustResultInputDatas)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem)) return;

            entrustItem.SetResultData(entrustResultInputDatas);
        }

        public bool GetEntrustResult(int entrustId, out EntrustResultInfo outEntrustResultInfo)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem))
            {
                outEntrustResultInfo = new EntrustResultInfo();
                return false;
            }

            return entrustItem.GetEntrustResult(out outEntrustResultInfo);
        }

        public bool AddEntrustResultItemWeightOffset(int entrustId, int inputDataIndex, int resultId, int changeWeight)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem)) return false;

            return entrustItem.AddEntrustResultItemWeightOffset(inputDataIndex, resultId, changeWeight);
        }

        public bool SetEntrustResultItemWeightBase(int entrustId, int inputDataIndex, int resultId, int weightBase)
        {
            if (!GetEntrustItem(entrustId, out EntrustItem entrustItem)) return false;

            return entrustItem.SetEntrustResultItemWeightBase(inputDataIndex, resultId, weightBase);
        }
        #endregion

        #region EntrustItem Action 委托项目事件
        //委托项目事件广播，管理器对委托进行处理

        private void OnStateChange(int id, EEntrustState oldState, EEntrustState newState)
        {
            //等待销毁状态委托，添加到等待销毁列表
            if(newState == EEntrustState.Destroy)
            {
                m_EntrustItem_Destroy.Add(id);
            }
        }

        private void OnRoundsChange(int id, EEntrustState state, int round, int roundUnderway, bool exceedLimitRounds)
        {

        }

        private void OnEnd(int id, int roundUnderway)
        {

        }

        private void OnTimeout(int id, EEntrustState state)
        {

        }

        #endregion

        #region EntrustRandomSpawn 委托随机生成

        //委托随机生成流程
        //我们需要先在m_RankEntrustsDic中添加不同RankKey的委托随机信息
        //之后设置m_GetEntrustRanks_EntrustRandomSpawn来决定当我们执行一次随机时，取用哪些EntrustRandomInfo参与到随机生成中
        //我们允许设置EntrustRandomInfo.prepositionProbability来决定EntrustRandomInfo是否参与到之后的全体权重随机中
        //运行委托随机生成时，我们先对目标RankKey中的所有EntrustRandomInfo进行一次随机，对比EntrustRandomInfo.prepositionProbability决定生成，并设置动态权重
        //所有的委托随机流程是分开RankKey进行的

        /// <summary>
        /// 委托出现概率调节处理方法
        /// 要求返回0-1间的值
        /// </summary>
        /// <param name="entrustRandomItemInfo">委托随机项目信息</param>
        /// <param name="gapRatio">当前委托数量和期望总数的差值占总数比例，在0-1之间</param>
        /// <returns></returns>
        public delegate float EntrustProbabilityAdjustProcessor(EntrustRandomItemInfo entrustRandomItemInfo, float gapRatio);

        /// <summary>
        /// 创建一个委托项目的方法
        /// </summary>
        /// <param name="entrustItem"></param>
        /// <returns>是否成功创建</returns>
        public delegate bool CreateEntrustProcessor(EntrustRandomItemDynamicInfo entrustId, out EntrustItem entrustItem);

        /// <summary>
        /// 委托随机，单个委托项目信息
        /// </summary>
        public struct EntrustRandomItemInfo
        {
            public int id;

            public int rankKey;

            /// <summary>
            /// 出现概率 0-1
            /// 随机0-1的数字，随机数大于等于此值时，此委托判定为出现
            /// </summary>
            public float probabilityBase;

            /// <summary>
            /// 权重
            /// </summary>
            public int weight;
        }

        /// <summary>
        /// 委托随机，项目动态信息
        /// </summary>
        public struct EntrustRandomItemDynamicInfo
        {
            public int id;

            public int RankKey;

            /// <summary>
            /// 本次随机产生的权重值
            /// </summary>
            public int weight;
        }

        /// <summary>
        /// 委托随机，某等级委托信息
        /// </summary>
        public struct EntrustRandomRankInfo
        {
            /// <summary>
            /// 等级Key，用于查询某一个Rank的所有委托
            /// </summary>
            public int rankKey;

            /// <summary>
            /// 数量权重，越高在期望总数量中此Rank的委托占比越高
            /// </summary>
            public int numWeight;

            /// <summary>
            /// 随机概率调节强度，不需要外部设置。在运行时分配此值。
            /// 在0-1之间
            /// </summary>
            public float probabilityAdjustIntensity;
        }

        /// <summary>
        /// 委托项目配置Dic 按等级区分
        /// Key=等级 Vaule=委托项目Id
        /// </summary>
        private Dictionary<int, List<EntrustRandomItemInfo>> m_RankEntrustsDic;

        /// <summary>
        /// 获取委托的等级
        /// </summary>
        private EntrustRandomRankInfo[] m_GetEntrustRanks_EntrustRandomSpawn;

        /// <summary>
        /// 期望总数，当前总数越接近期望总数，那么生成新委托的概率越接近0
        /// </summary>
        private int m_ExpectsTotalNumber_EntrustRandomSpawn;

        /// <summary>
        /// 概率随机数精度
        /// 默认为10000，及随机数计算到100.00小数点后两位
        /// </summary>
        private int m_ProbabilityRandomNumPrecision = 10000;

        /// <summary>
        /// 委托出现概率调整处理方法
        /// 允许项目设置此方法，用于按项目要求调整委托最终的出现概率。
        /// </summary>
        private EntrustProbabilityAdjustProcessor m_EntrustProbabilityAdjustProcessor;

        /// <summary>
        /// 初始化委托随机生成功能
        /// </summary>
        public void EntrustRandomSpawnInit()
        {
            m_RankEntrustsDic = new Dictionary<int, List<EntrustRandomItemInfo>>();
        }

        /// <summary>
        /// 添加一个委托随机信息
        /// 我们在初始化时将所有需要随机的委托信息都添加到此Dic
        /// 通过SetEntrustRandomSpawnGetEntrustRanks方法来设置随机时取用哪些RankKey的委托信息进行随机
        /// </summary>
        /// <param name="rankKey"></param>
        /// <param name="entrustRandomInfo"></param>
        public void AddEntrustRandomInfo(int rankKey, EntrustRandomItemInfo entrustRandomInfo)
        {
            if (!m_RankEntrustsDic.ContainsKey(rankKey))
                m_RankEntrustsDic.Add(rankKey, new List<EntrustRandomItemInfo>());

            m_RankEntrustsDic[rankKey].Add(entrustRandomInfo);
        }

        /// <summary>
        /// 设置获取哪些等级的委托来进行随机生成
        /// </summary>
        /// <param name="getEntrustRanks"></param>
        public void SetEntrustRandomSpawnGetEntrustRanks(EntrustRandomRankInfo[] getEntrustRanks)
        {
            m_GetEntrustRanks_EntrustRandomSpawn = getEntrustRanks;
        }

        /// <summary>
        /// 设置委托随机生成期望总数
        /// </summary>
        /// <param name="expectsTotalNumber"></param>
        public void SetEntrustRandomSpawnExpectsTotalNumber(int expectsTotalNumber)
        {
            m_ExpectsTotalNumber_EntrustRandomSpawn = expectsTotalNumber;
        }

        /// <summary>
        /// 设置委托概率调节方法
        /// 由项目决定委托的最终出现概率
        /// </summary>
        /// <param name="entrustProbabilityAdjustProcessor"></param>
        public void SetEntrustProbabilityAdjustProcessor(EntrustProbabilityAdjustProcessor entrustProbabilityAdjustProcessor)
        {
            if (entrustProbabilityAdjustProcessor == null) return;

            m_EntrustProbabilityAdjustProcessor = entrustProbabilityAdjustProcessor;
        }

        public void EntrustRandomSpawn(CreateEntrustProcessor createEntrustProcessor)
        {
            if (m_RankEntrustsDic.Count == 0
                || m_GetEntrustRanks_EntrustRandomSpawn == null
                || m_GetEntrustRanks_EntrustRandomSpawn.Length == 0
                || m_ExpectsTotalNumber_EntrustRandomSpawn == 0)
                return;

            //当前委托总数和期望值差距
            int expectsTotalGapNum = m_ExpectsTotalNumber_EntrustRandomSpawn - m_EntrustItemDic.Count;
            if (expectsTotalGapNum <= 0) return;

            //获得每个等级的期望数量
            List<EntrustRandomRankInfo> validGetEntrustRanks = new List<EntrustRandomRankInfo>();
            int weightTotal = 0;
            for (int i = 0; i < m_GetEntrustRanks_EntrustRandomSpawn.Length; i++)
            {
                var rankInfo = m_GetEntrustRanks_EntrustRandomSpawn[i];
                if (!m_RankEntrustsDic.ContainsKey(rankInfo.rankKey) || m_RankEntrustsDic[rankInfo.rankKey].Count == 0) continue;

                weightTotal += rankInfo.numWeight;
                validGetEntrustRanks.Add(rankInfo);
            }
            if (weightTotal == 0) return;//没有有效的权重数据

            //升序排列
            validGetEntrustRanks.Sort((EntrustRandomRankInfo a, EntrustRandomRankInfo b) => 
            {
                return a.rankKey > b.rankKey ? 1 : -1;
            });
            for (int i = 0; i < validGetEntrustRanks.Count; i++)
            {
                validGetEntrustRanks[i] = new EntrustRandomRankInfo()
                {
                    rankKey = validGetEntrustRanks[i].rankKey,
                    numWeight = validGetEntrustRanks[i].numWeight,
                    probabilityAdjustIntensity = (validGetEntrustRanks.Count - i) * 0.5f / validGetEntrustRanks.Count,//等级越高调节强度越低
                };
            }

            int maximalNumWeightRankIndex = 0;
            int totalNumTemp = 0;
            int[] rankExpectsTotalNumber = new int[validGetEntrustRanks.Count];
            for (int i = 0; i < rankExpectsTotalNumber.Length; i++)
            {
                rankExpectsTotalNumber[i] = Mathf.FloorToInt((float)validGetEntrustRanks[i].numWeight / weightTotal * m_ExpectsTotalNumber_EntrustRandomSpawn);

                //不足一个时补足一个
                if (rankExpectsTotalNumber[i] == 0)
                    rankExpectsTotalNumber[i] = 1;

                totalNumTemp += rankExpectsTotalNumber[i];

                if (validGetEntrustRanks[i].numWeight > validGetEntrustRanks[maximalNumWeightRankIndex].numWeight)
                    maximalNumWeightRankIndex = i;
            }
            //总数不足时，补充到numWeight最大的Rank。超出时暂不做处理
            if (totalNumTemp < m_ExpectsTotalNumber_EntrustRandomSpawn)
                rankExpectsTotalNumber[maximalNumWeightRankIndex] += m_ExpectsTotalNumber_EntrustRandomSpawn - totalNumTemp;

            //不同等级分别进行委托生成随机
            List<EntrustRandomItemDynamicInfo> spawnEntrusts = new List<EntrustRandomItemDynamicInfo>();
            int randomSeedBase_Ranks = (int)(Time.realtimeSinceStartup * 1000f);//随机种子初始化
            int index_Ranks = 0;
            for (int i = validGetEntrustRanks.Count - 1; i >= 0; i--)
            {
                //洗牌算法，决定本次执行哪个等级的委托组的随机
                UnityEngine.Random.InitState(randomSeedBase_Ranks + index_Ranks);
                index_Ranks = UnityEngine.Random.Range(0, i + 1);//获取的某等级委托数据组下标
                EntrustRandomRankInfo rankInfo = validGetEntrustRanks[index_Ranks];//获取这个委托信息
                int rankEntrustNumExpects = rankExpectsTotalNumber[index_Ranks];//此等级委托期望总数
                validGetEntrustRanks[index_Ranks] = validGetEntrustRanks[i];//最后一个移动到被随机到的位置
                rankExpectsTotalNumber[index_Ranks] = rankExpectsTotalNumber[i];

                int rankEntrustNumCur = GetEntrustNumByRank(rankInfo.rankKey);//此等级委托当前数量
                if (rankEntrustNumCur >= rankEntrustNumExpects) continue;//确认此等级委托数量是否已经达到期望数量

                var entrusts = m_RankEntrustsDic[rankInfo.rankKey].ToArray();//获取此等级所有委托信息
                spawnEntrusts.Clear();//本次需要生成的委托
                int randomSeedBase = (int)(Time.realtimeSinceStartup * 1000f) + 1;//随机种子初始化
                int randomNum = 0;//随机数0-m_ProbabilityRandomNumPrecision
                int index = 0;//获取的委托信息的下标
                int maxSpawnNum = rankEntrustNumExpects - rankEntrustNumCur;//最大生成数量
                float gapNumRatio = expectsTotalGapNum * 1f / m_ExpectsTotalNumber_EntrustRandomSpawn;//差距数占总期望数比例0-1
                //出现概率调节权重，等级在整个validGetEntrustRanks中越高，调节强度越低

                //计算委托是否出现，简略洗牌算法，最坏复杂度O(n)，最好在O(n-maxSpawnNum)
                for (int j = entrusts.Length - 1; j >= 0; j--)
                {
                    //简略洗牌算法，决定本次执行哪个委托的随机
                    UnityEngine.Random.InitState(randomSeedBase + index);
                    index = UnityEngine.Random.Range(0, j + 1);
                    var item = entrusts[index];//获取这个委托信息
                    entrusts[index] = entrusts[j];//最后一个移动到被随机到的位置

                    //不能添加多个相同Id的委托
                    if (ContainsEntrust(item.id))
                        continue;

                    //计算委托是否出现
                    bool succeed = false;
                    float probability;//出现概率0-1
                    
                    if (m_EntrustProbabilityAdjustProcessor != null)
                    {
                        //项目自定义的修正方法
                        probability = m_EntrustProbabilityAdjustProcessor(item, gapNumRatio);
                    }
                    else
                    {
                        //默认的概率修正方法，基础概率 + 缺少数量占比 * 调节强度
                        probability = item.probabilityBase + gapNumRatio * rankInfo.probabilityAdjustIntensity;
                    }

                    UnityEngine.Random.InitState(randomSeedBase + randomNum + index);
                    randomNum = UnityEngine.Random.Range(0, m_ProbabilityRandomNumPrecision + 1);
                    float randomNumFloat = randomNum * 1f / m_ProbabilityRandomNumPrecision;
                    succeed = randomNumFloat <= probability;

                    if (succeed)
                    {
                        spawnEntrusts.Add(
                            new EntrustRandomItemDynamicInfo()
                            {
                                id = item.id,
                                RankKey = item.rankKey,
                                weight = item.weight * randomNum
                            });

                        //已经达到最大生成数量，结束委托随机出现
                        if (spawnEntrusts.Count >= maxSpawnNum)
                            break;

                        //成功生成了一个，差距数占比重新计算
                        expectsTotalGapNum -= 1;
                        gapNumRatio = (expectsTotalGapNum) * 1f / m_ExpectsTotalNumber_EntrustRandomSpawn;
                    }

                    randomSeedBase++;
                }

                //将随机成功的委托排序后，获取指定数量并存储
                for (int j = 0; j < spawnEntrusts.Count; j++)
                {
                    if (!createEntrustProcessor(spawnEntrusts[j], out EntrustItem entrustItem)) continue;
                    CreateEntrustItem(entrustItem);
                }
            }
        }

        #endregion

        #region Debug
        /// <summary>
        /// 获取显示信息
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private EntrustShowInfo GetShowInfo(EntrustItem item)
        {
            if (item == null)
                return new EntrustShowInfo();

            return new EntrustShowInfo(
                item.EntrustItemHandler.Id,
                (EEntrustType)item.EntrustItemHandler.Type,
                item.EntrustItemHandler.Rank,
                item.EntrustItemHandler.RoundsLimit,
                item.EntrustItemHandler.RoundsNeedBase,
                item.EntrustItemHandler.CanTryMultipleTimes,
                item.EntrustItemHandler.Title,
                item.EntrustItemHandler.Describe,
                GetVenturerTeam(item.EntrustItemHandler.Id)
                );
        }
        #endregion
    }
}
