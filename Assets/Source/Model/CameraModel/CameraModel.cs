using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraModel : Singleton<CameraModel>, IDestroy
{
    /// <summary>
    /// 像素缩放比例 原像素/比例值
    /// </summary>
    public int ScaleRatio { get; } = 4;

    public override void Init()
    {
        base.Init();

    }

    #region 主摄像机
    /// <summary>
    /// 摄像机 主场景
    /// </summary>
    private Camera m_CameraMain;
    public Camera CameraMain
    {
        get
        {
            if (m_CameraMain == null) m_CameraMain = Camera.main;

            return m_CameraMain;
        }
    }

    private CameraController m_MainCameraController; //主摄像机 控制器

    /// <summary>
    /// 设置 主摄像机
    /// </summary>
    /// <param name="camera"></param>
    public void SetCameraMain(Camera camera)
    {
        m_CameraMain = camera;
        //主摄像机 缩放比
        m_MainCameraController = m_CameraMain.GetComponent<CameraController>();
        m_MainCameraController.SetCameraSizeScaleRatio(ScaleRatio);
    }

    /// <summary>
    /// 主相机 坐标转换 屏幕→世界
    /// </summary>
    /// <param name="posScreen"></param>
    /// <returns></returns>
    public Vector3 MainCameraScreenToWorldPoint(Vector2 posScreen)
    {
        Vector3 posParam = new Vector3(posScreen.x, posScreen.y, 0f);

        //屏幕坐标高度 映射 摄像机45度照射
        float sin45 = Mathf.Sin(45f * Mathf.Deg2Rad);
        posParam.y = posParam.y * sin45 + Screen.height * (1f - sin45) * 0.5f;
        //根据屏幕坐标所在高度 增减摄像机视锥距离
        float screenHeightCenter = Screen.height * 0.5f; //屏幕坐标高度中心点
        posParam.z = MainCameraTargetPosDistans + CameraMain.orthographicSize * ((posParam.y - screenHeightCenter) / screenHeightCenter);

        //转换为世界坐标
        var posWorld = CameraMain.ScreenToWorldPoint(posParam);

        return posWorld;
    }

    /// <summary>
    /// 主摄像机 目标点距离
    /// </summary>
    public float MainCameraTargetPosDistans
    {
        get { return m_MainCameraController.TargetPosDistans; }
        set { m_MainCameraController.TargetPosDistans = value; }
    }

    /// <summary>
    /// 设置 主摄像机位置的XY轴向的值
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="isImmediately">立即设置</param>
    public void SetCameraMainPos(Vector3 targetPos, bool isImmediately = false)
    {
        m_MainCameraController.SetCameraMainPos(targetPos, isImmediately);
    }

    /// <summary>
    /// 设置 摄像机缩放比 1像素:ScaleRatio
    /// </summary>
    /// <param name="scaleRatio">缩放像素比例</param>
    public void SetCameraMainSizeScaleRatio(float scaleRatio = 0)
    {
        if (scaleRatio == 0)
        {
            //无传参时 设置为全局默认值
            scaleRatio = ScaleRatio;
        }
        m_MainCameraController.SetCameraSizeScaleRatio(scaleRatio);
    }
    #endregion

    #region UI摄像机
    /// <summary>
    /// 摄像机 UI
    /// </summary>
    private Camera m_CameraUI;
    public Camera CameraUI
    {
        get
        {
            return m_CameraUI;
        }
    }

    /// <summary>
    /// 设置 UI摄像机
    /// </summary>
    /// <param name="cameara"></param>
    public void SetCameraUI(Camera cameara)
    {
        m_CameraUI = cameara;
        m_CameraUI.orthographicSize = 540f / ScaleRatio;
    }
    #endregion

}
