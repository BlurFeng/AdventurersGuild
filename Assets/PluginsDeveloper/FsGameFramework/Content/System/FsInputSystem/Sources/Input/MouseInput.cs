using UnityEngine;

namespace FsGameFramework.InputSystem
{
    public class MouseInput : InputDevice
    {
        public MouseInput(UInputControlComponent inputControl, bool enable = true)
            :base(inputControl, enable)
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

            Vector3 AimDir = Vector3.zero;
            AimDir.x = Input.GetAxis("Mouse X");
            AimDir.y = Input.GetAxis("Mouse Y");
            AimDir.z = Input.GetAxis("Mouse ScrollWheel");
            m_InputControl.OnAxisEvent(InputObjectType.JoystickAim, AimDir, AimDir.x, AimDir.y);

            m_InputControl.OnButtonEvent(InputObjectType.MouseLeft, Input.GetMouseButton(0));
            m_InputControl.OnButtonEvent(InputObjectType.MouseRight, Input.GetMouseButton(1));
            m_InputControl.OnButtonEvent(InputObjectType.MouseMiddle, Input.GetMouseButton(2));
        }
    }
}
