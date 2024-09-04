using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using FInteractionSystem;
using FsGameFramework.InputSystem;
using FsStateSystem;
using TMPro;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using Deploy;
using com.ootii.Messages;
using System;
using FsGridCellSystem;

public partial class CharacterMaster : CharacterBase
{
#if UNITY_EDITOR
    private bool m_Test_flipFlop;
#endif

    public override bool Init(System.Object outer)
    {
        bool succeed = base.Init(outer);

        //设置 玩家的冒险者信息
        SetVenturerInfo(PlayerModel.Instance.GetPlayerVenturerInfo());

        //设置 玩家的位置
        var posCur = PlayerModel.Instance.GetPlayerPositionCur();
        if (posCur != Vector3.zero)
        {
            TransformGet.position = posCur;
        }

        return succeed;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        MessageDispatcher.AddListener(PlayerModelMsgType.PLAYER_INFO_CHANGE, MsgPlayerInfoChange);
        MessageDispatcher.AddListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_CHARACTER_MOVE_CHANGE, MsgOperateCharacterChange);
        MessageDispatcher.AddListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, MsgOperateMouseSceneChange);
        MessageDispatcher.AddListener(WindowSystemMsgType.CURSORWINDOW_FURNITURE_GRIDCOORD_CHANGE, MsgCursorFurnitureGridCoordChange);
    }

    protected override void OnDestroyThis()
    {
        base.OnDestroyThis();

        MessageDispatcher.RemoveListener(PlayerModelMsgType.PLAYER_INFO_CHANGE, MsgPlayerInfoChange);
        MessageDispatcher.RemoveListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_CHARACTER_MOVE_CHANGE, MsgOperateCharacterChange);
        MessageDispatcher.RemoveListener(WindowSystemMsgType.WINDOWSYSTEM_OPERATE_MOUSE_SCENE_CHANGE, MsgOperateMouseSceneChange);
        MessageDispatcher.RemoveListener(WindowSystemMsgType.CURSORWINDOW_FURNITURE_GRIDCOORD_CHANGE, MsgCursorFurnitureGridCoordChange);
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        //设置相机方向
        var targetPos = TransformGet.position;
        targetPos.z += 0.3f; //上移至头部
        CameraModel.Instance.SetCameraMainPos(targetPos);

#if UNITY_EDITOR
        //测试关卡切换用
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!m_Test_flipFlop)
            {
                m_Test_flipFlop = !m_Test_flipFlop;
                FWorldContainer.SwitchLevel(
                    FWorldContainer.CurrentWorld.CurrentLevel.LevelTerrainConfig.passageways[1].targetLevel,
                    FWorldContainer.CurrentWorld.CurrentLevel.LevelTerrainConfig.passageways[1].targetLevelPassagewayIndex);
            }
            else
            {
                m_Test_flipFlop = !m_Test_flipFlop;
                FWorldContainer.SwitchLevel(
                    FWorldContainer.CurrentWorld.CurrentLevel.LevelTerrainConfig.passageways[0].targetLevel,
                    FWorldContainer.CurrentWorld.CurrentLevel.LevelTerrainConfig.passageways[0].targetLevelPassagewayIndex);
            }
        }

        //打开
        if (Input.GetKeyDown(KeyCode.M))
        {
            WindowSystem.Instance.OpenOrClose(WindowEnum.EntrustWindow);
        }
#endif
    }

    public override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);

    }

    //消息 玩家信息变化
    private void MsgPlayerInfoChange(IMessage rMessage)
    {
        SetVenturerInfo(PlayerModel.Instance.GetPlayerVenturerInfo());
    }

    #region 网格系统

    //更新 网格坐标
    protected override void RefreshGridItemGridCoord()
    {
        base.RefreshGridItemGridCoord();
        var gridCoordFloat = m_GridItemComponent.MainGridCoord + m_GridItemComponent.GridItemSize * 0.5f;
        GuildGridModel.Instance.RefreshPlayerInAreaInfo(gridCoordFloat);
    }

    #endregion

    #region 输入操作

    /// <summary>
    /// 开关 鼠标场景交互
    /// </summary>
    public bool EnableMouseScene;


    private GridCoord m_CursorFurnitureGridCoordCur; //光标 网格坐标 家具层 当前
    private GridCoord m_CursorClickFurnitureRange = new GridCoord(2, 2, 4); //光标 点击操作 家具层 限制距离

    public override void OnInputMainBtn(InputEventType inputEventType)
    {
        base.OnInputMainBtn(inputEventType);

        if (EnableInputAction == false || EnableMouseScene == false) { return; }

        switch (inputEventType)
        {
            case InputEventType.Click:
                ExecuteOperateObjectCur();
                break;
        }
    }

    public override void OnInputMouseLeftBtn(InputEventType inputEventType)
    {
        base.OnInputMouseLeftBtn(inputEventType);

        if (EnableMouseScene == false) { return; }

        switch (inputEventType)
        {
            case InputEventType.Click:
                InputMainClickFurniture();
                break;
        }
    }

    //输入 主按钮点击 操作家具
    private void InputMainClickFurniture()
    {
        var furnitureBase = WindowSystem.Instance.MainGameWindow.GetFurnitureBase(GuildGridModel.EGridLayer.Furniture, m_CursorFurnitureGridCoordCur);
        if (furnitureBase != null)
        {
            var gridCoordDis = m_GridItemComponent.MainGridCoord - m_CursorFurnitureGridCoordCur;
            if (Mathf.Abs(gridCoordDis.X) > m_CursorClickFurnitureRange.X || Mathf.Abs(gridCoordDis.Y) > m_CursorClickFurnitureRange.Y || Mathf.Abs(gridCoordDis.Z) > m_CursorClickFurnitureRange.Z)
                WindowSystem.Instance.ShowMsg("距离过远！无法操作！");
            else
                furnitureBase.ExecuteOperate();
        }
    }

    //消息 玩家角色操作 改变
    private void MsgOperateCharacterChange(IMessage rMessage)
    {
        var isEnable = (bool)rMessage.Data;
        EnableInputAction = isEnable;
    }

    //消息 鼠标场景交互
    private void MsgOperateMouseSceneChange(IMessage rMessage)
    {
        var isEnable = (bool)rMessage.Data;
        EnableMouseScene = isEnable;
    }

    //消息 光标所在 家具层网格坐标 改变
    private void MsgCursorFurnitureGridCoordChange(IMessage rMessage)
    {
        m_CursorFurnitureGridCoordCur = (GridCoord)rMessage.Data;
    }

    #endregion

    #region 场景交互
    private FurnitureBaseActor m_FurnitureCanOperateCur; //可操作的家具 当前
    private CharacterBase m_CharacterCanOperateCur; //可操作的角色 当前
    //不同方向的单元格检查范围
    private Dictionary<CharacterOrientation, GridCoord[]> m_DicOrientatioinCheckCellCoordRange = new Dictionary<CharacterOrientation, GridCoord[]>()
    {
        {CharacterOrientation.Up, new GridCoord[3]{ new GridCoord(0, 1, 0), new GridCoord(-1, 1, 0), new GridCoord(1, 1, 0) }},
        {CharacterOrientation.Down, new GridCoord[3]{ new GridCoord(0, -1, 0), new GridCoord(1, -1, 0), new GridCoord(-1, -1, 0) }},
        {CharacterOrientation.Left, new GridCoord[3]{ new GridCoord(-1, 0, 0), new GridCoord(-1, -1, 0), new GridCoord(-1, 1, 0) }},
        {CharacterOrientation.Right, new GridCoord[3]{ new GridCoord(1, 0, 0), new GridCoord(1, 1, 0), new GridCoord(1, -1, 0) }},
        {CharacterOrientation.UpLeft, new GridCoord[3]{ new GridCoord(-1, 1, 0), new GridCoord(0, 1, 0), new GridCoord(-1, 0, 0) }},
        {CharacterOrientation.UpRight, new GridCoord[3]{ new GridCoord(1, 1, 0), new GridCoord(0, 1, 0), new GridCoord(1, 0, 0) }},
        {CharacterOrientation.DownLeft, new GridCoord[3]{ new GridCoord(-1, -1, 0), new GridCoord(0, -1, 0), new GridCoord(-1, 0, 0) }},
        {CharacterOrientation.DownRight, new GridCoord[3]{ new GridCoord(1, -1, 0), new GridCoord(0, -1, 0), new GridCoord(1, 0, 0) }},
    };

    protected override void OnGridCoordOrOrientationChanged(GridCoord gridCoordCur, CharacterOrientation orientationCur)
    {
        base.OnGridCoordOrOrientationChanged(gridCoordCur, orientationCur);

        //更新 公会建筑系统 当前操作的网格坐标Z轴
        GuildGridModel.Instance.OperateGridCoordZ = gridCoordCur.Z;

        var checkCellCoordRange = m_DicOrientatioinCheckCellCoordRange[orientationCur]; //当前朝向的 检查单元格范围

        //检查当前网格范围
        FurnitureBaseActor furnitureNew = null;
        CharacterBase characterNew = null;
        //检查并记录家具
        for (int i = 0; i < checkCellCoordRange.Length; i++)
        {
            var cellcoordOffset = checkCellCoordRange[i]; //坐标位移

            
            var gridItenFurniture = GuildGridModel.Instance.GetMainGridItem(GuildGridModel.EGridLayer.Furniture, gridCoordCur + cellcoordOffset);
            if (gridItenFurniture != null && gridItenFurniture.Value != 0)
            {
                var furnitureBase = WindowSystem.Instance.MainGameWindow.GetFurnitureBase(GuildGridModel.EGridLayer.Furniture, gridItenFurniture.MainGridCoord);
                if (furnitureBase != null)
                {
                    //记录 检查的第一个家具
                    furnitureNew = furnitureBase;
                    break;
                } 
            }
        }
        //检查并记录角色
        for (int i = 0; i < checkCellCoordRange.Length; i++)
        {
            var cellcoordOffset = checkCellCoordRange[i]; //坐标位移

            var gridItemCharacter = GuildGridModel.Instance.GetMainGridItem(GuildGridModel.EGridLayer.Character, gridCoordCur + cellcoordOffset);
            if (gridItemCharacter != null && gridItemCharacter.Value != 0)
            {
                var characterBase = WindowSystem.Instance.MainGameWindow.GetVenturerCharacterBase(gridItemCharacter.Value);
                if (characterBase != null)
                {
                    //记录 检查的第一个角色
                    characterNew = characterBase;
                    break;
                }
            }
        }

        //更新 可操作的家具
        if (m_FurnitureCanOperateCur != furnitureNew)
        {
            //旧家具 隐藏操作提示
            if (m_FurnitureCanOperateCur != null)
            {
                m_FurnitureCanOperateCur.ShowOperateTip(false);
            }

            m_FurnitureCanOperateCur = furnitureNew;

            //新家具 显示操作提示
            if (m_FurnitureCanOperateCur != null)
            {
                m_FurnitureCanOperateCur.ShowOperateTip(true);
            }
        }

        //更新 可操作的角色
        if (m_CharacterCanOperateCur != characterNew)
        {
            //旧家具 隐藏操作提示
            if (m_CharacterCanOperateCur != null)
            {
                m_CharacterCanOperateCur.ShowOperateTip(false);
            }

            m_CharacterCanOperateCur = characterNew;

            //新家具 显示操作提示
            if (m_CharacterCanOperateCur != null)
            {
                m_CharacterCanOperateCur.ShowOperateTip(true);
            }
        }
    }

    //操作 当前对象
    private void ExecuteOperateObjectCur()
    {
        //优先操作家具
        if (m_FurnitureCanOperateCur != null)
        {
            m_FurnitureCanOperateCur.ExecuteOperate();
            return;
        }

        if (m_CharacterCanOperateCur != null)
        {
            m_CharacterCanOperateCur.ExcuteOperate();
            return;
        }
    }

    #endregion
}
