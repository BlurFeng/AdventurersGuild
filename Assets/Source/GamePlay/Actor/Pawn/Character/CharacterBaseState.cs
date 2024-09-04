using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using FsStateSystem;

public partial class CharacterBase : ACharacter
{
    //用于管理和状态相关的内容

    private FsStateSystemComponent m_StateSystemComponent;

    private void InitCharacterBaseState()
    {
        //初始化状态组件
        m_StateSystemComponent = new FsStateSystemComponent(this);
        BindStateAction();
        //添加初始状态
        m_StateSystemComponent.AddState(State.Normal);
    }

    #region Public

    /// <summary>
    /// 是否包含某状态
    /// </summary>
    /// <param name="state">状态</param>
    /// <returns></returns>
    public bool ContainState(State state)
    {
        return m_StateSystemComponent.ContainState(state);
    }

    #endregion

    private void OnDestroyCharacterBaseState()
    {
        UnbindStateAction();
    }

    public void TickCharacterBaseState(float deltaTime)
    {
        m_StateSystemComponent.Tick(deltaTime);
    }

    public void FixedTickCharacterBaseState(float fixedDeltaTime)
    {
        m_StateSystemComponent.FixedTick(fixedDeltaTime);

        //状态切换
        if (IsHaveMoveDir)
        {
            if(!ContainState(State.Walk))
                m_StateSystemComponent.AddState(State.Walk);
        }
        else
        {
            if (!ContainState(State.Idle))
                m_StateSystemComponent.AddState(State.Idle);
        }
    }

    //绑定事件到状态
    private void BindStateAction()
    {
        if (m_StateSystemComponent != null)
        {
            m_StateSystemComponent.AddAction(State.Grounded, StateActionType.OnAdd, OnStateGroundedAdd);
            m_StateSystemComponent.AddAction(State.Grounded, StateActionType.OnStayFixedTick, OnStateGroundedFixedTick);
            m_StateSystemComponent.AddAction(State.Idle, StateActionType.OnAdd, OnStateIdleAdd);
            m_StateSystemComponent.AddAction(State.Idle, StateActionType.OnStayFixedTick, OnStateIdelFixedTick);
            m_StateSystemComponent.AddAction(State.Walk, StateActionType.OnAdd, OnStateWalkAdd);
            m_StateSystemComponent.AddAction(State.Walk, StateActionType.OnStayFixedTick, OnStateWalkFixedTick);
            m_StateSystemComponent.AddAction(State.Jump, StateActionType.OnAdd, OnStateJumpAdd);
            m_StateSystemComponent.AddAction(State.Jump, StateActionType.OnStayFixedTick, OnStateJumpFixedTick);
        }

        m_OnLandCharacterBase += StateGroundedAdd;
        m_OnLandCharacterBase += StateGroundedRemove;
    }

    private void UnbindStateAction()
    {
        if(m_StateSystemComponent != null)
        {
            m_StateSystemComponent.RemoveAction(State.Grounded, StateActionType.OnAdd, OnStateGroundedAdd);
            m_StateSystemComponent.RemoveAction(State.Grounded, StateActionType.OnStayFixedTick, OnStateGroundedFixedTick);
            m_StateSystemComponent.RemoveAction(State.Idle, StateActionType.OnAdd, OnStateIdleAdd);
            m_StateSystemComponent.RemoveAction(State.Idle, StateActionType.OnStayFixedTick, OnStateIdelFixedTick);
            m_StateSystemComponent.RemoveAction(State.Walk, StateActionType.OnAdd, OnStateWalkAdd);
            m_StateSystemComponent.RemoveAction(State.Walk, StateActionType.OnStayFixedTick, OnStateWalkFixedTick);
            m_StateSystemComponent.RemoveAction(State.Jump, StateActionType.OnAdd, OnStateJumpAdd);
            m_StateSystemComponent.RemoveAction(State.Jump, StateActionType.OnStayFixedTick, OnStateJumpFixedTick);
        }

        m_OnLandCharacterBase -= StateGroundedAdd;
        m_OnLandCharacterBase -= StateGroundedRemove;
    }

    private void StateGroundedAdd(Vector3 v3)
    {
        m_StateSystemComponent.AddState(State.Grounded);
    }

    private void StateGroundedRemove(Vector3 v3)
    {
        m_StateSystemComponent.RemoveState(State.Grounded);
    }

    private void OnStateGroundedAdd()
    {
        OnGroundedAddAction();
    }

    private void OnStateGroundedFixedTick()
    {
        OnGroundedAction();
    }

    private void OnStateIdleAdd()
    {
        OnIdleAddAction();
    }

    private void OnStateIdelFixedTick()
    {
        OnIdleAction();
    }

    private void OnStateWalkAdd()
    {
        OnWalkAddAction();
    }

    private void OnStateWalkFixedTick()
    {
        OnWalkAction();
    }

    private void OnStateJumpAdd()
    {
        OnJumpAddAction();
    }

    private void OnStateJumpFixedTick()
    {
        OnJumpAction();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (m_StateSystemComponent == null) return;
        m_StateSystemComponent.OnGUI();
    }
#endif
}
