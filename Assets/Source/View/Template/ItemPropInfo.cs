using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using TMPro;
using com.ootii.Messages;

public class ItemPropInfo : MonoBehaviour
{
    [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
    //[SerializeField] private GameObject m_GoItemInfo = null; //物体 道具信息
    //[SerializeField] private Image m_ImgQuality = null; //图片 品质
    [SerializeField] private Image m_ImgIcon = null; //图片 道具
    [SerializeField] private TextMeshProUGUI m_TxtName = null; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtCount = null; //文本 数量

    private string m_BackpackChangeEventType = string.Empty; //背包类型

    /// <summary>
    /// 道具数量
    /// </summary>
    public int Count { get { return m_Count; } }
    protected int m_Count; //道具数量
    /// <summary>
    /// 道具配置
    /// </summary>
    public Prop_Config ItemConfig { get { return m_ItemConfig; } }
    protected Prop_Config m_ItemConfig; //道具配置文件
    /// <summary>
    /// 道具点击事件
    /// </summary>
    public Action<ItemPropInfo> ClickEvent { set { m_ClickEvent = value; } }
    protected Action<ItemPropInfo> m_ClickEvent;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        SetEnableMessageTypeItemChange(true);
    }

    private void OnDisable()
    {
        SetEnableMessageTypeItemChange(false);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void Init()
    {
        //默认 点击事件
        ClickListener.Get(m_BtnClick).SetClickHandler(OnClickItem);

        m_ClickEvent = OnOpenItemTips;
    }

    /// <summary>
    /// 设置 道具信息
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="count">道具数量 默认从背包获取道具数量</param>
    /// <param name="showName">是否显示道具名称</param>
    public void SetInfo(int itemId, int count = -1, bool showName = false)
    {
        SetBaseInfo(itemId, count, showName);
    }

    private void SetBaseInfo(int itemId, int count, bool showName = false) //设置 基础信息
    {
        //获取道具配置
        m_ItemConfig = ConfigSystem.Instance.GetConfig<Prop_Config>(itemId);
        if (m_ItemConfig == null) return;

        //设置 品质Icon
        //IconSystem.Instance.SetIcon(m_ImgQuality, "Common", $"Common_Rank_Frame_{m_ItemConfig.Rank}");
        //设置 道具Icon 若无配置时 使用默认路径
        string iconName = string.IsNullOrEmpty(m_ItemConfig.Icon) ? $"{m_ItemConfig.Id}_{(PropModel.EPropType)m_ItemConfig.Type}" : m_ItemConfig.Icon;
        AssetIconSystem.Instance.SetIcon(m_ImgIcon, "Prop", iconName);

        //设置 道具数量
        SetCount(count);

        //显示 道具名称 或 道具数量
        if (showName)
        {
            m_TxtName.gameObject.SetActive(true);
            m_TxtName.text = m_ItemConfig.Name;
        }
        else
        {
            m_TxtName.gameObject.SetActive(false);
        }

        //若为背包内道具 监听背包道具变化事件
        ChangeMessageTypeItemChange(PlayerModel.Instance.GetPropBackpackChangeMsg((PropModel.EPropType)m_ItemConfig.Type));
    }

    //改变 消息类型
    private void ChangeMessageTypeItemChange(string messageTypeNew)
    {
        if (m_BackpackChangeEventType.Equals(messageTypeNew)) { return; }

        //移除 旧消息监听
        SetEnableMessageTypeItemChange(false);

        //添加 新消息监听
        m_BackpackChangeEventType = messageTypeNew;
        SetEnableMessageTypeItemChange(true);
    }

    //设置 消息监听开启或关闭
    private void SetEnableMessageTypeItemChange(bool isEnable)
    {
        if (string.IsNullOrEmpty(m_BackpackChangeEventType)) { return; }

        if (isEnable)
        {
            MessageDispatcher.AddListener(m_BackpackChangeEventType, ActUpdateItems);
        }
        else
        {
            MessageDispatcher.RemoveListener(m_BackpackChangeEventType, ActUpdateItems);
        }
    }

    private void SetCount(int count) //设置 道具数量
    {
        m_Count = count;
        if (m_Count < 0)
        {
            m_Count = PlayerModel.Instance.GetPropCount(m_ItemConfig.Id);
        }

        m_TxtCount.text = m_Count.ToString();
    }

    private void OnClickItem(UnityEngine.EventSystems.PointerEventData eventData) //点击 打开道具详情弹窗
    {
        m_ClickEvent?.Invoke(this);
    }

    private void OnOpenItemTips(ItemPropInfo itemPropInfo)
    {
        //WindowModel.Instance.OpenWindow(WindowEnum.ItemTipsWindow, m_ItemConfig.ID);
    }

    private void ActUpdateItems(IMessage message) //回调 更新道具数量
    {
        if (m_ItemConfig == null)
        {
            return;
        }

        SetCount(PlayerModel.Instance.GetPropCount(m_ItemConfig.Id));
    }
}





