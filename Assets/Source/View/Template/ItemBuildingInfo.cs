using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using TMPro;
using com.ootii.Messages;

public class ItemBuildingInfo : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    //[SerializeField] private Image m_ImgIcon = null; //图片 道具
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称

    /// <summary>
    /// 道具配置
    /// </summary>
    public Building_Config cfgBuilding { get { return m_cfgBuilding; } }
    protected Building_Config m_cfgBuilding; //道具配置文件
    /// <summary>
    /// 道具点击事件
    /// </summary>
    public Action<ItemBuildingInfo> ClickEvent { set { m_ClickEvent = value; } }
    protected Action<ItemBuildingInfo> m_ClickEvent;

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void Init()
    {
        //默认 点击事件
        ClickListener.Get(m_BtnClick).SetClickHandler(OnClickItem);

        m_ClickEvent = OnOpenBuildingTips;
    }

    /// <summary>
    /// 设置 建筑信息
    /// </summary>
    /// <param name="buildingId">建筑ID</param>
    public void SetInfo(int buildingId)
    {
        //获取道具配置
        m_cfgBuilding = ConfigSystem.Instance.GetConfig<Building_Config>(buildingId);
        if (m_cfgBuilding == null) return;

        //设置 道具Icon 若无配置时 使用默认路径
        //string iconName = string.IsNullOrEmpty(m_cfgGuildBuilding.Icon) ? $"{m_cfgGuildBuilding.Id}_{(PlayerModel.EPropType)m_cfgGuildBuilding.Type}" : m_cfgGuildBuilding.Icon;
        //IconSystem.Instance.SetIcon(m_ImgIcon, "Prop", iconName);

        //显示 道具名称
        m_TxtName.text = m_cfgBuilding.Name;
    }

    private void OnClickItem(UnityEngine.EventSystems.PointerEventData eventData) //点击 打开道具详情弹窗
    {
        m_ClickEvent?.Invoke(this);
    }

    private void OnOpenBuildingTips(ItemBuildingInfo itemPropInfo)
    {
        //WindowModel.Instance.OpenWindow(WindowEnum.ItemTipsWindow, m_ItemConfig.ID);
    }
}





