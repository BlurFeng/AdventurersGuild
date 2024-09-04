using com.ootii.Messages;
using Deploy;
using FsGameFramework;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using FsGridCellSystem;

public class CursorWindow : WindowBase
{
    public override void OnLoaded()
    {
        base.OnLoaded();

        MessageDispatcher.AddListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, MsgOperateMouseSceneChange);

        MemoryLock = true;

        //原光标 设置 不可见
        Cursor.visible = false;

        LoadedCursorType(); //光标类型
    }

    public override void OnRelease()
    {
        base.OnRelease();

        MessageDispatcher.RemoveListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, MsgOperateMouseSceneChange);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        Vector3 mousePos = Input.mousePosition;

        //更新 光标位置
        m_RectTransCursor.anchoredPosition = mousePos / CameraModel.Instance.ScaleRatio;

        CheckCursorGridCoord();
    }

    #region 光标显示类型

    [SerializeField] private RectTransform m_RectTransCursor = null; //RectTrans 光标
    [SerializeField] private List<GameObject> m_ListGoCursorType = null; //列表 物体 光标类型

    private ECursorType m_ECursorTypeCur = ECursorType.None; //当前的 光标类型
    private GameObject m_GoCursorType = null; //当前的 光标物体

    public enum ECursorType
    {
        /// <summary>
        /// 不显示
        /// </summary>
        None,
        /// <summary>
        /// 正常
        /// </summary>
        Normal,
        /// <summary>
        /// 操作
        /// </summary>
        Operate,
    }

    //初始化 光标类型
    private void LoadedCursorType()
    {
        for (int i = 0; i < m_ListGoCursorType.Count; i++)
        {
            m_ListGoCursorType[i].SetActive(false);
        }
        SetCursorType(ECursorType.Normal);
    }

    //设置 光标类型
    private void SetCursorType(ECursorType type)
    {
        if (m_ECursorTypeCur == type) { return; }
        m_ECursorTypeCur = type;

        //隐藏 之前的光标
        if (m_GoCursorType != null)
        {
            m_GoCursorType.SetActive(false);
            m_GoCursorType = null;
        }

        //显示 当前的光标
        int index = (int)m_ECursorTypeCur;
        if (index < m_ListGoCursorType.Count)
        {
            m_GoCursorType = m_ListGoCursorType[index];
            m_GoCursorType.SetActive(true);
        }
    }

    #endregion

    #region 场景交互操作
    /// <summary>
    /// 开关 网格坐标 检查
    /// </summary>
    public bool EnableCheckGridCoord
    {
        get { return m_EnableCheckGridCoord; }
        set
        {
            m_EnableCheckGridCoord = value;
            if (m_EnableCheckGridCoord)
                CheckRaycastFurniture(m_CursorPosScreen);
            else
                SetCursorType(ECursorType.Normal);
        }
    }
    private bool m_EnableCheckGridCoord = true;

    private Vector3 m_CursorPosScreen; //鼠标屏幕坐标 当前
    private GridCoord m_FurnitureGridCoordCur; //家具层 网格坐标 当前

    //消息 玩家角色操作 改变
    private void MsgOperateMouseSceneChange(IMessage rMessage)
    {
        var isEnable = (bool)rMessage.Data;
        EnableCheckGridCoord = isEnable;
    }

    //检查 光标所在单元格
    private void CheckCursorGridCoord()
    {
        //检查 光标屏幕坐标
        var posScreenNew = Input.mousePosition;
        if ((m_CursorPosScreen - posScreenNew).magnitude < 10f) return;
        m_CursorPosScreen = posScreenNew;

        //类射线检测 网格坐标 网格项目
        if (m_EnableCheckGridCoord)
            CheckRaycastFurniture(m_CursorPosScreen);
    }

    //类射线检测 家具层网格坐标 网格项目
    private void CheckRaycastFurniture(Vector3 posScreen)
    {
        //射线检测 获取家具
        var gridItemFurniture = GuildGridModel.Instance.ScreenPointToRay(GuildGridModel.EGridLayer.Furniture, posScreen);
        GridCoord gridCoordNew = GridCoord.zero;

        if (gridItemFurniture != null && gridItemFurniture.Value > 0)
        {
            //有家具 光标显示 操作
            SetCursorType(ECursorType.Operate);
            gridCoordNew = gridItemFurniture.MainGridCoord;
        }
        else
        {
            //无家具 光标显示 正常
            SetCursorType(ECursorType.Normal);
            gridCoordNew = GuildGridModel.Instance.ScreenPointToGridCoord(posScreen, Vector3.zero, GuildGridModel.Instance.OperateGridCoordZ);
        }

        //光标所在的 家具层网格坐标 是否改变
        if (m_FurnitureGridCoordCur != gridCoordNew)
        {
            m_FurnitureGridCoordCur = gridCoordNew;
            MessageDispatcher.SendMessageData(WindowSystemMsgType.CURSORWINDOW_FURNITURE_GRIDCOORD_CHANGE, m_FurnitureGridCoordCur);
        }
    }
    #endregion
}
