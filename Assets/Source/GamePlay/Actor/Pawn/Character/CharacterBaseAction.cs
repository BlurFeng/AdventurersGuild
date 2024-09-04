using FsGameFramework;
using UnityEngine;

/// <summary>
/// 用于定义角色的行为动作
/// </summary>
public partial class CharacterBase : ACharacter
{
    private void InitCharacterBaseAction()
    {

    }

    private void OnDestroyCharacterBaseAction()
    {

    }

    #region Grounded

    private void OnGroundedAddAction()
    {

    }

    private void OnGroundedAction()
    {

    }

    #endregion

    #region Walk
    /// <summary>
    /// 角色朝向
    /// 八方朝向
    /// </summary>
    public enum CharacterOrientation
    {
        Up,
        Down,
        Left,
        Right,
        UpRight,
        DownRight,
        UpLeft,
        DownLeft
    }

    /// <summary>
    /// 角色当前朝向
    /// </summary>
    public CharacterOrientation CharacterOrientationCur { get; private set; }

    //true = Right, false = Left
    private bool frontIsRightOrLeft = true;
    private Vector3 m_MoveVelocity;

    protected Transform m_TransRootBody; //根节点 身体 渲染于碰撞挂载 用于转向

    [Header("静止时物理材质")]
    [SerializeField] private PhysicMaterial m_PMIdle;
    [Header("移动时物理材质")]
    [SerializeField] private PhysicMaterial m_PMMove;

    private void OnWalkAddAction()
    {
        GetCollider.material = m_PMMove;

        if (m_SpineAnim != null)
        {
            m_SpineAnim.AnimationState.SetAnimation(0, "Body_Walk", true);
        }
    }

    private void OnWalkAction()
    {
        //更新角色朝向
        if (IsHaveMoveDir)
        {
            bool haveX = GetMoveDirHorizontal != 0;
            bool haveY = GetMoveDirVertical != 0;
            bool orientationRight = GetMoveDirHorizontal > 0f;
            bool orientationUp = GetMoveDirVertical > 0f;

            if (haveX && haveY)
            {
                if (orientationRight)
                {
                    CharacterOrientationCur = orientationUp ? CharacterOrientation.UpRight : CharacterOrientation.DownRight;
                }
                else
                {
                    CharacterOrientationCur = orientationUp ? CharacterOrientation.UpLeft : CharacterOrientation.DownLeft;
                }
            }
            else if (haveX)
            {
                CharacterOrientationCur = orientationRight ? CharacterOrientation.Right : CharacterOrientation.Left;
            }
            else if (haveY)
            {
                CharacterOrientationCur = orientationUp ? CharacterOrientation.Up : CharacterOrientation.Down;
            }

            //左右转向
            if (m_TransRootBody != null &&
                (frontIsRightOrLeft && !orientationRight) ||
                (!frontIsRightOrLeft && orientationRight))
            {
                frontIsRightOrLeft = !frontIsRightOrLeft;
                m_TransRootBody.localScale = new Vector3(-m_TransRootBody.localScale.x, m_TransRootBody.localScale.y, m_TransRootBody.localScale.z);
            }
        }
    }
    #endregion

    #region Idle
    private void OnIdleAddAction()
    {
        GetCollider.material = m_PMIdle;

        if (m_SpineAnim != null)
        {
            m_SpineAnim.AnimationState.SetAnimation(0, "Body_Idle", true);
            m_SpineAnim.AnimationState.SetAnimation(1, "Eye_Blink", true);
        }
    }

    private void OnIdleAction()
    {
        if (GetRigidbody == null) return;
    }
    #endregion

    #region Jump
    private void OnJumpAddAction()
    {
        GetCollider.material = m_PMIdle;

        if (m_SpineAnim != null)
        {
            m_SpineAnim.AnimationState.SetAnimation(0, "Body_Idle", true);
            m_SpineAnim.AnimationState.SetAnimation(1, "Eye_Blink", true);
        }
    }

    private void OnJumpAction()
    {
        if (GetRigidbody == null) return;
    }
    #endregion


}