using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;
using com.ootii.Messages;

public class UIEntrustVenturerLibrary : CachedMonoBehaviour
{
    //显示可选的冒险者
    [SerializeField] private List<UIEntrustVenturerLibraryItem> m_Items;

    [SerializeField] private GameObject m_GobjLight; //图片 描边外发光

    [SerializeField] private UIEntrustVenturerLibraryPageTurning m_PageTurningLeft;
    [SerializeField] private UIEntrustVenturerLibraryPageTurning m_PageTurningRight;

    [SerializeField] private TextMeshProUGUI m_TxtPageNum;

    //所属的委托窗口
    private EntrustWindow m_EntrustWindow;

    public Action OnClickAction;

    //当且页码 从1开始
    private int m_PageCur;
    private const int m_PageItemNum = 5;

    //冒险者条目UI字典 key=冒险者Id vaule=冒险者条目UI
    private Dictionary<int, UIEntrustVenturerLibraryItem> m_VenturerItemDic;

    //当前选中 显示详细信息的Item
    private UIEntrustVenturerLibraryItem m_SelectItem;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(EntrustWindow entrustWindow)
    {
        m_VenturerItemDic = new Dictionary<int, UIEntrustVenturerLibraryItem>();

        m_EntrustWindow = entrustWindow;

        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

        m_GobjLight.SetActive(false);

        //初始化显示条目UI
        for (int i = 0; i < m_Items.Count; i++)
        {
            m_Items[i].Init();
            m_Items[i].onClickAction += OnClickVenturerItemAction;
        }

        //绑定翻页方法到对应按钮
        if(m_PageTurningLeft)
        {
            m_PageTurningLeft.Init();
            m_PageTurningLeft.SetInfo("上一页");
            m_PageTurningLeft.OnClickAction += OnClickPageTurningLeft;
        }

        if (m_PageTurningRight)
        {
            m_PageTurningRight.Init();
            m_PageTurningRight.SetInfo("下一页");
            m_PageTurningRight.OnClickAction += OnClickPageTurningRight;
        }

        //接收消息 当冒险者数量变化时刷新列表
        MessageDispatcher.AddListener(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_ADD, OnVenturerNumChange);

        //翻到第一页
        m_PageCur = 1;
        RefreshCurPage();
    }

    public void OnRelease()
    {
        MessageDispatcher.RemoveListener(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_ADD, OnVenturerNumChange);
    }

    //按钮 鼠标进入
    private void OnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(true);
    }

    //按钮 鼠标离开
    private void OnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjLight.SetActive(false);
    }

    //按钮 鼠标点击
    private void OnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnClickAction?.Invoke();
    }

    public bool IsLastPage { get { return m_PageCur * m_PageItemNum >= VenturerModel.Instance.VenturerInfosSelectable.Count; } }
    public bool IsFirstPage { get { return m_PageCur <= 1; } }

    /// <summary>
    /// 翻页
    /// </summary>
    /// <param name="right"></param>
    public void PageTurning(bool right = true)
    {
        if(right)
        {
            //已经是最后一页
            if (IsLastPage) return;

            m_PageCur++;

        }
        else
        {
            //已经是第一页
            if (m_PageCur <= 1) return;

            m_PageCur--;
        }

        //设置翻页按钮显示
        m_PageTurningRight.Show(!IsLastPage);
        m_PageTurningLeft.Show(!IsFirstPage);

        RefreshCurPage();
    }

    /// <summary>
    /// 设置某个冒险者项目UI状态
    /// </summary>
    /// <param name="venturerId">冒险者Id</param>
    /// <param name="venturerLibraryItemState">冒险者项目UI状态</param>
    public void SetVenturerItemState(int venturerId, UIEntrustVenturerLibraryItem.VenturerLibraryItemState venturerLibraryItemState)
    {
        if (!m_VenturerItemDic.ContainsKey(venturerId)) return;

        m_VenturerItemDic[venturerId].SetState(venturerLibraryItemState);
    }

    private void OnClickPageTurningLeft()
    {
        PageTurning(false);
    }

    private void OnClickPageTurningRight()
    {
        PageTurning(true);
    }

    //刷新当前页数显示的冒险者条目
    private void RefreshCurPage()
    {
        //显示当前页数
        m_TxtPageNum.text = m_PageCur.ToString();

        //清空缓存数据
        m_VenturerItemDic.Clear();

        //刷新显示内容
        int indexStart = (m_PageCur - 1) * m_PageItemNum;
        for (int i = 0; i < m_PageItemNum; i++)
        {
            int venturerInfoIndex = i + indexStart;
            if (i >= m_Items.Count) break;
            var item = m_Items[i];

            //超出可显示的冒险者数量
            if (venturerInfoIndex >= VenturerModel.Instance.VenturerInfosSelectable.Count)
            {
                item.GameObjectGet.SetActive(false);
                continue;
            }

            var venturerInfo = VenturerModel.Instance.VenturerInfosSelectable[venturerInfoIndex];
            item.GameObjectGet.SetActive(true);
            item.SetInfo(
                venturerInfo,
                EntrustModel.Instance.ContainsVenturerInEntrust(m_EntrustWindow.GetEntrustItemIdCur(), venturerInfo.Id) ?
                UIEntrustVenturerLibraryItem.VenturerLibraryItemState.JoinCur :
                UIEntrustVenturerLibraryItem.VenturerLibraryItemState.Idle);

            //设置选中显示
            item.SetSelectShow(venturerInfo.Id == m_EntrustWindow.GetVenturerIdCurShow());

            //设置Id对应冒险者条目UI的Dic
            m_VenturerItemDic.Add(venturerInfo.Id, item);
        }
    }

    /// <summary>
    /// 刷新显示信息 和当前显示的委托信息相关联的内容
    /// 冒险者项目会显示一些和当前委托相关的信息 比如冒险者是否已经加入了委托的队伍
    /// </summary>
    public void RefreshInfoWithCurEntrust()
    {
        for (int i = 0; i < m_Items.Count; i++)
        {
            //确认当前此条目Item的冒险者是否在当前显示的委托的队伍中

            bool contains = EntrustModel.Instance.ContainsVenturerInEntrust(m_EntrustWindow.GetEntrustItemIdCur(), m_Items[i].VenturerId);
            UIEntrustVenturerLibraryItem.VenturerLibraryItemState state = contains ? UIEntrustVenturerLibraryItem.VenturerLibraryItemState.JoinCur : UIEntrustVenturerLibraryItem.VenturerLibraryItemState.Idle;
            m_Items[i].SetState(state);
        }
    }

    /// <summary>
    /// 设置选中项目
    /// </summary>
    /// <param name="item"></param>
    private void SetSelectItem(UIEntrustVenturerLibraryItem item)
    {
        if (m_SelectItem == item) return;

        //处理之前选中先项目
        if(m_SelectItem)
        {
            m_SelectItem.SetSelectShow(false);
        }

        //更新当前选中项目
        m_SelectItem = item;

        //没有选中项目 清空显示的冒险者详细信息
        if (!m_SelectItem)
        {
            m_EntrustWindow.SetVenturerInfo(-1);
            return;
        }

        //设置新的选中的项目
        m_SelectItem.SetSelectShow(true);

        m_EntrustWindow.SetVenturerInfo(item.VenturerId);
    }

    private void OnVenturerNumChange(IMessage rMessage)
    {
        RefreshCurPage();
    }

    private void OnClickVenturerItemAction(UIEntrustVenturerLibraryItem item)
    {
        SetSelectItem(item);
    }
}
