using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Deploy;
using System.Text;
using EntrustSystem;

public class UIEntrustVenturerInfo : CachedMonoBehaviour
{
    [SerializeField] private Image m_ImgHead; //图片 头像
    [SerializeField] private TextMeshProUGUI m_TxtName; //文本 名称
    [SerializeField] private TextMeshProUGUI m_TxtRace; //文本 种族
    [SerializeField] private TextMeshProUGUI m_TxtGender; //文本 性别
    [SerializeField] private TextMeshProUGUI m_TxtRank; //文本 冒险者阶级

    [SerializeField] private TextMeshProUGUI m_TxtProfession; //文本 职业
    [SerializeField] private TextMeshProUGUI m_TxtLevel; //文本 等级
    [SerializeField] private TextMeshProUGUI m_TxtAge; //文本 年龄
    [SerializeField] private TextMeshProUGUI m_TxtAlignment; //文本 阵营

    [SerializeField] private Transform m_GiftRoot; //天赋词条组 根节点
    [SerializeField] private Transform m_SkillRoot; //技能词条组 根节点

    [SerializeField] private GameObject m_EntryTemplate1; //词条模板 等级1
    [SerializeField] private GameObject m_EntryTemplate2; //词条模板 等级2
    [SerializeField] private GameObject m_EntryTemplate3; //词条模板 等级3
    [SerializeField] private GameObject m_EntryTemplate4; //词条模板 等级4
    [SerializeField] private GameObject m_EntryTemplate5; //词条模板 等级5
    [SerializeField] private GameObject m_EntryTemplate6; //词条模板 等级6
    [SerializeField] private GameObject m_EntryTemplate7; //词条模板 等级7

    [SerializeField] private GameObject m_GobjLight; //图片 外发光

    [SerializeField] private UIEntrustVenturerInfoButton m_BtnUI;//按钮UI类

    //所属的委托创库
    private EntrustWindow m_EntrustWindow;

    /// <summary>
    /// 当前显示的冒险者Id
    /// </summary>
    public int VenturerId { get; private set; }
    private bool m_isValidVenturerId;//设定的冒险者Id是否有效

    private Dictionary<int, Stack<UIEntrustInfoEntry>> m_EntryPool;//词条UI池

    private List<UIEntrustInfoEntry> m_GiftsEntrys;
    private List<UIEntrustInfoEntry> m_SkillsEntrys;

    public void Init(EntrustWindow entrustWindow)
    {
        m_EntrustWindow = entrustWindow;

        m_EntryPool = new Dictionary<int, Stack<UIEntrustInfoEntry>>();
        m_GiftsEntrys = new List<UIEntrustInfoEntry>();
        m_SkillsEntrys = new List<UIEntrustInfoEntry>();

        ClickListener.Get(GameObjectGet).SetPointerEnterHandler(OnEnter);
        ClickListener.Get(GameObjectGet).SetPointerExitHandler(OnExit);

        //设置按钮
        m_BtnUI.Init();
        m_BtnUI.OnClickBtnJoinAction += OnClickBtnJoin;
        m_BtnUI.OnClickBtnLeaveAction += OnClickBtnLeave;

        m_GobjLight.SetActive(false);

        SetInfo(-1);
    }

    /// <summary>
    /// 设置信息
    /// 如果venturerId==-1 执行清空显示信息
    /// </summary>
    /// <param name="venturerId"></param>
    public void SetInfo(int venturerId)
    {
        if (VenturerId == venturerId) return;

        //如果输入的Id无效 会清空信息

        //获取冒险者信息
        var info = VenturerModel.Instance.GetVenturerInfo(venturerId);
        bool isValid = m_isValidVenturerId = info != null;

        //清空词条
        ClearEntry();

        //更新新的数据
        VenturerId = venturerId;

        //设置显示内容
        //IconSystem.Instance.SetIcon(m_ImgHead, "Venturer", info.);
        m_TxtName.text = isValid ? info.FullName : string.Empty;//名称
        m_TxtRace.text = isValid ? ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(info.RaceClanId).Name : string.Empty;//种族
        m_TxtGender.text = isValid ? ConfigSystem.Instance.GetConfig<Venturer_Gender>(info.GenderId).Name : string.Empty;//性别
        m_TxtRank.text = isValid ? ConfigSystem.Instance.GetConfig<Venturer_Rank>(info.RankId).Name : string.Empty;//冒险者阶级

        //职业 可以有多种职业
        string professionStr = string.Empty;
        if (isValid)
        {
            StringBuilder sbTemp = new StringBuilder();
            for (int i = 0; i < info.ProfessionIds.Count; i++)
            {
                sbTemp.Append(ConfigSystem.Instance.GetConfig<Profession_Config>(info.ProfessionIds[i]).Name);
                if (i != info.ProfessionIds.Count - 1)
                    sbTemp.Append(",");
            }
            professionStr = sbTemp.ToString();
        }
        m_TxtProfession.text = professionStr;//职业
        m_TxtLevel.text = isValid ? info.Level.ToString() : string.Empty;//等级
        m_TxtAge.text = isValid ? info.Age.ToString() : string.Empty;//年龄
        m_TxtAlignment.text = isValid ? ConfigSystem.Instance.GetConfig<Venturer_Alignment>(info.AlignmentId).Name : string.Empty;//人格

        if(isValid)
        {
            //天赋词条
            for (int i = 0; i < info.GiftIds.Count; i++)
            {
                var giftConfig = ConfigSystem.Instance.GetConfig<Gift_Config>(info.GiftIds[i]);

                //20211018 Winhoo TODO: 根据品质获取对应的词条预制体
                var entry = GetEntry(1);
                entry.SetInfo(giftConfig.Name);
                entry.TransformGet.SetParent(m_GiftRoot);

                m_GiftsEntrys.Add(entry);
            }

            //技能词条
            foreach (var kv in info.DicSkillGroupSkillPoint)
            {
                var skillConfig = ConfigSystem.Instance.GetConfig<Skill_Group>(kv.Key);

                var entry = GetEntry(1);
                entry.SetInfo(skillConfig.Name);
                entry.TransformGet.SetParent(m_SkillRoot);

                m_SkillsEntrys.Add(entry);
            }
        }

        RefreshInfoWithCurEntrust();
    }

    //清空词条
    public void ClearEntry()
    {
        for (int i = 0; i < m_GiftsEntrys.Count; i++)
        {
            ReturnEntry(m_GiftsEntrys[i]);
        }
        m_GiftsEntrys.Clear();
        for (int i = 0; i < m_SkillsEntrys.Count; i++)
        {
            ReturnEntry(m_SkillsEntrys[i]);
        }
        m_SkillsEntrys.Clear();
    }

    //当前显示的委托项目变化时 刷新相关信息
    public void RefreshInfoWithCurEntrust()
    {
        if (!m_isValidVenturerId
            || EntrustModel.Instance.EntrustCurState != EEntrustState.WaitDistributed //当前选中的委托不是待分配状态
            || EntrustModel.Instance.CheckVenturerIsWorking(VenturerId)) //当前显示的冒险者在工作中
        {
            m_BtnUI.GameObjectGet.SetActive(false);
            return;
        }

        var itemHandle = m_EntrustWindow.GetEntrustItemHandleCur();
        bool inEntrustTeam = itemHandle.ContainsVenturer(VenturerId);

        //冒险者不在委托队伍中，且委托队伍已满
        if (!inEntrustTeam && itemHandle.VenturerTeamIsFull)
        {
            m_BtnUI.GameObjectGet.SetActive(false);
            return;
        }

        m_BtnUI.GameObjectGet.SetActive(true);
        m_BtnUI.SetState(
            inEntrustTeam ?
            UIEntrustVenturerInfoButton.EButtonState.Leave : UIEntrustVenturerInfoButton.EButtonState.Join);
    }

    /// <summary>
    /// 获取词条UI
    /// </summary>
    /// <param name="type">类型 这里标识稀有度不同 1-7</param>
    /// <returns></returns>
    private UIEntrustInfoEntry GetEntry(int type)
    {
        UIEntrustInfoEntry entrustInfoEntry = null;

        if (!m_EntryPool.ContainsKey(type))
            m_EntryPool.Add(type, new Stack<UIEntrustInfoEntry>());

        if (m_EntryPool[type].Count > 0)
        {
            entrustInfoEntry = m_EntryPool[type].Pop();
        }
        else
        {
            GameObject template = null;
            switch (type)
            {
                case 1:
                    template = m_EntryTemplate1;
                    break;
                case 2:
                    template = m_EntryTemplate2;
                    break;
                case 3:
                    template = m_EntryTemplate3;
                    break;
                case 4:
                    template = m_EntryTemplate4;
                    break;
                case 5:
                    template = m_EntryTemplate5;
                    break;
                case 6:
                    template = m_EntryTemplate6;
                    break;
                case 7:
                    template = m_EntryTemplate7;
                    break;
            }

            if (template == null) return null;

            entrustInfoEntry = GameObject.Instantiate<GameObject>(template).GetComponent<UIEntrustInfoEntry>();
            entrustInfoEntry.Init(type);
        }

        entrustInfoEntry.GameObjectGet.SetActive(true);

        return entrustInfoEntry;
    }

    private void ReturnEntry(UIEntrustInfoEntry entrustInfoEntry)
    {
        if (entrustInfoEntry == null) return;

        entrustInfoEntry.GameObjectGet.SetActive(false);
        m_EntryPool[entrustInfoEntry.Type].Push(entrustInfoEntry);
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

    private void OnClickBtnJoin()
    {
        if (m_EntrustWindow == null) return;

        if(EntrustModel.Instance.AddVenturerToEntrust(m_EntrustWindow.GetEntrustItemIdCur(), VenturerId))
        {
            //成功添加到委托
            m_BtnUI.SetState(UIEntrustVenturerInfoButton.EButtonState.Leave);
            m_EntrustWindow.SetVenturerItemState(VenturerId, UIEntrustVenturerLibraryItem.VenturerLibraryItemState.JoinCur);
        }
    }

    private void OnClickBtnLeave()
    {
        if (EntrustModel.Instance.RemoveVenturerFromEntrust(m_EntrustWindow.GetEntrustItemIdCur(), VenturerId))
        {
            //成功移除从委托
            m_BtnUI.SetState(UIEntrustVenturerInfoButton.EButtonState.Join);
            m_EntrustWindow.SetVenturerItemState(VenturerId, UIEntrustVenturerLibraryItem.VenturerLibraryItemState.Idle);
        }
    }
}
