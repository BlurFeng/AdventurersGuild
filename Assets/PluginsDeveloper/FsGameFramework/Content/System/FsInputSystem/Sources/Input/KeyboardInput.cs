using UnityEngine;

namespace FsGameFramework.InputSystem
{
    public class KeyboardInput : InputDevice
    {
        public KeyboardInput(UInputControlComponent inputControl, bool enable = true)
            : base(inputControl, enable)
        {

        }

        public override void UpdateInput()
        {
            base.UpdateInput();

            LocalUpdateInput();
        }

        void LocalUpdateInput()
        {
            if (m_InputControl == null || !m_Enable) return;

            float hor = 0f;
            float ver = 0f;
            Vector3 moveDir = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                moveDir.z = ver = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDir.z = ver = - 1f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                moveDir.x = hor = - 1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveDir.x = hor = 1f;
            }

            moveDir.Normalize();
            m_InputControl.OnAxisEvent(InputObjectType.JoystickMain, moveDir, hor, ver);

            m_InputControl.OnButtonEvent(InputObjectType.Minor3, Input.GetKey(KeyCode.Space));

            m_InputControl.OnButtonEvent(InputObjectType.Main, Input.GetKey(KeyCode.F));
            m_InputControl.OnButtonEvent(InputObjectType.Minor1, Input.GetKey(KeyCode.K));
            m_InputControl.OnButtonEvent(InputObjectType.Minor2, Input.GetKey(KeyCode.L));
        }
    }
}
