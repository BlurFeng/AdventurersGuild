using com.ootii.Messages;
using Deploy;
using FsGameFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FsGridCellSystem;

public class MainGameWindow : WindowBase
{
    [SerializeField] private GameObject m_BtnScene = null; //按钮 场景
    [SerializeField] private GameObject m_BtnSkip = null; //按钮 跳过回合
    [SerializeField] private GameObject m_BtnRoleEditor = null; //按钮 角色编辑

    [Header("操作")]
    [SerializeField] private GameObject m_BtnBuild = null; //按钮 公会建造
    [SerializeField] private GameObject m_BtnNote = null; //按钮 笔记本
    [SerializeField] private GameObject m_BtnVenturer = null; //按钮 冒险者
    [SerializeField] private GameObject m_BtnMenu = null; //按钮 Esc菜单

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnScene).SetClickHandler(BtnScene);
        ClickListener.Get(m_BtnBuild).SetClickHandler(BtnBuild);
        ClickListener.Get(m_BtnNote).SetClickHandler(BtnNote);
        ClickListener.Get(m_BtnVenturer).SetClickHandler(BtnVenturer);
        ClickListener.Get(m_BtnMenu).SetClickHandler(BtnMenu);

        ClickListener.Get(m_BtnSkip).SetClickHandler((evt) => { TimeModel.Instance.ExecuteFinishCurDay(); });
        ClickListener.Get(m_BtnRoleEditor).SetClickHandler((evt) => 
        {
            //冒险者皮肤编辑面板
            var arg = new RoleEditorWindow.RoleEditorWindowArg();
            arg.VenturerInfo = PlayerModel.Instance.PlayerInfo.VenturerInfo;
            arg.OnConfirmCallBack = (info) =>
            {
                PlayerModel.Instance.SetPlayerVenturerInfo(info);
            };
            WindowSystem.Instance.OpenWindow(WindowEnum.RoleEditorWindow, arg);
        });

        //回合模块
        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, MsgDayCountChange);
        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_SEASON_CHANGE, MsgSeasonChange);
        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_ERAYEAR_CHANGE, MsgEraYearCountChange);
        //资产
        MessageDispatcher.AddListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_PROPERTY, MsgRefreshProperty);
        //冒险者模块
        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_INFO_STATE_CHANGE, MsgRefreshItemHeroInfo);
        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_ADD, MsgSpawnVenturerInfo);
        
        //初始化
        InitGameTime(); //游戏时间
        InitGuildInfo(); //公会信息
        InitGuildGrid(); //公会建造
        InitGuildGridArea(); //公会区域
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //切换世界
        FWorldContainer.SwitchWorld(ConfigSystem.Instance.UWorldConfigContainer.Get("MainGame_World"));

        //刷新 项目英雄信息
        RefreshItemHeroInfo();
        //打开 游戏时间
        OpenGameTime();
        //资产
        RefreshPropertyCoin();

        //气象系统 开启
        WeatherModel.Instance.SetGlobalEnableState(true);

        //临时代码 一些模块需要在游戏开始时主动调用实例化
        //因为一些数据在回合进行时需要更新
        WorldMap.WorldMapModel.Instance.GetType();

        //加载界面 淡出
        AsyncLoadWindow.FadeOut();
    }

    public override void OnRelease()
    {
        base.OnRelease();

        //回合模块
        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, MsgDayCountChange);
        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_SEASON_CHANGE, MsgSeasonChange);
        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_ERAYEAR_CHANGE, MsgEraYearCountChange);
        //资产
        MessageDispatcher.RemoveListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_PROPERTY, MsgRefreshProperty);
        //冒险者模块
        MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_INFO_STATE_CHANGE, MsgRefreshItemHeroInfo);
        MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_ADD, MsgSpawnVenturerInfo);

        //释放
        ReleaseGuildInfo(); //公会信息
        ReleaseGuildGrid(); //公会建造
        ReleaseGuildGridArea(); //公会区域
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //关闭 最上层的最后一个界面
            if (!WindowSystem.Instance.CloseLastLayerWindow())
            {
                //当前最上层界面为MainGameWindow 不关闭
                //打开 Esc菜单
                BtnMenu(null);
            }
        }
    }

    #region 按钮
    //按钮 点击场景
    private void BtnScene(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:

                break;
            case PointerEventData.InputButton.Right:

                break;
            case PointerEventData.InputButton.Middle:

                break;
        }
    }

    //按钮 Esc菜单
    private void BtnMenu(PointerEventData eventData)
    {
        WindowSystem.Instance.OpenWindow(WindowEnum.EscMenuWindow);
    }

    //按钮 建造
    private void BtnBuild(PointerEventData eventData)
    {
        WindowSystem.Instance.OpenWindow(WindowEnum.GuildBuildWindow);
    }

    //按钮 笔记本
    private void BtnNote(PointerEventData eventData)
    {
        WindowSystem.Instance.OpenWindow(WindowEnum.NotebookWindow);
    }

    //按钮 冒险者
    private void BtnVenturer(PointerEventData eventData)
    {
        //打开 冒险者列表
        WindowSystem.Instance.OpenWindow(WindowEnum.VenturerListWindow);
    }
    #endregion

    #region 资产信息UI
    [Header("资产")]
    [SerializeField] private ItemPropCoin m_ItemPropCoin = null; //项目 钱币

    //刷新 资产 货币
    private void RefreshPropertyCoin()
    {
        int coinCount = PlayerModel.Instance.GetPropCount(10001);
        m_ItemPropCoin.SetCoinCount(coinCount);
    }

    //消息 刷新道具数量
    private void MsgRefreshProperty(IMessage msg)
    {
        var propInfo = msg.Data as PropInfo;
        if (propInfo == null) { return; }

        if (propInfo.Id == 10001)
            RefreshPropertyCoin();
    }
    #endregion

    #region 游戏时间UI
    [Header("游戏时间")]
    [SerializeField] private Image m_ImgAnimClock = null; //材质球 帧动画 钟点数
    [SerializeField] private Image m_ImgAnimScene = null; //材质球 帧动画 时间场景
    [SerializeField] private Transform m_TransRootTimeClock = null; //根节点 钟点数字
    [SerializeField] private List<TextMeshProUGUI> m_ListTxtTimeClockNum = null; //文本 钟点数字
    [SerializeField] private TextMeshProUGUI m_TxtTimeEraYearCount = null; //文本 纪元年 数量
    [SerializeField] private TextMeshProUGUI m_TxtTimeMonthCount = null; //文本 月份
    [SerializeField] private TextMeshProUGUI m_TxtTimeDayCount = null; //文本 回合 数量

    private Coroutine m_CorAnimTimeClock;
    private Material m_MatImgAnimClock; //材质球 时钟
    private Material m_MatImgAnimScene; //材质球 场景

    //初始化 游戏时间
    private void InitGameTime()
    {
        m_MatImgAnimClock = m_ImgAnimClock.material;
        m_MatImgAnimClock.hideFlags = HideFlags.DontSaveInEditor;
        m_MatImgAnimScene = m_ImgAnimScene.material;
        m_MatImgAnimScene.hideFlags = HideFlags.DontSaveInEditor;

        //设置帧动画播放速度
        m_MatImgAnimClock.SetFloat("_SecondFrame", TimeModel.Instance.GameTimeScale / 200f); //帧动画 钟点数背景旋转 播放速度
        m_MatImgAnimScene.SetFloat("_SecondFrame", TimeModel.Instance.GameTimeScale / 960f); //帧动画 场景 播放速度
    }

    //打开 游戏时间
    private void OpenGameTime()
    {
        //时间动画 钟点 场景
        RestartAnimTimeClock();

        //设置 日期
        RefreshTimeDay();
        RefreshTimeMonth();
        RefreshTimeEraYear();
    }

    //重设 动画时间钟点
    private void RestartAnimTimeClock()
    {
        //帧动画 场景 开始秒数
        m_MatImgAnimScene.SetFloat("_TimeStart", Time.timeSinceLevelLoad - TimeModel.Instance.TimeInfoCur.RealTimeSeconds); 

        if (m_CorAnimTimeClock != null)
        {
            StopCoroutine(m_CorAnimTimeClock);
            m_CorAnimTimeClock = null;
        }

        m_CorAnimTimeClock = StartCoroutine(CorAnimTimeClock());
    }

    //设置 时间钟点数
    private void SetTimeClockNumRotate(float gameSecondsCur)
    {
        //旋转角度
        m_TransRootTimeClock.rotation = Quaternion.Euler(0f, 0f, (gameSecondsCur % 3600f) / 3600f * 60f);

        //文本钟点数字
        int hoursCur = (int)(gameSecondsCur / 3600f);
        m_ListTxtTimeClockNum[0].text = GetTimeClockNum(hoursCur - 1).ToString();
        m_ListTxtTimeClockNum[1].text = GetTimeClockNum(hoursCur).ToString();
        m_ListTxtTimeClockNum[2].text = GetTimeClockNum(hoursCur + 1).ToString();
        m_ListTxtTimeClockNum[3].text = GetTimeClockNum(hoursCur + 2).ToString();
    }

    //获取 钟点数字
    private int GetTimeClockNum(int hoursNum)
    {
        if (hoursNum == 0)
            hoursNum = 24;
        else if (hoursNum < 0)
            hoursNum = 24 - hoursNum;
        else if (hoursNum > 24)
            hoursNum -= 24;

        return hoursNum;
    }

    //协程 动画 时间钟点
    private IEnumerator CorAnimTimeClock()
    {
        while (true)
        {
            SetTimeClockNumRotate(TimeModel.Instance.TimeInfoCur.RealTimeSecondsFloat * TimeModel.Instance.GameTimeScale);
            yield return null;
        }
    }

    //刷新 时间 天数
    private void RefreshTimeDay()
    {
        m_TxtTimeDayCount.text = TimeModel.Instance.TimeInfoCur.GameTimeDay.ToString();

        RestartAnimTimeClock();
    }

    //刷新 时间 月份
    private void RefreshTimeMonth()
    {
        m_TxtTimeMonthCount.text = TimeModel.Instance.TimeInfoCur.GameTimeMonth.ToString();
    }

    //刷新 时间 纪元年
    private void RefreshTimeEraYear()
    {
        int eraYearCount = TimeModel.Instance.TimeInfoCur.GameTimeEraYear;
        m_TxtTimeEraYearCount.text = eraYearCount.ToString();
    }

    //消息 回合数量 改变
    private void MsgDayCountChange(IMessage rMessage)
    {
        RefreshTimeDay();
    }

    //消息 季节 改变
    private void MsgSeasonChange(IMessage rMessage)
    {
        RefreshTimeMonth();
    }

    //消息 纪元年 改变
    private void MsgEraYearCountChange(IMessage rMessage)
    {
        RefreshTimeEraYear();
    }
    #endregion

    #region 公会信息UI
    [Header("公会信息")]
    [SerializeField] private TextMeshProUGUI m_TxtGuildRank = null; //文本 公会 阶级
    [SerializeField] private TextMeshProUGUI m_TxtGuildBuildingScaleValue = null; //文本 公会 规模值
    [SerializeField] private TextMeshProUGUI m_TxtGuildFacilityValue = null; //文本 公会 设施值
    [SerializeField] private TextMeshProUGUI m_TxtGuildPrestigeValue = null; //文本 公会 声望值

    //初始化 公会信息
    private void InitGuildInfo()
    {
        //公会信息
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE, MsgGuildInfoChange);

        RefreshGuildInfo();
    }

    //释放 公会建造
    private void ReleaseGuildInfo()
    {
        //公会信息
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDMODEL_GUILDINFO_CHANGE, MsgGuildInfoChange);
    }

    //刷新 公会信息
    private void RefreshGuildInfo()
    {
        //公会阶级
        var cfgGuildRank = ConfigSystem.Instance.GetConfig<Guild_Rank>(GuildModel.Instance.GuildRank);
        m_TxtGuildRank.text = cfgGuildRank.Name;
        //公会规模值
        m_TxtGuildBuildingScaleValue.text = GuildModel.Instance.BuildingScaleValue.ToString();
        //公会设施值
        m_TxtGuildFacilityValue.text = GuildModel.Instance.FacilityValue.ToString();
        //公会声望值
        m_TxtGuildPrestigeValue.text = GuildModel.Instance.PrestigeValue.ToString();
    }

    //消息 刷新公会信息
    private void MsgGuildInfoChange(IMessage msg)
    {
        RefreshGuildInfo();
    }
    #endregion

    #region 冒险者模块
    //字典 冒险者ID,CharacterBase
    private Dictionary<int, CharacterBase> m_DicVenturerCharacterBase = new Dictionary<int, CharacterBase>();

    //消息 冒险者 诞生
    private void MsgSpawnVenturerInfo(IMessage msg)
    {
        VenturerInfo venturerInfo = msg.Data as VenturerInfo;
        if (venturerInfo == null) { return; }
        //在场景中实例化冒险者预制体
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(venturerInfo.RaceClanId);
        var prefabAddr = AssetAddressUtil.GetPrefabCharacterAddress(cfgRaceClan.PrefabName);
        //AssetSystem.Instance.LoadPrefab(prefabAddr, (prefab) =>
        //{
        //    var instance = GameObject.Instantiate(prefab);
        //    instance.SetActive(true);

        //    //设置出生点
        //    instance.transform.position = GuildGridModel.Instance.GetWorldPosition(new GridCoord(10, 10, 0));
        //    //设置冒险者信息
        //    CharacterBase characterBase = instance.GetComponent<CharacterBase>();
        //    characterBase.Init();
        //    characterBase.SetVenturerInfo(venturerInfo);

        //    //记录
        //    m_DicVenturerCharacterBase.Add(venturerInfo.Id, characterBase);
        //});
    }

    /// <summary>
    /// 获取 冒险者的CharacterBase实例
    /// </summary>
    /// <param name="venturerId"></param>
    /// <returns></returns>
    public CharacterBase GetVenturerCharacterBase(int venturerId)
    {
        CharacterBase characterBase = null;
        m_DicVenturerCharacterBase.TryGetValue(venturerId, out characterBase);

        return characterBase;
    }

    //消息 冒险者 信息更新
    private void MsgRefreshItemHeroInfo(IMessage msg)
    {
        RefreshItemHeroInfo();
    }

    //刷新 项目英雄信息
    private void RefreshItemHeroInfo()
    {
       
    }
    #endregion

    #region 公会建造模块
    //字典 单元格坐标,建筑物体
    private Dictionary<GridCoord, BuildingBaseActor> m_DicGuildBuilding = new Dictionary<GridCoord, BuildingBaseActor>();

    //字典 单元格坐标,家具物体
    private Dictionary<GuildGridModel.EGridLayer, Dictionary<GridCoord, FurnitureBaseActor>> m_DicGuildFurniture = new Dictionary<GuildGridModel.EGridLayer, Dictionary<GridCoord, FurnitureBaseActor>>();

    /// <summary>
    /// 获取 家具组件
    /// </summary>
    /// <param name="cellcoord"></param>
    /// <returns></returns>
    public FurnitureBaseActor GetFurnitureBase(GuildGridModel.EGridLayer layer, GridCoord cellcoord)
    {
        //检查 网格位置是否有网格项目
        var gridItem = GuildGridModel.Instance.GetMainGridItem(layer, cellcoord);
        if (gridItem == null || gridItem.Value == 0) { return null; }

        FurnitureBaseActor furnitureBase = null;
        Dictionary<GridCoord, FurnitureBaseActor> furnitureMap = null;
        if (m_DicGuildFurniture.TryGetValue(layer, out furnitureMap))
            furnitureMap.TryGetValue(gridItem.MainGridCoord, out furnitureBase);

        return furnitureBase;
    }

    //初始化 公会
    private void InitGuildGrid()
    {
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_BUILDINGINFO_CHANGE, MsgGuildGridBuildingInfoChange); //建筑信息 改变
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_FURNITUREINFO_CHANGE, MsgGuildGridFurnitureInfoChange); //家具信息 改变

        //实例化 所有建筑
        foreach (var buildingInfo in GuildGridModel.Instance.DicBuildingInfo.Values)
            AddGuildBuilding(buildingInfo);

        //实例化 所有家具
        foreach (var furnitureInfo in GuildGridModel.Instance.DicFurnitureInfo.Values)
            AddGuildFurniture(furnitureInfo);
    }

    //释放 公会建造
    private void ReleaseGuildGrid()
    {
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_BUILDINGINFO_CHANGE, MsgGuildGridBuildingInfoChange);
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_FURNITUREINFO_CHANGE, MsgGuildGridFurnitureInfoChange);

        //返还缓存池
        foreach (var buildingBase in m_DicGuildBuilding.Values)
            AssetTemplateSystem.Instance.ReturnTemplatePrefab(buildingBase.gameObject);

        foreach (var furnitureDic in m_DicGuildFurniture.Values)
        {
            foreach (var furnitureBase in furnitureDic.Values)
                AssetTemplateSystem.Instance.ReturnTemplatePrefab(furnitureBase.gameObject);
        }
    }

    //消息 公会建造 建筑信息 改变
    private void MsgGuildGridBuildingInfoChange(IMessage msg)
    {
        var msgInfo = (GuildGridModel.MsgBuildingInfoChange)msg.Data;
        if (msgInfo.IsRemove)
            RemoveGuildBuilding(msgInfo.BuildingInfo.MainGridCoord); //销毁已有的建筑
        else
        {
            RemoveGuildBuilding(msgInfo.BuildingInfo.MainGridCoord);
            AddGuildBuilding(msgInfo.BuildingInfo);
        }
    }

    //消息 公会建造 家具信息 改变
    private void MsgGuildGridFurnitureInfoChange(IMessage msg)
    {
        var msgInfo = (GuildGridModel.MsgFurnitureInfoChange)msg.Data;
        if (msgInfo.IsRemove)
            RemoveGuildFurniture(msgInfo.Layer, msgInfo.FurnitureInfo.MainGridCoord); //销毁已有的建筑
        else
        {
            RemoveGuildFurniture(msgInfo.Layer, msgInfo.FurnitureInfo.MainGridCoord);
            AddGuildFurniture(msgInfo.FurnitureInfo);
        }
    }

    //替换 家具
    private void AddGuildFurniture(GuildGridModel.FurnitureInfo furnitureInfo)
    {
        //先销毁已有的家具
        var gridCell = GuildGridModel.Instance.GetMainGridItem(furnitureInfo.Layer, furnitureInfo.MainGridCoord);
        var furnitureId = gridCell.Value;
        if (furnitureId == 0) { return; }

        //实例化游戏物体
        AssetTemplateSystem.Instance.CloneFurniturePrefab(gridCell.GridItemData, null, (furnitureBase) =>
        {
            //记录 家具预制体
            if (!m_DicGuildFurniture.ContainsKey(furnitureInfo.Layer))
                m_DicGuildFurniture.Add(furnitureInfo.Layer, new Dictionary<GridCoord, FurnitureBaseActor>());

            m_DicGuildFurniture[furnitureInfo.Layer].Add(furnitureInfo.MainGridCoord, furnitureBase);
            furnitureBase.GridItemComponent.RefreshViewObjPosition(); //立即刷新 渲染物体位置
            furnitureBase.EnableAutoRecordAreaInfo = true;
            //碰撞根节点 激活
            furnitureBase.GridItemComponent.SetColliderRootActive(true);
            //记录 实例化家具物体的网格项目数据
            furnitureInfo.GridItemComponent = furnitureBase.GridItemComponent;
        });
    }

    //移除 家具
    private void RemoveGuildFurniture(GuildGridModel.EGridLayer layer, GridCoord cellcoord)
    {
        FurnitureBaseActor furnitureBase = null;
        Dictionary<GridCoord, FurnitureBaseActor> furnitureMap = null;
        if (m_DicGuildFurniture.TryGetValue(layer, out furnitureMap))
        {
            furnitureMap.TryGetValue(cellcoord, out furnitureBase);
            if (furnitureBase != null)
            {
                AssetTemplateSystem.Instance.ReturnTemplatePrefab(furnitureBase.gameObject);
                furnitureMap.Remove(cellcoord);
            }
        }
    }

    //添加 建筑
    private void AddGuildBuilding(GuildGridModel.BuildingInfo buildingInfo)
    {
        var gridItem = GuildGridModel.Instance.GetMainGridItem(GuildGridModel.EGridLayer.Building, buildingInfo.MainGridCoord);
        if (gridItem == null || gridItem.Value == 0) { return; }

        //实例化游戏物体
        AssetTemplateSystem.Instance.CloneBuildingPrefab(gridItem.GridItemData, null, (buildingBase) =>
        {
            //记录 家具预制体
            m_DicGuildBuilding.Add(buildingInfo.MainGridCoord, buildingBase);
            //立即刷新 渲染物体位置
            buildingBase.GridItemComponent.RefreshViewObjPosition();
            //设置 建筑的区域信息
            buildingBase.SetGridCellSystemAreaInfo(buildingInfo.Id);
        });
    }

    //移除 建筑
    private void RemoveGuildBuilding(GridCoord cellcoord)
    {
        BuildingBaseActor buildingBase = null;
        m_DicGuildBuilding.TryGetValue(cellcoord, out buildingBase);
        if (buildingBase != null)
        {
            AssetTemplateSystem.Instance.ReturnTemplatePrefab(buildingBase.gameObject);
            m_DicGuildBuilding.Remove(cellcoord);
        }
    }
    #endregion

    #region 公会建造-区域
    [Header("公会建造-区域")]
    [SerializeField] private GameObject m_BtnAreaInfo = null; //按钮 区域信息
    [SerializeField] private TextMeshProUGUI m_TxtAreaItemName = null; //文本 区域名称

    private string m_TxtAreaItemNameDefault = string.Empty; //区域名称 默认

    //初始化 公会区域
    private void InitGuildGridArea()
    {
        ClickListener.Get(m_BtnAreaInfo).SetClickHandler(BtnAreaInfo);

        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_CHANGE, MsgGuildGridAreaCurChange); //当前区域 改变
        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE, MsgGuildGridAreaCurValueChange); //当前区域 值 改变

        m_TxtAreaItemNameDefault = m_TxtAreaItemName.text;
    }

    //释放 公会区域
    private void ReleaseGuildGridArea()
    {
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_CHANGE, MsgGuildGridAreaCurChange);
        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_VALUE_CHANGE, MsgGuildGridAreaCurValueChange);
    }

    //按钮 打开区域功能界面
    private void BtnAreaInfo(PointerEventData obj)
    {
        if (GuildGridModel.Instance.PlayerAreaInfoCur != null)
            WindowSystem.Instance.OpenWindow(WindowEnum.BuildingAreaWindow, GuildGridModel.Instance.PlayerAreaInfoCur);
    }

    //消息 玩家当前所处区域 改变
    private void MsgGuildGridAreaCurChange(IMessage rMessage)
    {
        if (GuildGridModel.Instance.PlayerAreaInfoCur == null)
            m_TxtAreaItemName.text = m_TxtAreaItemNameDefault;
        else
            SetGuildGridAreaInfo(GuildGridModel.Instance.PlayerAreaInfoCur.Value);
    }

    //消息 玩家当前所处区域 值 改变
    private void MsgGuildGridAreaCurValueChange(IMessage rMessage)
    {
        SetGuildGridAreaInfo((int)rMessage.Data);
    }

    //设置 区域信息
    private void SetGuildGridAreaInfo(int id)
    {
        var cfgBuildingArea = ConfigSystem.Instance.GetConfig<Building_Area>(id);
        if (cfgBuildingArea == null) return;

        m_TxtAreaItemName.text = cfgBuildingArea.Name;
    }
    #endregion
}
