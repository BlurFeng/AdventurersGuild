using com.ootii.Messages;
using Deploy;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;
using Google.Protobuf.Collections;
using FsListItemPages;

public class RoleEditorWindow : WindowBase
{
    /// <summary>
    /// 角色编辑界面 传参
    /// </summary>
    public class RoleEditorWindowArg
    {
        /// <summary>
        /// 冒险者信息
        /// </summary>
        public VenturerInfo VenturerInfo
        {
            get
            {
                if (m_VenturerInfo == null)
                    m_VenturerInfo = new VenturerInfo();
                return m_VenturerInfo;
            }

            set
            {
                m_VenturerInfo = new VenturerInfo(value);
            }
        }
        private VenturerInfo m_VenturerInfo;

        /// <summary>
        /// 完成 回调
        /// </summary>
        public Action<VenturerInfo> OnConfirmCallBack;
    }

    [SerializeField] private GameObject m_BtnConfirm = null; //按钮 确定
    [SerializeField] private GameObject m_BtnCancel = null; //按钮 取消

    [Header("种族")]
    [SerializeField] private ListItemPagesComponent m_ListItemPagesComponentRaceClan = null; //翻页项目列表 种族
    [Header("性别")]
    [SerializeField] private List<ItemRoleEditorGender> m_ListItemGender = null; //列表 项目 性别
    [Header("皮肤选择")]
    [SerializeField] private List<ItemRoleEditorSkinBody> m_ListItemSkinBody = null; //列表 项目 皮肤
    [Header("皮肤颜色")]
    [SerializeField] private TextMeshProUGUI m_TxtSkinColorName = null; //文本 自定义Spine 名称
    [SerializeField] private GameObject m_BtnSkinColorLeft = null; //按钮 皮肤颜色 左
    [SerializeField] private GameObject m_BtnSkinColorRight = null; //按钮 皮肤颜色 右
    [SerializeField] private Slider m_SldSkinColorHue = null; //滑条 颜色 色相
    [SerializeField] private Slider m_SldSkinColorSaturation = null; //滑条 颜色 纯度
    [SerializeField] private Slider m_SldSkinColorValue = null; //滑条 颜色 明度
    [SerializeField] private Image m_ImgSkinColorBarHue = null; //图片 颜色条 色相
    [SerializeField] private Image m_ImgSkinColorBarSaturation = null; //图片 颜色条 纯度
    [SerializeField] private Image m_ImgSkinColorBarValue = null; //图片 颜色条 明度
    [Header("姓名")]
    [SerializeField] private GameObject m_BtnNameRandom = null; //按钮 随机名称
    [SerializeField] private TMP_InputField m_InputTxtName = null; //文本输入 名称
    [Header("角色预览")]
    [SerializeField] private GameObject m_BtnEquip = null; //按钮 装备显示
    [SerializeField] private CharacterBase m_CharacterBase = null; //组件 冒险者角色

    private RoleEditorWindowArg RoleEditorWindowArgGet
    {
        get 
        {
            if (m_RoleEditorWindowArg == null)
            {
                m_RoleEditorWindowArg = new RoleEditorWindowArg();
                m_RoleEditorWindowArg.VenturerInfo = new VenturerInfo();
            }
            return m_RoleEditorWindowArg;
        }
    }
    private RoleEditorWindowArg m_RoleEditorWindowArg; //界面传参

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnConfirm).SetClickHandler(BtnConfirm);
        ClickListener.Get(m_BtnCancel).SetClickHandler(BtnCancel);

        //初始化
        InitGender();
        InitSkinBody();
        InitSkinColor();
        InitName();
        InitRolePreview();
        InitRaceClan();
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //传参解析
        m_RoleEditorWindowArg = userData as RoleEditorWindowArg;
        if (m_RoleEditorWindowArg != null && m_RoleEditorWindowArg.VenturerInfo != null)
        {
            //复制 新的冒险者信息
            m_RoleEditorWindowArg.VenturerInfo = new VenturerInfo(m_RoleEditorWindowArg.VenturerInfo);
        }

        //选中种族
        m_ListItemPagesComponentRaceClan.SelectItem(RoleEditorWindowArgGet.VenturerInfo.RaceClanId);
        //选中性别
        for (int i = 0; i < m_ListItemGender.Count; i++)
        {
            var item = m_ListItemGender[i];
            if (m_RoleEditorWindowArg.VenturerInfo.GetGender() == item.Gender)
            {
                EvtItemGenderOnClick(item);
                break;
            }
        }
        //选中皮肤
        for (int i = 0; i < m_ListItemSkinBody.Count; i++)
        {
            var item = m_ListItemSkinBody[i];
            if (!item.gameObject.activeSelf) { continue; }
            int skinPartID = item.SkinPartId;
            int skinPropID = m_RoleEditorWindowArg.VenturerInfo.GetSkinPartProp((VenturerModel.EVenturerSkinPart)skinPartID);
            if (skinPropID > 0)
            {
                item.SetSkinPropId(skinPropID);
            }
        }
        //设置名称
        m_InputTxtName.text = m_RoleEditorWindowArg.VenturerInfo.FullName;
        //设置角色预览
        SetRolePreviewSpine(m_RoleEditorWindowArg.VenturerInfo);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        //销毁 颜色条HSV 贴图
        Destroy(m_SpriteBarHue.texture);
        Destroy(m_SpriteBarSaturation.texture);
        Destroy(m_SpriteBarValue.texture);

        //销毁 预览角色
        m_CharacterBase.DestroySelf();
    }

    //按钮 确认
    private void BtnConfirm(PointerEventData obj)
    {
        //记录 名称
        m_RoleEditorWindowArg.VenturerInfo.SurnameCustom = m_InputTxtName.text;
        m_RoleEditorWindowArg.VenturerInfo.NameCustom = m_InputTxtName.text;
        m_RoleEditorWindowArg.VenturerInfo.ExtranameCustom = m_InputTxtName.text;

        //装备数据 还原
        SetShowEquip(true);

        //设置 修改后的数据
        if (VenturerModel.Instance.GetVenturerInfo(m_RoleEditorWindowArg.VenturerInfo.Id) != null)
        {
            //在冒险者列表中 直接修改VenturerModel中的数据
            VenturerModel.Instance.SetVenturerInfo(m_RoleEditorWindowArg.VenturerInfo);
        }
        else
        {
            //不在冒险者列表中 回调传参设置的数据
            m_RoleEditorWindowArg.OnConfirmCallBack?.Invoke(m_RoleEditorWindowArg.VenturerInfo);
        }

        CloseWindow();
    }

    //按钮 取消
    private void BtnCancel(PointerEventData obj)
    {
        CloseWindow();
    }

    #region 种族
    //初始化 种族
    public void InitRaceClan()
    {
        //获取 所有种族的配置ID
        m_ListItemPagesComponentRaceClan.Init(VenturerModel.Instance.GetUsableRaceClanData(), OnSelectItemChange);
    }
    
    //选中的项目改表
    private void OnSelectItemChange(IItemPagesData data)
    {
        //设置 冒险者信息
        RoleEditorWindowArgGet.VenturerInfo.RaceClanId = data.GetId();
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(RoleEditorWindowArgGet.VenturerInfo.RaceClanId);
        if (cfgRaceClan == null) { return; }
        //设置 可以选择的性别
        SetListItemGender(cfgRaceClan.GenderWeight);
        //设置 可以选择的皮肤
        SetListItemSkinBody(cfgRaceClan.SkinPartSlotMap);
        //设置 可以选择的自定义皮肤颜色
        SetListItemCustomSkin(cfgRaceClan.SkinPartSlotMap, cfgRaceClan.SkinColorHSVRange);
        //设置 角色预览
        RefreshRolePreviewSpine();
    }
    #endregion

    #region 性别
    private ItemRoleEditorGender m_ItemGenderCur; //性别选项 当前

    //初始化 性别
    private void InitGender()
    {
        //性别选项的点击事件
        for (int i = 0; i < m_ListItemGender.Count; i++)
        {
            var item = m_ListItemGender[i];
            item.OnClick = EvtItemGenderOnClick;
        }
    }

    //时间 点击性别选项
    private void EvtItemGenderOnClick(ItemRoleEditorGender item)
    {
        if (m_ItemGenderCur == item) { return; }

        //旧项目 取消选中
        if (m_ItemGenderCur != null)
        {
            m_ItemGenderCur.SetSelect(false);
        }

        //新项目 选中
        m_ItemGenderCur = item;
        m_ItemGenderCur.SetSelect(true);

        //设置 冒险者信息
        m_RoleEditorWindowArg.VenturerInfo.SetGender(m_ItemGenderCur.Gender);
    }

    //设置 可以选择的性别按钮
    private void SetListItemGender(MapField<int, int> genderWeight)
    {
        //设置 性别选项
        int index = 0;
        foreach (var kv in genderWeight)
        {
            //超出项目数 结束
            if (index >= m_ListItemGender.Count) { break; }

            var genderId = kv.Key;
            var item = m_ListItemGender[index];
            item.gameObject.SetActive(true);
            item.SetInfo(genderId);

            index++;
        }
        //隐藏 多余的项目
        for (int i = index; i < m_ListItemGender.Count; i++)
        {
            var item = m_ListItemGender[i];
            m_ListItemGender[i].gameObject.SetActive(false);
        }

        //当前种族不存在被选中的性别
        if (m_ItemGenderCur == null || !genderWeight.ContainsKey((int)m_ItemGenderCur.Gender))
        {
            //默认选中第一个性别
            EvtItemGenderOnClick(m_ListItemGender[0]);
        }
    }
    #endregion

    #region 皮肤-身体
    //初始化 皮肤
    private void InitSkinBody()
    {
        //遍历配置表获取数据 皮肤身体部位 皮肤道具ID
        Dictionary<int, List<int>> m_DicSkinPartSkinId = new Dictionary<int, List<int>>();
        var cfgSkinPart = ConfigSystem.Instance.GetConfigMap<Venturer_SkinPart>();
        foreach (var skinPartId in cfgSkinPart.Keys)
        {
            if (skinPartId >= 20) { continue; }
            m_DicSkinPartSkinId.Add(skinPartId, new List<int>());
        }
        var cfgPropSkin = ConfigSystem.Instance.GetConfigMap<Prop_SkinAttr>();
        foreach (var kv in cfgPropSkin)
        {
            int skinPartId = kv.Value.SkinPartEnum;
            if (skinPartId >= 20) { continue; }
            m_DicSkinPartSkinId[skinPartId].Add(kv.Key);
        }

        //设置 皮肤部位&皮肤道具ID列表
        int indexCur = 0;
        foreach (var kv in m_DicSkinPartSkinId)
        {
            if (indexCur >= m_ListItemSkinBody.Count) { break; }

            var item = m_ListItemSkinBody[indexCur];
            item.SetInfo(kv.Key, kv.Value);
            item.OnClick = EvtItemSkinBodyOnClick;
            indexCur++;
        }
    }

    //事件 皮肤选项 点击
    private void EvtItemSkinBodyOnClick(ItemRoleEditorSkinBody item)
    {
        //设置 冒险者信息
        m_RoleEditorWindowArg.VenturerInfo.SetSkinPartProp((VenturerModel.EVenturerSkinPart)item.SkinPartId, item.SkinPropId);
    }

    //设置 可以选择的皮肤部位
    private void SetListItemSkinBody(MapField<int, string> skinPartSlotMap)
    {
        for (int i = 0; i < m_ListItemSkinBody.Count; i++)
        {
            var item = m_ListItemSkinBody[i];
            if (skinPartSlotMap.ContainsKey(item.SkinPartId))
            {
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region 皮肤颜色
    private List<VenturerModel.ECustomSkin> m_ListItemCustomSkin = new List<VenturerModel.ECustomSkin>(); //列表项目 自定义皮肤
    private int m_SkinColorItemIndexCur = 0; //自定义皮肤 项目 当前
    private Dictionary<VenturerModel.ECustomSkin, float[]> m_DicCustomSkinColorHSVRange = new Dictionary<VenturerModel.ECustomSkin, float[]>(); //字典 自定义皮肤 颜色范围
    private float[] m_CustomSkinColorHSVRangeDefault = new float[6] { 0, 1, 0, 1, 0, 1 };
    private int m_TexHSVWidthPixelCount = 46;
    private Sprite m_SpriteBarHue; //精灵图 色相
    private Sprite m_SpriteBarSaturation; //精灵图 纯度
    private Sprite m_SpriteBarValue; //精灵图 明度

    //初始化 皮肤颜色
    private void InitSkinColor()
    {
        ClickListener.Get(m_BtnSkinColorLeft).SetClickHandler(BtnSkinColorLeft);
        ClickListener.Get(m_BtnSkinColorRight).SetClickHandler(BtnSkinColorRight);

        //滑块 颜色HSV
        m_SldSkinColorHue.onValueChanged.AddListener(EvtSkinColorHueChange);
        m_SldSkinColorSaturation.onValueChanged.AddListener(EvtSkinColorSaturationChange);
        m_SldSkinColorValue.onValueChanged.AddListener(EvtSkinColorValueChange);
        //创建Sprite 颜色条 HSV
        Rect rect = new Rect(Vector2.zero, new Vector2(m_TexHSVWidthPixelCount, 1f));
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Texture2D texHue = new Texture2D(m_TexHSVWidthPixelCount, 1);
        texHue.filterMode = FilterMode.Point;
        m_SpriteBarHue = Sprite.Create(texHue, rect, pivot);
        m_ImgSkinColorBarHue.sprite = m_SpriteBarHue;
        Texture2D texSaturation = new Texture2D(m_TexHSVWidthPixelCount, 1);
        texSaturation.filterMode = FilterMode.Point;
        m_SpriteBarSaturation = Sprite.Create(texSaturation, rect, pivot);
        m_ImgSkinColorBarSaturation.sprite = m_SpriteBarSaturation;
        Texture2D texValue = new Texture2D(m_TexHSVWidthPixelCount, 1);
        texValue.filterMode = FilterMode.Point;
        m_SpriteBarValue = Sprite.Create(texValue, rect, pivot);
        m_ImgSkinColorBarValue.sprite = m_SpriteBarValue;
    }

    //设置 可以选择的 自定义皮肤颜色
    private void SetListItemCustomSkin(MapField<int, string> skinPartSlotMap, MapField<int, string> SkinColorHSVRangeMap)
    {
        //记录 上次选中的项目
        var eCustomSkinLast = VenturerModel.ECustomSkin.None;
        if (m_ListItemCustomSkin.Count > 0)
        {
            eCustomSkinLast = m_ListItemCustomSkin[m_SkinColorItemIndexCur];
        }

        //获取 自定义的Spine皮肤
        m_ListItemCustomSkin.Clear();
        m_SkinColorItemIndexCur = 0;
        foreach (var kv in VenturerModel.Instance.DicCustomSkinPart)
        {
            var eCustomSkin = kv.Key;
            var listSkinPart = kv.Value;

            //检查该种族的皮肤部位 是否可以自定义皮肤颜色
            for (int i = 0; i < listSkinPart.Count; i++)
            {
                var skinPart = listSkinPart[i];
                if (skinPartSlotMap.ContainsKey((int)skinPart))
                {
                    m_ListItemCustomSkin.Add(eCustomSkin);
                    //记录 上次选中的项目的下标
                    if (eCustomSkinLast == eCustomSkin)
                    {
                        m_SkinColorItemIndexCur = m_ListItemCustomSkin.Count - 1;
                    }
                    break;
                }
            }
        }

        //获取 颜色范围
        m_DicCustomSkinColorHSVRange.Clear();
        foreach (var kv in SkinColorHSVRangeMap)
        {
            var eCustomSkin = (VenturerModel.ECustomSkin)kv.Key;
            var paramsStr = kv.Value.Split('|');
            float[] hsvRange = new float[6];
            for (int i = 0; i < paramsStr.Length; i++)
            {
                hsvRange[i] = float.Parse(paramsStr[i]);
            }
            m_DicCustomSkinColorHSVRange.Add(eCustomSkin, hsvRange);
        }

        //设置 选中的自定义皮肤
        SetEntrySkinColorIndex(m_SkinColorItemIndexCur);
    }

    //按钮 条目自定义皮肤 向右
    private void BtnSkinColorRight(PointerEventData obj)
    {
        //切换至下一个
        m_SkinColorItemIndexCur++;
        if (m_SkinColorItemIndexCur >= m_ListItemCustomSkin.Count)
        {
            m_SkinColorItemIndexCur = 0;
        }
        SetEntrySkinColorIndex(m_SkinColorItemIndexCur);
    }

    //按钮 条目自定义皮肤 向左
    private void BtnSkinColorLeft(PointerEventData obj)
    {
        //切换至 上一个
        m_SkinColorItemIndexCur--;
        if (m_SkinColorItemIndexCur < 0)
        {
            m_SkinColorItemIndexCur = m_ListItemCustomSkin.Count - 1;
        }
        SetEntrySkinColorIndex(m_SkinColorItemIndexCur);
    }

    //设置 条目 皮肤颜色
    private void SetEntrySkinColorIndex(int index)
    {
        if (m_RoleEditorWindowArg == null || index >= m_ListItemCustomSkin.Count) { return; }

        var eCustomSkin = m_ListItemCustomSkin[index];
        //自定义皮肤的名称
        m_TxtSkinColorName.text = eCustomSkin.ToString();
        //设置 冒险者当前自定义皮肤部位的颜色
        Color color = m_RoleEditorWindowArg.VenturerInfo.GetSkinColor(eCustomSkin);
        float h; float s; float v;
        Color.RGBToHSV(color, out h, out s, out v);
        m_SldSkinColorHue.value = h;
        m_SldSkinColorSaturation.value = s;
        m_SldSkinColorValue.value = v;
        RefreshSkinColorBar();
    }

    //设置 皮肤颜色 色相
    private void EvtSkinColorHueChange(float value)
    {
        m_SldSkinColorHue.value = value;
        RefreshSkinColorBar();
    }

    //设置 皮肤颜色 纯度
    private void EvtSkinColorSaturationChange(float value)
    {
        m_SldSkinColorSaturation.value = value;
        RefreshSkinColorBar();
    }

    //设置 皮肤颜色 明度
    private void EvtSkinColorValueChange(float value)
    {
        m_SldSkinColorValue.value = value;
        RefreshSkinColorBar();
    }

    //刷新 皮肤颜色条
    private void RefreshSkinColorBar()
    {
        //颜色范围
        float[] hsvRange;
        m_DicCustomSkinColorHSVRange.TryGetValue(m_ListItemCustomSkin[m_SkinColorItemIndexCur], out hsvRange);
        if (hsvRange == null)
        {
            hsvRange = m_CustomSkinColorHSVRangeDefault;
        }

        float h = Mathf.Lerp(hsvRange[0], hsvRange[1], m_SldSkinColorHue.value); //色相
        float s = Mathf.Lerp(hsvRange[2], hsvRange[3], m_SldSkinColorSaturation.value); //纯度
        float v = Mathf.Lerp(hsvRange[4], hsvRange[5], m_SldSkinColorValue.value); //明度

        //重新计算 HSV三条颜色的渐变
        for (int i = 0; i < m_TexHSVWidthPixelCount; i++)
        {
            int value = i + 1;
            Color colorH = Color.HSVToRGB(Mathf.Lerp(hsvRange[0], hsvRange[1], (float)value / m_TexHSVWidthPixelCount), 1f, 1f);
            m_SpriteBarHue.texture.SetPixel(i, 0, colorH);
            m_SpriteBarHue.texture.Apply();
            Color colorS = Color.HSVToRGB(h, Mathf.Lerp(hsvRange[2], hsvRange[3], (float)value / m_TexHSVWidthPixelCount), v);
            m_SpriteBarSaturation.texture.SetPixel(i, 0, colorS);
            m_SpriteBarSaturation.texture.Apply();
            Color colorV = Color.HSVToRGB(h, s, Mathf.Lerp(hsvRange[4], hsvRange[5], (float)value / m_TexHSVWidthPixelCount));
            m_SpriteBarValue.texture.SetPixel(i, 0, colorV);
            m_SpriteBarValue.texture.Apply();
        }

        //记录当前的自定义皮肤颜色
        var eCustomSkin = m_ListItemCustomSkin[m_SkinColorItemIndexCur];
        SetVenturerInfoSkinColor(eCustomSkin);
    }

    //设置 冒险者信息皮肤颜色 十六进制
    private void SetVenturerInfoSkinColor(VenturerModel.ECustomSkin eCustomSkin)
    {
        //颜色范围
        float[] hsvRange;
        m_DicCustomSkinColorHSVRange.TryGetValue(m_ListItemCustomSkin[m_SkinColorItemIndexCur], out hsvRange);
        if (hsvRange == null)
        {
            hsvRange = m_CustomSkinColorHSVRangeDefault;
        }

        float h = Mathf.Lerp(hsvRange[0], hsvRange[1], m_SldSkinColorHue.value); //色相
        float s = Mathf.Lerp(hsvRange[2], hsvRange[3], m_SldSkinColorSaturation.value); //纯度
        float v = Mathf.Lerp(hsvRange[4], hsvRange[5], m_SldSkinColorValue.value); //明度
        //转换为RGBA颜色 十六进制
        var color = Color.HSVToRGB(h, s, v);
        m_RoleEditorWindowArg.VenturerInfo.SetSkinColor(eCustomSkin, color);
    }
    #endregion

    #region 姓名
    //初始化 姓名
    private void InitName()
    {
        ClickListener.Get(m_BtnNameRandom).SetClickHandler(BtnNameRandom);
    }

    //按钮 随机姓名
    private void BtnNameRandom(PointerEventData obj)
    {
        m_InputTxtName.text = "随机名称";
    }
    #endregion

    #region 角色预览
    private Dictionary<VenturerModel.EVenturerSkinPart, int> m_DicSkinPartEuipId = new Dictionary<VenturerModel.EVenturerSkinPart, int>(); //字典 角色的皮肤部位装备ID
    private Dictionary<VenturerModel.EVenturerSkinPart, int> m_DicSkinPartEuipIdNone = new Dictionary<VenturerModel.EVenturerSkinPart, int>(); //字典 角色的皮肤部位装备ID 无装备
    private bool m_IsShowEquip = true; //正在 显示装备

    //初始化 角色预览
    private void InitRolePreview()
    {
        ClickListener.Get(m_BtnEquip).SetClickHandler(BtnEquip);

        m_CharacterBase.Init();
        m_CharacterBase.EnableUpdateViewObjTrans = false;
    }

    //按钮 显示装备
    private void BtnEquip(PointerEventData obj)
    {
        //切换显示 装备的数据
        bool isShow = !m_IsShowEquip;
        SetShowEquip(isShow);
    }

    //设置 显示装备
    private void SetShowEquip(bool isShow)
    {
        m_IsShowEquip = isShow;
        m_RoleEditorWindowArg.VenturerInfo.SetSkinPartProp(m_IsShowEquip ? m_DicSkinPartEuipId : m_DicSkinPartEuipIdNone);
    }

    //设置 角色预览 Spine实例
    private void SetRolePreviewSpine(VenturerInfo venturerInfo)
    {
        if (m_CharacterBase == null) { return; }

        //记录 当前的装备ID
        m_DicSkinPartEuipId.Clear();
        m_DicSkinPartEuipIdNone.Clear();

        //记录当前使用的装备
        foreach (var kv in m_RoleEditorWindowArg.VenturerInfo.SkinInfo)
        {
            int skinPartId = kv.Key;
            int skinPropId = kv.Value;
            if (skinPartId < 20) { continue; }
            m_DicSkinPartEuipId.Add((VenturerModel.EVenturerSkinPart)skinPartId, skinPropId);
            m_DicSkinPartEuipIdNone.Add((VenturerModel.EVenturerSkinPart)skinPartId, 0);
        }

        m_CharacterBase.SetVenturerInfo(venturerInfo);
    }

    //刷新 角色预览 Spine实例
    private void RefreshRolePreviewSpine()
    {
        if (m_CharacterBase == null) { return; }

        m_CharacterBase.RefreshSpineInstance();
    }
    #endregion
}
