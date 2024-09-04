using FsGameFramework;
using UnityEngine;
using Pathfinding;
using CMF;
using FsGameFramework.InputSystem;
using System;
using System.Collections.Generic;
using AstarPathfindingExtension;

/// <summary>
/// 角色代理人功能，寻路，按路径自动移动到目标点。
/// </summary>
public partial class CharacterBase : ACharacter
{
    //到达目标点近似距离，当和目标点距离小于此值时认为已经到达目的地
    private const float m_ReachPointNearlyDis = 0.1f;

    //重新计算路径的速率时间
    private const float m_Rem_PathRateTime = 3f;

    private bool m_StartMove;
    //移动目标位置
    private Vector3 m_MoveTargetPos;

    //寻路组件
    private Seeker m_Seeker;

    //路径
    private Path m_Path;

    //当前移动目标路径点下标
    private int m_PathPointIndexCur;

    //最后一次重新计算路径的事件点
    private float m_Rem_PathTimeLast;

    private void InitCharacterBaseAgent()
    {
        m_Seeker = GetComponent<Seeker>();

        PathfindingModel.Instance.BindEventWithOnGraphUpdate(OnGraphUpdate, true);
    }

    private void OnDestroyCharacterBaseAgent()
    {
        PathfindingModel.Instance.BindEventWithOnGraphUpdate(OnGraphUpdate, false);
    }

    public void TickCharacterBaseAgent(float deltaTime)
    {
        //测试代码，点击移动到目标
        //if (Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit hit;
        //    if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
        //    {
        //        return;
        //    }
        //    if (!hit.transform)
        //    {
        //        return;
        //    }
        //    m_MoveTargetPos = hit.point;
        //    m_Rem_PathTimeLast = Time.time;
        //    m_Seeker.StartPath(transform.position, m_MoveTargetPos, OnPathComplete);
        //    Debug.Log($"CharacterBaseAgent  ---  设定移动目的地坐标：{m_MoveTargetPos}");
        //}

        //自动寻路进行中
        if(m_StartMove && m_MoveTargetPos != null && m_Seeker != null)
        {
            if (Time.time > m_Rem_PathTimeLast + m_Rem_PathRateTime && m_Seeker.IsDone())
            {
                m_Rem_PathTimeLast = Time.time;

                // Start a new m_Path to the m_MoveTarget, call the the OnPathComplete function
                // when the m_Path has been calculated (which may take a few frames depending on the complexity)
                m_Seeker.StartPath(transform.position, m_MoveTargetPos, OnPathComplete);
            }

            if (m_Path == null)
            {
                // We have no m_Path to follow yet, so don't do anything
                return;
            }

            // Check in a loop if we are close enough to the current waypoint to switch to the next one.
            // We do this in a loop because many waypoints might be close to each other and we may reach
            // several of them in the same frame.

            // The distance to the next waypoint in the m_Path
            float distanceToWaypoint;
            while (true)
            {
                // If you want maximum performance you can check the squared distance instead to get rid of a
                // square root calculation. But that is outside the scope of this tutorial.
                distanceToWaypoint = Vector3.Distance(transform.position, m_Path.vectorPath[m_PathPointIndexCur]);
                if (distanceToWaypoint < m_ReachPointNearlyDis)
                {
                    // Check if there is another waypoint or if we have reached the end of the m_Path
                    if (m_PathPointIndexCur + 1 < m_Path.vectorPath.Count)
                    {
                        m_PathPointIndexCur++;
                    }
                    else
                    {
                        // Set a status variable to indicate that the agent has reached the end of the m_Path.
                        // You can use this to trigger some special code if your game requires that.
                        m_StartMove = false;
                        m_Path.Release(this);
                        m_Path = null;
                        m_Seeker.CancelCurrentPathRequest();
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void FixedTickCharacterBaseAgent(float fixedDeltaTime)
    {
        //朝目的地移动
        if(m_StartMove && m_Mover != null && m_Path.vectorPath != null && m_Path.vectorPath.Count > 0)
        {
            Vector3 dir = (m_Path.vectorPath[m_PathPointIndexCur] - transform.position).normalized;
            dir.y = 0f;
            SetMoveDirFixed(dir);
        }
        else
        {
            ClearMoveDirFixed();
        }
    }

    public void OnPathComplete(Path path)
    {
        // Path pooling. To avoid unnecessary allocations paths are reference counted.
        // Calling Claim will increase the reference count by 1 and Release will reduce
        // it by one, when it reaches zero the path will be pooled and then it may be used
        // by other scripts. The ABPath.Construct and Seeker.StartPath methods will
        // take a path from the pool if possible. See also the documentation page about path pooling.

        if(!path.error)
            PathfindingUtility.PathSmoothLayerGridDiagonal(path);

        path.Claim(this);
        if (!path.error)
        {
            if (m_Path != null) 
                m_Path.Release(this);

            m_Path = path;
            m_PathPointIndexCur = 0;
            m_StartMove = true;
        }
        else
        {
            path.Release(this);
            m_StartMove = false;
        }
    }

    public void OnGraphUpdate()
    {
        //重新计算路径
        if (m_StartMove && m_MoveTargetPos != null && m_Seeker != null)
        {
            m_Seeker.StartPath(transform.position, m_MoveTargetPos, OnPathComplete);
        }
    }

#if UNITY_EDITOR

    public void OnDrawGizmosCharacterBaseAgent()
    {
        if (m_Seeker != null && m_Seeker.drawGizmos && m_Path != null && m_Path.vectorPath != null)
        {
            Gizmos.color = new Color(1f, 0.6f, 0, 1f);

            if (m_Path.vectorPath != null)
            {
                for (int i = 0; i < m_Path.vectorPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(m_Path.vectorPath[i], m_Path.vectorPath[i + 1]);
                }
            }
        }
    }

#endif
}