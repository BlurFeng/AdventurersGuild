using UnityEngine;
using FsGameFramework;
using FsGameFramework.InputSystem;
/// <summary>
/// 观众，漫游演员，可以控制摄像机在场景中漫游观察。
/// </summary>
public class ASpectator : APawn
{
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        //操控摄像机功能 需要恢复
        //float ratSpeed = 0.1f;
        //Vector3 dir = CameraSystemManager.Instance.Camera.transform.rotation
        //            * new Vector3(
        //            m_DirectionInfoComponent.MoveDir.Direction.x,
        //            0f,
        //            m_DirectionInfoComponent.MoveDir.Direction.z + m_DirectionInfoComponent.AimDir.Direction.z * 100f);

        //bool attackBtnStay = m_DirectionInfoComponent.AttackBtn.IsStay;
        //Vector3 rat;
        //if (attackBtnStay)
        //{
        //    rat = new Vector3(
        //    m_DirectionInfoComponent.AimDir.Direction.x,
        //    -m_DirectionInfoComponent.AimDir.Direction.y,
        //    0f) * ratSpeed;
        //}
        //else
        //    rat = Vector3.zero;

        //if (dir != Vector3.zero || rat != Vector3.zero)
        //{
        //    CameraEvents.CameraCtrlInput?.Invoke(dir, rat);
        //}
    }

    public override void OnInputAction(InputObjectType inputObjectType, InputEventType inputEventType)
    {
        switch (inputObjectType)
        {
            case InputObjectType.Minor1:
                OnInputMainBtn(inputEventType);
                break;
        }
    }
}
