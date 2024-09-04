using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float m_MovementSmoothingValue = 2f; // 移动的平滑系数

    private float m_ScaleRatio = 1f; //缩放比

    private Camera m_Camera; //摄像机
    private Transform m_CameraTrans; //摄像机Transform
    private Vector3 m_CameraTargetPos; //目标位置
    private Vector3 m_CurrentVelocity = Vector3.zero; //移动速度
    private bool m_IsMovement = false; //是否进行移动
    private Tweener m_TweenerScaleRatio; //DoTween 比例缩放

    /// <summary>
    /// 摄像机 视觉目标点距离
    /// </summary>
    public float TargetPosDistans { get; set; } = 10f;

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_CameraTrans = transform;
        m_CameraTargetPos = m_CameraTrans.position;

        //初始化 设置相机缩放比
        SetCameraSizeScaleRatio(m_ScaleRatio);
    }

    private void FixedUpdate()
    {
        if (m_IsMovement)
        {
            float targetDis = Vector3.Distance(m_CameraTrans.position, m_CameraTargetPos);
            if (targetDis < 0.0001f)
            {
                //距离目标点很近时 停止进行移动
                m_IsMovement = false;
                m_CameraTrans.position = m_CameraTargetPos;
            }
            else
                //平滑移动
                m_CameraTrans.position = Vector3.SmoothDamp(m_CameraTrans.position, m_CameraTargetPos, ref m_CurrentVelocity, m_MovementSmoothingValue * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 设置 摄像机位置的XY轴向的值
    /// </summary>
    /// <param name="targetPos">目标点位置</param>
    /// <param name="isImmediately">立即设置</param>
    public void SetCameraMainPos(Vector3 targetPos, bool isImmediately = false)
    {
        //摄像机目标位置 45度后退
        float length = Mathf.Sin(45f * Mathf.Deg2Rad) * TargetPosDistans;
        m_CameraTargetPos = new Vector3(targetPos.x, targetPos.y + length, targetPos.z - length);
        
        if (isImmediately)
            //立即设置Pos
            m_CameraTrans.position = m_CameraTargetPos;
        else
        {
            //平滑移动
            m_IsMovement = true;
            m_CurrentVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 设置 摄像机缩放比 1像素:ScaleRatio
    /// </summary>
    /// <param name="scaleRatio">缩放像素比例</param>
    /// <param name="isTween">补间动画</param>
    public void SetCameraSizeScaleRatio(float scaleRatio, bool isTween = true)
    {
        m_ScaleRatio = scaleRatio;

        if (m_Camera == null) { return; }

        if (isTween) //补间动画
        {
            float scaleRatioCur = 5.4f / m_Camera.orthographicSize; //当前缩放比
            if (scaleRatioCur == m_ScaleRatio) return;

            if (m_TweenerScaleRatio != null)
            {
                m_TweenerScaleRatio.Kill();
                m_TweenerScaleRatio = null;
            }
            m_TweenerScaleRatio = DOTween.To(() => scaleRatioCur, x => scaleRatioCur = x, m_ScaleRatio, 0.6f);
            m_TweenerScaleRatio.OnUpdate(() => { m_Camera.orthographicSize = 5.4f / scaleRatioCur; });
        }
        else
            //立即设置
            m_Camera.orthographicSize = 5.4f / m_ScaleRatio;
    }
}
