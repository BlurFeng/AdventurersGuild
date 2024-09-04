using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsStateSystem
{
    ///// <summary>
    ///// 状态之间关系
    ///// </summary>
    //public enum StateRelation
    //{
    //    //小于等于-3 //Blocked State 阻止添加 新状态添加时，如果有状态关系数<=-3的状态（阻止状态）的数量大于等于需求数量（最小的状态关系数的绝对值减2）时不能添加。阻止和前提同时满足时，阻止状态添加。
    //    Blocked = -3,
    //    None = -2,//无
    //    Ignore = -1,//未设置关系
    //    Cancel = 0, //新状态无法设置
    //    Cover = 1, //设置的新状态要覆盖（移除）该旧状态
    //    Coexist = 2, //共存不冲突
    //    Required = 3,
    //    //大于state等于3 //Required State 添加前提条件 新状态添加时，需要有状态关系数>=3的状态（前提状态）的数量大于等于需求数量（最大的状态关系数减2）时才能添加。阻止和前提同时满足时，阻止状态添加。
    //}

    public class FsStateSystemManager : MonoBehaviourSingleton<FsStateSystemManager>
    {
        bool init;

        int[,] m_StateDoubleArray;//状态二维关系表
        bool[] m_StateAddConditionArray;//添加该状态时是否需要其他前提状态才能添加

        public void Init(List<Dictionary<string, string>> stateRelationTable, string headStateKey)
        {
            if (init) return;
            init = true;

            ReadState(stateRelationTable, headStateKey);

            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// 检测状态与状态的关系 0=state新状态无法添加 1=state新状态可添加且targetState旧状态会被移除 2=state新状态可添加和targetState旧状态共存
        /// </summary>
        /// <param name="state">新状态</param>
        /// <param name="targetState">旧状态</param>
        /// <returns></returns>
        public int CheckState(State state, State targetState)
        {
            return m_StateDoubleArray[(int)state, (int)targetState];
        }

        /// <summary>
        /// 检查参数状态state能否被顺利添加到自身
        /// </summary>
        /// <param name="state">添加状态</param>
        /// <param name="stateItemsCached">已经添加到自身的状态组</param>
        /// <param name="coverState">需要覆盖的状态</param>
        /// <returns></returns>
        public bool CheckAllowAdd(State state, List<StateItem> stateItemsCached, List<StateItem> coverState = null)
        {
            if(!init)
            {
                Debug.LogWarning("FsStateSystem FsStateSystemManager: manager is not init!");
                return false;
            }

            if (state == State.None) return false;
            bool stateNeedPremise = CheckStateNeedCondition(state);//是否需要前提状态

            int canAddMax = 3;//前提状态需求量 等于type值且只能增大
            int canAddNum = 0;//前提状态数量
            int cantAddMax = 3;//阻止添加状态需求量
            int cantAddNum = 0;//阻止添加状态数量 等于type值的绝对值且只能增大

            for (int i = 0; i < stateItemsCached.Count; i++)
            {
                StateItem stateCached = stateItemsCached[i];
                int type = CheckState(state, stateCached.State);

                switch (type)
                {
                    case -2://不处理
                        break;
                    case -1://不处理
                        break;
                    case 0://无法添加 直接返回
                        return false;
                    case 1://需要覆盖的旧状态
                        if (coverState != null)
                            coverState.Add(stateCached);
                        break;
                    case 2://共存
                        break;
                    default:
                        //type >=3 为添加state的条件状态
                        //type <= -3 为阻止state添加的状态
                        if (type >= 3)
                        {
                            if (type > canAddMax) canAddMax = type;
                            canAddNum++;
                        }
                        else if (type <= -3)
                        {
                            if (-type > cantAddMax) cantAddMax = -type;
                            cantAddNum++;
                        }
                        break;
                }
            }

            bool canAdd = cantAddNum < cantAddMax - 2 && //阻止添加状态数量不足 且。。。
                (!stateNeedPremise //不需要前提状态
                || (canAddNum >= canAddMax - 2));//或 需要前提状态且前提状态数量足够

            return canAdd;
        }

        /// <summary>
        /// 该状态添加时是否需要其他前提状态存在才能添加
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        bool CheckStateNeedCondition(State state)
        {
            return m_StateAddConditionArray[(int)state];
        }

        public void ReadState(List<Dictionary<string, string>> stateRelationTable, string headStateKey)
        {
            if (stateRelationTable == null) return;

            m_StateDoubleArray = new int[(int)State.MAX_VALUE, (int)State.MAX_VALUE];
            m_StateAddConditionArray = new bool[(int)State.MAX_VALUE];

            StartCoroutine(IEReadState(stateRelationTable, headStateKey));
        }

        /// <summary>
        /// Reads the state.
        /// 根据设置配置文件生成动态数据。
        /// </summary>
        IEnumerator IEReadState(List<Dictionary<string, string>> stateRelationTable, string headStateKey)
        {
            yield return null;

            //初始化所有内容
            for (int iState = 0; iState < (int)State.MAX_VALUE; iState++)
            {
                for (int jTargetState = 0; jTargetState < (int)State.MAX_VALUE; jTargetState++)
                {
                    m_StateDoubleArray[iState, jTargetState] = -1;
                    m_StateAddConditionArray[iState] = false;
                }
            }

            yield return null;

            //根据配置的关系表设置内容
            for (int i = 0; i < stateRelationTable.Count; i++)
            {
                if(!stateRelationTable[i].ContainsKey(headStateKey))
                {
                    Debug.LogWarning("FsStateSystem FsStateSystemManager: cant find headStateKey!");
                    continue;
                }

                //第一个为Key状态 之后为其他状态和此状态的关系
                //含义为 当添加Key状态时如果已经拥有其他某个状态，根据关系进行处理
                State state = FsStateSystemUtility.Parse(stateRelationTable[i][headStateKey]);
                stateRelationTable[i].Remove(headStateKey);

                foreach (var item in stateRelationTable[i])
                {
                    State targetState = FsStateSystemUtility.Parse(item.Key);
                    int value = int.Parse(item.Value);
                    int key1 = (int)state;
                    m_StateDoubleArray[key1, (int)targetState] = value;
                    if (value >= 3)
                        m_StateAddConditionArray[key1] = true;
                }
            }
        }
    }
}
