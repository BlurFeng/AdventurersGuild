using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsListItemPages;

public class ItemNotebookHelpInfo : ListItemPagesItemBase
{
    [SerializeField] private TextMeshProUGUI m_TxtHelpName = null; //文本 帮助名称

    private Notebook_Help m_CfgNotebook_Help;

    public override void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
    {
        base.SetInfo(data, pageIndex, stripIndex);

        var cfg = ConfigSystem.Instance.GetConfig<Notebook_Help>(data.GetId());
        if (cfg == null) return;

        m_CfgNotebook_Help = cfg;

        //设置显示信息
        m_TxtHelpName.text = m_CfgNotebook_Help.Title;
    } 
}
