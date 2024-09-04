using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsListItemPages;

public class ItemAreaInfo : ListItemPagesItemBase
{
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 帮助名称

    private Building_Area m_CfgBuildingArea;

    public override void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
    {
        base.SetInfo(data, pageIndex, stripIndex);

        var cfg = ConfigSystem.Instance.GetConfig<Building_Area>(data.GetId());
        if (cfg == null) return;

        m_CfgBuildingArea = cfg;

        //设置显示信息
        m_TxtName.text = m_CfgBuildingArea.Name;
    } 
}
