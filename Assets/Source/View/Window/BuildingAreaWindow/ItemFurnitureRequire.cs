using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsListItemPages;
using com.ootii.Messages;

public class ItemFurnitureRequire : ListItemPagesItemBase
{
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 帮助名称
    [SerializeField] private TextMeshProUGUI m_TxtCount = null; //文本 数量
    [SerializeField] private TextMeshProUGUI m_TxtCountCur = null; //文本 当前区域的数量

    private Prop_FurnitureType m_CfgPropFurnitureType;
    private int CountRequire; //需求的 家具数量
    private Color m_ColorCountCurDefault; //颜色 当前数量 默认
    private Color m_ColorCountCurLess = Color.red; //颜色 当前数量 不足

    protected override void Awake()
    {
        base.Awake();

        MessageDispatcher.AddListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_INTRAGRIDITEM_CHANGE, MsgGuildGridAreaIntraGridItemChange); //当前区域 值 改变

        if (m_TxtCountCur != null)
            m_ColorCountCurDefault = m_TxtCountCur.color;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        MessageDispatcher.RemoveListener(GuildModelMsgType.GUILDGRIDMODEL_AREAITEMCUR_INTRAGRIDITEM_CHANGE, MsgGuildGridAreaIntraGridItemChange); //当前区域 值 改变
    }

    public override void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
    {
        base.SetInfo(data, pageIndex, stripIndex);

        var cfg = ConfigSystem.Instance.GetConfig<Prop_FurnitureType>(data.GetId());
        if (cfg == null) return;

        m_CfgPropFurnitureType = cfg;

        //设置显示信息
        m_TxtName.text = m_CfgPropFurnitureType.Name; //家具名称
        string countRequireStr = data.GetCustomString();
        m_TxtCount.text = countRequireStr; //设置 需要的数量
        CountRequire = int.Parse(countRequireStr);
        //刷新 当前区域的该类型家具数量
        RefreshAreaCountCur();
    }

    protected override void BtnClick(PointerEventData eventData)
    {
        //弹窗 家具详情
    }

    //刷新 当前区域的该类型家具数量
    private void RefreshAreaCountCur()
    {
        if (m_TxtCountCur == null) { return; }

        int countCur = 0;
        if (GuildGridModel.Instance.PlayerAreaInfoCur != null)
            countCur = GuildGridModel.Instance.PlayerAreaInfoCur.GetIntraGridItemListCount(m_CfgPropFurnitureType.Id);

        //当前数量
        m_TxtCountCur.text = countCur.ToString();
        //显示的颜色
        m_TxtCountCur.color = countCur >= CountRequire ? m_ColorCountCurDefault : m_ColorCountCurLess;
    }

    //消息 当前网格区域 内部网格项目 改变
    private void MsgGuildGridAreaIntraGridItemChange(IMessage rMessage)
    {
        if (m_CfgPropFurnitureType == null) { return; }

        var furnitureTypeId = (int)rMessage.Data;
        //刷新 当前区域内 该家具类型的数量
        if (m_CfgPropFurnitureType.Id == furnitureTypeId)
            RefreshAreaCountCur();
    }
}
