using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;
using EntrustSystem;

public class EntrustWindow : WindowBase
{
    /// <summary>
    /// 委托信息界面
    /// </summary>
    [SerializeField] private UIEntrustInfo m_UIEntrustInfo;

    /// <summary>
    /// 委托池切换按钮
    /// </summary>
    [SerializeField] private UIEntrustSwitch m_UIEntrustSwitch;

    /// <summary>
    /// 冒险者信息
    /// </summary>
    [SerializeField] private UIEntrustVenturerInfo m_UIEntrustVenturerInfo;

    /// <summary>
    /// 冒险者库 包含所有可选择参加委托的冒险者
    /// </summary>
    [SerializeField] private UIEntrustVenturerLibrary m_UIEntrustVenturerLibrary;

    /// <summary>
    /// 委托项目组 滑动条 内容Ts
    /// </summary>
    [SerializeField] private RectTransform m_EntrustItemsContentRoot;

    /// <summary>
    /// 委托项目组 滑动条 显示窗口Ts
    /// </summary>
    [SerializeField] private RectTransform m_EntrustItemsViewportRoot;

    /// <summary>
    /// 委托项目组 滑动条 内容节点尺寸调整器
    /// </summary>
    [SerializeField] private ContentSizeFitter m_ContentSizeFitter;
    
    /// <summary>
    /// 委托项目模板
    /// </summary>
    [SerializeField] private GameObject m_UIEntrustItemTemplate;

    /// <summary>
    /// 冒险者库 项目模板
    /// </summary>
    [SerializeField] private GameObject m_UIEntrustVenturerLibraryItemTemplate;

    /// <summary>
    /// 委托项目,左侧滑动条目信息UI。的缓存池。
    /// </summary>
    private List<UIEntrustItem> m_UIEntrustItemPool;

    /// <summary>
    /// 委托项目 左侧滑动条目信息UI
    /// </summary>
    private Dictionary<int, UIEntrustItem> m_UIEntrustItemDic;

    /// <summary>
    /// 开启左侧滑条滑动功能的Item临界点数量
    /// </summary>
    private int m_ContentSizeFitterEnableNum = 7;

    /// <summary>
    /// 当前显示的委托池
    /// </summary>
    private EEntrustPoolType m_EntrustPoolTypeCur;

    /// <summary>
    /// 获取的用于显示的委托列表
    /// </summary>
    private EntrustItemHandler[] m_ItemHandlers;

    /// <summary>
    /// 当前显示的委托对应的UIEntrustItem
    /// </summary>
    private UIEntrustItem m_UIEntrustItemSelect;

    public override void OnLoaded()
    {
        base.OnLoaded();

        Init();

        MemoryLock = true;
        EntrustModel.Instance.onEntrustStateChange += OnEntrustStateChange;
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //设置默认显示的委托项目
        SetUIEntrustItem(EEntrustPoolType.UnacceptedAndAcceptedPool);

        AutoChooseEntrustItem();
    }

    public override void OnClose()
    {
        base.OnClose();

        //放弃当前选中的委托
        if (m_UIEntrustInfo.EntrustItemHandle != null && m_UIEntrustInfo.EntrustItemHandle.State == EEntrustState.WaitDistributed)
            EntrustModel.Instance.AbortEntrust(m_UIEntrustInfo.EntrustItemHandle.Id);

        SetUIEntrustItem(EEntrustPoolType.None);
    }

    public override void OnRelease()
    {
        base.OnRelease();

        m_UIEntrustVenturerLibrary.OnRelease();

        //释放委托项目池中的UI 并清空
        //因为类对应的GameObject属于Window 在Window释放时已经被销毁
        for (int i = 0; i < m_UIEntrustItemPool.Count; i++)
        {
            m_UIEntrustItemPool[i].OnRelease();
        }
        m_UIEntrustItemPool.Clear();

        EntrustModel.Instance.onEntrustStateChange -= OnEntrustStateChange;
    }

    private void Init()
    {
        //初始化各个子UI组件
        m_UIEntrustInfo?.Init(this);

        m_UIEntrustSwitch?.Init(this);
        m_UIEntrustSwitch.OnClickAction += OnClickSwitch;

        m_UIEntrustVenturerInfo?.Init(this);

        m_UIEntrustVenturerLibrary?.Init(this);


        m_UIEntrustItemPool = new List<UIEntrustItem>();
        m_UIEntrustItemDic = new Dictionary<int, UIEntrustItem>();
    }

    //获取一个委托项目UI
    private UIEntrustItem GetUIEntrustItem()
    {
        UIEntrustItem item;

        //从池中获取
        if (m_UIEntrustItemPool.Count > 0)
        {
            item = m_UIEntrustItemPool[0];
            m_UIEntrustItemPool.RemoveAt(0);
        }
        //实例化一个新的委托项目UI
        else
        {
            GameObject itemObj = GameObject.Instantiate<GameObject>(m_UIEntrustItemTemplate);
            item = itemObj.GetComponent<UIEntrustItem>();
            item.TransformGet.SetParent(m_EntrustItemsContentRoot);
            item.Init();
            item.OnClickAction = OnClickEntrustItem;
        }
        
        item.GameObjectGet.SetActive(true);

        return item;
    }

    private void ReturnUIEntrustItem(UIEntrustItem item)
    {
        item.GameObjectGet.SetActive(false);
        m_UIEntrustItemPool.Add(item);
    }

    /// <summary>
    /// 设置当前显示的委托列表内容
    /// </summary>
    /// <param name="entrustPoolType"></param>
    private void SetUIEntrustItem(EEntrustPoolType entrustPoolType)
    {
        if (m_EntrustPoolTypeCur == entrustPoolType) return;

        //获取需要显示的委托项目列表
        switch (entrustPoolType)
        {
            case EEntrustPoolType.UnacceptedPool:
                m_ItemHandlers = EntrustModel.Instance.GetEntrustHandles(EEntrustPoolType.UnacceptedPool);
                break;
            case EEntrustPoolType.AcceptedPool:
                m_ItemHandlers = EntrustModel.Instance.GetEntrustHandles(EEntrustPoolType.AcceptedPool);
                break;
            case EEntrustPoolType.UnacceptedAndAcceptedPool:
                m_ItemHandlers = EntrustModel.Instance.GetEntrustHandles(EEntrustPoolType.UnacceptedAndAcceptedPool);
                break;
        }

        //回收当前正在使用的ItemUI
        foreach (var item in m_UIEntrustItemDic.Values)
        {
            ReturnUIEntrustItem(item);
        }
        m_UIEntrustItemDic.Clear();

        //刷新显示的委托项目
        if (m_ItemHandlers != null)
        {
            //设置显示内容
            for (int i = 0; i < m_ItemHandlers.Length; i++)
            {
                var item = GetUIEntrustItem();
                item.SetInfo(m_ItemHandlers[i], entrustPoolType);
                m_UIEntrustItemDic.Add(item.EntrustItemHandle.Id, item);
            }

            //设置ScrollView的Content尺寸
            //当项目数量过少时 Content尺寸设置为和View一样长 多出时打开SizeFitter自动设置
            if (m_ItemHandlers.Length >= m_ContentSizeFitterEnableNum)
            {
                m_ContentSizeFitter.enabled = true;
            }
            else
            {
                m_ContentSizeFitter.enabled = false;

                //设置Content的高度尺寸和View一致
                m_EntrustItemsContentRoot.sizeDelta = Vector2.zero;
            }
        }

        m_EntrustPoolTypeCur = entrustPoolType;
    }

    /// <summary>
    /// 设置当前显示的冒险者信息
    /// </summary>
    /// <param name="venturerId"></param>
    public void SetVenturerInfo(int venturerId)
    {
        if (!m_UIEntrustVenturerInfo) return;

        m_UIEntrustVenturerInfo.SetInfo(venturerId);
    }

    /// <summary>
    /// 获取当前显示的委托项目
    /// </summary>
    /// <returns></returns>
    public EntrustItemHandler GetEntrustItemHandleCur()
    {
        return m_UIEntrustInfo.EntrustItemHandle;
    }

    /// <summary>
    /// 获取当前显示的委托项目Id
    /// </summary>
    /// <returns></returns>
    public int GetEntrustItemIdCur()
    {
        var item = GetEntrustItemHandleCur();
        return item != null ? item.Id : -1;
    }

    /// <summary>
    /// 获取当前显示详细信息的冒险者Id
    /// </summary>
    /// <returns></returns>
    public int GetVenturerIdCurShow()
    {
        return m_UIEntrustVenturerInfo.VenturerId;
    }

    /// <summary>
    /// 设置某个冒险者项目UI状态
    /// </summary>
    /// <param name="venturerId">冒险者Id</param>
    /// <param name="venturerLibraryItemState">冒险者项目UI状态</param>
    public void SetVenturerItemState(int venturerId, UIEntrustVenturerLibraryItem.VenturerLibraryItemState venturerLibraryItemState)
    {
        m_UIEntrustVenturerLibrary.SetVenturerItemState(venturerId, venturerLibraryItemState);
    }

    private void OnEntrustStateChange(int entrustId, EEntrustState oldState, EEntrustState newState)
    {
        if (newState == EEntrustState.Destroy)
        {
            if(m_UIEntrustItemDic.ContainsKey(entrustId))
            {
                ReturnUIEntrustItem(m_UIEntrustItemDic[entrustId]);
                m_UIEntrustItemDic.Remove(entrustId);
            }
        }

        //当前显示的委托状态变化时
        if (entrustId == m_UIEntrustInfo.EntrustItemId)
        {
            m_UIEntrustVenturerInfo.RefreshInfoWithCurEntrust();

            if (newState == EEntrustState.Destroy)
                AutoChooseEntrustItem();
        }
    }

    /// <summary>
    /// 自动选择显示一个委托详情
    /// </summary>
    public void AutoChooseEntrustItem()
    {
        if (m_UIEntrustItemDic.Count == 0)
        {
            m_UIEntrustInfo.GameObjectGet.SetActive(false);
        }
        else
        {
            m_UIEntrustInfo.GameObjectGet.SetActive(true);

            //默认选中第一个可用项目
            UIEntrustItem item = m_UIEntrustItemDic.Values.First();
            OnClickEntrustItem(item);
        }
    }

    #region button Action

    private void SwitchEntrustPool(EEntrustPoolType entrustPoolType = EEntrustPoolType.None)
    {
        //没有指定，自动循环
        if (entrustPoolType == EEntrustPoolType.None)
        {
            entrustPoolType = m_EntrustPoolTypeCur + 1;
            if (entrustPoolType == EEntrustPoolType.Max)
                entrustPoolType = (EEntrustPoolType)1;
        }

        //切换当前显示的委托
        switch (entrustPoolType)
        {
            case EEntrustPoolType.UnacceptedPool:
                SetUIEntrustItem(EEntrustPoolType.UnacceptedPool);
                //m_UIEntrustSwitch.SetInfo("查看已受理委托");
                break;
            case EEntrustPoolType.AcceptedPool:
                SetUIEntrustItem(EEntrustPoolType.AcceptedPool);
                //m_UIEntrustSwitch.SetInfo("查看未受理委托");
                break;
            case EEntrustPoolType.UnacceptedAndAcceptedPool:
                SetUIEntrustItem(EEntrustPoolType.UnacceptedAndAcceptedPool);
                break;
        }

        m_EntrustPoolTypeCur = entrustPoolType;

        //切换显示的委托池时 清空当前显示的委托详情
        m_UIEntrustInfo.SetInfo(null);
    }

    /// <summary>
    /// 点击切换委托池按钮
    /// </summary>
    private void OnClickSwitch()
    {
        SwitchEntrustPool();
    }

    /// <summary>
    /// 点击委托数组中的某个项目
    /// </summary>
    /// <param name="id"></param>
    private void OnClickEntrustItem(UIEntrustItem item)
    {
        if (m_UIEntrustInfo == null) return;

        EntrustItemHandler itemHandle = item.EntrustItemHandle;
        if (itemHandle == null) return;
        if (m_UIEntrustInfo.EntrustItemId == itemHandle.Id) return;

        //自动停止受理上一个委托，并受理当前点击的委托
        if (m_UIEntrustInfo.EntrustItemHandle != null && m_UIEntrustInfo.EntrustItemHandle.State == EEntrustState.WaitDistributed)
        {
            EntrustModel.Instance.AbortEntrust(m_UIEntrustInfo.EntrustItemHandle.Id);
        }
        if(m_UIEntrustItemSelect != null)
        {
            m_UIEntrustItemSelect.SetSelect(false);
        }
        
        if (itemHandle.State == EEntrustState.Unaccepted)
        {
            EntrustModel.Instance.AcceptEntrust(itemHandle.Id);
        }
        item.SetSelect(true);

        EntrustModel.Instance.EntrustCurState = itemHandle.State;

        //刷新委托信息界面的信息
        m_UIEntrustInfo.SetInfo(itemHandle);

        //当显示委托信息变化时
        //刷新当前显示的冒险者目录
        //冒险者目录可能显示一些和当前显示委托有关联的信息 比如是否加入到当前委托队伍中等
        m_UIEntrustVenturerLibrary.RefreshInfoWithCurEntrust();

        //刷新当前显示的冒险者信息 
        //冒险者信息UI有一些和当前显示委托有关联的信息 比如加入或离开当前委托队伍的按钮UI
        m_UIEntrustVenturerInfo.RefreshInfoWithCurEntrust();

        m_UIEntrustItemSelect = item;
    }
    #endregion
}
