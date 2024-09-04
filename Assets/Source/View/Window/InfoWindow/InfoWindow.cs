using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FsGameFramework;
using TMPro;
using com.ootii.Messages;
using DG.Tweening;
using Deploy;
using System;

public class InfoWindow : WindowBase
{
    [Header("主界面")]
    [SerializeField] private RectTransform m_RectTrans = null;
    [SerializeField] private TextMeshProUGUI m_TxtType = null; //文本 类型

    public class InfoWindowArg
    {
        public EInfoType Type = EInfoType.None;
        public int Id;
        public bool customAnchorPos = false;
        public Vector3 AnchorPos;
    }

    public override void OnLoaded()
    {
        base.OnLoaded();

        MemoryLock = true;

        OnLoadedEntry();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //参数解析
        InfoWindowArg arg = (InfoWindowArg)userData;
        if (arg == null)
        {
            CloseWindow();
            return;
        }

        CloseAllGoEntry();

        //设置 类型对应信息
        switch (arg.Type)
        {
            case EInfoType.Venturer:
                SetInfoHero(arg);
                break;
            case EInfoType.Attribute:
                SetInfoAttribute(arg);
                break;
            case EInfoType.Skill:
                SetInfoSkill(arg);
                break;
            case EInfoType.Profession:
                SetInfoProfession(arg);
                break;
        }

        //自适应弹窗位置
        StartCoroutine(SetInfoWindowPosition(arg.AnchorPos, arg.customAnchorPos));
    }

    public void OnDisable()
    {
        //返还 旧的itemProp实例
        while (m_RootItemPropInfo.childCount > 0)
        {
            var item = m_RootItemPropInfo.GetChild(0);
            AssetTemplateSystem.Instance.ReturnTemplatePrefab(item.gameObject);
        }
    }

    //设置 信息弹窗位置
    private IEnumerator SetInfoWindowPosition(Vector3 anchorPos, bool customAnchorPos = false)
    {
        m_RectTrans.anchoredPosition = new Vector2(-1000f, -1000f);

        if (!customAnchorPos)
        {
            anchorPos = Input.mousePosition / CameraModel.Instance.ScaleRatio;
            anchorPos += new Vector3(20f, 50f, 0f);
        }

        yield return null;

        float screenW = Screen.width;
        float screenH = Screen.height;
        float posX = anchorPos.x;
        float posXamend = posX + m_RectTrans.rect.width;
        if (posXamend > screenW)
        {
            posX -= posXamend - screenW;
        }
        float posY = anchorPos.y;
        float posYamend = posY - m_RectTrans.rect.height;
        if (posYamend < 0)
        {
            posY -= posYamend;
        }
        else if (posY > screenH)
        {
            posY = screenH;
        }
        m_RectTrans.anchoredPosition = new Vector2(posX, posY);
    }

    #region 条目
    /// <summary>
    /// 条目类型
    /// </summary>
    public enum EEntryType
    {
        None,
        /// <summary>
        /// 描述
        /// </summary>
        Desc,
        /// <summary>
        /// 钱币
        /// </summary>
        Coin,
        /// <summary>
        /// 道具列表
        /// </summary>
        PropList,
        /// <summary>
        /// 道具信息
        /// </summary>
        PropInfo,
        /// <summary>
        /// 冒险者信息
        /// </summary>
        Venturer,
        /// <summary>
        /// 属性信息
        /// </summary>
        Attribute,
        /// <summary>
        /// 技能信息
        /// </summary>
        Skill,
        /// <summary>
        /// 职业
        /// </summary>
        Profession,
    }

    private Dictionary<EEntryType, GameObject> m_GoEntryDic = new Dictionary<EEntryType, GameObject>();
    private List<EEntryType> m_OpenGoEntryList = new List<EEntryType>();

    //初始化 条目
    private void OnLoadedEntry()
    {
        AddGoEntry(EEntryType.Desc, m_GoEntryDesc);
        AddGoEntry(EEntryType.Coin, m_GoEntryCoint);
        AddGoEntry(EEntryType.PropList, m_GoEntryPropList);
        AddGoEntry(EEntryType.PropInfo, m_GoEntryPropInfo);
        AddGoEntry(EEntryType.Venturer, m_GoEntryHero);
        AddGoEntry(EEntryType.Skill, m_GoEntrySkill);
    }

    //添加一个词条到字典
    private void AddGoEntry(EEntryType type, GameObject obj)
    {
        m_GoEntryDic.Add(type, obj);
        obj.SetActive(false);
    }

    //打开词条GameObject
    private void OpenGoEntry(EEntryType type, bool newOpen)
    {
        if (!m_GoEntryDic.ContainsKey(type)) return;

        var obj = m_GoEntryDic[type];
        if (obj.activeSelf != newOpen)
        {
            obj.SetActive(newOpen);
            if (newOpen)
                m_OpenGoEntryList.Add(type);

            else
                m_OpenGoEntryList.Remove(type);
        }
    }

    //关闭所有已经打开的词条GameObject
    private void CloseAllGoEntry()
    {
        for (int i = 0; i < m_OpenGoEntryList.Count; i++)
        {
            m_GoEntryDic[m_OpenGoEntryList[i]].SetActive(false);
        }

        m_OpenGoEntryList.Clear();
    }

    #region 名称
    [Header("名称")]
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称

    //设置条目 名称
    private void SetEntryName(string name)
    {
        m_TxtName.text = name;
    }
    #endregion

    #region 描述
    [Header("描述")]
    [SerializeField] private GameObject m_GoEntryDesc = null; //物体 条目 描述
    [SerializeField] private TextMeshProUGUI m_TxtDesc = null; //文本 描述

    //设置条目 描述
    private void SetEntryDesc(string desc)
    {
        OpenGoEntry(EEntryType.Desc, true);

        m_TxtDesc.text = desc;
    }
    #endregion

    #region 钱币
    [Header("钱币")]
    [SerializeField] private GameObject m_GoEntryCoint = null; //物体 条目 描述
    [SerializeField] private TextMeshProUGUI m_TxtCoinTitle = null; //文本 标题
    [SerializeField] private ItemPropCoin m_ItemPropCoin = null; //项目 钱币

    //设置条目 钱币
    private void SetEntryCoin(int count, string title = null)
    {
        OpenGoEntry(EEntryType.Coin, true);

        m_TxtCoinTitle.text = title == null ? string.Empty : title;
        m_ItemPropCoin.SetCoinCount(count);
    }
    #endregion

    #region 道具列表
    [Header("道具列表")]
    [SerializeField] private GameObject m_GoEntryPropList = null; //物体 条目 道具列表
    [SerializeField] private TextMeshProUGUI m_TxtPropListTitle = null; //文本 道具列表 标题
    [SerializeField] private Transform m_RootItemPropInfo = null; //根节点 道具信息

    //设置条目 道具列表
    private void SetEntryPropList(string title, Dictionary<int, int> propDic, bool showName = false)
    {
        OpenGoEntry(EEntryType.PropList, true);

        m_TxtPropListTitle.text = title;
        //克隆 itemProp实例
        foreach (var kv in propDic)
        {
            int id = kv.Key;
            int count = kv.Value;
            AssetTemplateSystem.Instance.CloneItemPropInfo(id, m_RootItemPropInfo, count, showName);
        }
    }
    #endregion

    #region 道具信息
    [Header("道具信息")]
    [SerializeField] private GameObject m_GoEntryPropInfo = null; //物体 条目 道具信息
    #endregion

    #region 冒险者
    [Header("冒险者")]
    [SerializeField] private GameObject m_GoEntryHero = null; //物体 条目 英雄
    [SerializeField] private TextMeshProUGUI m_TxtProfession = null; //文本 职业
    [SerializeField] private TextMeshProUGUI m_TxtLevel = null; //文本 等级
    [SerializeField] private TextMeshProUGUI m_TxtExp = null; //文本 经验值
    [SerializeField] private TextMeshProUGUI m_TxtExpReq = null; //文本 经验值 升级所需
    [SerializeField] private TextMeshProUGUI m_TxtBirthDate = null; //文本 出生日期
    [SerializeField] private TextMeshProUGUI m_TxtAge = null; //文本 年龄
    [SerializeField] private TextMeshProUGUI m_TxtState = null; //文本 状态

    //设置条目 冒险者
    private void SetEntryVenturer(VenturerInfo venturerInfo)
    {
        OpenGoEntry(EEntryType.Venturer, true);

        string txtProfessions = string.Empty;
        for (int i = 0; i < venturerInfo.ProfessionIds.Count; i++)
        {
            var cfgProfession = ConfigSystem.Instance.GetConfig<Profession_Config>(venturerInfo.ProfessionIds[i]);
            txtProfessions += $"{cfgProfession.Name} ";
        }
        m_TxtProfession.text = txtProfessions; //文本 职业

        m_TxtLevel.text = venturerInfo.Level.ToString(); //文本 等级

        //经验值
        m_TxtExp.text = venturerInfo.Exp.ToString(); //文本 经验值
        string expReq = "-";
        var cfgLevelNext = ConfigSystem.Instance.GetConfig<Venturer_Level>(venturerInfo.Level + 1);
        if (cfgLevelNext != null)
        {
            expReq = (cfgLevelNext.ExpRequire - venturerInfo.Exp).ToString();
        }
        m_TxtExpReq.text = expReq;

        //出生日期
        var timeInfo = venturerInfo.BirthDate;
        m_TxtBirthDate.text = $"{timeInfo.GameTimeEraYear}年 {timeInfo.GameTimeMonth}月 {timeInfo.GameTimeDay}日";

        m_TxtAge.text = ((int)(venturerInfo.LifeTurnCur * 0.25f)).ToString(); //文本 年龄
        m_TxtState.text = venturerInfo.GetHeroStateText();
    }
    #endregion

    #region 技能
    [Header("技能")]
    [SerializeField] private GameObject m_GoEntrySkill = null; //物体 条目 技能
    [SerializeField] private TextMeshProUGUI m_TxtSkillFaction = null; //文本 派系
    [SerializeField] private TextMeshProUGUI m_TxtSkillType = null; //文本 类型

    //设置条目 技能
    private void SetEntrySkill(Skill_Group skillCfg)
    {
        OpenGoEntry(EEntryType.Skill, true);

        //技能派系
        var cfgFaction = ConfigSystem.Instance.GetConfig<Skill_Faction>(skillCfg.Faction);
        m_TxtSkillFaction.text = cfgFaction.Name;
        //技能类型
        var cfgType = ConfigSystem.Instance.GetConfig<Skill_Type>(skillCfg.Type);
        m_TxtSkillType.text = cfgType.Name;
    }
    #endregion
    #endregion

    #region 设置指定类型信息
    /// <summary>
    /// 信息类型
    /// </summary>
    public enum EInfoType
    {
        None,
        /// <summary>
        /// 道具
        /// </summary>
        Prop,
        /// <summary>
        /// 冒险者
        /// </summary>
        Venturer,
        /// <summary>
        /// 属性
        /// </summary>
        Attribute,
        /// <summary>
        /// 技能
        /// </summary>
        Skill,
        /// <summary>
        /// 职业
        /// </summary>
        Profession,
    }

    //设置信息 冒险者
    private void SetInfoHero(InfoWindowArg arg)
    {
        m_TxtType.text = "冒险者";

        var heroInfo = VenturerModel.Instance.GetVenturerInfo(arg.Id);

        SetEntryName(heroInfo.FullName);
        SetEntryVenturer(heroInfo);
    }

    //设置信息 属性
    private void SetInfoAttribute(InfoWindowArg arg)
    {
        m_TxtType.text = "属性";

        var cfg = ConfigSystem.Instance.GetConfig<Common_Attribute>(arg.Id);

        SetEntryName(cfg.Name);
        SetEntryDesc(cfg.Desc);
    }

    //设置信息 技能
    private void SetInfoSkill(InfoWindowArg arg)
    {
        m_TxtType.text = "技能";

        var cfg = ConfigSystem.Instance.GetConfig<Skill_Group>(arg.Id);

        SetEntryName(cfg.Name);
        SetEntryDesc(cfg.Desc);
        SetEntrySkill(cfg);
    }

    //设置信息 职业
    private void SetInfoProfession(InfoWindowArg arg)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Profession_Config>(arg.Id);

        SetEntryName(cfg.Name);
        SetEntryDesc(cfg.Desc);
    }
    #endregion
}