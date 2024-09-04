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
using FsNotificationSystem;
using FsWeatherSystem;

public partial class CharacterBase : ACharacter
{
    public override bool Init(System.Object outer = null)
    {
        bool succeed = base.Init(outer);

        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_INFO_CHANGE, MsgVenturerInfoChange);

        //分部脚本
        InitCharacterBaseState();
        InitCharacterBaseAction();
        InitCharacterBaseMovement();
        InitCharacterBaseAgent();

        //初始化 冒险者信息
        InitVenturerInfo();
        //初始化 网格系统
        InitGridCellSystem();
        //初始化 交互操作
        InitInteractive();

        return succeed;
    }

    protected override void OnDestroyThis()
    {
        base.OnDestroyThis();

        MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_INFO_CHANGE, MsgVenturerInfoChange);

        //分部脚本
        OnDestroyCharacterBaseState();
        OnDestroyCharacterBaseAction();
        OnDestroyCharacterBaseAgent();

        //冒险者信息事件
        DestroyVenturerInfo();
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        //设置 网格系统管理器
        m_GridItemComponent.SetGridCellSystemManager(GuildGridModel.Instance.GridCellSystemManager);
    }

    protected override void OnEnableThis()
    {
        base.OnEnableThis();

        m_GridItemComponent.EnableLocationEmitter = true;
    }

    protected override void OnDisableThis()
    {
        base.OnDisableThis();

        m_GridItemComponent.EnableLocationEmitter = false;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        //分部脚本
        TickCharacterBaseState(deltaTime);
        TickCharacterBaseMovement(deltaTime);
        TickCharacterBaseAgent(deltaTime);

        //网格系统
        TickGridCellSystem();
    }

    public override void FixedTick(float fixedDeltaTime)
    {
        base.FixedTick(fixedDeltaTime);

        //分部脚本
        FixedTickCharacterBaseState(fixedDeltaTime);
        FixedTickCharacterBaseMovement(fixedDeltaTime);
        FixedTickCharacterBaseAgent(fixedDeltaTime);
    }

    public override void OnInputJump(InputEventType inputEventType)
    {
        base.OnInputJump(inputEventType);
    }

    public override void OnInputMoveDirection(Vector3 inputDirection, InputEventType inputEventType, float hor, float ver)
    {
        base.OnInputMoveDirection(inputDirection, inputEventType, hor, ver);
    }

    private void OnGroundedFunc()
    {
        m_StateSystemComponent.RemoveState(State.Jump);
    }

    private void OnUnGroundedFunc()
    {

    }

    private void OnStayGroundedFunc(float duration)
    {
        if (duration > 0.3f)
        {
            m_StateSystemComponent.RemoveState(State.Jump);
        }
    }

    #region 冒险者信息、Spine
    [Header("冒险者信息")]
    [SerializeField] private Transform m_RootSpineAnim = null; //SpineAnim 根节点
    [SerializeField] private TextMeshPro m_TxtVenturerName = null; //冒险者名称
    [SerializeField] private String m_SpineLayerName = null; //Spine 渲染层级
    [SerializeField] private int m_SpineOrder = -1; //Spine 渲染排序

    private VenturerInfo m_VenturerInfo; //冒险者信息
    private SkeletonAnimation m_SpineAnim = null; //SpineAnim
    private bool m_isLoadingSpine = false; //是否 正在加载Spine

    private Skin m_SpineSkinMainMix = new Skin("SkinMainMix"); //Spine皮肤 混合
    private Skin m_SpineSkinElse = new Skin("SkinElse"); //Spine皮肤 其他
    private Dictionary<VenturerModel.ECustomSkin, Skin> m_DicSpineSkinCustom = new Dictionary<VenturerModel.ECustomSkin, Skin>(); //皮肤 自定义部位
    private Dictionary<VenturerModel.ECustomSkin, Skin> m_DicSpineSkinDirty = new Dictionary<VenturerModel.ECustomSkin, Skin>(); //列表 标脏皮肤 需要打包处理
    private Dictionary<Skin, Skin> m_DicSpineSkinRepack = new Dictionary<Skin, Skin>(); //字典 皮肤，打包皮肤
    private Dictionary<Skin, Material> m_DicSpineSkinRepackMat = new Dictionary<Skin, Material>(); //字典 皮肤，打包贴图
    private Dictionary<Skin, Texture2D> m_DicSpineSkinRepackTex = new Dictionary<Skin, Texture2D>(); //字典 皮肤，打包材质
    private Material m_MatSpineSkinTemplate;

    //初始化 冒险者信息
    private void InitVenturerInfo()
    {
        //实例化 模板材质球
        var shader = ConfigSystem.Instance.GetShader("Spine/Skeleton Tint");
        m_MatSpineSkinTemplate = new Material(shader);

        //Spine皮肤 自定义部位
        foreach (var kv in m_DicSpineSkinCustom)
            kv.Value.Clear();

        m_DicSpineSkinCustom.Clear();
        foreach (var kv in VenturerModel.Instance.DicCustomSkinPart)
        {
            var eCustomSkin = kv.Key;
            var skin = new Skin(eCustomSkin.ToString());
            m_DicSpineSkinCustom.Add(eCustomSkin, skin);
        }
    }

    //销毁 冒险者信息
    private void DestroyVenturerInfo()
    {
        RemoveVenturerInfoEvt();

        //销毁 Spine皮肤
        foreach (var kv in m_DicSpineSkinCustom)
            kv.Value.Clear();

        Destroy(m_MatSpineSkinTemplate);
    }

    /// <summary>
    /// 设置 冒险者信息
    /// </summary>
    /// <param name="venturerInfo"></param>
    public void SetVenturerInfo(VenturerInfo venturerInfo)
    {
        var venturerInfoLast = m_VenturerInfo;
        //移除 冒险者信息事件
        RemoveVenturerInfoEvt();
        m_VenturerInfo = venturerInfo;
        //添加 冒险者信息事件
        AddVenturerInfoEvt();

        //是否刷新 Spine实例
        if (venturerInfoLast == null || m_VenturerInfo.RaceClanId != venturerInfoLast.RaceClanId)
            RefreshSpineInstance();
        else
            RefreshSpineSkinAll();

        //设置 基础信息
        m_TxtVenturerName.text = m_VenturerInfo.FullName;

        //刷新 网格数据
        var cellCoord = GuildGridModel.Instance.GetGridCoord(TransformGet.position);
        m_GridCoordCur = cellCoord; //记录坐标
        m_GridItemComponent.Value = m_VenturerInfo.Id; //记录 冒险者ID
        RefreshGridItemGridCoord();  //刷新 角色在网格坐标中的数据
    }

    /// <summary>
    /// 刷新 Spine实例
    /// </summary>
    public void RefreshSpineInstance()
    {
        if (m_VenturerInfo == null) { return; }

        if (m_SpineAnim != null)
        {
            Destroy(m_SpineAnim.gameObject);
            m_SpineAnim = null;
        }

        //实例化 对应种族的Spine
        if (!m_isLoadingSpine)
        {
            m_isLoadingSpine = true;
            try
            {
                //在场景中实例化冒险者预制体
                var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(m_VenturerInfo.RaceClanId);
                if (cfgRaceClan == null) { return; }
                var prefabAddr = AssetAddressUtil.GetPrefabCharacterSpineAddress(cfgRaceClan.PrefabName);
                AssetSystem.Instance.LoadPrefab(prefabAddr, (prefab) =>
                {
                    //设置位置
                    m_TransRootBody = prefab.transform;
                    //所有角色 占地一个网格单元格 移动至单元格中心位置
                    m_TransRootBody.localPosition = Vector3.zero;
                    //获取Spine动画组件
                    m_SpineAnim = prefab.GetComponent<SkeletonAnimation>();
                    //设置 渲染层级
                    if (!string.IsNullOrEmpty(m_SpineLayerName))
                        m_SpineAnim.gameObject.layer = LayerMask.NameToLayer(m_SpineLayerName);
                    //设置 渲染Order
                    if (m_SpineOrder != -1)
                        m_SpineAnim.GetComponent<MeshRenderer>().sortingOrder = m_SpineOrder;
                    //旋转角度 朝向摄像机
                    if (EnableUpdateViewObjTrans)
                        EnableUpdateViewObjTrans = true; //需要更新渲染物体位置 重设为true 刷新位置
                    //设置监听事件
                    var item = WeatherModel.Instance.GetWeatherItem<WeatherItemGlobalLight>();
                    item.OnLightChange += (light) => 
                    {
                        var color = light.color * light.intensity;
                        color.a = 1f;
                        m_SpineAnim.skeleton.SetColor(color);
                    };

                    //刷新Spine皮肤
                    RefreshSpineSkinAll();
                }, m_RootSpineAnim);
            }
            finally
            {
                m_isLoadingSpine = false;
            }
        }
    }

    //添加 冒险者信息事件
    private void AddVenturerInfoEvt()
    {
        if (m_VenturerInfo == null) { return; }

        m_VenturerInfo.EvtSkinPartUpdate += EvtSkinPartPropChange;
        m_VenturerInfo.EvtSkinPartUpdateAll += EvtSkinPartPropChangeAll;
    }

    //移除 冒险者信息事件
    private void RemoveVenturerInfoEvt()
    {
        if (m_VenturerInfo == null) { return; }

        m_VenturerInfo.EvtSkinPartUpdate -= EvtSkinPartPropChange;
        m_VenturerInfo.EvtSkinPartUpdateAll -= EvtSkinPartPropChangeAll;
    }

    //事件 皮肤部位的道具变化
    private void EvtSkinPartPropChange(VenturerModel.EVenturerSkinPart skinPart)
    {
        if (m_VenturerInfo == null) { return; }

        int propId = m_VenturerInfo.GetSkinPartProp(skinPart);
        SetSpineSkinPartProp(skinPart, propId);
    }

    //事件 皮肤部位的道具变化 所有
    private void EvtSkinPartPropChangeAll()
    {
        RefreshSpineSkinAll();
    }

    //消息 冒险者信息 变更
    private void MsgVenturerInfoChange(IMessage rMessage)
    {
        var venturerInfo = (VenturerInfo)rMessage.Data;
        if (m_VenturerInfo.Id == venturerInfo.Id)
            SetVenturerInfo(venturerInfo);
    }

    #region Spine皮肤部件切换
    /// <summary>
    /// 刷新 Spine皮肤 所有
    /// </summary>
    public void RefreshSpineSkinAll()
    {
        if (m_VenturerInfo == null || m_SpineAnim == null) { return; }

        foreach (var kv in m_VenturerInfo.SkinInfo)
            SetSpineSkinPartProp((VenturerModel.EVenturerSkinPart)kv.Key, kv.Value);
    }

    /// <summary>
    /// 刷新 Spine皮肤
    /// </summary>
    /// <param name="skinPart"></param>
    public void RefreshSpineSkin(VenturerModel.EVenturerSkinPart skinPart)
    {
        if (m_VenturerInfo == null) { return; }

        int skinPropId;
        if (m_VenturerInfo.SkinInfo.TryGetValue((int)skinPart, out skinPropId))
            SetSpineSkinPartProp(skinPart, skinPropId);
    }

    //设置 Spine皮肤部位的道具
    private void SetSpineSkinPartProp(VenturerModel.EVenturerSkinPart skinPart, int propId)
    {
        if (m_SpineAnim == null) { return; }

        int skinPartId = (int)skinPart;
        string skinPartName = skinPart.ToString();

        //皮肤部位的插槽
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(m_VenturerInfo.RaceClanId);
        if (cfgRaceClan == null) return;

        //所有的插槽 设置对应的图片
        string slotParam = null;
        cfgRaceClan.SkinPartSlotMap.TryGetValue(skinPartId, out slotParam);
        if (slotParam == null)
        {
            Debug.LogError($"CharacterBase.SetSpineSkinPartProp() Error! >> Venturer_RaceClan配置表的SkinPartSlotMap字段中，没有配置皮肤部位的插槽 SkinPartId-{skinPartId}");
            return;
        }

        //皮肤部位的图片
        int skinSpriteIdDefault = propId * 10000 + m_VenturerInfo.RaceClanId * 10;
        var cfgSkinSprite = ConfigSystem.Instance.GetConfig<Prop_SkinSprite>(skinSpriteIdDefault + m_VenturerInfo.GenderId);

        var slotArray = slotParam.Split('|'); //Spine插槽名称
        int spriteCount = cfgSkinSprite == null ? slotArray.Length : cfgSkinSprite.SlotSprites.Count; //Sprite数量
        var assetLoadCount = 0; //加载Sprite计数
        for (int i = 0; i < slotArray.Length; i++)
        {
            //获取 插槽数据
            string slotName = slotArray[i];
            var skeletonData = m_SpineAnim.skeletonDataAsset.GetSkeletonData(true);
            int slotIndex = skeletonData.FindSlot(slotName).Index;

            if (i < spriteCount)
            {
                //设置插槽中的图片
                string skinSpriteName = string.Empty;
                if (cfgSkinSprite == null)
                    //默认路径的图片
                    skinSpriteName = $"{skinSpriteIdDefault}_{slotName}";
                else
                    //配置表设置的图片
                    skinSpriteName = cfgSkinSprite.SlotSprites[i];

                if (string.IsNullOrEmpty(skinSpriteName) || propId == 0)
                {
                    //图片名称为空时 置空插槽中的图片
                    SetSpineSkinPart(skinPart, slotIndex, slotName);
                }
                else
                {
                    assetLoadCount++;
                    //获取 图片Address 加载图片
                    string addrSprite = AssetAddressUtil.GetTextureCharacterSkinSpriteAddress($"{m_VenturerInfo.RaceClanId}/{skinPartName}/{propId}", skinSpriteName);
                    AssetSystem.Instance.LoadSprite(addrSprite, (sprite) =>
                    {
                        //图片为空时 置空插槽中的图片
                        if (sprite == null)
                        {
                            SetSpineSkinPart(skinPart, slotIndex, slotName);
                            return;
                        }

                        //生成附件
                        var attachment = GenerateSpineAttachment(sprite, slotIndex, "default", slotName);
                        //设置皮肤
                        SetSpineSkinPart(skinPart, slotIndex, slotName, attachment);

                        //所有资源加载完成后 应用皮肤并刷新
                        if (--assetLoadCount == 0)
                            //应用皮肤并刷新
                            ApplySpineSkinMix();
                    });
                }
            }
            else
                SetSpineSkinPart(skinPart, slotIndex, slotName);
        }

        //无需等待异步加载时 直接应用新皮肤
        if (assetLoadCount == 0)
            ApplySpineSkinMix();
    }

    //设置 皮肤 部位
    private void SetSpineSkinPart(VenturerModel.EVenturerSkinPart skinPart, int slotIndex, string slotName, Attachment attachment = null)
    {
        //不同身体部位 设置在 不同的Spine皮肤中
        Skin skin = m_SpineSkinElse;
        var eCustomSkin = VenturerModel.ECustomSkin.None;
        foreach (var kv in VenturerModel.Instance.DicCustomSkinPart)
        {
            var listSkinPart = kv.Value;
            if (listSkinPart.Contains(skinPart))
            {
                eCustomSkin = kv.Key;
                skin = m_DicSpineSkinCustom[eCustomSkin];
                break;
            }
        }

        //置空插槽中的图片
        if (attachment == null)
            skin.RemoveAttachment(slotIndex, slotName);
        else
            //设置皮肤
            skin.SetAttachment(slotIndex, slotName, attachment);

        //记录标脏 等待处理
        if (!m_DicSpineSkinDirty.ContainsKey(eCustomSkin))
            m_DicSpineSkinDirty.Add(eCustomSkin, skin);
    }

    //应用 皮肤 混合所有皮肤
    private void ApplySpineSkinMix()
    {
        if (m_SpineAnim == null) { return; }

        //遍历处理 所有标脏的皮肤
        foreach (var kv in m_DicSpineSkinDirty)
        {
            var eCustomSkin = kv.Key;
            var skin = kv.Value;
            //有皮肤附件时 进行打包处理
            if (skin.Attachments.Count > 0)
            {
                Material mat = null;
                Texture2D tex = null;
                var skinRepack = skin.GetRepackedSkin($"{skin.Name}_Repack", m_MatSpineSkinTemplate, out mat, out tex);
                m_SpineSkinMainMix.AddSkin(skinRepack); //组合进主皮肤

                //清除上次的打包皮肤 记录新的打包皮肤数据
                Skin skinRepackLast = null;
                if (m_DicSpineSkinRepack.TryGetValue(skin, out skinRepackLast))
                {
                    skinRepackLast.Clear();
                    Destroy(m_DicSpineSkinRepackMat[skin]);
                    Destroy(m_DicSpineSkinRepackTex[skin]);
                    m_DicSpineSkinRepack[skin] = skinRepack;
                    m_DicSpineSkinRepackMat[skin] = mat;
                    m_DicSpineSkinRepackTex[skin] = tex;
                }
                else
                {
                    m_DicSpineSkinRepack.Add(skin, skinRepack);
                    m_DicSpineSkinRepackMat.Add(skin, mat);
                    m_DicSpineSkinRepackTex.Add(skin, tex);
                }

                //设置 皮肤的颜色
                mat.color = m_VenturerInfo.GetSkinColor(eCustomSkin);
            }
        }
        m_DicSpineSkinDirty.Clear();

        //设置并刷新Skin
        m_SpineAnim.Skeleton.SetSkin(m_SpineSkinMainMix);
        m_SpineAnim.Skeleton.SetSlotsToSetupPose();
        m_SpineAnim.AnimationState.Apply(m_SpineAnim.Skeleton);

        AtlasUtilities.ClearCache();
    }

    //生成 Spine附件
    private Attachment GenerateSpineAttachment(Sprite sprite, int slotIndex, string templateSkinName, string templateAttachmentName)
    {
        if (m_SpineAnim == null) { return null; }

        Attachment attachment = null;
        //是否有 缓存Attachment
        //m_AttachmentsCached.TryGetValue(sprite.name, out attachment);

        if (attachment == null)
        {
            var skeletonData = m_SpineAnim.skeletonDataAsset.GetSkeletonData(true);
            var templateSkin = skeletonData.FindSkin(templateSkinName);
            Attachment templateAttachment = templateSkin.GetAttachment(slotIndex, templateAttachmentName);
            var sourceMaterial = m_SpineAnim.skeletonDataAsset.atlasAssets[0].PrimaryMaterial;
            attachment = templateAttachment.GetRemappedClone(sprite, sourceMaterial, premultiplyAlpha: true);

            //记录 缓存Attachment
            //m_AttachmentsCached.Add(sprite.name, attachment);
        }

        return attachment;
    }
    #endregion
    #endregion

    #region 网格系统
#if UNITY_EDITOR
    [Header("网格系统-定位功能-场景线框")]
    [Tooltip("定位中心点")]
    [SerializeField] private bool m_GizmosLocationCenter;
    [Tooltip("定位接收者检测范围中心点")]
    [SerializeField] private bool m_GizmosLocationReceiverRangeCenter;
    [Tooltip("定位接收者检测范围")]
    [SerializeField] private bool m_GizmosLocationReceiverRangeSize;

    private void OnDrawGizmos()
    {
        //定位中心点
        if (m_GizmosLocationCenter)
        {
            Gizmos.DrawSphere(GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationCenter), 0.06f);
        }

        //定位接收者检测范围中心点
        if (m_GizmosLocationReceiverRangeCenter)
        {
            Gizmos.DrawSphere(GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeCenter), 0.06f);
        }

        //定位接收者检测范围
        if (m_GizmosLocationReceiverRangeSize)
        {
            var centerPos = GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeCenter);
            Gizmos.DrawWireCube(centerPos, GuildGridModel.Instance.GetWorldPosition(m_GridItemComponent.LocationReceiverRangeSize));
        }

        OnDrawGizmosCharacterBaseAgent();
    }
#endif

    /// <summary>
    /// 更新显示物体的位置
    /// </summary>
    public bool EnableUpdateViewObjTrans
    {
        get
        {
            return m_EnableUpdateViewObjTrans;
        }

        set
        {
            m_EnableUpdateViewObjTrans = value;
        }
    }
    private bool m_EnableUpdateViewObjTrans = true;

    protected GridItemComponent m_GridItemComponent = new GridItemComponent();
    private GridCoord m_GridCoordCur = new GridCoord(-1, -1, -1); //单元格坐标 当前
    private CharacterOrientation m_OrientationLast; //朝向 上一次

    //初始化 网格系统
    private void InitGridCellSystem()
    {
        //初始化 网格数据
        m_GridItemComponent.SetData(0, FsGridCellSystem.EDirection.None, EGridItemType.Main, m_GridCoordCur);
        m_GridItemComponent.GridItemSize = new GridCoord(1, 1, 4); //角色的占地尺寸
        m_GridItemComponent.AddRealVolume(new RealVolume(gameObject.name, new GridCoordFloat(1, 0.1f, 4), new GridCoordFloat(0, 0.45f, 0)));
        m_GridItemComponent.SetRealVolumeCur(gameObject.name);

        //设置 网格系统 定位功能参数
        m_GridItemComponent.SetLocationCenter(ELocationCenterType.UpperCenter); //定位中心点

        //获取 网格坐标
        if (m_GridItemComponent.MainGridCoord == GridCoord.invalid)
        {
            //根据物体的世界位置 获取网格坐标
            var posSizeAmend = GuildGridModel.Instance.GetGridItemSizePositionAmend(m_GridItemComponent.GridItemSize);
            //网格坐标为物体占地尺寸左下角 世界坐标修正
            var gridCoord = GuildGridModel.Instance.GetGridCoord(TransformGet.position - posSizeAmend);
            m_GridCoordCur = gridCoord;
            RefreshGridItemGridCoord();
        }
    }

    //渲染帧执行 网格系统
    private void TickGridCellSystem()
    {
        CheckGridCoordChange();
        CheckOrientationChange();

        //位置改变 需要更新显示物体的位置
        if (EnableUpdateViewObjTrans)
        {
            RefreshSpineViewPosition();
        }
    }

    //检查 位置网格坐标是否改变
    private void CheckGridCoordChange()
    {
        //根据世界坐标 获取 网格坐标
        var gridCoord = GuildGridModel.Instance.GetGridCoord(TransformGet.position);
        //网格坐标是否改变
        if (m_GridCoordCur.Equals(gridCoord)) { return; }
        m_GridCoordCur = gridCoord; //记录新坐标

        //刷新 角色在网格坐标中的数据
        RefreshGridItemGridCoord();

        //执行回调 位置网格坐标改变
        OnGridCoordChanged(m_GridCoordCur);
        OnGridCoordOrOrientationChanged(m_GridCoordCur, m_OrientationLast);
    }

    /// <summary>
    /// 执行 位置网格坐标改变
    /// </summary>
    /// <param name="gridCoordCur">当前坐标</param>
    protected virtual void OnGridCoordChanged(GridCoord gridCoordCur)
    {

    }

    /// <summary>
    /// 更新 网格数据 坐标
    /// </summary>
    protected virtual void RefreshGridItemGridCoord()
    {
        //移除 上次坐标的网格项目数据d
        GuildGridModel.Instance.RemoveMainGridItemValue(GuildGridModel.EGridLayer.Character, m_GridCoordCur);
        //记录 新的坐标的网格项目数据
        m_GridItemComponent.MainGridCoord = m_GridCoordCur;
        GuildGridModel.Instance.PushMainGridItemValue(GuildGridModel.EGridLayer.Character, m_GridItemComponent.GridItemData);
    }

    //检查 朝向发生变化
    private void CheckOrientationChange()
    {
        //单元格坐标是否改变
        if (m_OrientationLast == CharacterOrientationCur) { return; }
        m_OrientationLast = CharacterOrientationCur; //记录 当前朝向

        OnOrientationChanged(m_OrientationLast);
        OnGridCoordOrOrientationChanged(m_GridCoordCur, m_OrientationLast);
    }

    /// <summary>
    /// 执行 朝向改变
    /// </summary>
    /// <param name="orientationCur">当前朝向</param>
    protected virtual void OnOrientationChanged(CharacterOrientation orientationCur)
    {

    }

    /// <summary>
    /// 执行 网格坐标或朝向改变
    /// </summary>
    protected virtual void OnGridCoordOrOrientationChanged(GridCoord gridCoordCur, CharacterOrientation orientationCur)
    {

    }

    //刷新 Spine显示物体 坐标
    private void RefreshSpineViewPosition()
    {
        //仅在 玩家移动状态时 才进行渲染排序
        if (ContainState(State.Idle)) { return; }

        //场景3D化 不再需要渲染排序
        //刷新 Spine位置
        //if (m_RootSpineAnim != null)
        //{
        //    var SpinePosNew = GuildGridModel.Instance.GetWorldPosToViewPos(TransformGet.position);
        //    var posLocalOri = m_RootSpineAnim.position; //渲染深度Y 使用原数据
        //    m_RootSpineAnim.position = new Vector3(SpinePosNew.x, posLocalOri.y, SpinePosNew.z);
        //}
    }

    #endregion

    #region 场景交互
    [Header("场景交互")]
    [SerializeField] private GameObject m_GoOperateTip = null; //按钮 操作
    [SerializeField] private NotificationComponent m_NotificationComponent = null; //对话框

    //初始化 交互
    private void InitInteractive()
    {
        ShowOperateTip(false);
    }

    /// <summary>
    /// 显示 可操作提示
    /// </summary>
    public virtual void ShowOperateTip(bool isShow)
    {
        if (m_GoOperateTip == null) { return; }

        m_GoOperateTip.SetActive(isShow);
    }

    /// <summary>
    /// 执行操作
    /// </summary>
    public virtual void ExcuteOperate()
    {
        //默认操作 开始对话
        ShowOperateTip(false);
        //PlayDialogue("你说什么？你大声一点");
    }

    /// <summary>
    /// 播放 对话
    /// </summary>
    /// <param name="content"></param>
    public void PlayDialogue(string content)
    {
        m_NotificationComponent.PlayTextContent(content);
        m_NotificationComponent.OnPlayComplete = OnDialoguePlayComplete;
    }

    //回调 对话播放完毕
    private void OnDialoguePlayComplete()
    {
        ShowOperateTip(true);
    }
    #endregion
}
