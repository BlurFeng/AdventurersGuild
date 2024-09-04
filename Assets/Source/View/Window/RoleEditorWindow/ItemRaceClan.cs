using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using com.ootii.Messages;
using System;
using FsListItemPages;

public class ItemRaceClan : ListItemPagesItemBase
{
    //[SerializeField] private Image m_ImgHead = null; //图片 头像
    [SerializeField] private TextMeshProUGUI m_TxtClanName = null; //文本 族名称
    [SerializeField] private TextMeshProUGUI m_TxtVarietyName = null; //文本 种名称
    [SerializeField] private TextMeshProUGUI m_TxtClassName = null; //文本 类名称

    private Venturer_RaceClan m_CfgRaceClan;

    public override void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
    {
        base.SetInfo(data, pageIndex, stripIndex);

        var cfg = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(data.GetId());
        if (cfg == null) { return; }

        //记录 配置表
        m_CfgRaceClan = cfg;

        //族名称
        m_TxtClanName.text = m_CfgRaceClan.Name;
        //类名称
        string className = string.Empty;
        var cfgClass = ConfigSystem.Instance.GetConfig<Venturer_RaceClass>(m_CfgRaceClan.Class);
        if (cfgClass != null)
        {
            className = cfgClass.Name;
        }
        m_TxtClassName.text = className;
        //种名称
        string varietyName = string.Empty;
        var cfgVariety = ConfigSystem.Instance.GetConfig<Venturer_RaceVariety>(m_CfgRaceClan.Variety);
        if (cfgVariety != null)
        {
            varietyName = cfgVariety.Name;
        }
        m_TxtVarietyName.text = varietyName;
    }
}
