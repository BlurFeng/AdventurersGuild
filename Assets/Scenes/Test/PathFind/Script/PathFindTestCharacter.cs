using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(CharacterController))]
public class PathFindTestCharacter : AIPath
{
    public float sleepVelocity = 0.4F;

    public CharacterController characterController;

    public override void OnTargetReached()
    {
    }

    public override Vector3 GetFeetPosition()
    {
        return tr.position;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                return;
            }
            if (!hit.transform)
            {
                return;
            }
            destination = hit.point;
            Debug.Log($"PathFindTestCharacter  ---  设定移动目的地坐标：{destination}");
        }

        //Get velocity in world-space  
        Vector3 velocity;
        if (canMove)
        {
            //Calculate desired velocity  
            Vector3 dir = desiredVelocity;//CalculateVelocity(GetFeetPosition());


            //Rotate towards targetDirection (filled in by CalculateVelocity)  
            //RotateTowards(targetDirection);

            dir.y = 0;
            if (dir.sqrMagnitude > sleepVelocity * sleepVelocity)
            {
                //If the velocity is large enough, move  
            }
            else
            {
                //Otherwise, just stand still (this ensures gravity is applied)  
                dir = Vector3.zero;
            }

            if (this.rvoController != null)
            {
                rvoController.Move(dir);
                velocity = rvoController.velocity;
            }
            else if (false/*navController != null*/)
            {
#if FALSE
                    navController.SimpleMove (GetFeetPosition(), dir);  
#endif
                //velocity = Vector3.zero;
            }
            else if (controller != null)
            {
                controller.SimpleMove(dir);
                velocity = controller.velocity;
            }
            else
            {
                Debug.LogWarning("No NavmeshController or CharacterController attached to GameObject");
                velocity = Vector3.zero;
            }
        }
        else
        {
            velocity = Vector3.zero;
        }
    }
}
