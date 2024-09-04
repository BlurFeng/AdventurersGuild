using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using FsGameFramework.InputSystem;

public class ADemoPawn : APawn
{
    public override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);

        Movement();
    }

    [SerializeField]
    private float m_MoveSpeed = 1f;

    protected void Movement()
    {
        //m_DirectionInfoComponent组件用于记录输入的信息 由父类提供
        //子类可以根据需求继承UInputInfoComponent类并扩展更多信息 在初始化时初始化自己的输入信息组件 或者直接新建自己的组件记录额外信息并添加到自己的Pawn(Actor)

        //行为实现
        if (null != m_InputInfoComponent)
        {
            if (m_InputInfoComponent.MoveDir.Direction != Vector3.zero)
            {
                TransformGet.Translate(
                    m_InputInfoComponent.MoveDir.Direction * m_InputInfoComponent.MoveDir.Magnitude * m_MoveSpeed);
            }
        }
    }

    public override void OnInputAction(InputObjectType inputObjectType, InputEventType inputEventType)
    {
        base.OnInputAction(inputObjectType, inputEventType);

        //you can override button action，可以重载对按钮点击信息的反应
    }
}