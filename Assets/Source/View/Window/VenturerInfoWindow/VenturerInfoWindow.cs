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

public class VenturerInfoWindow : WindowBase
{
    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭

    public override void OnLoaded()
    {
        base.OnLoaded();

        OnLoadedBaseInfo(); //初始化 基础信息
        OnLoadedInfoPanel(); //初始化 信息面板
        OnLoadedAttrPanel(); //初始化 属性面板
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //传参解析
        var venturerInfo = userData as VenturerInfo;
        SetVenturerInfo(venturerInfo);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    //按钮 取消
    private void BtnClose(PointerEventData obj)
    {
        CloseWindow();
    }

    #region 基础信息
    [Header("基础信息")]
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtRaceClan = null; //文本 种族
    [SerializeField] private TextMeshProUGUI m_TxtGender = null; //文本 性别
    [SerializeField] private TextMeshProUGUI m_TxtAge = null; //文本 年龄
    [SerializeField] private TextMeshProUGUI m_TxtAlignment = null; //文本 阵营
    [SerializeField] private TextMeshProUGUI m_TxtLevel = null; //文本 等级
    [SerializeField] private TextMeshProUGUI m_TxtExp = null; //文本 经验值
    //[SerializeField] private Image m_ImgExpProgress = null; //图片 经验值 进度

    //初始化 基础信息
    private void OnLoadedBaseInfo()
    {
        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
    }

    //设置 冒险者信息
    private void SetVenturerInfo(VenturerInfo venturerInfo)
    {
        //姓名
        m_TxtName.text = venturerInfo.FullName;
        //种族
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(venturerInfo.RaceClanId);
        m_TxtRaceClan.text = cfgRaceClan.Name;
        //性别
        var cfgGender = ConfigSystem.Instance.GetConfig<Venturer_Gender>(venturerInfo.GenderId);
        m_TxtGender.text = cfgGender.Name;
        //年龄
        var timeInfo = TimeModel.Instance.GetTimeInfo(venturerInfo.LifeTurnCur);
        m_TxtAge.text = timeInfo.GameTimeEraYear.ToString();
        //阵营
        var cfgAlignment = ConfigSystem.Instance.GetConfig<Venturer_Alignment>(venturerInfo.AlignmentId);
        m_TxtAlignment.text = cfgAlignment.Name;
        //等级
        m_TxtLevel.text = venturerInfo.Level.ToString();
        //经验值
        m_TxtExp.text = venturerInfo.Exp.ToString();
        //m_ImgExpProgress.fillAmount = venturerInfo.GetExpProgress(); //经验值 进度

        SetAttrPanelInfo(venturerInfo); //设置 属性面板
        SetSkillPanelInfo(venturerInfo); //设置 技能面板
        SetProfessionPanelInfo(venturerInfo); //设置 职业面板
    }
    #endregion

    #region 装备

    //[Header("装备")]

    #endregion

    #region 信息面板
    [Header("信息面板")]
    [SerializeField] private GameObject m_BtnTogAttr = null; //按钮 属性
    [SerializeField] private GameObject m_BtnTogSkill = null; //按钮 技能
    [SerializeField] private GameObject m_BtnTogProfession = null; //按钮 职业
    [SerializeField] private GameObject m_BtnTogState = null; //按钮 状态
    [SerializeField] private GameObject[] m_ArrayInfoPanel = null; //面板组 信息面板

    private EInfoPanelType m_EInfoPanelTypeCur = EInfoPanelType.None; //信息面板类型 当前

    /// <summary>
    /// 信息面板 类型
    /// </summary>
    private enum EInfoPanelType
    {
        None = 0,
        /// <summary>
        /// 属性
        /// </summary>
        Attr,
        /// <summary>
        /// 技能
        /// </summary>
        Skill,
        /// <summary>
        /// 职业
        /// </summary>
        Profession,
        /// <summary>
        /// 状态
        /// </summary>
        State
    }

    private void OnLoadedInfoPanel()
    {
        ClickListener.Get(m_BtnTogAttr).SetClickHandler(BtnTogAttr);
        ClickListener.Get(m_BtnTogSkill).SetClickHandler(BtnTogSkill);
        ClickListener.Get(m_BtnTogProfession).SetClickHandler(BtnTogProfession);
        ClickListener.Get(m_BtnTogState).SetClickHandler(BtnTogState);

        //默认选中 属性面板
        for (int i = 0; i < m_ArrayInfoPanel.Length; i++)
        {
            m_ArrayInfoPanel[i].SetActive(false);
        }
        SetInfoPanelType(EInfoPanelType.Attr);
    }

    //按钮 属性面板
    private void BtnTogAttr(PointerEventData obj)
    {
        SetInfoPanelType(EInfoPanelType.Attr);
    }

    //按钮 技能面板
    private void BtnTogSkill(PointerEventData obj)
    {
        SetInfoPanelType(EInfoPanelType.Skill);
    }

    //按钮 职业面板
    private void BtnTogProfession(PointerEventData obj)
    {
        SetInfoPanelType(EInfoPanelType.Profession);
    }

    //按钮 状态面板
    private void BtnTogState(PointerEventData obj)
    {
        SetInfoPanelType(EInfoPanelType.State);
    }

    //设置 信息面板 类型
    private void SetInfoPanelType(EInfoPanelType type)
    {
        if (m_EInfoPanelTypeCur == type) { return; }

        //隐藏 当前面板
        if (m_EInfoPanelTypeCur != EInfoPanelType.None)
        {
            int panelCur = (int)m_EInfoPanelTypeCur - 1;
            m_ArrayInfoPanel[panelCur].SetActive(false);
        }

        //显示 新面板
        m_EInfoPanelTypeCur = type;
        int panelNew = (int)m_EInfoPanelTypeCur - 1;
        m_ArrayInfoPanel[panelNew].SetActive(true);
    }

    #region 属性面板
    [Header("属性面板")]
    [SerializeField] private GameObject m_BtnTogAttrBasic = null; //按钮 基础属性
    [SerializeField] private GameObject m_BtnTogAttrAppetency = null; //按钮 魔素亲和属性
    [SerializeField] private GameObject m_AreaAttrBasic = null; //区域 基础属性
    [SerializeField] private ItemAttrValue[] m_ArrayItemAttrValue1 = null; //列表 基础属性1
    [SerializeField] private ItemAttrValue[] m_ArrayItemAttrValue2 = null; //列表 基础属性2
    [SerializeField] private GameObject m_AreaAttrAppetency = null; //区域 魔素亲和属性
    [SerializeField] private ItemAttrValue[] m_ArrayItemAttrAppetency = null; //列表 魔素亲和属性

    private EPanelAttrType m_EPanelAttrTypeCur = EPanelAttrType.None; //信息面板类型 当前

    /// <summary>
    /// 属性面板 类型
    /// </summary>
    private enum EPanelAttrType
    {
        None = 0,
        /// <summary>
        /// 基础
        /// </summary>
        Basic,
        /// <summary>
        /// 魔素亲和
        /// </summary>
        Appetency,
    }

    private void OnLoadedAttrPanel()
    {
        ClickListener.Get(m_BtnTogAttrBasic).SetClickHandler(BtnTogAttrBasic);
        ClickListener.Get(m_BtnTogAttrAppetency).SetClickHandler(BtnTogAttrAppetency);

        SetPanelAttrType(EPanelAttrType.Basic);
    }

    //按钮 基础属性
    private void BtnTogAttrBasic(PointerEventData obj)
    {
        SetPanelAttrType(EPanelAttrType.Basic);
    }

    //按钮 魔素亲和属性
    private void BtnTogAttrAppetency(PointerEventData obj)
    {
        SetPanelAttrType(EPanelAttrType.Appetency);
    }

    //设置 属性面板 显示的属性类型
    private void SetPanelAttrType(EPanelAttrType type)
    {
        if (m_EPanelAttrTypeCur == type) { return; }
        m_EPanelAttrTypeCur = type;

        m_AreaAttrBasic.SetActive(m_EPanelAttrTypeCur == EPanelAttrType.Basic);
        m_AreaAttrAppetency.SetActive(m_EPanelAttrTypeCur == EPanelAttrType.Appetency);
    }

    //设置 属性面板 冒险者信息
    private void SetAttrPanelInfo(VenturerInfo venturerInfo)
    {
        //基础属性1
        for (int i = 0; i < 5; i++)
        {
            if (i > m_ArrayItemAttrValue1.Length) break;

            var attrId = 1001 + i;
            var item = m_ArrayItemAttrValue1[i];
            item.SetInfo(attrId);

            //冒险者属性值
            var value = venturerInfo.GetAttributeValue(attrId);
            item.SetValue(value);
        }
        //基础属性2
        for (int i = 0; i < 2; i++)
        {
            if (i > m_ArrayItemAttrValue2.Length) break;

            var attrId = 2001 + i;
            var item = m_ArrayItemAttrValue2[i];
            item.SetInfo(attrId);

            var value = venturerInfo.GetAttributeValue(attrId);
            item.SetValue(value);
        }

        //魔素亲和属性
        for (int i = 0; i < 8; i++)
        {
            if (i > m_ArrayItemAttrAppetency.Length) break;

            var attrId = 3001 + i;
            var item = m_ArrayItemAttrAppetency[i];
            item.SetInfo(attrId);

            var value = venturerInfo.GetAttributeValue(attrId);
            item.SetValue(value);
        }
    }
    #endregion

    #region 技能面板
    [Header("技能面板")]
    [SerializeField] private ItemSkillInfo[] m_ArrayItemSkillInfo = null; //列表 技能信息项目

    //设置 技能面板 冒险者信息
    private void SetSkillPanelInfo(VenturerInfo venturerInfo)
    {
        //获取所有技能组ID
        var skillGroupIds = venturerInfo.GetAllSkillGroupId();
        
        //设置 技能组信息
        for (int i = 0; i < skillGroupIds.Length; i++)
        {
            if (i >= m_ArrayItemSkillInfo.Length) break;

            var item = m_ArrayItemSkillInfo[i];
            var skillGroupId = skillGroupIds[i];
            var rank = venturerInfo.GetSkillGroupRank(skillGroupId);
            var progress = venturerInfo.GetSkillGroupPointCurRankProgress(skillGroupId);
            item.SetState(ItemSkillInfo.EState.Exist);
            item.SetInfo(skillGroupId, rank, progress);
        }

        //多余Item 设置为不可用
        for (int i = skillGroupIds.Length; i < m_ArrayItemSkillInfo.Length; i++)
        {
            var item = m_ArrayItemSkillInfo[i];
            item.SetState(ItemSkillInfo.EState.None);
        }
    }
    #endregion

    #region 职业面板
    [Header("职业面板")]
    [SerializeField] private ItemProfessionInfo[] m_ArrayItemProfessionInfo = null; //列表 职业信息项目

    //设置 技能面板 冒险者信息
    private void SetProfessionPanelInfo(VenturerInfo venturerInfo)
    {
        //设置 所有职业信息
        for (int i = 0; i < venturerInfo.ProfessionIds.Count; i++)
        {
            if (i >= m_ArrayItemProfessionInfo.Length) break;

            var item = m_ArrayItemProfessionInfo[i];
            var professionId = venturerInfo.ProfessionIds[i];
            item.SetInfo(professionId);
            item.gameObject.SetActive(true);
        }

        //多余Item 设置为不可用
        for (int i = venturerInfo.ProfessionIds.Count; i < m_ArrayItemProfessionInfo.Length; i++)
        {
            var item = m_ArrayItemProfessionInfo[i];
            item.gameObject.SetActive(false);
        }
    }
    #endregion
    #endregion
}
