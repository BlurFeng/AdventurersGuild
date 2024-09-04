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
using DG.Tweening;
using FsListItemPages;

public class NotebookWindow : WindowBase
{
    /// <summary>
    /// 标签页类型
    /// </summary>
    private enum ELabelType
    {
        None = 0,
        /// <summary>
        /// 计划
        /// </summary>
        Plan,
        /// <summary>
        /// 故事
        /// </summary>
        Story,
        /// <summary>
        /// 图鉴
        /// </summary>
        Wiki,
        /// <summary>
        /// 成就
        /// </summary>
        Achievement,
        /// <summary>
        /// 帮助
        /// </summary>
        Help,
    }

    [SerializeField] private GameObject m_BtnClose = null; //按钮 关闭
    [SerializeField] private GameObject m_BtnLabelPlan = null; //按钮 计划
    [SerializeField] private GameObject m_BtnLabelStory = null; //按钮 故事
    [SerializeField] private GameObject m_BtnLabelWiki = null; //按钮 图鉴
    [SerializeField] private GameObject m_BtnLabelAchievement = null; //按钮 成就
    [SerializeField] private GameObject m_BtnLabelHelp = null; //按钮 帮助
    [SerializeField] private List<Toggle> m_ListTogLabel = null; //列表 开关 标签按钮
    [SerializeField] private List<GameObject> m_ListGoLabelPanel = null; //列表 物体 标签面板

    private ELabelType m_ELabelTypeCur; //标签类型 当前

    public override void OnLoaded()
    {
        base.OnLoaded();

        ClickListener.Get(m_BtnClose).SetClickHandler(BtnClose);
        ClickListener.Get(m_BtnLabelPlan).SetClickHandler(BtnLabelPlan);
        ClickListener.Get(m_BtnLabelStory).SetClickHandler(BtnLabelStory);
        ClickListener.Get(m_BtnLabelWiki).SetClickHandler(BtnLabelWiki);
        ClickListener.Get(m_BtnLabelAchievement).SetClickHandler(BtnLabelAchievement);
        ClickListener.Get(m_BtnLabelHelp).SetClickHandler(BtnLabelHelp);

        InitHelp();
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //默认标签类型 计划
        SwitchLabelType(ELabelType.Plan);
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }

    #region 按钮

    //按钮 关闭
    private void BtnClose(PointerEventData eventData)
    {
        CloseWindow();
    }

    //按钮 计划
    private void BtnLabelPlan(PointerEventData obj)
    {
        SwitchLabelType(ELabelType.Plan);
    }

    //按钮 故事
    private void BtnLabelStory(PointerEventData obj)
    {
        SwitchLabelType(ELabelType.Story);
    }

    //按钮 图鉴
    private void BtnLabelWiki(PointerEventData obj)
    {
        SwitchLabelType(ELabelType.Wiki);
    }

    //按钮 成就
    private void BtnLabelAchievement(PointerEventData obj)
    {
        SwitchLabelType(ELabelType.Achievement);
    }

    //按钮 帮助
    private void BtnLabelHelp(PointerEventData obj)
    {
        SwitchLabelType(ELabelType.Help);
    }

    #endregion

    //切换 标签类型
    private void SwitchLabelType(ELabelType labelType)
    {
        if (m_ELabelTypeCur == labelType) { return; }
        m_ELabelTypeCur = labelType;

        //选中 标签按钮
        var index = (int)m_ELabelTypeCur - 1;
        index = index < 0 ? 0 : index;
        m_ListTogLabel[index].isOn = true;

        //打开 选中的标签面板
        for (int eLabelType = 1; eLabelType <= m_ListGoLabelPanel.Count; eLabelType++)
        {
            var panelGo = m_ListGoLabelPanel[eLabelType - 1];
            panelGo.SetActive((int)m_ELabelTypeCur == eLabelType);
        }
    }

    #region 帮助面板

    [Header("帮助")]
    [SerializeField] private TMP_Dropdown m_DropdHelpType = null; //下拉窗 帮助类型
    [SerializeField] private ListItemPagesComponent m_ListItemPagesHelp = null; //翻页项目列表 种族
    [SerializeField] private GameObject m_BtnHelpInfoLastStrip = null; //按钮 帮助信息 上一条
    [SerializeField] private GameObject m_BtnHelpInfoNextStrip = null; //按钮 帮助信息 下一条
    [SerializeField] private Image m_ImgHelpInfo; //图片 帮助信息
    [SerializeField] private TextMeshProUGUI m_TxtHelpInfoTitle; //文本 帮助信息 标题
    [SerializeField] private TextMeshProUGUI m_TxtHelpInfoContent; //文本 帮助信息 内容

    private HelpModel.EHelpType m_EHelpTypeCur; //当前选中的 帮助类型
    private Notebook_Help m_CfgNotebookHelpCur; //配置表 帮助信息 当前
    private int m_HelpInfoStripInexCur; //帮助信息 第几条 当前

    //初始化 帮助
    private void InitHelp()
    {
        ClickListener.Get(m_BtnHelpInfoLastStrip).SetClickHandler(BtnHelpInfoLastStrip);
        ClickListener.Get(m_BtnHelpInfoNextStrip).SetClickHandler(BtnHelpInfoNextStrip);

        m_DropdHelpType.onValueChanged.AddListener(OnDropdHelpTypeValueChanged);
        m_ListItemPagesHelp.OnSelectItemChange = OnSelectItemChange;

        //默认选中 基础
        SwitchHelpType(HelpModel.EHelpType.Basic);
    }

    //按钮 上一条
    private void BtnHelpInfoLastStrip(PointerEventData obj)
    {
        SetHelpInfoStripIndex(m_HelpInfoStripInexCur - 1);
    }

    //按钮 下一条
    private void BtnHelpInfoNextStrip(PointerEventData obj)
    {
        SetHelpInfoStripIndex(m_HelpInfoStripInexCur + 1);
    }

    //选中的项目改表
    private void OnSelectItemChange(IItemPagesData data)
    {
        var cfg = ConfigSystem.Instance.GetConfig<Notebook_Help>(data.GetId());
        if (cfg == null) { return; }
        m_CfgNotebookHelpCur = cfg;

        //设置 帮助信息 标题
        m_TxtHelpInfoTitle.text = cfg.Title;
        //默认选中 帮助信息 第一条
        SetHelpInfoStripIndex(0);
    }

    //下拉窗数据改变 帮助类型
    private void OnDropdHelpTypeValueChanged(int index)
    {
        SwitchHelpType((HelpModel.EHelpType)(index + 1));
    }

    //切换 帮助类型
    private void SwitchHelpType(HelpModel.EHelpType helpType)
    {
        if (m_EHelpTypeCur == helpType) { return; }
        m_EHelpTypeCur = helpType;

        var listCfgId = HelpModel.Instance.GetListHelpCfgId(m_EHelpTypeCur);
        m_ListItemPagesHelp.Init(listCfgId);
    }

    //设置 帮助信息 第几条
    private void SetHelpInfoStripIndex(int index)
    {
        if (index < 0 || index >= m_CfgNotebookHelpCur.Content.Count) { return; }
        m_HelpInfoStripInexCur = index;

        //按钮显示状态
        m_BtnHelpInfoLastStrip.SetActive(m_HelpInfoStripInexCur > 0);
        m_BtnHelpInfoNextStrip.SetActive(m_HelpInfoStripInexCur < m_CfgNotebookHelpCur.Content.Count - 1);

        //帮助信息 文本内容
        m_TxtHelpInfoContent.text = m_CfgNotebookHelpCur.Content[index];
        //帮助信息 图片
        string imgName = string.Empty;
        if (index < m_CfgNotebookHelpCur.Image.Count)
        {
            imgName = m_CfgNotebookHelpCur.Image[index];
        }
        if (string.IsNullOrEmpty(imgName))
        {
            imgName = "None_Default";
        }
        AssetIconSystem.Instance.SetIcon(m_ImgHelpInfo, "HelpInfo", imgName);
    }

    #endregion
}