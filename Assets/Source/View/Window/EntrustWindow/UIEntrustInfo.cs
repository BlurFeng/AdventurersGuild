using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntrustSystem;
using Deploy;

public class UIEntrustInfo : CachedMonoBehaviour
{
    [SerializeField] private GameObject m_GobjLight; //图片 委托描边外发光

    [SerializeField] private TextMeshProUGUI m_TxtTitle; //文本 委托名称
    [SerializeField] private TextMeshProUGUI m_TxtDesSimple; //文本 委托简单描述

    [SerializeField] private TextMeshProUGUI m_TxtReward; //文本 报酬
    [SerializeField] private TextMeshProUGUI m_TxtConditionDes; //文本 委托要求描述

    [SerializeField] private TextMeshProUGUI m_TxtType; //文本 委托类型
    [SerializeField] private Image m_ImgLevelIcon; //图片 等级Icon
    [SerializeField] private TextMeshProUGUI m_TxtLevel; //文本 委托等级
    [SerializeField] private TextMeshProUGUI m_TxtRounds; //文本 剩余时间
    [SerializeField] private TextMeshProUGUI m_TxtRoundsConsume; //文本 消耗时间 执行一次委托
    [SerializeField] private GameObject m_GobjCanTryAgainTips; //文本 允许失败后再次执行的提示
    [SerializeField] private GameObject m_TeamGroupRoot; //队伍组根节点

    [SerializeField] private TextMeshProUGUI m_TxtSuccessRate; //文本 成功率
    [SerializeField] private Image m_ImgProgressBG_SuccessRate; //图片_成功率进度条外框
    [SerializeField] private Image m_ImgProgressBar_SuccessRate_Front; //图片_成功率进度条图片_前面
    [SerializeField] private Image m_ImgProgressBar_SuccessRate_Back; //图片_成功率进度条图片_后面

    [SerializeField] private TextMeshProUGUI m_TxtState; //文本 委托当前状态
    [SerializeField] private GameObject m_GobjProgress; //委托进度条
    [SerializeField] private Image m_ImgProgressBar; //委托进度条图片

    [SerializeField] private GameObject m_GobjBtnLight; //图片 按钮描边外发光
    [SerializeField] private GameObject m_GobjBtn_Start; //按钮 开始按钮
    [SerializeField] private GameObject m_GobjBtn_Cancel; //按钮 取消按钮
    [SerializeField] private GameObject m_GobjBtn_Statement; //按钮 受理按钮
    [SerializeField] private TextMeshProUGUI m_TxtButton; //文本 按钮

    [SerializeField] private List<UIEntrustInfoMember> m_Members;//成员槽位列表

    //所属的委托窗口
    private EntrustWindow m_EntrustWindow;

    /// <summary>
    /// 绑定的委托项目
    /// </summary>
    public EntrustItemHandler EntrustItemHandle { get; private set; }

    /// <summary>
    /// 当前显示的委托项目Id
    /// </summary>
    public int EntrustItemId
    {
        get
        {
            return EntrustItemHandle != null ? EntrustItemHandle.Id : 0;
        }
    }

    //当前最大成员数量
    //private int m_MemberMaxNum = 0;

    public void Init(EntrustWindow entrustWindow)
    {
        m_EntrustWindow = entrustWindow;

        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);

        ClickListener.Get(m_GobjBtn_Start).SetPointerEnterHandler(OnEnterBtnStart);
        ClickListener.Get(m_GobjBtn_Start).SetPointerExitHandler(OnExitBtnStart);
        ClickListener.Get(m_GobjBtn_Start).SetClickHandler(OnClickBtnStart);

        ClickListener.Get(m_GobjBtn_Cancel).SetPointerEnterHandler(OnEnterBtnCancel);
        ClickListener.Get(m_GobjBtn_Cancel).SetPointerExitHandler(OnExitBtnCancel);
        ClickListener.Get(m_GobjBtn_Cancel).SetClickHandler(OnClickBtnCancel);

        ClickListener.Get(m_GobjBtn_Statement).SetPointerEnterHandler(OnEnterBtnStatement);
        ClickListener.Get(m_GobjBtn_Statement).SetPointerExitHandler(OnExitBtnStatement);
        ClickListener.Get(m_GobjBtn_Statement).SetClickHandler(OnClickBtnStatement);

        m_GobjLight.SetActive(false);
        SetButtonStyle(ButtonStyle.Default);

        //初始化成员槽位UI
        for (int i = 0; i < m_Members.Count; i++)
        {
            m_Members[i].Init();
        }

        //清空信息
        SetInfo(null);
    }

    /// <summary>
    /// 设置信息
    /// 如果entrustItem==null则会清空信息
    /// </summary>
    /// <param name="entrustItemHandle">委托项目</param>
    public void SetInfo(EntrustItemHandler entrustItemHandle)
    {
        //处理上一个显示的委托
        if(EntrustItemHandle != null)
        {
            //接触绑定的事件
            BindAction(false);
        }

        //设置显示委托
        EntrustItemHandle = entrustItemHandle;
        bool isValid = EntrustItemHandle != null;
        if (!isValid)
        {
            GameObjectGet.SetActive(false);
            return;
        }
        GameObjectGet.SetActive(true);

        BindAction(true);//绑定相关事件
        m_TxtTitle.text = isValid ? EntrustItemHandle.Title : string.Empty;
        m_TxtDesSimple.text = isValid ? EntrustItemHandle.DescribeSimple : string.Empty;

        //报酬
        Entrust_Reward entrustReward = ConfigSystem.Instance.GetConfig<Entrust_Reward>(EntrustItemHandle.Id);
        int coinBase = entrustReward.Coin;
        int prestigBase = entrustReward.Prestige;
        int expBase = entrustReward.VentureExp;
        m_TxtReward.text = $"{coinBase}c {prestigBase}p {expBase}exp";

        //委托条件
        m_TxtConditionDes.text = "";
        int[] conditionNameIds;
        if (EntrustItemHandle.GetConditionNameIds(out conditionNameIds))
        {
            for (int i = 0; i < conditionNameIds.Length; i++)
            {
                Entrust_Condition conditionConfig = ConfigSystem.Instance.GetConfig<Entrust_Condition>(conditionNameIds[i]);
                m_TxtConditionDes.text += conditionConfig.Name + ";";
            }
        }

        //其他信息
        m_TxtType.text = isValid ? EntrustModel.Instance.GetEntrustTypeTrans(EntrustItemHandle).ToString() : string.Empty;
        if(isValid)
        {
            //20211016 TODO 加载等级对应的Icon
            //IconSystem.Instance.SetIcon(m_ImgLevelIcon, "Building", info.iconPath);
        }
        else
        {
            m_ImgLevelIcon.sprite = null;
        }
        m_TxtLevel.text = isValid ? EntrustItemHandle.Rank.ToString() : string.Empty;
        m_TxtRounds.text = isValid ? EntrustItemHandle.RoundsRemaining.ToString() : string.Empty;
        m_TxtRoundsConsume.text = isValid ? EntrustItemHandle.RoundsNeedBase.ToString() : string.Empty;
        m_GobjCanTryAgainTips.SetActive(isValid ? EntrustItemHandle.CanTryMultipleTimes : false);

        //成功率
        RefreshSucceedRateProgress();

        //当前委托状态
        m_TxtState.text = isValid ? EntrustItemHandle.State.ToString() : string.Empty;
        switch (isValid ? EntrustItemHandle.State : EntrustSystem.EEntrustState.None)
        {
            case EntrustSystem.EEntrustState.None:
                SetButtonStyle(ButtonStyle.Default);
                break;
            case EntrustSystem.EEntrustState.WaitDistributed:
                SetButtonStyle(ButtonStyle.Start);
                break;
            case EntrustSystem.EEntrustState.Underway:
                SetButtonStyle(ButtonStyle.Cancel);
                break;
            case EntrustSystem.EEntrustState.Complete:
                SetButtonStyle(ButtonStyle.Statement);
                break;
            case EntrustSystem.EEntrustState.Destroy:
            case EntrustSystem.EEntrustState.Timeout:
                SetButtonStyle(ButtonStyle.Default);
                break;
            default:
                SetButtonStyle(ButtonStyle.Default);
                break;
        }

        //成员列表
        int memberIndex = 0;
        if(isValid && EntrustItemHandle.VenturerNumMax > 0)
        {
            //必须成员
            for (int i = 0; i < entrustItemHandle.VenturerNumMust; i++)
            {
                if (memberIndex >= m_Members.Count)
                {
                    Debug.LogWarning($"委托{entrustItemHandle.Title}最大成员人数配置为{entrustItemHandle.VenturerNumMax}，超出了最大显示数量{m_Members.Count}，请检查配置是否正确！");
                    break;
                }

                var member = m_Members[memberIndex];

                member.ClearInfo();
                member.SetStyle(1);
                member.GameObjectGet.SetActive(true);

                memberIndex++;
            }
            //可选成员
            for (int i = 0; i < entrustItemHandle.VenturerNumOptional; i++)
            {
                if (memberIndex >= m_Members.Count)
                {
                    Debug.LogWarning($"委托{entrustItemHandle.Title}最大成员人数配置为{entrustItemHandle.VenturerNumMax}，超出了最大显示数量{m_Members.Count}，请检查配置是否正确！");
                    break;
                }

                var member = m_Members[memberIndex];

                member.ClearInfo();
                member.SetStyle(2);
                member.GameObjectGet.SetActive(true);

                memberIndex++;
            }
        }
        
        //关闭多余成员槽位
        for (int i = memberIndex; i < m_Members.Count; i++)
        {
            m_Members[i].GameObjectGet.SetActive(false);
        }

       
        RefreshMembersByVenturers(); //刷新成员列表

        RefrashShowByState(); //刷新和状态相关内容
    }

    public void RefrashShowByState()
    {
        bool isValid = EntrustItemHandle != null;

        m_TxtState.text = isValid ? EntrustItemHandle.State.ToString() : string.Empty;

        //设置进度条
        m_GobjProgress.SetActive(isValid ? EntrustItemHandle.State == EntrustSystem.EEntrustState.Underway : false);
        if (m_GobjProgress.activeSelf)
        {
            m_ImgProgressBar.fillAmount = EntrustItemHandle.UnderwayProgress;
        }
    }


    public void BindAction(bool bind)
    {
        if (EntrustItemHandle == null) return;

        if(bind)
        {
            EntrustItemHandle.BindEventWithVenturerAddOrRemove(OnVenturerAddOrRemove, true);
        }
        else
        {
            EntrustItemHandle.BindEventWithVenturerAddOrRemove(OnVenturerAddOrRemove, false);
        }
    }

    private void OnDestroy()
    {
        
    }

    public void OnVenturerAddOrRemove(int entrustId, bool add, int venturerIndex, int venturerId)
    {
        //重新刷新列表
        RefreshMembersByVenturers();

        //刷新成功率进度条
        RefreshSucceedRateProgress();
    }

    //根据当前显示的委托的冒险者队伍列表 刷新整个成员列表
    public void RefreshMembersByVenturers()
    {
        var venturerIds = EntrustItemHandle != null ? EntrustItemHandle.GetVenturerTeam() : null;

        if (venturerIds == null) return;

        //重新刷新列表
        int clearMemberIndexStart = 0;
        for (int i = 0; i < venturerIds.Length; i++)
        {
            int venturerIdTemp = venturerIds[i];

            //获取冒险者信息
            var info = VenturerModel.Instance.GetVenturerInfo(venturerIdTemp);
            if (info == null) continue;

            //20211030 Winhoo TODO:设置冒险者信息 显示冒险者头像
            m_Members[i].SetInfo(venturerIdTemp);

            clearMemberIndexStart++;
        }
        //清空没有冒险者信息的槽位
        for (int i = clearMemberIndexStart; i < EntrustItemHandle.VenturerNumMax; i++)
        {
            m_Members[i].ClearInfo();
        }
    }

    //刷新成功率进度条
    public void RefreshSucceedRateProgress()
    {
        //成功率
        //成功率0-3代表0%到300%
        //外框变色，0-1红到绿。1-2绿到深绿。2-3深绿到金
        //进度条显示三条，第一条
        EntrustModel.Instance.GetEntrustSuccessRate(EntrustItemHandle.Id, out float successRate);
        m_TxtSuccessRate.text = $"{Mathf.CeilToInt(successRate * 100f)}%";

        var common_Quality3 = ConfigSystem.Instance.GetConfig<Common_Quality>(3);
        var common_Quality4 = ConfigSystem.Instance.GetConfig<Common_Quality>(4);
        var common_Quality5 = ConfigSystem.Instance.GetConfig<Common_Quality>(5);
        var common_Quality7 = ConfigSystem.Instance.GetConfig<Common_Quality>(7);
        Color color3 = ColorUtil.HexToColor(common_Quality3.ColorHex);
        Color color4 = ColorUtil.HexToColor(common_Quality4.ColorHex);
        Color color5 = ColorUtil.HexToColor(common_Quality5.ColorHex);
        Color color7 = ColorUtil.HexToColor(common_Quality7.ColorHex);

        //框颜色
        m_ImgProgressBG_SuccessRate.color = Color.Lerp(Color.white, color7, successRate / 3f);

        if (successRate <= 1)
        {
            m_ImgProgressBar_SuccessRate_Back.color = color3;
            m_ImgProgressBar_SuccessRate_Back.fillAmount = successRate;
            m_ImgProgressBar_SuccessRate_Front.fillAmount = 0f;
        }
        else if (successRate <= 2)
        {
            m_ImgProgressBar_SuccessRate_Back.color = color3;
            m_ImgProgressBar_SuccessRate_Back.fillAmount = 1f;
            m_ImgProgressBar_SuccessRate_Front.color = color4;
            m_ImgProgressBar_SuccessRate_Front.fillAmount = successRate - 1f;
        }
        else if (successRate <= 3)
        {
            m_ImgProgressBar_SuccessRate_Back.color = color4;
            m_ImgProgressBar_SuccessRate_Back.fillAmount = 1f;
            m_ImgProgressBar_SuccessRate_Front.color = color5;
            m_ImgProgressBar_SuccessRate_Front.fillAmount = successRate - 2f;
        }
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

    #region Button Action

    private enum ButtonStyle
    {
        None,

        Default,
        Start,
        Cancel,
        Statement
    }

    private ButtonStyle m_ButtonStyleCur;

    private void SetButtonStyle(ButtonStyle style)
    {
        if (m_ButtonStyleCur == style) return;
        m_ButtonStyleCur = style;
        switch (m_ButtonStyleCur)
        {
            case ButtonStyle.Default:
                m_GobjBtn_Start.SetActive(false);
                m_GobjBtn_Cancel.SetActive(false);
                m_GobjBtn_Statement.SetActive(false);
                m_TxtButton.text = string.Empty;
                m_GobjBtnLight.SetActive(false);
                break;
            case ButtonStyle.Start:
                m_GobjBtn_Start.SetActive(true);
                m_GobjBtn_Cancel.SetActive(false);
                m_GobjBtn_Statement.SetActive(false);
                m_TxtButton.text = "开始";
                break;
            case ButtonStyle.Cancel:
                m_GobjBtn_Start.SetActive(false);
                m_GobjBtn_Cancel.SetActive(true);
                m_GobjBtn_Statement.SetActive(false);
                m_TxtButton.text = "停止";
                break;
            case ButtonStyle.Statement:
                m_GobjBtn_Start.SetActive(false);
                m_GobjBtn_Cancel.SetActive(false);
                m_GobjBtn_Statement.SetActive(true);
                m_TxtButton.text = "结算";
                break;
            default:
                break;
        }
    }


    //鼠标进入时 开始按钮
    private void OnEnterBtnStart(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(true);
    }

    //鼠标离开时 开始按钮
    private void OnExitBtnStart(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(false);
    }

    //点击时 开始按钮
    private void OnClickBtnStart(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (EntrustItemHandle == null) return;

        //在未分配状态时 才能开始委托
        if (EntrustItemHandle.State != EntrustSystem.EEntrustState.WaitDistributed) return;

        EntrustItemHandle.CheckCanStart();//确认委托项目是否允许开始任务
        //20211017 Winhoo TODO: 确认委托要求是否都已经满足

        if(EntrustModel.Instance.StartEntrust(EntrustItemHandle.Id))
        {
            //成功开始了委托
            SetButtonStyle(ButtonStyle.Cancel);//按钮显示为可取消
            RefrashShowByState();
        }
        else
        {
            Debug.Log("EntrustItem can't start!  Id:" + EntrustItemHandle.Id);
        }
    }


    //鼠标进入时 取消按钮
    private void OnEnterBtnCancel(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(true);
    }

    //鼠标离开时 取消按钮
    private void OnExitBtnCancel(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(false);
    }

    //点击时 取消按钮
    private void OnClickBtnCancel(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (EntrustItemHandle == null) return;

        //在进行中时 才能取消执行
        if (EntrustItemHandle.State != EntrustSystem.EEntrustState.Underway) return;

        //20211017 Winhoo TODO: 确认是否允许取消执行委托

        if (EntrustModel.Instance.CancelEntrust(EntrustItemHandle.Id))
        {
            //成功取消了委托

            SetButtonStyle(ButtonStyle.Start);
            RefrashShowByState();
        }
    }


    //鼠标进入时 结算按钮
    private void OnEnterBtnStatement(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(true);
    }

    //鼠标离开时 结算按钮
    private void OnExitBtnStatement(UnityEngine.EventSystems.PointerEventData eventData)
    {
        m_GobjBtnLight.SetActive(false);
    }

    //点击时 结算按钮
    private void OnClickBtnStatement(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (EntrustItemHandle == null) return;

        if (EntrustItemHandle.State != EntrustSystem.EEntrustState.Complete) return;

        EntrustModel.Instance.StatementEntrust(EntrustItemHandle.Id);
    }
    #endregion
}
