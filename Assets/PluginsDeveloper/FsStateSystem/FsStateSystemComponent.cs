using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FsStateSystem
{
    /// <summary>
    /// 状态系统组件
    /// </summary>
    public class FsStateSystemComponent
    {
        /// <summary>
        /// 拥有者
        /// </summary>
        private Component m_Owner;

        private List<StateItem> m_StateItemsCached;//人物现在拥有的状态项目
        private List<StateItem> m_CoverStateTemp;//需要移除的状态暂存列表
        private float m_StateItemsRefreshTimer;//转台刷新计时器
        private const float m_StateItemsRefreshIntervalTime = 0.1f;//状态刷新间隔时间

        //状态事件容器
        private StateActionContainer m_StateActionContainer;

        public FsStateSystemComponent(Component owner)
        {
            this.m_Owner = owner;

            m_StateItemsCached = new List<StateItem>();
            m_CoverStateTemp = new List<StateItem>();
            m_StateActionContainer = new StateActionContainer();
            m_StateItemsRefreshTimer = 0f;
        }

        void Reset()
        {
            m_StateItemsCached.Clear();
            m_StateItemsRefreshTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            StateItemsOnUpdate(deltaTime);

            m_StateActionContainer?.Tick(deltaTime);

#if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.F7))
            {
                mShowStateLog = !mShowStateLog;
            }
#endif
        }

        public void FixedTick(float fixedDeltaTime)
        {
            m_StateActionContainer?.FixedTick(fixedDeltaTime);
        }

#if UNITY_EDITOR
        [Header("显示玩家状态变化LOG打印 F7开关")]
        bool mShowStateLog;
        string mShowStateTxt;

        public void OnGUI()
        {
            if (!mShowStateLog) return;
            string txt = m_Owner.name + ": ";
            for (int i = 0; i < m_StateItemsCached.Count; i++)
            {
                txt += m_StateItemsCached[i].State + "-";
            }
            if (!txt.Equals(mShowStateTxt))
            {
                Debug.Log("状态" + txt);
                mShowStateTxt = txt;
            }

            Vector2 pos = UnityEngine.Camera.main.WorldToScreenPoint(m_Owner.transform.position);
            GUI.Box(new Rect(pos.x, Screen.height - pos.y, 600, 50), mShowStateTxt);
        }
#endif

        #region Public Func 外部调用方法

        #region Add or Remove

        /// <summary>
        /// 添加状态项目
        /// </summary>
        /// <param name="state">状态枚举</param>
        public void AddState(State state)
        {
            AddStateExecute(new StateItem(state, string.Empty));
        }

        /// <summary>
        /// 添加状态项目
        /// </summary>
        /// <param name="state">状态枚举</param>
        /// <param name="casterUID">造成此状态的玩家</param>
        public void AddState(State state, string casterUID = "")
        {
            AddStateExecute(new StateItem(state, casterUID));
        }

        /// <summary>
        /// 添加状态项目
        /// </summary>
        /// <param name="state">状态枚举</param>
        /// <param name="duration">持续时间</param>
        public void AddState(State state, float duration)
        {
            AddStateExecute(new StateItem(state, string.Empty, duration));
        }

        /// <summary>
        /// 添加状态项目
        /// </summary>
        /// <param name="state">状态枚举</param>
        /// <param name="duration">持续时间</param>
        /// <param name="casterUID">造成此状态的人UID</param>
        /// <param name="stateParam">状态携带的信息参数</param>
        public void AddState(State state, float duration, string casterUID, string stateParam = "")
        {
            AddStateExecute(new StateItem(state, casterUID, duration, stateParam));
        }

        /// <summary>
        /// 添加状态项目
        /// </summary>
        /// <param name="stateItem">状态项目</param>
        public void AddState(StateItem stateItem)
        {
            AddStateExecute(stateItem);
        }

        /// <summary>
        /// 移除状态项目
        /// </summary>
        /// <param name="state"></param>
        public void RemoveState(State state)
        {
            StateItem stateItem;
            stateItem = GetStateItem(state);

            if (stateItem != null)
                RemoveStateExecute(stateItem);
        }

        /// <summary>
        /// 移除状态项目 根据stateParam
        /// </summary>
        /// <param name="state">状态枚举</param>
        /// <param name="stateParam">状态携带的信息参数</param>
        public void RemoveState(State state, string stateParam)
        {
            if (string.IsNullOrEmpty(stateParam))
            {
                RemoveState(state);
                return;
            }

            StateItem stateItem = GetStateItem(state, stateParam);

            if (stateItem != null)
            {
                RemoveStateExecute(stateItem);
            }
        }

        /// <summary>
        /// 添加某个状态并移除其他所有状态
        /// </summary>
        /// <param name="state"></param>
        public void AddStateOnly(State state)
        {
            RemoveAllStates();

            AddStateExecute(new StateItem(state, string.Empty, -1f));
        }

        /// <summary>
        /// 移除所有状态
        /// </summary>
        public void RemoveAllStates()
        {
            if (m_StateItemsCached.Count <= 0) return;

            for (int i = m_StateItemsCached.Count - 1; i >= 0; i--)
            {
                //防止此方法执行时 nsCastSkillPhysicalDriven状态被协程RemoveState()移除导致最后数组越界
                if (m_StateItemsCached.Count == 0)
                    break;

                if (null != m_StateItemsCached[i])
                    RemoveStateExecute(m_StateItemsCached[i]);
            }
        }

        #endregion

        #region State Action
        /// <summary>
        /// 添加一个事件到状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateActionType">事件类型</param>
        /// <param name="action"></param>
        public bool AddAction(State state, StateActionType stateActionType, Action action)
        {
            if (m_StateActionContainer == null) return false;

            return m_StateActionContainer.AddAction(state, stateActionType, action);
        }

        /// <summary>
        /// 移除一个事件从状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateActionType">事件类型</param>
        /// <param name="action"></param>
        public bool RemoveAction(State state, StateActionType stateActionType, Action action)
        {
            if (m_StateActionContainer == null) return false;

            return m_StateActionContainer.RemoveAction(state, stateActionType, action);
        }
        #endregion

        /// <summary>
        /// 是否包含某状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public bool ContainState(State state)
        {
            return GetStateItem(state) != null;
        }

        /// <summary>
        /// 是否包含某casterUID造成的某状态
        /// </summary>
        /// <param name="casterUID">造成此状态的玩家UID</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        public bool ContainStateWithCaster(string casterUID, State state)
        {
            StateItem stateObj = null;

            foreach (StateItem StateItem in m_StateItemsCached)
            {
                if ((StateItem.State == state) && (StateItem.CasterUID == casterUID))
                {
                    stateObj = StateItem;
                }
            }

            return (stateObj != null);
        }

        /// <summary>
        /// 获取造成此状态的玩家UID
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public string GetCasterIDWithState(State state)
        {
            for (int i = 0; i < m_StateItemsCached.Count; ++i)
            {
                if (state == m_StateItemsCached[i].State)
                {
                    return m_StateItemsCached[i].CasterUID;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取stateParam的list 根据state状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="outParameters">返回内容list</param>
        public void GetAllStateParamsOfTargetState(State state, ref List<string> outParameters)
        {
            for (int i = 0; i < m_StateItemsCached.Count; ++i)
            {
                StateItem stateItem = m_StateItemsCached[i];
                if (stateItem.State == state)
                {
                    outParameters.Add(stateItem.StateParam);
                }
            }
        }

        /// <summary>
        /// 获取stateParam 根据state状态
        /// </summary>
        /// <param name="state">状态</param>
        public string GetStateParamsOfTargetState(State state)
        {
            for (int i = 0; i < m_StateItemsCached.Count; ++i)
            {
                StateItem stateItem = m_StateItemsCached[i];
                if (stateItem.State == state)
                {
                    return stateItem.StateParam;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取StateItem对象的List 根据state
        /// </summary>
        /// <param name="targetState">状态</param>
        /// <param name="outBehaviourList"></param>
        public void GetTargetStateItemList(State targetState, List<StateItem> outBehaviourList)
        {
            for (int i = 0; i < m_StateItemsCached.Count; ++i)
            {
                if (targetState == m_StateItemsCached[i].State)
                {
                    if (!outBehaviourList.Contains(m_StateItemsCached[i]))
                        outBehaviourList.Add(m_StateItemsCached[i]);
                }
            }
        }
        #endregion

        

        //人物拥有的状态更新
        void StateItemsOnUpdate(float deltaTime)
        {
            if (m_StateItemsCached.Count == 0)
            {
                m_StateItemsRefreshTimer = 0f;
                return;
            }

            //更新间隔时间
            if (FsUtility.OverflowValue(ref m_StateItemsRefreshTimer, deltaTime, m_StateItemsRefreshIntervalTime))
            {
                for (int i = m_StateItemsCached.Count - 1; i >= 0; i--)
                {
                    var item = m_StateItemsCached[i];

                    if (item.OnUpdate(m_StateItemsRefreshIntervalTime))
                    {
                        RemoveStateExecute(item);
                    }
                }
            }
        }



        /// <summary>
        /// 获取某个StateItem对象
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        private StateItem GetStateItem(State state)
        {
            foreach (StateItem StateItem in m_StateItemsCached)
            {
                if (StateItem.State == state)
                {
                    return StateItem;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取某个StateItem对象 根据stateParam
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateParam">状态参数</param>
        /// <returns></returns>
        private StateItem GetStateItem(State state, string stateParam)
        {
            foreach (StateItem StateItem in m_StateItemsCached)
            {
                if ((StateItem.State == state) && (StateItem.StateParam == stateParam))
                {
                    return StateItem;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查参数状态state能否被顺利添加到自身
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        private bool CheckAllowAdd(State state, List<StateItem> coverState = null)
        {
            return FsStateSystemManager.Instance.CheckAllowAdd(state, m_StateItemsCached, coverState);
        }

        //添加一个状态
        private void AddStateExecute(StateItem stateObject)
        {
            State state = stateObject.State;
            if (state == State.None) return;
            string stateParam = stateObject.StateParam;

            if (!CheckAllowAdd(state, m_CoverStateTemp)) return;

            m_StateItemsCached.Add(stateObject);

            //移除需要覆盖的状态
            if (m_CoverStateTemp.Count > 0)
            {
                for (int i = 0; i < m_CoverStateTemp.Count; i++)
                    RemoveStateExecute(m_CoverStateTemp[i]);
                m_CoverStateTemp.Clear();
            }

            m_StateActionContainer?.OnStateAdd(stateObject.State);
        }

        //移除一个状态
        void RemoveStateExecute(StateItem stateObject)
        {
            State state = stateObject.State;
            if (state == State.None) return;
            string stateParam = stateObject.StateParam;

            m_StateItemsCached.Remove(stateObject);

            m_StateActionContainer?.OnStateRemove(stateObject.State);
        }
    }
}