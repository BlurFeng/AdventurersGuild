using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using TMPro;
using com.ootii.Messages;
using DG.Tweening;

public class WorldMapWindow : WindowBase
{
    [SerializeField] private GameObject m_BtnCloseWindow = null; //按钮 关闭窗口
    [SerializeField] private GameObject m_BtnMapDragMove = null; //按钮 拖拽移动

    //大地图 拖拽
    private Camera m_CameraMain; //主相机
    private Transform m_MainCameraTrans; //主相机 Transform
    private Vector3 m_CameraPosOrigin = Vector3.zero; //相机 位置 原值
    private float m_CameraSizeOrigin; //相机 尺寸 原值
    private Vector3 m_CameraPosLast = Vector3.zero; //相机 位置 之前
    private Vector3 m_MousePosOLast = Vector3.zero; //鼠标 位置 之前
    private float m_MouseDragSpeed = 0.01f; //鼠标拖拽速度 标准
    private float m_MouseDragSpeedCur; //鼠标拖拽速度 当前
    private float m_SizeMax = 5f; //相机尺寸 最大值
    private float m_SizeMin = 1f; //相机尺寸 最小值
    private float m_PosXMax = 20.48f; //相机位置Y轴 最大值
    private float m_PosYMax = 13.65f; //相机位置X轴 最大值
    private float m_PosXMaxCur; //相机Y轴 最大值 当前
    private float m_PosXMinCur; //相机Y轴 最小值 当前
    private float m_PosYMaxCur; //相机X轴 最大值 当前
    private float m_PosYMinCur; //相机X轴 最小值 当前

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnCloseWindow).SetClickHandler(BtnCloseWindow);
        ClickListener.Get(m_BtnMapDragMove).SetPointerDownHandler(BtnMapDragMoveDown);
        ClickListener.Get(m_BtnMapDragMove).SetDragHandler(BtnMapDragMoveDrag);

        m_CameraMain = CameraModel.Instance.CameraMain;
        m_MainCameraTrans = m_CameraMain.transform;
        m_CameraPosOrigin = m_MainCameraTrans.position;
        m_CameraSizeOrigin = m_CameraMain.orthographicSize;
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //20211107 Winhoo: 冒险者公会项目暂时用不到WorldMapWindow 也没有配置对应的Level
        ////切换关卡
        //if(FWorldContainer.CurrentWorld.WorldConfig.levelConfigs.Count > 0)
        //    FWorldContainer.SwitchLevel(FWorldContainer.CurrentWorld.WorldConfig.levelConfigs[0]);

        SetCamerePosLimit(m_CameraMain.orthographicSize);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        var axisWheel = Input.GetAxis("Mouse ScrollWheel");
        if (axisWheel < 0 && m_CameraMain.orthographicSize < m_SizeMax)
        {
            //缩小地图
            m_CameraMain.orthographicSize += 0.2f;
            if (m_CameraMain.orthographicSize > m_SizeMax)
            {
                m_CameraMain.orthographicSize = m_SizeMax;
            }

            SetCamerePosLimit(m_CameraMain.orthographicSize);
        }
        else if (axisWheel > 0 && m_CameraMain.orthographicSize > m_SizeMin)
        {
            //放大地图
            m_CameraMain.orthographicSize -= 0.2f;
            if (m_CameraMain.orthographicSize < m_SizeMin)
            {
                m_CameraMain.orthographicSize = m_SizeMin;
            }

            SetCamerePosLimit(m_CameraMain.orthographicSize);
        }
    }

    //按钮 关闭窗口
    private void BtnCloseWindow(UnityEngine.EventSystems.PointerEventData eventData)
    {
        //相机位置 还原
        m_MainCameraTrans.position = m_CameraPosOrigin;
        m_CameraMain.orthographicSize = m_CameraSizeOrigin;

        //切换关卡
        FWorldContainer.SwitchLevel(FWorldContainer.CurrentWorld.WorldConfig.persistentLevel);

        CloseWindow();
    }

    //按钮 地图拖拽 按下
    private void BtnMapDragMoveDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_CameraPosLast = m_MainCameraTrans.position;
        m_MousePosOLast = Input.mousePosition;
    }

    //按钮 地图拖拽 长按
    private void BtnMapDragMoveDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        MoveCameraPosition2D();
    }

    //设置 相机位置 上限值
    private void SetCamerePosLimit(float sizeCur)
    {
        float sizeScale = sizeCur / 5.4f;
        float screenX = Screen.width * 0.005f;
        float screenY = Screen.height * 0.005f;

        m_PosXMaxCur = m_PosXMax - screenX * sizeScale;
        m_PosXMinCur = m_PosXMaxCur * -1f;
        m_PosYMaxCur = m_PosYMax - screenY * sizeScale;
        m_PosYMinCur = m_PosYMaxCur * -1f;

        m_MouseDragSpeedCur = m_MouseDragSpeed * sizeScale;

        SetCameraPos(m_MainCameraTrans.position); //重新设置 相机位置
    }

    //相机 移动位置 2D
    private void MoveCameraPosition2D()
    {
        //鼠标位移
        Vector3 direction = (Input.mousePosition - m_MousePosOLast) * m_MouseDragSpeedCur;
        if (direction == Vector3.zero) { return; }

        //目标位置
        Vector2 posTarget = new Vector2(m_CameraPosLast.x - direction.x, m_CameraPosLast.y - direction.y);
        SetCameraPos(posTarget);
    }

    //设置 相机位置
    private void SetCameraPos(Vector2 posTarget)
    {
        //限制范围
        if (posTarget.x > m_PosXMaxCur)
        {
            posTarget.x = m_PosXMaxCur;
        }
        else if (posTarget.x < m_PosXMinCur)
        {
            posTarget.x = m_PosXMinCur;
        }

        if (posTarget.y > m_PosYMaxCur)
        {
            posTarget.y = m_PosYMaxCur;
        }
        else if (posTarget.y < m_PosYMinCur)
        {
            posTarget.y = m_PosYMinCur;
        }

        m_MainCameraTrans.position = new Vector3(posTarget.x, posTarget.y, 0);
    }
}
