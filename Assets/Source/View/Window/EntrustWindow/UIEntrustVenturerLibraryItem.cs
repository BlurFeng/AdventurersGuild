using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;

public class UIEntrustVenturerLibraryItem : CachedMonoBehaviour
{
    public enum VenturerLibraryItemState
    {
        None,

        /// <summary>
        /// 闲置
        /// </summary>
        Idle,

        /// <summary>
        /// 加入了当前显示的委托队伍
        /// </summary>
        JoinCur,

        /// <summary>
        /// 忙碌中
        /// 比如已经在执行其他的委托
        /// </summary>
        Busy,
    }

    [SerializeField] private Image m_ImgHead; //图片 头像
    [SerializeField] private TextMeshProUGUI m_TxtName; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtProfession; //文本 职业
    [SerializeField] private TextMeshProUGUI m_TxtLevel; //文本 等级
    [SerializeField] private TextMeshProUGUI m_TxtRank; //文本 冒险者等级

    [SerializeField] protected GameObject m_GobjLight; //图片 外发光
    [SerializeField] private GameObject m_GobjJoinTag; //图片 在当前选中委托队伍中的标记
    [SerializeField] private GameObject m_GobjSelect; //图片 被选中 显示详细信息

    /// <summary>
    /// 冒险者Id
    /// </summary>
    public int VenturerId { get; private set; }

    public Action<UIEntrustVenturerLibraryItem> onClickAction;

    private VenturerLibraryItemState m_VenturerLibraryItemState;

    public void Init()
    {
        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);
        ClickListener.Get(GameObjectGet).SetClickHandler(OnClick);

        m_GobjLight.SetActive(false);

        GameObjectGet.SetActive(false);
        m_GobjSelect.SetActive(false);
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="venturerLibraryItemInfo"></param>
    public void SetInfo(VenturerInfo info, VenturerLibraryItemState venturerLibraryItemState = VenturerLibraryItemState.Idle)
    {
        if (info == null) return;
        if (info.Id == VenturerId) return;

        VenturerId = info.Id;
        GameObjectGet.SetActive(true);

        //m_ImgHead.sprite = venturer_Info.
        m_TxtName.text = info.FullName;
        m_TxtProfession.text = ConfigSystem.Instance.GetConfig<Venturer_Alignment>(info.AlignmentId).Name;
        m_TxtLevel.text = info.Level.ToString();
        m_TxtRank.text = ConfigSystem.Instance.GetConfig<Venturer_Rank>(info.RankId).Name;

        SetState(venturerLibraryItemState);
    }

    public void SetState(VenturerLibraryItemState venturerLibraryItemState)
    {
        if (m_VenturerLibraryItemState == venturerLibraryItemState) return;

        m_VenturerLibraryItemState = venturerLibraryItemState;

        switch (m_VenturerLibraryItemState)
        {
            case VenturerLibraryItemState.None:
                m_GobjJoinTag.SetActive(false);
                break;
            case VenturerLibraryItemState.Idle:
                m_GobjJoinTag.SetActive(false);
                break;
            case VenturerLibraryItemState.JoinCur:
                m_GobjJoinTag.SetActive(true);
                break;
            case VenturerLibraryItemState.Busy:
                m_GobjJoinTag.SetActive(true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    /// <param name="enable"></param>
    public void SetSelectShow(bool enable)
    {
        m_GobjSelect.SetActive(enable);
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
        onClickAction?.Invoke(this);
    }
}
