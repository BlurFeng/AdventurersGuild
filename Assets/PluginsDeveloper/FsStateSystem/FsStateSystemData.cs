using System.Collections.Generic;
using System;

namespace FsStateSystem
{
    /// <summary>
    /// 状态项目
    /// </summary>
    public class StateItem
    {
        /// <summary>
        /// 状态枚举
        /// </summary>
        public State State { get; private set; }

        /// <summary>
        /// 造成此状态的人
        /// </summary>
        public string CasterUID { get; private set; }

        /// <summary>
        /// 状态携带的信息参数
        /// </summary>
        public string StateParam { get; private set; }

        /// <summary>
        /// 持续时间 初始化时设置为<0时只能主动移除
        /// </summary>
        public float Duration { get; private set; }

        private readonly bool m_haveDuration;//是否有持续时间设置
        

        public StateItem(State state, string casterUID)
        {
            this.State = state;
            this.Duration = -1f;
            m_haveDuration = Duration > 0;
            this.CasterUID = casterUID;
            this.StateParam = string.Empty;
        }

        public StateItem(State state, string casterUID, float duration = -1f)
        {
            this.State = state;
            this.Duration = duration;
            m_haveDuration = duration > 0;
            this.CasterUID = casterUID;
            this.StateParam = string.Empty;
        }

        public StateItem(State state, string casterUID, float duration = -1f, string stateParam = "")
        {
            this.State = state;
            this.Duration = duration;
            m_haveDuration = duration > 0;
            this.CasterUID = casterUID;
            this.StateParam = stateParam;
        }

        /// <summary>
        /// 生命周期刷新
        /// </summary>
        /// <param name="time">变化时间</param>
        /// <returns>true=持续时间结束 false=持续时间未结束或未设置</returns>
        internal bool OnUpdate(float time)
        {
            if (m_haveDuration)
            {
                Duration -= time;
                if (Duration <= 0f)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum StateActionType
    {
        None,

        /// <summary>
        /// 当状态呢添加时调用一次
        /// </summary>
        OnAdd,

        /// <summary>
        /// 当状态移除时调用一次
        /// </summary>
        OnRemove,

        /// <summary>
        /// 当状态存在时 在Update中调用
        /// </summary>
        OnStayTick,

        /// <summary>
        /// 当状态存在时 在FixedUpdate中调用
        /// </summary>
        OnStayFixedTick,
    }

    /// <summary>
    /// 状态事件容器
    /// 允许绑定事件到某个状态
    /// </summary>
    public class StateActionContainer
    {
        private Dictionary<State, Action> m_StateActionOnAddDic;
        private Dictionary<State, Action> m_StateActionOnRemoveDic;
        private Dictionary<State, Action> m_StateActionOnStayDic;
        private Dictionary<State, Action> m_StateActionOnStayFixedDic;

        public StateActionContainer()
        {
            m_StateActionOnAddDic = new Dictionary<State, Action>();
            m_StateActionOnRemoveDic = new Dictionary<State, Action>();
            m_StateActionOnStayDic = new Dictionary<State, Action>();
            m_StateActionOnStayFixedDic = new Dictionary<State, Action>();
        }

        /// <summary>
        /// 添加一个事件到状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateActionType">事件类型</param>
        /// <param name="action"></param>
        public bool AddAction(State state, StateActionType stateActionType, Action action)
        {
            switch (stateActionType)
            {
                case StateActionType.None:
                    break;
                case StateActionType.OnAdd:
                    if (!m_StateActionOnAddDic.ContainsKey(state))
                    {
                        m_StateActionOnAddDic.Add(state, action);
                    }
                    else
                    {
                        if (!Check(m_StateActionOnAddDic[state], action)) return false;

                        m_StateActionOnAddDic[state] += action;
                    }
                    break;
                case StateActionType.OnRemove:
                    if (!m_StateActionOnRemoveDic.ContainsKey(state))
                    {
                        m_StateActionOnRemoveDic.Add(state, action);
                    }
                    else
                    {
                        if (!Check(m_StateActionOnRemoveDic[state], action)) return false;
                        m_StateActionOnRemoveDic[state] += action;
                    }
                    break;
                case StateActionType.OnStayTick:
                    if (!m_StateActionOnStayDic.ContainsKey(state))
                    {
                        m_StateActionOnStayDic.Add(state, action);
                    }
                    else
                    {
                        if (!Check(m_StateActionOnStayDic[state], action)) return false;
                        m_StateActionOnStayDic[state] += action;
                    }
                    break;
                case StateActionType.OnStayFixedTick:
                    if (!m_StateActionOnStayFixedDic.ContainsKey(state))
                    {
                        m_StateActionOnStayFixedDic.Add(state, action);
                    }
                    else
                    {
                        if (!Check(m_StateActionOnStayFixedDic[state], action)) return false;
                        m_StateActionOnStayFixedDic[state] += action;
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        /// <summary>
        /// 移除一个事件从状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <param name="stateActionType">事件类型</param>
        /// <param name="action"></param>
        public bool RemoveAction(State state, StateActionType stateActionType, Action action)
        {
            switch (stateActionType)
            {
                case StateActionType.None:
                    break;
                case StateActionType.OnAdd:
                    if (!m_StateActionOnAddDic.ContainsKey(state))
                        return false;
                    else
                    {
                        m_StateActionOnAddDic[state] -= action;

                        if (m_StateActionOnAddDic[state] == null)
                            m_StateActionOnAddDic.Remove(state);
                    }
                    break;
                case StateActionType.OnRemove:
                    if (!m_StateActionOnRemoveDic.ContainsKey(state))
                        return false;
                    else
                    {
                        m_StateActionOnRemoveDic[state] += action;
                        if (m_StateActionOnRemoveDic[state] == null)
                            m_StateActionOnRemoveDic.Remove(state);
                    }
                    break;
                case StateActionType.OnStayTick:
                    if (!m_StateActionOnStayDic.ContainsKey(state))
                        return false;
                    else
                    {
                        m_StateActionOnStayDic[state] += action;
                        if (m_StateActionOnStayDic[state] == null)
                            m_StateActionOnStayDic.Remove(state);
                    }
                    break;
                case StateActionType.OnStayFixedTick:
                    if (!m_StateActionOnStayFixedDic.ContainsKey(state))
                        return false;
                    else
                    {
                        m_StateActionOnStayFixedDic[state] += action;
                        if (m_StateActionOnStayFixedDic[state] == null)
                            m_StateActionOnStayFixedDic.Remove(state);
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        internal bool Check(Action actionCur, Action actionAdd)
        {
            var array = actionCur.GetInvocationList();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Method.Name.Equals(actionAdd.Method.Name))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 当状态添加时 调用委托
        /// </summary>
        /// <param name="state"></param>
        internal void OnStateAdd(State state)
        {
            if (!m_StateActionOnAddDic.ContainsKey(state)) return;
            m_StateActionOnAddDic[state]?.Invoke();
        }

        /// <summary>
        /// 当状态移除时 调用委托
        /// </summary>
        /// <param name="state"></param>
        internal void OnStateRemove(State state)
        {
            if (!m_StateActionOnRemoveDic.ContainsKey(state)) return;
            m_StateActionOnRemoveDic[state]?.Invoke();
        }

        /// <summary>
        /// 在Update中调用事件
        /// </summary>
        internal void Tick(float deltaTime)
        {
            foreach (var action in m_StateActionOnStayDic.Values)
            {
                action?.Invoke();
            }
        }

        /// <summary>
        /// 在FixedUpdate中调用事件
        /// </summary>
        internal void FixedTick(float fixedDeltaTime)
        {
            foreach (var action in m_StateActionOnStayFixedDic.Values)
            {
                action?.Invoke();
            }
        }
    }
}
