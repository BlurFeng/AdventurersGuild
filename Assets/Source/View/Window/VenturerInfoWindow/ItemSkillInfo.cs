using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using UnityEngine.EventSystems;

public class ItemSkillInfo : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtRank = null; //文本 阶级
    [SerializeField] private Image m_ImgSkillPointProgress = null; //图片 技能点进度

    private Skill_Group m_Config; //配置表

    private void Awake()
    {
        ClickListener.Get(m_BtnClick).SetPointerEnterHandler(BtnEnter);
        ClickListener.Get(m_BtnClick).SetPointerExitHandler(BtnExit);
    }

    /// <summary>
    /// 设置 信息
    /// </summary>
    /// <param name="skillGroupId">技能组ID</param>
    /// <param name="rankId">阶级</param>
    /// <param name="progress">当前进度</param>
    public void SetInfo(int skillGroupId, int rankId, float progress)
    { 
        //技能组名称
        m_Config = ConfigSystem.Instance.GetConfig<Skill_Group>(skillGroupId);
        m_TxtName.text = m_Config.Name;
        //阶级
        var cfgRank = ConfigSystem.Instance.GetConfig<Common_Rank>(rankId);
        m_TxtRank.text = cfgRank.Name;
        //技能点进度
        m_ImgSkillPointProgress.fillAmount = progress;
    }

    //按钮 鼠标进入
    private void BtnEnter(PointerEventData obj)
    {
        var arg = new InfoWindow.InfoWindowArg();
        arg.Type = InfoWindow.EInfoType.Skill;
        arg.Id = m_Config.Id;
        WindowSystem.Instance.OpenWindow(WindowEnum.InfoWindow, arg);
    }

    //按钮 鼠标离开
    private void BtnExit(PointerEventData obj)
    {
        WindowSystem.Instance.CloseWindow(WindowEnum.InfoWindow);
    }

    #region 状态
    [Header("状态")]
    [SerializeField] private GameObject m_AreaStateNone = null; //物体 状态 不可用槽
    [SerializeField] private GameObject m_AreaStateEmpty = null; //按钮 状态 空槽
    [SerializeField] private GameObject m_AreaStateExist = null; //按钮 状态 存在技能
    public enum EState
    {
        /// <summary>
        /// 不可用槽
        /// </summary>
        None,
        /// <summary>
        /// 空槽
        /// </summary>
        Empty,
        /// <summary>
        /// 存在技能
        /// </summary>
        Exist,
    }

    /// <summary>
    /// 设置 状态
    /// </summary>
    /// <param name="state"></param>
    public void SetState(EState state)
    {
        m_AreaStateNone.SetActive(state == EState.None);
        m_AreaStateEmpty.SetActive(state == EState.Empty);
        m_AreaStateExist.SetActive(state == EState.Exist);
    }
    #endregion
}
