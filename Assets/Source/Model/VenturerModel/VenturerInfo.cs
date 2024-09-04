using Deploy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;
using System;

[Serializable]
public class VenturerInfo
{
	public VenturerInfo()
	{
		ConstructorData();
	}

	public VenturerInfo(int id)
	{
		Id = id;
		ConstructorData();
	}

	public VenturerInfo(VenturerInfo venturerInfo)
	{
		if (venturerInfo == null) { return; }

		Id = venturerInfo.Id;
		CreateTime = venturerInfo.CreateTime;
		LifeTurnLimit = venturerInfo.LifeTurnLimit;
		LifeTurnCur = venturerInfo.LifeTurnCur;
		State = venturerInfo.State;
		RaceClanId = venturerInfo.RaceClanId;
		GenderId = venturerInfo.GenderId;
		AlignmentId = venturerInfo.AlignmentId;
		SurnameId = venturerInfo.SurnameId;
        NameId = venturerInfo.NameId;
		ExtranameId = venturerInfo.ExtranameId;
		GiftIds = new List<int>(venturerInfo.GiftIds);
		m_DicSkillGroupSkillPoint = new Dictionary<int, int>(venturerInfo.DicSkillGroupSkillPoint);
		Exp = venturerInfo.Exp;
		Level = venturerInfo.Level;
		RankId = venturerInfo.RankId;
		ProfessionIds = new List<int>(venturerInfo.ProfessionIds);
		AttributeBasicAmend = new Dictionary<int, int>(venturerInfo.AttributeBasicAmend);
		AttributeConst = new Dictionary<int, int>(venturerInfo.AttributeConst);
		SkinInfo = new Dictionary<int, int>(venturerInfo.SkinInfo);
		SkinColor = venturerInfo.SkinColor;
	}

	//构造 数据数据初始化
	private void ConstructorData()
	{
		CreateTime = UtilityFunction.GetTimestampCurrent();
		State = EVenturerState.None;
		LifeTurnCur = 0;
		LifeTurnLimit = 100;
	}

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
		InitAttribute(); //初始化 属性
		//刷新所有技能组阶级
		//方法内会刷新职业列表
		RefreshSkillGroupRankAll();
	}

	#region 基础信息
	/// <summary>
	/// 冒险者ID
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// 创建时间 时间戳
	/// </summary>
	public long CreateTime { get; set; }

	/// <summary>
	/// 种族ID
	/// </summary>
	public int RaceClanId { get; set; }

	/// <summary>
	/// 性别
	/// </summary>
	public int GenderId { get; set; }

	/// <summary>
	/// 九宫格人格
	/// </summary>
	public int AlignmentId { get; set; }

	#region 年龄、寿命
	/// <summary>
	/// 寿命回合数 上限
	/// </summary>
	public int LifeTurnLimit { get; set; }

	/// <summary>
	/// 寿命回合数 当前
	/// </summary>
	public int LifeTurnCur { get; set; }

	/// <summary>
	/// 年龄
	/// </summary>
	public int Age
	{
		get
		{
			return LifeTurnCur / 360;
		}
	}

	/// <summary>
	/// 冒险者出生日期
	/// </summary>
	/// <returns></returns>
	public TimeModel.TimeInfo BirthDate
	{
        get
		{
			var timeInfoLife = TimeModel.Instance.GetTimeInfo(LifeTurnCur); //寿命的时间信息
			var timeInfoBirth = TimeModel.Instance.TimeInfoCur - timeInfoLife;
			return timeInfoBirth;
		}
	}
	#endregion

	#region 名称

	/// <summary>
	/// 全名
	/// </summary>
	public string FullName
    {
        get
        {
			string fullName = string.Empty;
			int nameSortType = 1;

			var cfg = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(RaceClanId);
			if (cfg != null)
				nameSortType = cfg.NameSortType;

			switch(nameSortType)
            {
				case 1:
					fullName = GetSurname() + GetName(true) + GetExtraname(true);
					break;
				case 2:
					fullName = GetName() + GetExtraname(true) + GetSurname(true);
					break;
				case 3:
					fullName = GetExtraname() + GetName(true) + GetSurname(true);
					break;
			}

			return fullName;
		}
    }

	/// <summary>
	/// 姓名 姓氏 自定义
	/// </summary>
	public string SurnameCustom { get; set; }

	/// <summary>
	/// 姓名 名称 自定义
	/// </summary>
	public string NameCustom { get; set; }

	/// <summary>
	/// 姓名 特殊名 自定义
	/// </summary>
	public string ExtranameCustom { get; set; }

	/// <summary>
	/// 姓名 姓氏ID
	/// </summary>
	public int SurnameId { get; set; }

	/// <summary>
	/// 姓名 名称ID
	/// </summary>
	public int NameId { get; set; }

	/// <summary>
	/// 姓名 特殊名ID
	/// </summary>
	public int ExtranameId { get; set; }

	/// <summary>
	/// 姓名 姓氏
	/// </summary>
	public string GetSurname(bool isCombine = false)
	{
		//直接返回自定义文本
		if (!string.IsNullOrEmpty(SurnameCustom)) return SurnameCustom;

		//获取 自动生成的词语ID 对应多语言文本
		var name = ConfigSystem.Instance.GetConfigLanguageValue(SurnameId);
		if (!string.IsNullOrEmpty(name) && isCombine)
			return $"·{name}";
		else
			return name;
	}

	/// <summary>
	/// 姓名 名称
	/// </summary>
	public string GetName(bool isCombine = false)
	{
		if (!string.IsNullOrEmpty(NameCustom)) return NameCustom;

		var name = ConfigSystem.Instance.GetConfigLanguageValue(NameId);
		if (!string.IsNullOrEmpty(name) && isCombine)
			return $"·{name}";
		else
			return name;
	}

	/// <summary>
	/// 姓名 特殊名
	/// </summary>
	public string GetExtraname(bool isCombine = false)
	{
		if (!string.IsNullOrEmpty(ExtranameCustom)) return ExtranameCustom;

		var name = ConfigSystem.Instance.GetConfigLanguageValue(ExtranameId);
		if (!string.IsNullOrEmpty(name) && isCombine)
			return $"·{name}";
		else
			return name;
	}

	#endregion
	#endregion

	#region 等级&经验值
	/// <summary>
	/// 等级
	/// </summary>
	public int Level { get; set; }

	/// <summary>
	/// 经验值
	/// </summary>
	public int Exp { get; set; }

	/// <summary>
	/// 改变 经验值
	/// </summary>
	/// <param name="expChange"></param>
	public void ChangeExp(int expChange)
	{
		Exp += expChange;
		//是否升级或降级
	}

	/// <summary>
	/// 获取 经验值进度值
	/// </summary>
	public float GetExpProgress()
	{
		var cfgLevelCur = ConfigSystem.Instance.GetConfig<Venturer_Level>(Level);
		var cfgLevelNext = ConfigSystem.Instance.GetConfig<Venturer_Level>(Level + 1);
		if (cfgLevelNext == null) { return 1f; }

		int expCur = cfgLevelCur.ExpRequire - Exp;
		int expReq = cfgLevelNext.ExpRequire - cfgLevelCur.ExpRequire;
		float progress = (float)expCur / expReq;

		return progress;
	}

	#endregion

	#region 属性值
	/// <summary>
	/// 属性值 额外属性 元素属性
	/// 幸运 魅力 元素亲和 不会随等级提升而提升的固定属性
	/// </summary>
	public Dictionary<int, int> AttributeConst { get; set; } = new Dictionary<int, int>();

	/// <summary>
	/// 属性值 基础属性 补正
	/// 不同种族对不同属性的加成 万分比补正
	/// 冒险者个体会有随机浮动差异
	/// </summary>
	public Dictionary<int, int> AttributeBasicAmend { get; set; } = new Dictionary<int, int>();

	// 属性值 当前
	private Dictionary<int, int> m_AttributeValueCurrent = new Dictionary<int, int>();

	// 属性值 加成
	// 职业对属性的加成 万分比补正
	private Dictionary<int, int> m_AttributeValueBonus = new Dictionary<int, int>();

	/// <summary>
	/// 初始化 属性
	/// </summary>
	public void InitAttribute()
    {
		//初始化 属性值字典
		m_AttributeValueCurrent.Clear(); //属性值当前
		m_AttributeValueBonus.Clear(); //属性加成值
		var cfgMapAttr = ConfigSystem.Instance.GetConfigMap<Common_Attribute>();
		foreach (int attrId in cfgMapAttr.Keys)
		{
			m_AttributeValueBonus.Add(attrId, 0);
			m_AttributeValueCurrent.Add(attrId, 0);
		}

		//属性值 额外&元素亲和
		//幸运 魅力 元素亲和 不会随等级提升而提升的固定属性
		foreach (var kv in AttributeConst)
		{
			m_AttributeValueCurrent[kv.Key] = kv.Value;
		}

		//刷新 属性加成
		RefreshAttrBonus();
	}

	/// <summary>
	/// 刷新 属性加成
	/// </summary>
	public void RefreshAttrBonus()
	{
		//属性值 基础
		var cfgLevel = ConfigSystem.Instance.GetConfig<Venturer_Level>(Level);
		foreach (var kv in AttributeBasicAmend)
		{
			int attrId = kv.Key;
			int attrLevel = cfgLevel.AttributeValues[attrId]; //当前等级属性值
			m_AttributeValueCurrent[attrId] = attrLevel + (int)(attrLevel * kv.Value * 0.0001f); //加上属性值补正 不同种族对不同属性的万分比补正
		}

		//属性值 加成
		int[] attrIds = new int[m_AttributeValueBonus.Keys.Count];
		m_AttributeValueBonus.Keys.CopyTo(attrIds, 0);
		foreach (var attrId in attrIds)
		{
			//清零旧的属性值加成
			m_AttributeValueBonus[attrId] = 0;
		}
		//职业 属性加成 万分比加成
		if (ProfessionIds.Count > 0)
		{
			//主职业加成100%
			var cfgProfessionMain = ConfigSystem.Instance.GetConfig<Profession_Config>(ProfessionIds[0]);
			foreach (var kv in cfgProfessionMain.AttrBonus)
			{
				int attrId = kv.Key;
				float attrBonus = kv.Value * 0.0001f;
				m_AttributeValueBonus[attrId] += (int)(m_AttributeValueCurrent[attrId] * attrBonus);
			}

			//副职业加成60%
			for (int i = 1; i < ProfessionIds.Count; i++)
			{
				var cfgProfession = ConfigSystem.Instance.GetConfig<Profession_Config>(ProfessionIds[i]);

				foreach (var kv in cfgProfession.AttrBonus)
				{
					int attrId = kv.Key;
					float attrBonus = kv.Value * 0.0001f * 0.6f;
					m_AttributeValueBonus[attrId] += (int)(m_AttributeValueCurrent[attrId] * attrBonus);
				}
			}
		}
	}

	/// <summary>
	/// 获取 属性值
	/// </summary>
	/// <param name="attrId"></param>
	/// <returns></returns>
	public int GetAttributeValue(int attrId)
    {
		int value = 0;
		int valueBonus = 0;
		//当前属性
		m_AttributeValueCurrent.TryGetValue(attrId, out value);
		//职业加成属性
		m_AttributeValueBonus.TryGetValue(attrId, out valueBonus);

		return value + valueBonus;
    }
	#endregion

	#region 天赋
	/// <summary>
	/// 天赋列表
	/// </summary>
	public List<int> GiftIds { get; set; } = new List<int>();
	#endregion

	#region 技能
	/// <summary>
	/// 技能组ID:技能点
	/// </summary>
	public Dictionary<int, int> DicSkillGroupSkillPoint { get { return m_DicSkillGroupSkillPoint; } set { m_DicSkillGroupSkillPoint = value; } }
	private Dictionary<int, int> m_DicSkillGroupSkillPoint = new Dictionary<int, int>();

	/// <summary>
	/// 获取 所有技能组ID
	/// </summary>
	/// <returns></returns>
	public int[] GetAllSkillGroupId()
    {
		var skillGropuIds = m_DicSkillGroupSkillPoint.Keys;
		int[] skillGroupAll = new int[skillGropuIds.Count];
		skillGropuIds.CopyTo(skillGroupAll, 0);
		return skillGroupAll;
	}

	#region 技能组技能点
	/// <summary>
	/// 增加 技能组的技能点
	/// </summary>
	/// <param name="addPoint"></param>
	public void AddSkillGroupPoint(int skillGroupId, int addPoint)
	{
		if (!m_DicSkillGroupSkillPoint.ContainsKey(skillGroupId)) return;

		m_DicSkillGroupSkillPoint[skillGroupId] += addPoint;
		//技能点改变 刷新技能组阶级
		RefreshSkillGroupRank(skillGroupId);
	}

	/// <summary>
	/// 获取 技能组当前技能点
	/// </summary>
	/// <param name="skillGroupId"></param>
	/// <returns></returns>
	public int GetSkillGroupPoint(int skillGroupId)
	{
		m_DicSkillGroupSkillPoint.TryGetValue(skillGroupId, out int skillPoint);
		return skillPoint;
	}

	/// <summary>
	/// 获取 技能组 当前阶级技能点进度
	/// </summary>
	/// <returns></returns>
	public float GetSkillGroupPointCurRankProgress(int skillGroupId)
	{
		var cfg = ConfigSystem.Instance.GetConfig<Skill_Group>(skillGroupId);
		//当前阶级 技能点需求
		int rankCur = GetSkillGroupRank(skillGroupId);
		int rankSkillPointReqCur = 0;
		if (rankCur > 0)
			rankSkillPointReqCur = cfg.RankSkillPointRequire[rankCur - 1];
		//下一阶级 技能点需求
		int rankNext = rankCur + 1;
		int rankSkillPointReqNext = rankSkillPointReqCur;
		//是否大于最大技能组阶级
		if (rankNext > cfg.RankSkillPointRequire.Count)
			return 1;
		else
			rankSkillPointReqNext = cfg.RankSkillPointRequire[rankNext - 1];

		//计算 当前阶级技能点获取进度
		var skillPointTotal = GetSkillGroupPoint(skillGroupId); //当前技能点总数
		var progress = (float)(skillPointTotal - rankSkillPointReqCur) / (rankSkillPointReqNext - rankSkillPointReqCur);

		return progress;
	}
	#endregion

	#region 技能组阶级
	// 技能组ID:阶级
	private Dictionary<int, int> DicSkillGroupSkillRank = new Dictionary<int, int>();

	//刷新 所有技能组 阶级
	private void RefreshSkillGroupRankAll()
	{
		DicSkillGroupSkillRank.Clear();
		foreach (var skillGroupId in m_DicSkillGroupSkillPoint.Keys)
		{
			RefreshSkillGroupRank(skillGroupId, false);
		}
		//刷新 职业列表
		RefreshProfessionList();
	}

	/// <summary>
	/// 刷新 技能组 阶级
	/// </summary>
	/// <param name="skillGroupId"></param>
	/// <param name="refreshProfessionList">刷新职业列表</param>
	private void RefreshSkillGroupRank(int skillGroupId, bool refreshProfessionList = true)
	{
		//获取 当前技能组阶级
		if (!DicSkillGroupSkillRank.TryGetValue(skillGroupId, out int skillRankCur))
			DicSkillGroupSkillRank.Add(skillGroupId, 0);
		//获取 新的技能组阶级
		var cfg = ConfigSystem.Instance.GetConfig<Skill_Group>(skillGroupId);
		int skillRankNew = cfg.RankSkillPointRequire.Count;
		var skillPoint = GetSkillGroupPoint(skillGroupId);
		for (int i = 0; i < cfg.RankSkillPointRequire.Count; i++)
		{
			var skillPointRequire = cfg.RankSkillPointRequire[i];
			if (skillPoint < skillPointRequire)
			{
				skillRankNew = i;
				break;
			}
		}

		//记录 技能组阶级
		if (skillRankNew != skillRankCur)
		{
			DicSkillGroupSkillRank[skillGroupId] = skillRankNew;
			if (refreshProfessionList)
				RefreshProfessionList();
		}
	}

	/// <summary>
	/// 获取 技能阶级
	/// </summary>
	/// <param name="skillGroupId"></param>
	/// <returns></returns>
	public int GetSkillGroupRank(int skillGroupId)
	{
		DicSkillGroupSkillRank.TryGetValue(skillGroupId, out int skillRank);
		return skillRank;
	}

	/// <summary>
	/// 获取 某派系技能的最高阶级
	/// </summary>
	/// <param name="factionId">派系ID</param>
	/// <param name="isBest">最高的</param>
	/// <returns></returns>
	private int GetSkillGroupRankByFaction(int factionId, bool isBest = true)
	{
		//职业等级为 该类型技能最高等级
		int rank = 0;
		foreach (var skillGroupId in DicSkillGroupSkillRank.Keys)
		{
			var cfgSkillGroup = ConfigSystem.Instance.GetConfig<Skill_Group>(skillGroupId);
			if (cfgSkillGroup.Faction != factionId) { continue; }
			var skillRank = GetSkillGroupRank(skillGroupId);
			if ((isBest && skillRank > rank) || (!isBest && skillRank < rank))
				rank = skillRank;
		}

		return rank;
	}

	/// <summary>
	/// 获取 技能组阶级 在所有阶级中的占比
	/// </summary>
	/// <returns></returns>
	private float GetSkillGroupRankProportion(int skillGroupId)
	{
		//职业等级为 该类型技能最高等级
		int rankTotal = 0;
		foreach (var kv in DicSkillGroupSkillRank)
		{
			var skillGroupRank = GetSkillGroupRank(kv.Key);
			rankTotal += skillGroupRank;
		}

		if (rankTotal == 0)
			return 0f;
		else
			return (float)GetSkillGroupRank(skillGroupId) / rankTotal;
	}
	#endregion
	#endregion

	#region 职业
	/// <summary>
	/// 职业列表
	/// </summary>
	public List<int> ProfessionIds { get; set; } = new List<int>();

	/// <summary>
	/// 主职业ID
	/// </summary>
	public int MainProfessionID
	{
		get
		{
			return ProfessionIds[0];
		}
	}

	//刷新 职业列表
	private void RefreshProfessionList()
	{
		ProfessionIds.Clear();

		var cfgMap = ConfigSystem.Instance.GetConfigMap<Profession_Config>();
		foreach (var cfg in cfgMap.Values)
		{
			bool isMeet = true;
			//检查 技能组阶级是否满足
			foreach (var condRank in cfg.ConditionRank)
			{
				int condId = condRank.Key;
				int value = 0;

				if (condId < 1000) //技能派系ID
					value = GetSkillGroupRankByFaction(condId);
				else //技能组Id
					value = GetSkillGroupRank(condId);

				if (value < condRank.Value)
				{
					isMeet = false;
					break;
				}
			}
			//检查技能组占比是否满足
			foreach (var condProp in cfg.ConditionProportion)
			{
				if (GetSkillGroupRankProportion(condProp.Key) < condProp.Value * 0.0001f)
				{
					isMeet = false;
					break;
				}
			}

			//满足条件 添加至职业列表
			if (isMeet)
				ProfessionIds.Add(cfg.Id);
		}

		//移除派生职业的基础职业
		for (int i = 0; i < ProfessionIds.Count; i++)
        {
			var cfg = ConfigSystem.Instance.GetConfig<Profession_Config>(ProfessionIds[i]);
			for (int j = 0; j < cfg.BasedProfessionIdArray.Count; j++)
			{
				ProfessionIds.Remove(cfg.BasedProfessionIdArray[j]);
			}
		}

		//刷新主职业
		RefreshProfessionMain();
	}

	//刷新 主职业
	private void RefreshProfessionMain()
	{
		if (ProfessionIds.Count < 2) return;

		int professionIdMain = ProfessionIds[0];
		int professionIdMainSkillRankTotal = 0;
		for (int i = 0; i < ProfessionIds.Count; i++)
		{
			var professionId = ProfessionIds[i];
			//计算 该职业需求技能组的 阶级总和
			int rankTotal = 0;
			var cfg = ConfigSystem.Instance.GetConfig<Profession_Config>(professionId);
			foreach (var kv in cfg.ConditionRank)
			{
				var id = kv.Key;
				if (id < 1000) //技能派系ID
					rankTotal += GetSkillGroupRankByFaction(id);
				else //技能组Id
					rankTotal += GetSkillGroupRank(id);
			}
			//技能阶级总和更高 记录为新的主职业
			if (rankTotal > professionIdMainSkillRankTotal)
			{
				professionIdMain = professionId;
				professionIdMainSkillRankTotal = rankTotal;
			}
		}

		//设置 新的主职业
		if (ProfessionIds[0] != professionIdMain)
		{
			ProfessionIds.Remove(professionIdMain);
			ProfessionIds.Insert(0, professionIdMain);
		}
	}

	/// <summary>
	/// 获取 职业阶级
	/// </summary>
	/// <param name="professionId"></param>
	/// <returns></returns>
	public int GetProfessionRank(int professionId)
	{
		var cfg = ConfigSystem.Instance.GetConfig<Profession_Config>(professionId);
		if (cfg == null) { return 0; }

		//职业等级为 该类型技能最高等级
		int professionRank = GetSkillGroupRankByFaction(cfg.Faction);

		return professionRank;
	}
	#endregion

	#region 行动状态
	public enum EVenturerState
	{
		None = 0,
		/// <summary>
		/// 空闲
		/// </summary>
		Idle,
		/// <summary>
		/// 执行委托中
		/// </summary>
		Entrust,
		/// <summary>
		/// 死亡
		/// </summary>
		Dead,
	}

	/// <summary>
	/// 当前状态
	/// </summary>
	public EVenturerState State { get; set; }

	/// <summary>
	/// 获取 英雄状态
	/// </summary>
	public string GetHeroStateText()
	{
		switch (State)
		{
			case EVenturerState.Idle:
				return "空闲";
			case EVenturerState.Entrust:
				return "执行委托中";
			case EVenturerState.Dead:
				return "死亡";
			default:
				return "无";
		}
	}
	#endregion

	#region 公会信息
	/// <summary>
	/// 冒险者阶级
	/// </summary>
	public int RankId { get; set; }
	#endregion

	#region 冒险者皮肤
	/// <summary>
	/// 皮肤信息 部位ID:皮肤ID
	/// </summary>
	public Dictionary<int, int> SkinInfo { get; set; } = new Dictionary<int, int>();

	/// <summary>
	/// 皮肤颜色 Spine自定义皮肤枚举:RGBA十六进制
	/// </summary>
	public Dictionary<int, string> SkinColor { get; set; } = new Dictionary<int, string>();

	/// <summary>
	/// 事件 皮肤部位更新
	/// </summary>
	public Action<VenturerModel.EVenturerSkinPart> EvtSkinPartUpdate { get { return m_EvtSkinPartUpdate; } set { m_EvtSkinPartUpdate = value; } }
	private Action<VenturerModel.EVenturerSkinPart> m_EvtSkinPartUpdate;

	/// <summary>
	/// 事件 皮肤部位更新 所有
	/// </summary>
	public Action EvtSkinPartUpdateAll { get { return m_EvtSkinPartUpdateAll; } set { m_EvtSkinPartUpdateAll = value; } }
	private Action m_EvtSkinPartUpdateAll;

	/// <summary>
	/// 设置 皮肤部位 的道具ID
	/// </summary>
	/// <param name="skinPart"></param>
	/// <param name="propId"></param>
	public void SetSkinPartProp(VenturerModel.EVenturerSkinPart skinPart, int propId)
	{
		int skinPartId = (int)skinPart;
		int propIdCur = 0;
		if (SkinInfo.TryGetValue(skinPartId, out propIdCur))
		{
			if (propIdCur != propId)
			{
				SkinInfo[skinPartId] = propId;
				m_EvtSkinPartUpdate?.Invoke(skinPart);
			}
		}
		else
		{
			SkinInfo.Add(skinPartId, propId);
			m_EvtSkinPartUpdate?.Invoke(skinPart);
		}
	}

	public void SetSkinPartProp(Dictionary<VenturerModel.EVenturerSkinPart, int> dicSkinPartPropId)
	{
		foreach (var kv in dicSkinPartPropId)
		{
			int skinPartId = (int)kv.Key;
			int skinPropId = kv.Value;
			if (SkinInfo.ContainsKey(skinPartId))
			{
				SkinInfo[skinPartId] = skinPropId;
			}
			else
			{
				SkinInfo.Add(skinPartId, skinPropId);
			}

		}

		//刷新 Spine皮肤
		m_EvtSkinPartUpdateAll?.Invoke();
	}

	/// <summary>
	/// 获取 皮肤部位 的道具ID
	/// </summary>
	/// <param name="skinPart"></param>
	public int GetSkinPartProp(VenturerModel.EVenturerSkinPart skinPart)
	{
		int skinPartId = (int)skinPart;
		int propIdCur = -1;
		SkinInfo.TryGetValue(skinPartId, out propIdCur);

		return propIdCur;
	}

	/// <summary>
	/// 设置 性别
	/// </summary>
	/// <param name="gender"></param>
	public void SetGender(VenturerModel.EGender gender)
	{
		GenderId = (int)gender;
		//刷新 Spine皮肤
		m_EvtSkinPartUpdateAll?.Invoke();
	}

	/// <summary>
	/// 获取 性别
	/// </summary>
	/// <returns></returns>
	public VenturerModel.EGender GetGender()
	{
		var gender = (VenturerModel.EGender)GenderId;
		return gender;
	}

	/// <summary>
	/// 设置 自定义皮肤颜色
	/// </summary>
	public void SetSkinColor(VenturerModel.ECustomSkin eCustomSkin, Color color)
	{
		int enumInt = (int)eCustomSkin;
		string colorHex = ColorUtility.ToHtmlStringRGBA(color);
		if (SkinColor.ContainsKey(enumInt))
		{
			SkinColor[enumInt] = colorHex;
		}
		else
		{
			SkinColor.Add(enumInt, colorHex);
		}

		//回调 皮肤部位更新
		List<VenturerModel.EVenturerSkinPart> listSkinPart;
		VenturerModel.Instance.DicCustomSkinPart.TryGetValue(eCustomSkin, out listSkinPart);
		for (int i = 0; i < listSkinPart.Count; i++)
		{
			m_EvtSkinPartUpdate?.Invoke(listSkinPart[i]);
		}
	}

	/// <summary>
	/// 获取 自定义皮肤颜色
	/// </summary>
	/// <param name="eCustomSkin"></param>
	/// <returns></returns>
	public Color GetSkinColor(VenturerModel.ECustomSkin eCustomSkin)
	{
		Color color = Color.white;

		int enumInt = (int)eCustomSkin;
		string colorHex = string.Empty;
		if (SkinColor.TryGetValue(enumInt, out colorHex))
		{
			ColorUtility.TryParseHtmlString($"#{colorHex}", out color);
		}

		return color;
	}

    #endregion

    #region 背包
    /// <summary>
    /// 道具背包
    /// </summary>
    public Dictionary<PropModel.EPropType, PropBag> PropBag { get; set; } = new Dictionary<PropModel.EPropType, PropBag>();

    /// <summary>
    /// 获取 某ID的道具
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>道具信息</returns>
    public PropInfo GetPropInfo(int id)
    {
        //道具配置表
        var cfg = ConfigSystem.Instance.GetConfig<Prop_Config>(id);
        if (cfg == null) return null;

        //获取 对应类型 道具背包
        PropBag propBag = null;
        if (!PropBag.TryGetValue((PropModel.EPropType)cfg.Type, out propBag)) return null;

        //遍历 查找指定id的道具
        PropInfo propInfo = null;
        propBag.PropInfos.TryGetValue(id, out propInfo);

        return propInfo;
    }

    /// <summary>
    /// 获取 某类型的所有道具
    /// </summary>
    /// <param name="type">道具类型</param>
    /// <returns>道具信息列表</returns>
    public Dictionary<int, PropInfo> GetPropInfos(PropModel.EPropType type)
    {
        PropBag propBag = null;
        PropBag.TryGetValue(type, out propBag);

        if (propBag != null)
            return propBag.PropInfos;

        return new Dictionary<int, PropInfo>();
    }

    /// <summary>
    /// 获取 某ID的道具的数量
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>道具数量</returns>
    public int GetPropCount(int id)
    {
        PropInfo propInfo = GetPropInfo(id);

        if (propInfo != null)
            return propInfo.Count;

        return 0;
    }

    /// <summary>
    /// 增加 某ID的道具
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public PropInfo AddPropInfo(int id, int count = 0)
    {
        //道具配置表
        var cfg = ConfigSystem.Instance.GetConfig<Prop_Config>(id);
        if (cfg == null) return null;

        PropInfo propInfo = GetPropInfo(id);
        if (propInfo != null)
            propInfo.Count += count;
        else
        {
            //获取 对应类型 道具背包
            PropBag propBag = null;
            if (!PropBag.TryGetValue((PropModel.EPropType)cfg.Type, out propBag))
            {
                propBag = new PropBag();
                PropBag.Add((PropModel.EPropType)cfg.Type, propBag);
            }
            //添加 新道具
            propInfo = new PropInfo(id, count);
            propBag.PropInfos.Add(propInfo.Id, propInfo);
        }

        return propInfo;
    }

    /// <summary>
    /// 移除 某ID的道具
    /// </summary>
    /// <param name="id"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public PropInfo RemovePropInfo(int id, int count = 0)
    {
        //道具配置表
        var cfg = ConfigSystem.Instance.GetConfig<Prop_Config>(id);
        if (cfg == null) return null;

        PropInfo propInfo = GetPropInfo(id);
        if (propInfo == null) return null;

        propInfo.Count -= count;
        if (propInfo.Count <= 0 || count == 0)
        {
            //获取 对应类型 道具背包
            PropBag propBag = null;
            if (!PropBag.TryGetValue((PropModel.EPropType)cfg.Type, out propBag)) return null;

            propBag.PropInfos.Remove(id);
        }

        return propInfo;
    }

    /// <summary>
    /// 尝试消耗 某些道具指定数量
    /// </summary>
    /// <param name="propsMap">道具Map<propID,Count> </param>
    /// <returns></returns>
    public bool TryConsumePropsCount(Dictionary<int, int> propsMap)
    {
        if (propsMap == null || propsMap.Count == 0) return true;

        //检查是否所有的资源都足够
        foreach (var item in propsMap)
        {
            var propInfo = GetPropInfo(item.Key);
            if (propInfo == null) return false; //没有资源

            if (propInfo.Count < item.Value) return false; //资源数量不足
        }

        //消耗所有传入资源
        foreach (var item in propsMap)
            RemovePropInfo(item.Key, item.Value);

        return true;
    }

    /// <summary>
    /// 尝试消耗 某道具指定数量
    /// </summary>
    /// <param name="id">道具Id</param>
    /// <param name="count">道具数量</param>
    /// <returns></returns>
    public bool TryConsumePropCount(int id, int count)
    {
        var propInfo = GetPropInfo(id);
        if (propInfo == null) return false;

        if (propInfo.Count >= count)
        {
            RemovePropInfo(id, count);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 改变货币数量
    /// </summary>
    /// <param name="changeCount">改变数量</param>
    public void ChangeCoinCount(int changeCount)
    {
        if (changeCount > 0)
            AddPropInfo(10001, changeCount); //钱币
        else if (changeCount < 0)
        {
            changeCount *= -1;
            RemovePropInfo(10001, changeCount); //钱币
        }
    }
    #endregion
}

[Serializable]
/// <summary>
/// 道具背包
/// </summary>
public class PropBag
{
    /// <summary>
    /// 道具类型
    /// </summary>
    public PropModel.EPropType Type { get; set; }

    /// <summary>
    /// 道具列表
    /// </summary>
    public Dictionary<int, PropInfo> PropInfos { get; set; } = new Dictionary<int, PropInfo>();
}

