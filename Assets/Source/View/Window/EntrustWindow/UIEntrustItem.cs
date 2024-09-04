using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntrustSystem;

public class UIEntrustItem : CachedMonoBehaviour
{
    [SerializeField] protected RectTransform m_DesRoot; //图片 等级Icon

    [SerializeField] protected Image m_ImgLevelIcon; //图片 等级Icon
    [SerializeField] protected TextMeshProUGUI m_TxtLevel; //文本 委托等级
    [SerializeField] protected TextMeshProUGUI m_TxtType; //文本 委托类型
    [SerializeField] protected GameObject m_GobjHover; //图片 外发光
    [SerializeField] protected Image m_ImgBG; //图片组件 背景
    [SerializeField] protected Image m_ImgHover; //图片组件 外发光
    [SerializeField] protected RectTransform m_ImgBGRectTrans;
    [SerializeField] protected RectTransform m_ImgHoverRectTrans;

    [SerializeField] protected Sprite m_SpriteBG_Normal; //背景图片 正常
    [SerializeField] protected Sprite m_SpriteBG_Normal_Select; //背景图片 正常
    [SerializeField] protected Sprite m_SpriteBG_Underway; //背景图片 进行中
    [SerializeField] protected Sprite m_SpriteBG_Underway_Select; //背景图片 进行中
    [SerializeField] protected Sprite m_SpriteBG_Complete; //背景图片 完成
    [SerializeField] protected Sprite m_SpriteBG_Complete_Select; //背景图片 完成

    [SerializeField] protected Sprite m_SpriteHover_Normal; //背景图片 正常
    [SerializeField] protected Sprite m_SpriteHover_Normal_Select; //背景图片 正常
    [SerializeField] protected Sprite m_SpriteHover_Underway; //背景图片 进行中
    [SerializeField] protected Sprite m_SpriteHover_Underway_Select; //背景图片 进行中

    [SerializeField] protected GameObject m_BtnRange; //按钮点击范围

    public Action<UIEntrustItem> OnClickAction;

    private ShowType m_ShowTypeCur;
    private bool m_IsSelect;

    /// <summary>
    /// 绑定的委托项目
    /// </summary>
    public EntrustItemHandler EntrustItemHandle { get; private set; }

    public void Init()
    {
        ClickListener.Get(m_BtnRange).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(m_BtnRange).SetPointerExitHandler(OnExit);
        ClickListener.Get(m_BtnRange).SetClickHandler(OnClick);

        m_GobjHover.SetActive(false);
        SetSelect(false);
    }

    public void OnRelease()
    {
        if (EntrustItemHandle != null)
        {
            EntrustItemHandle.BindEventWithStateChange(OnItemStateChange, false);
        }
    }

    public void SetInfo(EntrustItemHandler entrustItemHandle, EEntrustPoolType entrustPoolType)
    {
        if(EntrustItemHandle != null)
        {
            EntrustItemHandle.BindEventWithStateChange(OnItemStateChange, false);
        }

        EntrustItemHandle = entrustItemHandle;

        m_TxtLevel.text = EntrustItemHandle.Rank.ToString();
        //20211016 TODO 加载等级对应的Icon
        //IconSystem.Instance.SetIcon(m_ImgIcon, "Building", info.iconPath);
        m_TxtType.text = EntrustModel.Instance.GetEntrustTypeTrans(EntrustItemHandle).ToString();

        EntrustItemHandle.BindEventWithStateChange(OnItemStateChange);
        OnItemStateChange(EntrustItemHandle.Id, EEntrustState.None, EntrustItemHandle.State);
    }

    //按钮 鼠标进入
    private void OnEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjHover.SetActive(true);
    }

    //按钮 鼠标离开
    private void OnExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjHover.SetActive(false);
    }

    //按钮 鼠标点击
    private void OnClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (EntrustItemHandle == null) return;

        OnClickAction?.Invoke(this);
    }

    private void OnItemStateChange(int id, EEntrustState oldState, EEntrustState newState)
    {
        if (id != EntrustItemHandle.Id) return;

        CheckSetShow();
    }

    public void SetSelect(bool select)
    {
        if (m_IsSelect == select) return;
        m_IsSelect = select;

        CheckSetShow();
    }
    public enum ShowType
    {
        None,

        Normal,
        NormalSelect,
        Underway,
        UnderwaySelect,
        Complete,
        CompleteSelect
    }

    private void CheckSetShow()
    {
        ShowType showType = ShowType.None;

        EEntrustState state = EntrustItemHandle.State;
        switch (state)
        {
            case EEntrustState.Unaccepted:
            case EEntrustState.WaitDistributed:
                if (m_IsSelect) showType = ShowType.NormalSelect;
                else showType = ShowType.Normal;
                break;
            case EEntrustState.Underway:
                if (m_IsSelect) showType = ShowType.UnderwaySelect;
                else showType = ShowType.Underway;
                break;
            case EEntrustState.Complete:
            case EEntrustState.Statement:
                if (m_IsSelect) showType = ShowType.CompleteSelect;
                else showType = ShowType.Complete;
                break;
            case EEntrustState.Timeout:
                break;
            case EEntrustState.Destroy:
                GameObjectGet.SetActive(false);
                break;
            default:
                break;
        }

        if (m_ShowTypeCur == showType) return;

        switch (showType)
        {
            case ShowType.Normal:
                m_ImgBG.sprite = m_SpriteBG_Normal;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Normal.texture.width, m_SpriteBG_Normal.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Normal;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Normal.texture.width, m_SpriteHover_Normal.texture.height);
                break;
            case ShowType.NormalSelect:
                m_ImgBG.sprite = m_SpriteBG_Normal_Select;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Normal_Select.texture.width, m_SpriteBG_Normal_Select.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Normal_Select;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Normal_Select.texture.width, m_SpriteHover_Normal_Select.texture.height);
                break;
            case ShowType.Underway:
                m_ImgBG.sprite = m_SpriteBG_Underway;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Underway.texture.width, m_SpriteBG_Underway.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Underway;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Underway.texture.width, m_SpriteHover_Underway.texture.height);
                break;
            case ShowType.UnderwaySelect:
                m_ImgBG.sprite = m_SpriteBG_Underway_Select;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Underway_Select.texture.width, m_SpriteBG_Underway_Select.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Underway_Select;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Underway_Select.texture.width, m_SpriteHover_Underway_Select.texture.height);
                break;
            case ShowType.Complete:
                m_ImgBG.sprite = m_SpriteBG_Complete;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Complete.texture.width, m_SpriteBG_Complete.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Underway;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Underway.texture.width, m_SpriteHover_Underway.texture.height);
                break;
            case ShowType.CompleteSelect:
                m_ImgBG.sprite = m_SpriteBG_Complete_Select;
                m_ImgBGRectTrans.sizeDelta = new Vector2(m_SpriteBG_Complete_Select.texture.width, m_SpriteBG_Complete_Select.texture.height);
                m_ImgHover.sprite = m_SpriteHover_Underway_Select;
                m_ImgHoverRectTrans.sizeDelta = new Vector2(m_SpriteHover_Underway_Select.texture.width, m_SpriteHover_Underway_Select.texture.height);
                break;
            default:
                break;
        }

        if (m_IsSelect)
        {
            m_DesRoot.anchoredPosition = new Vector2(45f, 1f);
        }
        else
        {
            m_DesRoot.anchoredPosition = new Vector2(35f, 1f);
        }
    }
}
