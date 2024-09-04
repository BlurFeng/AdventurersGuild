using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using System;

public class UGroundedCheckComponent : UActorComponent
{
    public UGroundedCheckComponent(AActor actor) : base(actor)
    {
        //init，在此初始化需要的内容
    }

    private bool m_IsEnableGroundedCheck;

    //事件
    protected Action OnGrounded;
    protected Action<float> OnStayGrounded;
    protected Action OnUngrounded;
    protected Action<float> OnStayUnGrounded;

    private RaycastHit[] m_GroundedCheckRaycastHits;//地面探测用射线投射返回数据缓存
    /// <summary>
    /// 脚下地面检测Hit数据缓存
    /// </summary>
    public RaycastHit GroundedCheckHit { get { return m_GroundedCheckHit; } }
    private RaycastHit m_GroundedCheckHit;
    /// <summary>
    /// 前方地面检测Hit数据缓存
    /// </summary>
    public RaycastHit GroundedCheckHit_Front { get { return m_GroundedCheckHit_Front; } }
    private RaycastHit m_GroundedCheckHit_Front;

    private float m_GroundedCheckIntervalTime;//地面检测间隔时间
    private float m_GroundedCheckTimer;//地面检测间隔时间计时器
    private Vector3 m_BoundsExtentsCached;//碰撞器范围盒范围缓存
    private float m_GroundedCheckDis;//确认地面用射线距离
    private float m_GroundedStayTimer;//地面时间计数器
    private Vector3 m_FrontDir;//前方向量
    private float m_GroundedFrontDirOffsetDis;//确认移动方向是否有地面时 射线中心点向移动方向偏移距离

    [Header("地面确认距离修正")]
    [SerializeField] private float NotGroundedLimitDisOffset;
    private LayerMask m_GroundMask;//地面层遮罩 确认哪些层为地面 为空则检测所有层
    private float m_GroundedLimitDis;//确认为贴面的限制距离
    private float m_NotGroundedLimitDis;//确认为离地的限制距离
    private float m_GroundedFrontDirLimitDis;//移动方向 确认为贴面的限制距离
    private float m_NotGroundedFrontDirLimitDis;//移动方向 确认为离地的限制距离

    //一个默认的获取Collider的最后目标
    private Collider m_ColliderNone;

    private bool m_IsGrounded;
    /// <summary>
    /// 是否接触地面
    /// </summary>
    public bool IsGrounded { get { return m_IsGrounded; } }

    private bool m_IsGroundedFlat;
    /// <summary>
    /// 是否在平整的地面 (在离地时能用于确认最后一次着地是否在平整的地面)
    /// </summary>
    public bool IsGroundedFlat { get { return m_IsGroundedFlat; } }

    private bool m_IsGroundedFrontDir;
    /// <summary>
    /// 移动方向前方是否有地面
    /// </summary>
    public bool IsGroundedFrontDir { get { return m_IsGroundedFrontDir; } }

    private bool m_IsGroundedFlatFrontDir;
    /// <summary>
    /// 移动方向前方地面 是否是平整的地面 (在离地时能用于确认最后一次着地是否在平整的地面)
    /// </summary>
    public bool IsGroundedFlatFrontDir { get { return m_IsGroundedFlatFrontDir; } }

    #region Public

    /// <summary>
    /// 开关地面监测功能
    /// </summary>
    /// <param name="enable"></param>
    public void EnableGroundedCheck(bool enable)
    {
        if (enable == m_IsEnableGroundedCheck) return;

        m_IsEnableGroundedCheck = enable;
    }

    /// <summary>
    /// 设置前方方向
    /// 一般此数据需要在角色移动或转变方向时更新
    /// </summary>
    /// <param name="frontDir"></param>
    public void SetFrontDir(Vector3 frontDir)
    {
        m_FrontDir = frontDir;
    }

    /// <summary>
    /// 设置地面检测的LayerMask
    /// </summary>
    /// <param name="newLayerMask"></param>
    public void SetGroundLayerMask(LayerMask newLayerMask)
    {
        m_GroundMask = newLayerMask;
    }

    #endregion

    public override void Begin()
    {
        base.Begin();

        m_GroundedCheckRaycastHits = new RaycastHit[16];
        m_GroundedCheckIntervalTime = 0.1f;
        m_IsEnableGroundedCheck = true;
    }

    public override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);

        FixedTickGroundedCheck(fixedDeltaTime);

        if (!m_IsEnableGroundedCheck) return;

        if (m_IsGrounded)
        {
            OnStayGrounded?.Invoke(m_GroundedStayTimer);
        }
        else
        {
            OnStayUnGrounded?.Invoke(m_GroundedStayTimer);
        }
    }

    private Collider GetCollider()
    {
        AActor actor = GetOwner();
        if (actor)
        {
            return actor.GetCollider;
        }
        else
        {
            return GetColliderCustom();
        }
    }

    protected virtual Collider GetColliderCustom()
    {
        if (m_ColliderNone == null) m_ColliderNone = new Collider();

        return m_ColliderNone;
    }

    /// <summary>
    /// 刷新地面检测
    /// </summary>
    /// <param name="fixedDeltaTime"></param>
    private void FixedTickGroundedCheck(float fixedDeltaTime)
    {
        if (!m_IsEnableGroundedCheck) return;

        //计数器 间隔时间执行
        if (!FsUtility.OverflowValue(ref m_GroundedCheckTimer, fixedDeltaTime, m_GroundedCheckIntervalTime)) return;
        if (!CheckColliderBounds()) return;

        m_GroundedStayTimer += fixedDeltaTime;
        float dis;
        if (RaycastGroundedCheck(GetCollider().bounds.center, Vector3.down, m_GroundedCheckDis, out dis, out m_GroundedCheckHit))
        {
            //确认在地面或者离开地面
            //使用两个限定距离来进行离地和贴地的切换 而不是一个点 因为一个点可能导致状态来回抖动的情况
            if (m_IsGrounded)
            {
                if (dis >= m_NotGroundedLimitDis)
                    SwitchIsUnGrounded();
            }
            else
            {
                if (dis <= m_GroundedLimitDis)
                    SwitchIsGrounded();
            }
        }
        else
            SwitchIsUnGrounded();

        //确认移动方向地面是否为地面
        if (m_FrontDir != Vector3.zero
            && RaycastGroundedCheck(GetCollider().bounds.center + m_FrontDir * m_GroundedFrontDirOffsetDis, Vector3.down, m_GroundedCheckDis, out dis, out m_GroundedCheckHit_Front))
        {
            if (m_IsGroundedFrontDir)
            {
                if (dis >= m_NotGroundedFrontDirLimitDis)
                    SwitchIsUnGroundedFrontDir();
            }
            else
            {
                if (dis <= m_GroundedFrontDirLimitDis)
                    SwitchIsGroundedFrontDir();
            }
        }
        else
            SwitchIsUnGroundedFrontDir();
    }

    /// <summary>
    /// 切换到着地
    /// </summary>
    /// <param name="hit"></param>
    private void SwitchIsGrounded()
    {
        if (m_IsGrounded) return;

        m_IsGrounded = true;
        m_IsGroundedFlat = Vector3.Dot(GroundedCheckHit.normal, Vector3.up) >= 0.95f;//在平整的地面
        m_GroundedStayTimer = 0f;
        OnGrounded?.Invoke();
        //Debug.Log("角色着地，" + (m_IsGroundedFlat ? "在平地" : "在斜坡" ) + "  " + GroundedCheckHit.collider.transform.parent.gameObject.name);
    }

    /// <summary>
    /// 切换到离地
    /// </summary>
    private void SwitchIsUnGrounded()
    {
        if (!m_IsGrounded) return;

        m_IsGrounded = false;
        m_GroundedStayTimer = 0f;
        OnUngrounded?.Invoke();
        //Debug.Log("角色离地，" + (m_IsGroundedFlat ? "最后一次着地在平地" : "最后一次着地在斜坡") + "  " + GroundedCheckHit.collider.transform.parent.gameObject.name);
    }

    /// <summary>
    /// 切换到着地
    /// </summary>
    /// <param name="hit"></param>
    private void SwitchIsGroundedFrontDir()
    {
        if (m_IsGroundedFrontDir) return;

        m_IsGroundedFrontDir = true;
        m_IsGroundedFlatFrontDir = Vector3.Dot(m_GroundedCheckHit_Front.normal, Vector3.up) >= 0.95f;//在平整的地面
    }

    /// <summary>
    /// 切换到离地
    /// </summary>
    private void SwitchIsUnGroundedFrontDir()
    {
        if (!m_IsGroundedFrontDir) return;

        m_IsGroundedFrontDir = false;
    }

    /// <summary>
    /// 射线地面检测
    /// </summary>
    /// <param name="origin">中心点</param>
    /// <param name="dir">方向</param>
    /// <param name="rayDis">射线距离</param>
    /// <param name="dis">最近的hit点和自身底部距离</param>
    /// <param name="hit">最近的点</param>
    /// <returns></returns>
    private bool RaycastGroundedCheck(Vector3 origin, Vector3 dir, float rayDis, out float dis, out RaycastHit hit)
    {
        bool haveHits = RaycastNonAllocForGroundedCheck(origin, dir, rayDis, out int hitNum);
        //Debug.DrawLine(origin, origin + dir * rayDis);

        if (haveHits)
        {
            if (hitNum > 0)
            {
                //返回的m_GroundedCheckRaycastHits是无序的
                var raycastHitSort = new List<RaycastHit>(m_GroundedCheckRaycastHits);
                raycastHitSort.Sort((RaycastHit a, RaycastHit b) => { return a.distance > b.distance ? 1 : -1; });
                m_GroundedCheckRaycastHits = raycastHitSort.ToArray();
            }

            hit = m_GroundedCheckRaycastHits[0];
            Vector3 bottomPoint = origin + Vector3.down * GetCollider().bounds.extents.y;
            //Debug.DrawLine(bottomPoint, bottomPoint + Vector3.down * 0.1f, Color.red);
            dis = bottomPoint.y - hit.point.y;

            return true;
        }

        //射线未投射到任何点
        hit = new RaycastHit();
        dis = 0f;
        return false;
    }

    private bool RaycastNonAllocForGroundedCheck(Vector3 origin, Vector3 dir, float maxDis, out int hitNum)
    {
        //Debug.DrawLine(origin, origin + dir * maxDis);

        if (m_GroundMask.value != 0)
            hitNum = Physics.RaycastNonAlloc(origin, dir, m_GroundedCheckRaycastHits, maxDis, m_GroundMask);
        else
            hitNum = Physics.RaycastNonAlloc(origin, dir, m_GroundedCheckRaycastHits, maxDis);

        return hitNum > 0;
    }

    /// <summary>
    /// 确认碰撞体包围盒范围，设置相关数据
    /// </summary>
    /// <returns></returns>
    private bool CheckColliderBounds()
    {
        if (GetCollider() == null) return false;

        if (m_BoundsExtentsCached != GetCollider().bounds.extents)
        {
            m_GroundedCheckDis = GetCollider().bounds.extents.magnitude * 2f;
            m_GroundedLimitDis = m_GroundedCheckDis * 0.1f;
            m_NotGroundedLimitDis = m_GroundedCheckDis * 0.1f + NotGroundedLimitDisOffset;
            m_GroundedFrontDirLimitDis = m_GroundedCheckDis * 0.5f;
            m_NotGroundedFrontDirLimitDis = m_GroundedCheckDis * 0.5f;

            m_GroundedFrontDirOffsetDis = GetCollider().bounds.size.x * 0.6f;
        }

        return true;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        //Vector3 center = GetCollider.bounds.center + m_DirectionInfoComponent.FrontDir.Direction * m_GroundedFrontDirOffsetDis;
        //Gizmos.DrawSphere(center, 0.05f);
        //Debug.DrawLine(center, center + Vector3.down * m_GroundedCheckDis, Color.red, 0.2f);
    }

#endif
}