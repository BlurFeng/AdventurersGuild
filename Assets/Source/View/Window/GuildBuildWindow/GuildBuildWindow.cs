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

public class GuildBuildWindow : WindowBase
{
    public enum EPanelType
    {
        None = 0,
        /// <summary>
        /// 建筑模式
        /// </summary>
        Build = 1,
        /// <summary>
        /// 家具模式
        /// </summary>
        Furniture = 2,
    }

    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private GameObject m_BtnScene = null; //按钮 场景
    [SerializeField] private GameObject m_BtnSwitchPanelBuild = null; //按钮 切换 建筑模式
    [SerializeField] private GameObject m_BtnSwitchPanelFurniture = null; //按钮 切换 家具模式
    [SerializeField] private PanelGuildBuild m_PanelGuildBuild = null; //面板 建筑模式
    [SerializeField] private PanelGuildFurniture m_PanelGuildFurniture = null; //面板 家具模式
    [Header("场景网格尺寸提示")]
    [SerializeField] private Transform m_RootFrameGridCell = null; //根节点 占地网格
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellDown = null; //SpriteRender 网格单元格 下
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellUp = null; //SpriteRender 网格单元格 上
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellBack = null; //SpriteRender 网格单元格 后
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellFront = null; //SpriteRender 网格单元格 前
    [SerializeField] private GameObject m_GoFrameGridCellHeight = null; //物体 高度网格提示
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellHeightDown = null; //SpriteRender 网格单元格 高度 下
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellHeightBack = null; //SpriteRender 网格单元格 高度 后
    [SerializeField] private SpriteRenderer m_SpRenderFrameGridCellHeightFront = null; //SpriteRender 网格单元格 高度 前
    [SerializeField] private TextMeshPro m_TxtOperateGridCoordZ = null; //文本 当前操作的坐标高度
    [SerializeField] private GameObject m_GoSpOperateTipMoveXY = null; //物体 操作提示 移动XY轴
    [SerializeField] private GameObject m_GoSpOperateTipMoveZ = null; //物体 操作提示 移动Z轴

    private EPanelType m_EPanelTypeCur = EPanelType.None; //当前的面板模式
    private PanelGuildBase m_PanelGuildCur; //面板 当前
    private GridCoord m_OperateGridCoordCur; //单元格坐标 当前

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnScene).SetClickHandler(BtnScene);
        ClickListener.Get(m_BtnSwitchPanelBuild).SetClickHandler(BtnSwitchPanelBuild);
        ClickListener.Get(m_BtnSwitchPanelFurniture).SetClickHandler(BtnSwitchPanelFurniture);
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //默认打开 家具面板
        SwitchPanelType(EPanelType.Furniture);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        //视野缩小 默认比例
        CameraModel.Instance.SetCameraMainSizeScaleRatio();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        OnHandlerPointMove();
    }

    //切换 当前面板类型
    private void SwitchPanelType(EPanelType panelType)
    {
        if (m_EPanelTypeCur == panelType) { return; }
        m_EPanelTypeCur = panelType;

        //关闭 旧面板
        if (m_PanelGuildCur != null)
            m_PanelGuildCur.OnClose();

        switch (m_EPanelTypeCur)
        {
            case EPanelType.Build:
                m_PanelGuildCur = m_PanelGuildBuild;
                //视野扩大
                CameraModel.Instance.SetCameraMainSizeScaleRatio(2f);
                break;
            case EPanelType.Furniture:
                m_PanelGuildCur = m_PanelGuildFurniture;
                //视野缩小 默认比例
                CameraModel.Instance.SetCameraMainSizeScaleRatio();
                break;
        }

        //打开 新面板
        if (m_PanelGuildCur != null)
        {
            m_PanelGuildCur.OnOpen();
            //消息回调 操作点网格物体改变
            if (m_PanelGuildCur.OnChangePrefabComplete == null)
                m_PanelGuildCur.OnChangePrefabComplete = EvtOnChangePrefabComplete;

            m_OperateStepCur = m_PanelGuildCur.HandlerHadObject ? 1 : 0;
            //刷新 操作提示 移动
            RefreshOperateMoveTip();
        }
    }

    #region 按钮

    //按钮 关闭界面
    private void BtnClose(PointerEventData obj)
    {
        //视野缩小 默认比例
        CameraModel.Instance.SetCameraMainSizeScaleRatio();

        CloseWindow();
    }

    //按钮 点击场景
    private void BtnScene(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                OnInputButtonClickLeft();
                break;
            case PointerEventData.InputButton.Right:
                OnInputButtonClickRight();
                break;
            case PointerEventData.InputButton.Middle:
                OnInputButtonClickMiddle();
                break;
        }
    }

    private void BtnSwitchPanelBuild(PointerEventData obj)
    {
        SwitchPanelType(EPanelType.Build);
    }

    private void BtnSwitchPanelFurniture(PointerEventData obj)
    {
        SwitchPanelType(EPanelType.Furniture);
    }

    #endregion

    #region 操作点
    /// <summary>
    /// 操作步骤 当前 0操作点无物体 1选择物体XY坐标 2选择物体Z坐标
    /// </summary>
    private int m_OperateStepCur;
    private Vector3 m_posScreenLast = Vector3.zero; //上次的光标屏幕坐标

    #region 操作点
    //改变家具(家具Id或朝向) 完成
    private void EvtOnChangePrefabComplete()
    {
        RefreshOperatePointPosAmend(); //刷新 操作点位置修正
        RefreshOperateStepCur(); //更新 操作步骤

        //检查并更新三维网格提示
        CheckRefreshGirdCellUIAsPanelGuild();
    }

    //刷新 操作点位置修正
    private void RefreshOperatePointPosAmend()
    {
        //世界坐标修正 使操作点单元格 处于家具占地网格的中心
        m_HandlerOperatePointPosAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(m_PanelGuildCur.ItemOperateBase.GetGridItemSize) - GuildGridModel.Instance.GetGridItemSizePositionAmend(GridCoord.one);
        m_HandlerOperatePointPosAmend.y = 0; //高度不修正 操作点在物体底部中心
    }

    /// <summary>
    /// 世界坐标修正 使操作点在家具中心点
    /// </summary>
    protected Vector3 m_HandlerOperatePointPosAmend;
    #endregion

    //操作点 移动
    private void OnHandlerPointMove()
    {
        if (m_PanelGuildCur == null) { return; }

        //当前的光标屏幕坐标
        var posScreen = Input.mousePosition; //根据光标坐标 获取单元格坐标
        //性能优化 大于最小移动距离 才进行检查
        if ((m_posScreenLast - posScreen).magnitude < 10f) return;
        m_posScreenLast = posScreen;

        //操作的网格层
        GuildGridModel.EGridLayer gridLayer = GuildGridModel.EGridLayer.Furniture;
        switch (m_EPanelTypeCur)
        {
            case EPanelType.Build:
                gridLayer = GuildGridModel.EGridLayer.Building;
                break;
            case EPanelType.Furniture:
                gridLayer = GuildGridModel.EGridLayer.Furniture;
                break;
        }

        switch (m_OperateStepCur)
        {
            case 0: //操作点无物体
                //类射线检测 获取场景中的网格物体
                var posAmend = -m_HandlerOperatePointPosAmend;
                GridItemComponent gridItem = GuildGridModel.Instance.ScreenPointToRay(gridLayer, Input.mousePosition, posAmend);

                if (gridItem != null) //射线检测到网格物体
                {
                    //更新 当前网格坐标
                    SetOperateGridCoordCur(gridItem.MainGridCoord);
                    //刷新三维网格提示
                    CheckRefreshGirdCellUI(gridItem.MainGridCoord, gridItem.GetGridItemSizeAtDirection); 
                    SetGridCellUIState(EGridCellUIState.HadItem); //设置三维网格UI提示状态
                }
                else //无网格物体 使用操作中的高度层
                {
                    //更新 当前网格坐标
                    SetOperateGridCoordCur(GuildGridModel.Instance.ScreenPointToGridCoord(Input.mousePosition, posAmend, GuildGridModel.Instance.OperateGridCoordZ));
                    //刷新三维网格提示
                    CheckRefreshGirdCellUI(m_OperateGridCoordCur, GridCoord.one);
                    SetGridCellUIState(EGridCellUIState.Default); //设置三维网格UI提示状态
                }
                break;
            case 1: //选择物体XY坐标
                //更新 当前网格坐标
                SetOperateGridCoordCur(GuildGridModel.Instance.ScreenPointToGridCoord(Input.mousePosition, -m_HandlerOperatePointPosAmend, GuildGridModel.Instance.OperateGridCoordZ));
                break;
            case 2: //选择物体Z坐标
                var posViewMouse = CameraModel.Instance.MainCameraScreenToWorldPoint(posScreen);

                //光标移动仅影响摆放的高度
                var posViewCur = GuildGridModel.Instance.GetGridCoordToViewPos(m_OperateGridCoordCur + new GridCoord(0, 0, 1));
                int gridCoordZDis = (int)((posViewCur.z - posViewMouse.z) / GuildGridModel.Instance.CellUnitSizeZ);
                var gridCoordNew = m_OperateGridCoordCur;
                gridCoordNew.Z -= gridCoordZDis;
                SetOperateGridCoordCur(gridCoordNew);
                break;
        }
    }

    /// <summary>
    /// 设置 当前正在操作的网格坐标
    /// 。操作点有网格项目时 会自动刷新三维网格UI提示
    /// </summary>
    private void SetOperateGridCoordCur(GridCoord gridCoordNew)
    {
        if (m_OperateGridCoordCur.Equals(gridCoordNew) || gridCoordNew.Z < 0) return;

        m_OperateGridCoordCur = gridCoordNew;
        m_PanelGuildCur.SetOperateGridCoord(m_OperateGridCoordCur);
        SetTxtOperateGridCoordZ(m_OperateGridCoordCur.Z); //文本显示 当前高度
    }

    private void SetTxtOperateGridCoordZ(int z)
    {
        //UI显示上 高度每2个单元格 为高度1
        //使玩家操作时 单元格尺寸ZXY为1的时候 长宽高相等
        m_TxtOperateGridCoordZ.text = (z * 0.5f).ToString();
    }

    //按钮点击 鼠标左键
    private void OnInputButtonClickLeft()
    {
        switch (m_OperateStepCur)
        {
            case 0: //操作点无物体
                m_PanelGuildCur.GetGridCoordToHandlerObject();
                RefreshOperateStepCur(true);
                break;
            case 1: //选择物体XY坐标
                SetOperateStepCur(2);
                break;
            case 2: //选择物体Z坐标
                m_PanelGuildCur.SetHandlerObjectToGridCoord();
                RefreshOperateStepCur();
                break;
        }

        //刷新 操作提示 移动
        RefreshOperateMoveTip();
    }

    //按钮点击 鼠标右键
    private void OnInputButtonClickRight()
    {
        switch (m_OperateStepCur)
        {
            case 0: //操作点无物体
                break;
            case 1: //选择物体XY坐标
                SetOperateStepCur(0); //回到上一步操作模式
                m_PanelGuildCur.ReturenHandlerObject(); //返还 操作点的物体
                break;
            case 2: //选择物体Z坐标
                SetOperateStepCur(1); //回到上一步操作模式
                //更新 当前网格坐标
                //编辑高度模式回退 网格坐标高度需要重设为OperateGridCoordZ
                SetOperateGridCoordCur(GuildGridModel.Instance.ScreenPointToGridCoord(Input.mousePosition, -m_HandlerOperatePointPosAmend, GuildGridModel.Instance.OperateGridCoordZ));
                break;
        }

        //刷新 操作提示 移动
        RefreshOperateMoveTip();
    }

    // 按钮点击 鼠标中键
    private void OnInputButtonClickMiddle()
    {
        m_PanelGuildCur.RotateHandlerObject();
    }

    //设置 当前操作点的操作步骤
    private void SetOperateStepCur(int stepNew)
    {
        m_OperateStepCur = stepNew;
    }

    //刷新 当前操作点的操作步骤
    private void RefreshOperateStepCur(bool isReset = false)
    {
        int value = isReset ? 1 : m_OperateStepCur;
        if (value == 0)
            value = 1;

        //检查 操作点是否有物体 设置当前操作步骤
        m_OperateStepCur = m_PanelGuildCur.HandlerHadObject ? value : 0;
    }

    //刷新 操作提示 移动
    private void RefreshOperateMoveTip()
    {
        m_GoSpOperateTipMoveXY.SetActive(m_OperateStepCur == 1);
        m_GoSpOperateTipMoveZ.SetActive(m_OperateStepCur == 2);
    }
    #endregion

    #region 三维网格UI提示
    private Color m_ColorGridCellDefault = new Color(1, 1, 1, 0.2f); //默认色 白色
    private Color m_ColorGridCellHadItem = new Color(1, 1, 1, 0.8f); //默认色 白色
    private Color m_ColorGridCellCanSet = new Color(0.4f, 1, 0.4f, 0.8f); //可设置 绿色
    private Color m_ColorGridCellUnSet = new Color(1, 0.2f, 0.2f, 0.8f); //不可设置 红色

    /// <summary>
    /// 三维网格UI提示 状态
    /// </summary>
    private enum EGridCellUIState
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,
        /// <summary>
        /// 有网格物体
        /// </summary>
        HadItem,
        /// <summary>
        /// 可设置
        /// </summary>
        CanSet,
        /// <summary>
        /// 不可设置
        /// </summary>
        UnSet,
    }

    /// <summary>
    /// 设置 三维网格UI状态
    /// </summary>
    /// <param name="state"></param>
    private void SetGridCellUIState(EGridCellUIState state)
    {
        //根据状态 设置颜色
        var color = Color.gray;
        switch (state)
        {
            case EGridCellUIState.Default:
                color = m_ColorGridCellDefault;
                break;
            case EGridCellUIState.HadItem:
                color = m_ColorGridCellHadItem;
                break;
            case EGridCellUIState.CanSet:
                color = m_ColorGridCellCanSet;
                break;
            case EGridCellUIState.UnSet:
                color = m_ColorGridCellUnSet;
                break;
        }

        m_SpRenderFrameGridCellDown.color = color;
        m_SpRenderFrameGridCellUp.color = color;
        m_SpRenderFrameGridCellBack.color = color;
        m_SpRenderFrameGridCellFront.color = color;
    }

    /// <summary>
    /// 检查 刷新 网格提示UI
    /// </summary>
    /// <param name="gridCoord"></param>
    /// <param name="gridItemSize"></param>
    /// <param name="checkSet"></param>
    /// <param name="layer"></param>
    /// <param name="direction"></param>
    /// <param name="setType"></param>
    /// <returns>是否 可放置</returns>
    private bool CheckRefreshGirdCellUI(GridCoord gridCoord, GridCoord gridItemSize,
        bool checkSet = false, GuildGridModel.EGridLayer layer = GuildGridModel.EGridLayer.Building, EDirection direction = EDirection.None,
        GuildGridModel.EGridItemSetType setType = GuildGridModel.EGridItemSetType.None)
    {
        //根据家具单元格尺寸 修正本地坐标 对齐左下角原点
        var posSizeAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(gridItemSize);
        var posGridCellUI = GuildGridModel.Instance.GetWorldPosition(gridCoord);
        posGridCellUI += posSizeAmend;
        //网格UI位置
        m_SpRenderFrameGridCellDown.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y - posSizeAmend.y, posGridCellUI.z); //渲染于家具后方
        m_SpRenderFrameGridCellUp.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y + posSizeAmend.y, posGridCellUI.z); //渲染于家具前方
        m_SpRenderFrameGridCellBack.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y, posGridCellUI.z + posSizeAmend.z); //背板底部 对齐 底板顶部
        m_SpRenderFrameGridCellFront.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y, posGridCellUI.z - posSizeAmend.z); //前板底部 对齐 底板底部
        //网格UI尺寸
        var gridItemSizeWorldScale = GuildGridModel.Instance.GetGridItemSizeToWorldScale(gridItemSize);
        m_SpRenderFrameGridCellDown.size = gridItemSizeWorldScale;
        m_SpRenderFrameGridCellUp.size = gridItemSizeWorldScale;
        m_SpRenderFrameGridCellBack.size = new Vector2(gridItemSizeWorldScale.x, gridItemSizeWorldScale.z);
        m_SpRenderFrameGridCellFront.size = new Vector2(gridItemSizeWorldScale.x, gridItemSizeWorldScale.z);

        //网格颜色
        bool canSet = false; //是否 可设置
        if (checkSet)
        {
            //检查当前单元格 是否被阻挡
            var isObstruct = GuildGridModel.Instance.CheckGridItemSizeIsObstruct(layer, gridCoord, gridItemSize);

            //检查 放置类型 满足
            bool setTypeIsMeet = GuildGridModel.Instance.CheckGridItemSizeSetTypeIsMeet(layer, gridCoord, gridItemSize, direction, setType);

            //设置 单元格UI颜色
            if (isObstruct || !setTypeIsMeet)
            {
                //不可放置 红色
                SetGridCellUIState(EGridCellUIState.UnSet);
            }
            else
            {
                //可放置 绿色
                SetGridCellUIState(EGridCellUIState.CanSet);
                canSet = true;
            }

            //检查 操作点是否有物体 设置当前操作步骤
            if (m_OperateStepCur == 0)
            {
                m_OperateStepCur = m_PanelGuildCur.HandlerHadObject ? 1 : 0;
                //刷新 操作提示 移动
                RefreshOperateMoveTip();
            }

            //刷新 高度网格提示
            RefreshGirdCellUIHeight(gridCoord, gridItemSize);
        }
        else
        {
            //隐藏高度网格提示
            m_GoFrameGridCellHeight.SetActive(false);

            //无物体 白色
            SetGridCellUIState(EGridCellUIState.Default);

            var posOri = m_RootFrameGridCell.position;
            m_RootFrameGridCell.position = new Vector3(posOri.x, 1f, posOri.z);
        }

        return canSet;
    }

    //刷新 网格提示UI 高度
    private void RefreshGirdCellUIHeight(GridCoord gridCoord, GridCoord gridItemSize)
    {
        //高度为0时 不显示高度网格提示
        if (gridCoord.Z == 0)
        {
            m_GoFrameGridCellHeight.SetActive(false);
            return;
        }
        else
            m_GoFrameGridCellHeight.SetActive(true);

        //根据家具单元格尺寸 修正本地坐标 对齐左下角原点
        gridItemSize.Z = gridCoord.Z; //显示网格尺寸高度 为坐标高度
        gridCoord.Z = 0; //网格坐标为0 显示0高度位置
        var posSizeAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(gridItemSize);
        var posGridCellUI = GuildGridModel.Instance.GetWorldPosition(gridCoord);
        posGridCellUI += posSizeAmend;
        m_SpRenderFrameGridCellHeightDown.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y - posSizeAmend.y, posGridCellUI.z); //渲染于家具后方
        m_SpRenderFrameGridCellHeightBack.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y, posGridCellUI.z + posSizeAmend.z); //背板底部 对齐 底板顶部
        m_SpRenderFrameGridCellHeightFront.transform.position = new Vector3(posGridCellUI.x, posGridCellUI.y, posGridCellUI.z - posSizeAmend.z); //前板底部 对齐 底板底部
        //网格尺寸
        var gridItemSizeWorldScale = GuildGridModel.Instance.GetGridItemSizeToWorldScale(gridItemSize);
        m_SpRenderFrameGridCellHeightDown.size = gridItemSizeWorldScale;
        m_SpRenderFrameGridCellHeightBack.size = new Vector2(gridItemSizeWorldScale.x, gridItemSizeWorldScale.z);
        m_SpRenderFrameGridCellHeightFront.size = new Vector2(gridItemSizeWorldScale.x, gridItemSizeWorldScale.z);
    }

    /// <summary>
    /// 检查 刷新 网格提示UI
    /// 使用操作面板的网格物体参数
    /// </summary>
    private void CheckRefreshGirdCellUIAsPanelGuild()
    {
        //操作的网格层
        GuildGridModel.EGridLayer gridLayer = GuildGridModel.EGridLayer.Furniture;
        switch (m_EPanelTypeCur)
        {
            case EPanelType.Build:
                gridLayer = GuildGridModel.EGridLayer.Building;
                break;
            case EPanelType.Furniture:
                gridLayer = GuildGridModel.EGridLayer.Furniture;
                break;
        }

        var checkSet = m_PanelGuildCur.ItemOperateBase.ValueId != 0; //是否检查放置区域
        var gridItemSize = m_PanelGuildCur.ItemOperateBase.GetGridItemSize; //网格尺寸
        var direction = m_PanelGuildCur.ItemOperateBase.Direction; //朝向
        var setType = m_PanelGuildCur.ItemOperateBase.SetType; //放置类型
        //无家具时 立即更新光标所在网格位置
        if (checkSet == false)
        {
            var posAmend = -m_HandlerOperatePointPosAmend;
            SetOperateGridCoordCur(GuildGridModel.Instance.ScreenPointToGridCoord(Input.mousePosition, posAmend, GuildGridModel.Instance.OperateGridCoordZ));
        }

        //刷新 网格UI
        m_PanelGuildCur.CanSet = CheckRefreshGirdCellUI(m_OperateGridCoordCur, gridItemSize, checkSet, gridLayer, direction, setType);
    }
    #endregion
}
