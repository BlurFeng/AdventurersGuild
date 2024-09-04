using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using com.ootii.Messages;
using Google.Protobuf.Collections;
using Spine;
using FsListItemPages;

public class VenturerModel : Singleton<VenturerModel>, IDestroy, ISaveData
{
    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        base.Init();

        InitVenturerPool(); //初始化 冒险者池 业务逻辑

        MessageDispatcher.AddListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, MsgExecuteFinishTurn);
        MessageDispatcher.AddListener(EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_START, MsgEntrustStart);
        MessageDispatcher.AddListener(EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_STATEMENT, MsgEntrustStatement);
    }

    public override void Destroy()
    {
        base.Destroy();

        MessageDispatcher.RemoveListener(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, MsgExecuteFinishTurn);
        MessageDispatcher.RemoveListener(EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_START, MsgEntrustStart);
        MessageDispatcher.RemoveListener(EntrustModelMsgType.ENTRUSTMODEL_ENTRUST_STATEMENT, MsgEntrustStatement);
    }

    public void SaveData(ES3File saveData)
    {
        //保存 冒险者池
        saveData.Save<string>("VenturerModel_MapVenturerInfoPool", PlayerPrefsUtil.SerializeData(m_MapVenturerInfoPool));
    }

    public void LoadData(ES3File saveData)
    {
        m_MapVenturerInfoPool.Clear();
        m_VenturerIdMax = 0;

        string strMapVenturerInfoPool = saveData.Load<string>("VenturerModel_MapVenturerInfoPool", string.Empty);

        if (!string.IsNullOrEmpty(strMapVenturerInfoPool))
        {
            //有存档 使用存档数据
            var mapVenturerInfoPool = PlayerPrefsUtil.DeserializeData<Dictionary<int, VenturerInfo>>(strMapVenturerInfoPool); ;

            //有存档 记录存档中的冒险者信息
            foreach (var kv in mapVenturerInfoPool)
            {
                var venturerInfo = kv.Value;
                venturerInfo.Init();
                AddVenturerInfo(venturerInfo);
                //记录 当前最大冒险者ID
                if (venturerInfo.Id > m_VenturerIdMax)
                {
                    m_VenturerIdMax = venturerInfo.Id;
                }
            }
        }
        else
        {
            //无存档 随机生成冒险者 初始数量
            for (int i = 0; i < 2; i++)
            {
                //诞生新的冒险者
                var venturerInfo = SpawnNewVenturerInfo();
                AddVenturerInfo(venturerInfo);
            }
        }
        

        MessageDispatcher.SendMessage(VenturerModelMsgType.VENTURERMODEL_LOAD_SUCCESS);
    }

    #region 冒险者基础数据
    #region 数据定义
    /// <summary>
    /// 枚举 属性
    /// </summary>
    public enum EAttributeType
    {
        None = 0,
        /// <summary>
        /// 体质
        /// </summary>
        Corporeity = 1001,
        /// <summary>
        /// 力量
        /// </summary>
        Strength = 1002,
        /// <summary>
        /// 敏捷
        /// </summary>
        Dexterity = 1003,
        /// <summary>
        /// 智慧
        /// </summary>
        Intelligence = 1004,
        /// <summary>
        /// 技巧
        /// </summary>
        Technic = 1005,

        /// <summary>
        /// 幸运
        /// </summary>
        Lucky = 2001,
        /// <summary>
        /// 魅力
        /// </summary>
        Charm = 2002,

        /// <summary>
        /// 奥魔素亲和
        /// </summary>
        Arcane = 3001,
        /// <summary>
        /// 火魔素亲和
        /// </summary>
        Fire = 3002,
        /// <summary>
        /// 水魔素亲和
        /// </summary>
        Water = 3003,
        /// <summary>
        /// 土魔素亲和
        /// </summary>
        Earth = 3004,
        /// <summary>
        /// 风魔素亲和
        /// </summary>
        Air = 3005,
        /// <summary>
        /// 光魔素亲和
        /// </summary>
        Light = 3006,
        /// <summary>
        /// 暗魔素亲和
        /// </summary>
        Dark = 3007,
        /// <summary>
        /// 精素亲和
        /// </summary>
        Spirit = 3008,
    }

    public enum ESkillRank
    {
        None,
        /// <summary>
        /// 初阶
        /// </summary>
        Primary,
        /// <summary>
        /// 中阶
        /// </summary>
        Middle,
        /// <summary>
        /// 上阶
        /// </summary>
        Superior,
        /// <summary>
        /// 高阶
        /// </summary>
        Advanced,
        /// <summary>
        /// 灵阶
        /// </summary>
        Spirituality,
        /// <summary>
        /// 圣阶
        /// </summary>
        Divine,
        /// <summary>
        /// 神阶
        /// </summary>
        Godly
    }

    #region 皮肤

    /// <summary>
    /// 冒险者皮肤部位
    /// </summary>
    public enum EVenturerSkinPart
    {
        None = 0,
        /// <summary>
        /// 身体 脑袋
        /// </summary>
        BodyHead = 11,
        /// <summary>
        /// 身体 躯干
        /// </summary>
        BodyTrunk = 12,
        /// <summary>
        /// 身体 眼睛
        /// </summary>
        BodyEye = 13,
        /// <summary>
        /// 身体 眉毛
        /// </summary>
        BodyBrow = 14,
        /// <summary>
        /// 身体 头发-前
        /// </summary>
        BodyHairFront = 15,
        /// <summary>
        /// 身体 头发-后
        /// </summary>
        BodyHairBack = 16,
        /// <summary>
        /// 身体 其他1
        /// </summary>
        BodyOther1 = 17,
        /// <summary>
        /// 身体 其他2
        /// </summary>
        BodyOther2 = 18,
        /// <summary>
        /// 身体 其他3
        /// </summary>
        BodyOther3 = 19,

        /// <summary>
        /// 装备 头部
        /// </summary>
        EquipHelmet = 21,
        /// <summary>
        /// 装备 胸部
        /// </summary>
        EquipArmour = 22,
        /// <summary>
        /// 装备 手部
        /// </summary>
        EquipGloves = 23,
        /// <summary>
        /// 装备 腿部
        /// </summary>
        EquipLeggings = 24,
        /// <summary>
        /// 装备 脚部
        /// </summary>
        EquipBoots = 25,
        /// <summary>
        /// 装备 主手
        /// </summary>
        EquipMainhand = 26,
        /// <summary>
        /// 装备 副手
        /// </summary>
        EquipOffhand = 27,
        /// <summary>
        /// 装备 其他
        /// </summary>
        EquipOther = 28,
    }

    /// <summary>
    /// Spine皮肤 自定义
    /// </summary>
    public enum ECustomSkin
    {
        None = 0,
        /// <summary>
        /// 身体
        /// </summary>
        SkinBody = 1,
        /// <summary>
        /// 眼睛
        /// </summary>
        SkinEye = 2,
        /// <summary>
        /// 毛发
        /// </summary>
        SkinHair = 3,
        /// <summary>
        /// 其他1
        /// </summary>
        SkinOther1 = 4,
        /// <summary>
        /// 其他2
        /// </summary>
        SkinOther2 = 5,
        /// <summary>
        /// 其他3
        /// </summary>
        SkinOther3 = 6,
    }

    /// <summary>
    /// 自定义Spine皮肤&列表皮肤部位
    /// </summary>
    public Dictionary<ECustomSkin, List<EVenturerSkinPart>> DicCustomSkinPart { get { return m_DicCustomSkinPart; } }
    private Dictionary<ECustomSkin, List<EVenturerSkinPart>> m_DicCustomSkinPart = new Dictionary<ECustomSkin, List<EVenturerSkinPart>>()
    {
         { ECustomSkin.SkinBody, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyHead, EVenturerSkinPart.BodyTrunk } },
         { ECustomSkin.SkinEye, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyEye } },
         { ECustomSkin.SkinHair, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyBrow, EVenturerSkinPart.BodyHairFront, EVenturerSkinPart.BodyHairBack } },
         { ECustomSkin.SkinOther1, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyOther1 } },
         { ECustomSkin.SkinOther2, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyOther2 } },
         { ECustomSkin.SkinOther3, new List<EVenturerSkinPart>() { EVenturerSkinPart.BodyOther3 } },
    };

    #endregion

    #region 性别

    public enum EGender
    {
        None = 0,
        /// <summary>
        /// 男性
        /// </summary>
        Male = 1,
        /// <summary>
        /// 女性
        /// </summary>
        Female = 2,
        /// <summary>
        /// 双性
        /// </summary>
        Androgyne = 3,
        /// <summary>
        /// 无性
        /// </summary>
        Agender = 4,
    }

    #endregion
    #endregion

    /// <summary>
    /// 可选择的 冒险者列表
    /// </summary>
    public List<VenturerInfo> VenturerInfosSelectable
    {
        get
        {
            var list = new List<VenturerInfo>();
            foreach (var info in m_MapVenturerInfoPool.Values)
            {
                list.Add(info);
            }
            return list;
        }
    }

    [ES3Serializable]
    private Dictionary<int, VenturerInfo> m_MapVenturerInfoPool = new Dictionary<int, VenturerInfo>(); //冒险者列表 总池
    private int m_VenturerIdMax; //冒险者ID最大值

    /// <summary>
    /// 添加 冒险者信息
    /// </summary>
    /// <param name=""></param>
    public void AddVenturerInfo(VenturerInfo venturerInfo)
    {
        if (m_MapVenturerInfoPool.ContainsKey(venturerInfo.Id)) { return; }

        m_MapVenturerInfoPool.Add(venturerInfo.Id, venturerInfo);
        AddVenturerIdStayTurn(venturerInfo.Id); //记录 冒险者逗留回合数

        MessageDispatcher.SendMessageData(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_ADD, venturerInfo);
    }

    /// <summary>
    /// 移除 冒险者信息
    /// </summary>
    /// <param name="id"></param>
    public void RemoveVenturerInfo(int id)
    {
        if (!m_MapVenturerInfoPool.ContainsKey(id)) { return; }

        var venturerInfo = GetVenturerInfo(id);
        m_MapVenturerInfoPool.Remove(id);
        RemoveVenturerIdStayTurn(id); //移除 冒险者逗留回合数

        MessageDispatcher.SendMessageData(VenturerModelMsgType.VENTURERMODEL_VENTURERPOOL_RED, venturerInfo);
    }

    /// <summary>
    /// 获取 某ID的冒险者信息
    /// </summary>
    /// <param name="id">道具ID</param>
    /// <returns>道具信息</returns>
    public VenturerInfo GetVenturerInfo(int id)
    {
        VenturerInfo venturerInfo = null;
        m_MapVenturerInfoPool.TryGetValue(id, out venturerInfo);

        return venturerInfo;
    }

    /// <summary>
    /// 获取 所有冒险者ID
    /// </summary>
    /// <returns></returns>
    public List<IItemPagesData> GetAllVenturerInfoData()
    {
        var listData = new List<IItemPagesData>();
        foreach (VenturerInfo venturerInfo in m_MapVenturerInfoPool.Values)
        {
            listData.Add(new ItemPagesData(venturerInfo.Id));
        }

        return listData;
    }

    /// <summary>
    /// 获取 可选的种族ID
    /// </summary>
    /// <returns></returns>
    public List<IItemPagesData> GetUsableRaceClanData()
    {
        var listData = new List<IItemPagesData>();
        //获取 所有种族的配置ID
        var cfgMap = ConfigSystem.Instance.GetConfigMap<Venturer_RaceClan>();
        foreach (var id in cfgMap.Keys)
        {
            listData.Add(new ItemPagesData(id));
        }

        return listData;
    }

    /// <summary>
    /// 获取 某状态的所有冒险者信息
    /// </summary>
    /// <param name="type">道具类型</param>
    /// <returns>道具信息列表</returns>
    public List<VenturerInfo> GetVenturerInfos(VenturerInfo.EVenturerState type = VenturerInfo.EVenturerState.None)
    {
        var list = new List<VenturerInfo>();
        foreach (VenturerInfo venturerInfo in m_MapVenturerInfoPool.Values)
        {
            if (venturerInfo.State == type || type == VenturerInfo.EVenturerState.None)
            {
                list.Add(venturerInfo);
            }
        }

        return list;
    }

    #endregion

    #region 冒险者数据修改
    /// <summary>
    /// 设置 冒险者信息
    /// </summary>
    /// <param name="venturerInfo"></param>
    public void SetVenturerInfo(VenturerInfo venturerInfo)
    {
        if (m_MapVenturerInfoPool.ContainsKey(venturerInfo.Id))
        {
            m_MapVenturerInfoPool[venturerInfo.Id] = venturerInfo;
        }

        MessageDispatcher.SendMessageData(VenturerModelMsgType.VENTURERMODEL_INFO_CHANGE, venturerInfo);
    }

    /// <summary>
    /// 设置 冒险者 状态
    /// </summary>
    /// <param name="venturerId"></param>
    public void SetVenturerState(int venturerId, VenturerInfo.EVenturerState state)
    {
        var venturerInfo = GetVenturerInfo(venturerId);
        venturerInfo.State = state;
        MessageDispatcher.SendMessageData(VenturerModelMsgType.VENTURERMODEL_INFO_STATE_CHANGE, venturerInfo);
    }

    /// <summary>
    /// 改变 冒险者经验值
    /// </summary>
    /// <param name="venturerId"></param>
    /// <param name="expChange"></param>
    public void ChangeVenturerExp(int venturerId, int expChange)
    {
        var venturerInfo = GetVenturerInfo(venturerId);
        if (venturerInfo == null) { return; }

        venturerInfo.ChangeExp(expChange);
    }
    #endregion

    #region 冒险者池业务逻辑
    /// <summary>
    /// 旅行冒险者 信息
    /// </summary>
    private struct TravelVenturerInfo
    {
        public TravelVenturerInfo(int id)
        {
            VenturerId = id;
            StayDay = 0;
        }

        public int VenturerId;
        public int StayDay;
    }

    //初始化 
    private void InitVenturerPool()
    {
        //记录字典 种族ID:生成随机权重
        m_MapRaceClanWeight.Clear();
        var cfgMapRaceClan = ConfigSystem.Instance.GetConfigMap<Venturer_RaceClan>();
        foreach (Venturer_RaceClan cfg in cfgMapRaceClan.Values)
        {
            m_MapRaceClanWeight.Add(cfg.Id, cfg.SpawnWeight);
        }
    }

    //冒险者池 执行回合(跨天)
    private void ExecuteTurnVenturerPool()
    {
        //旅行冒险者
        ExecuteTurnTravelVenturer();
    }

    #region 旅行冒险者
    private List<TravelVenturerInfo> m_ListTravelVenturerIdInfo = new List<TravelVenturerInfo>(); //旅行冒险者 信息
    //冒险者池 数量
    private float m_PrestigeVenturerCountRatio = 0.01f; //声望 冒险者数量 比例
    private float m_VenturerLimitCountFloatRatio = 1.1f; //冒险者上限数量 浮动增量比例
    private int m_VenturerSpawnPercentLessLimit = 50; //冒险者出现概率 数量小于上限时
    private int m_VenturerSpawnPercentGreaterLimit = 10; //冒险者出现概率 数量大于上限时

    //增加 冒险者Id逗留回合
    private void AddVenturerIdStayTurn(int id)
    {
        //不能重复添加
        for (int i = 0; i < m_ListTravelVenturerIdInfo.Count; i++)
        {
            var travelVenturerInfo = m_ListTravelVenturerIdInfo[i];
            if (travelVenturerInfo.VenturerId == id) { return; }
        }

        m_ListTravelVenturerIdInfo.Add(new TravelVenturerInfo(id));
    }

    //移除 冒险者Id逗留回合
    private void RemoveVenturerIdStayTurn(int id)
    {
        for (int i = 0; i < m_ListTravelVenturerIdInfo.Count; i++)
        {
            var travelVenturerInfo = m_ListTravelVenturerIdInfo[i];
            if (travelVenturerInfo.VenturerId != id) { continue; }

            m_ListTravelVenturerIdInfo.RemoveAt(i);
            break;
        }
    }

    //执行回合 旅行冒险者
    private void ExecuteTurnTravelVenturer()
    {
        //旅行冒险者 离开
        for (int i = m_ListTravelVenturerIdInfo.Count - 1; i >= 0; i--)
        {
            var travelVenturerInfo = m_ListTravelVenturerIdInfo[i];

            //累计逗留天数
            var id = travelVenturerInfo.VenturerId;
            travelVenturerInfo.StayDay += 1;
            m_ListTravelVenturerIdInfo[i] = travelVenturerInfo;

            var venturerInfo = GetVenturerInfo(id);
            //冒险者执行委托中 不会离开
            if (venturerInfo.State == VenturerInfo.EVenturerState.Entrust) { continue; }
            //超过最小逗留回合后
            var cfgAlignment = ConfigSystem.Instance.GetConfig<Venturer_Alignment>(venturerInfo.AlignmentId);
            var greaterTurn = travelVenturerInfo.StayDay - cfgAlignment.MinStayTurn;
            if (greaterTurn > 0)
            {
                //根据超出回合数 概率离开
                var percent = greaterTurn * cfgAlignment.PerTurnLeavePercent;
                if (Random.Range(0, 10000) > percent)
                    RemoveVenturerInfo(id);
            }
        }

        //旅行冒险者 出现
        int venturerCountCur = m_MapVenturerInfoPool.Count; //冒险者数量 当前
        float venturerCountTar = GuildModel.Instance.PrestigeValue * m_PrestigeVenturerCountRatio; //冒险者数量 目标
        float venturerCountLimit = 0; //数量上限
        float spawnPercent = 0; //出现概率
        if (venturerCountCur < venturerCountTar)
        {
            //小于目标数量
            venturerCountLimit = venturerCountTar;
            spawnPercent = m_VenturerSpawnPercentLessLimit;
        }
        else
        {
            //达到了目标数量 
            venturerCountLimit = venturerCountTar * m_VenturerLimitCountFloatRatio; //浮动增量最大数
            spawnPercent = m_VenturerSpawnPercentGreaterLimit;
        }
        //未达到目标数量 概率出现新冒险者
        if (venturerCountCur < venturerCountLimit)
        {
            while (Random.Range(0, 100) < spawnPercent)
            {
                //诞生新的冒险者
                var venturerInfo = SpawnNewVenturerInfo();
                AddVenturerInfo(venturerInfo);

                //当前冒险者数量 达到目标数量 结束
                venturerCountCur++;
                if (venturerCountCur >= venturerCountLimit) { break; }
            }
        }
    }
    #endregion

    #region 冒险者诞生
    private MapField<int, int> m_MapRaceClanWeight = new MapField<int, int>(); //字典 种族ID:生成随机权重
    private Dictionary<int, int> m_PrestigeVenturerRankWeight = new Dictionary<int, int>() //声望 冒险者阶级权重
    { { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, };

    /// <summary>
    /// 生成 新的冒险者信息
    /// </summary>
    /// <returns></returns>
    public VenturerInfo SpawnNewVenturerInfo()
    {
        var venturerInfo = new VenturerInfo(++m_VenturerIdMax);

        //随机种子
        Random.InitState(venturerInfo.GetHashCode());

        //冒险者阶级 权重随机
        venturerInfo.RankId = GetVenturerSpawnRank();

        //种族 权重随机
        venturerInfo.RaceClanId = RandomWeightMap(m_MapRaceClanWeight);
        var cfgRaceClan = ConfigSystem.Instance.GetConfig<Venturer_RaceClan>(venturerInfo.RaceClanId);
        //性别 权重随机
        venturerInfo.GenderId = RandomWeightMap(cfgRaceClan.GenderWeight);
        //姓名
        venturerInfo.SurnameId = GetVenturerSpawnSurNameId(cfgRaceClan);
        venturerInfo.NameId = GetVenturerSpawnNameId(venturerInfo.GenderId, cfgRaceClan);
        venturerInfo.ExtranameId = GetVenturerSpawnExtraNameId(cfgRaceClan);
        //生命回合数上限
        venturerInfo.LifeTurnLimit = Random.Range(cfgRaceClan.LifeTurnRange[0], cfgRaceClan.LifeTurnRange[1]); //范围内随机
        //当前年龄 回合
        venturerInfo.LifeTurnCur = GetVenturerSpawnLifeTurnCur(venturerInfo.LifeTurnLimit, venturerInfo.RankId);
        //人格 权重随机
        venturerInfo.AlignmentId = RandomWeightMap(cfgRaceClan.AlignmentWeight);

        //等级
        venturerInfo.Level = Random.Range(0, 99);
        //经验值
        var cfgLevelCur = ConfigSystem.Instance.GetConfig<Venturer_Level>(venturerInfo.Level);
        var cfgLevelNext = ConfigSystem.Instance.GetConfig<Venturer_Level>(venturerInfo.Level + 1);
        venturerInfo.Exp = Random.Range(cfgLevelCur.ExpRequire, cfgLevelNext.ExpRequire);

        //基础属性 修正
        int attrId = (int)EAttributeType.Corporeity;
        venturerInfo.AttributeBasicAmend[attrId] = cfgRaceClan.AttrAmend[attrId] * Random.Range(0, 10000) / 10000;
        attrId = (int)EAttributeType.Strength;
        venturerInfo.AttributeBasicAmend[attrId] = cfgRaceClan.AttrAmend[attrId] * Random.Range(0, 10000) / 10000;
        attrId = (int)EAttributeType.Dexterity;
        venturerInfo.AttributeBasicAmend[attrId] = cfgRaceClan.AttrAmend[attrId] * Random.Range(0, 10000) / 10000;
        attrId = (int)EAttributeType.Intelligence;
        venturerInfo.AttributeBasicAmend[attrId] = cfgRaceClan.AttrAmend[attrId] * Random.Range(0, 10000) / 10000;
        attrId = (int)EAttributeType.Technic;
        venturerInfo.AttributeBasicAmend[attrId] = cfgRaceClan.AttrAmend[attrId] * Random.Range(0, 10000) / 10000;
        //额外属性
        venturerInfo.AttributeConst[(int)EAttributeType.Lucky] = Random.Range(1, 100);
        venturerInfo.AttributeConst[(int)EAttributeType.Charm] = Random.Range(1, 100);
        //元素亲和修正 五分之一的概率会有高亲和属性
        bool isMagic = Random.Range(0, 5) == 0;
        //元素亲和
        float ElementAppetencyAmend = isMagic ? 1 : 0.1f;
        venturerInfo.AttributeConst[(int)EAttributeType.Arcane] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Fire] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Water] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Earth] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Air] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Light] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Dark] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        venturerInfo.AttributeConst[(int)EAttributeType.Spirit] = (int)(Random.Range(1, 100) * ElementAppetencyAmend);
        //初始化属性值 作为生成技能组的参数
        venturerInfo.InitAttribute();

        //天赋 权重随机
        GetVenturerSpawnGiftIds(venturerInfo.GiftIds, cfgRaceClan.GiftCountWeight, new MapField<int, int>() { cfgRaceClan.GiftWeight });
        //技能
        venturerInfo.DicSkillGroupSkillPoint = GetVenturerSpawnSkillGroup(venturerInfo, isMagic);

        //初始状态
        venturerInfo.State = VenturerInfo.EVenturerState.Idle;

        //初始化 刷新附加数据
        venturerInfo.Init();

        //设置 皮肤信息
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyHead, 41001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyTrunk, 42001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyEye, 43001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyBrow, 44001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyHairFront, 45001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyHairBack, 46001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.BodyOther1, 47001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipHelmet, 51001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipArmour, 52001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipGloves, 53001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipLeggings, 54001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipBoots, 55001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipMainhand, 56001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipOffhand, 57001);
        venturerInfo.SetSkinPartProp(EVenturerSkinPart.EquipOther, 58001);

        return venturerInfo;
    }

    /// <summary>
    /// 权重随机
    /// </summary>
    /// <param name="mapIdWeight">ID:权重</param>
    /// <returns></returns>
    private int RandomWeightMap(IDictionary<int, int> mapIdWeight)
    {
        int Id = 0;
        int value = 0;
        foreach (var kv in mapIdWeight)
        {
            int newValue = kv.Value * Random.Range(0, 100);
            if (newValue > value)
            {
                Id = kv.Key;
                value = newValue;
            }
        }

        return Id;
    }

    /// <summary>
    /// 权重随机 获取value组
    /// </summary>
    /// <param name="mapIdWeight">ID:权重</param>
    /// <returns></returns>
    private int[] RandomWeightMapGetArray(IDictionary<int, int> mapIdWeight, int count)
    {
        Dictionary<int, int> dicIdValue = new Dictionary<int, int>();
        foreach (var kv in mapIdWeight)
        {
            int newId = kv.Key;
            int newValue = kv.Value * Random.Range(0, 100);

            if (dicIdValue.Count < count)
                dicIdValue.Add(newId, newValue);
            else
            {
                foreach (var kvResult in dicIdValue)
                {
                    if (newValue > kvResult.Value)
                    {
                        dicIdValue.Remove(kvResult.Key);
                        dicIdValue.Add(newId, newValue);
                        break;
                    }
                }
            }
        }

        int[] keyArray = new int[dicIdValue.Count];
        dicIdValue.Keys.CopyTo(keyArray, 0);
        return keyArray;
    }

    #region 冒险者阶级
    //获取 阶级
    private int GetVenturerSpawnRank()
    {
        //随机出现的冒险者的阶级
        var cfgGuildRank = ConfigSystem.Instance.GetConfig<Guild_Rank>(GuildModel.Instance.GuildRank); //公会阶级
        var venturerRankWeightMap = new MapField<int, int>() { cfgGuildRank.VenturerRankWeight }; //出现的冒险者阶级权重 基础
        //公会声望 影响权重 
        foreach (var kv in venturerRankWeightMap)
        {
            var rankId = kv.Key;
            var weightAdd = m_PrestigeVenturerRankWeight[rankId] * GuildModel.Instance.PrestigeValue;
            venturerRankWeightMap[rankId] = kv.Value + weightAdd;
        }
        //权重随机 获得冒险者阶级
        var venturerRank = RandomWeightMap(venturerRankWeightMap);

        return venturerRank;
    }
    #endregion

    #region 年龄
    //获取 当前年龄 回合数(天)
    private int GetVenturerSpawnLifeTurnCur(int lifeTurnLimit, int rank)
    {
        //冒险者阶级越高 当前年龄越高
        int turnCountBase = (int)(lifeTurnLimit * 0.5f * (rank / 7f) + lifeTurnLimit * 0.16f); //基础值
        int turnCountRandom = (int)(lifeTurnLimit * 0.1f * Random.Range(-1f, 1f)); //随机浮动值

        return turnCountBase + turnCountRandom;
    }
    #endregion

    #region 天赋
    //获取 天赋
    private void GetVenturerSpawnGiftIds(List<int> giftIdsContainer, MapField<int, int> giftCountWeight, MapField<int, int> giftWeight)
    {
        giftIdsContainer.Clear();
        int giftCount = RandomWeightMap(giftCountWeight);
        //天赋数量不能超过 会出现的天赋总数
        if (giftCount > giftWeight.Count)
        {
            giftCount = giftWeight.Count;
        }

        for (int i = 0; i < giftCount; i++)
        {
            //天赋 权重随机
            int giftId = RandomWeightMap(giftWeight);
            giftWeight.Remove(giftId); //天赋不会重复
            giftIdsContainer.Add(giftId);
        }
    }
    #endregion

    #region 技能
    //获取 技能组信息
    private Dictionary<int, int> GetVenturerSpawnSkillGroup(VenturerInfo venturerInfo, bool isMagic)
    {
        Dictionary<int, int> dicSkillGroupPoint = new Dictionary<int, int>();

        //倾向于学习属性优势更大的技能组
        //计算 技能组权重
        Dictionary<int, int> dicProfessionIdWeight = new Dictionary<int, int>();
        var cfgMapSkillGroup = ConfigSystem.Instance.GetConfigMap<Skill_Group>();
        foreach (var cfgSkillGroup in cfgMapSkillGroup.Values)
        {
            //元素亲和低时 不会学习魔法系技能
            if (!isMagic && cfgSkillGroup.Faction == 5) { continue; }

            int weight = 0;
            var attrValueRequire = cfgSkillGroup.GetRankAttributeRequire(1);
            foreach (var kv in attrValueRequire)
            {
                var value = venturerInfo.GetAttributeValue(kv.Key);
                //当前属性值*技能属性值需求
                weight += value * kv.Value;
            }
            dicProfessionIdWeight.Add(cfgSkillGroup.Id, weight);
        }

        //权重随机 获得学习的技能组ID列表
        int skillCount = 3; //技能数量
        var skillGroupIdArray = RandomWeightMapGetArray(dicProfessionIdWeight, skillCount);

        //获取当前等级的总计技能点
        int skillPpointTotal = 0;
        for (int i = 0; i <= venturerInfo.Level; i++)
        {
            var cfgLevel = ConfigSystem.Instance.GetConfig<Venturer_Level>(i);
            skillPpointTotal += cfgLevel.SkillPointGet;
        }

        //随机分配技能点
        skillPpointTotal -= skillCount; //每个技能至少1点
        for (int i = 0; i < skillGroupIdArray.Length; i++)
        {
            int skillPoint = Random.Range(0, skillPpointTotal + 1) + 1;
            //记录当前技能的技能点
            dicSkillGroupPoint.Add(skillGroupIdArray[i], skillPoint);
            //减去使用掉的技能点
            skillPpointTotal -= skillPoint;
        }
        dicSkillGroupPoint[skillGroupIdArray[0]] += skillPpointTotal; //剩余技能点 全部加进第一个技能

        return dicSkillGroupPoint;
    }
    #endregion

    #region 冒险者名称

    //获取 冒险者名称 姓氏
    private int GetVenturerSpawnSurNameId(Venturer_RaceClan cfg)
    {
        //随机 词语组Id
        int index = Random.Range(0, cfg.SurnameGroupIds.Count);
        int wordGroupId = cfg.SurnameGroupIds[index];
        var cfgWordGroup = ConfigSystem.Instance.GetConfig<Language_WordGroup>(wordGroupId);

        //随机 多语言表ID
        return Random.Range(cfgWordGroup.WordIdsStart, cfgWordGroup.WordIdsEnd + 1);
    }

    //获取 冒险者名称 名称
    private int GetVenturerSpawnNameId(int genderId, Venturer_RaceClan cfg)
    {
        string data = string.Empty;
        if (!cfg.NameGroupIds.TryGetValue(genderId, out data)) return 0;

        var groupIds = data.Split('|');

        int index = Random.Range(0, cfg.NameGroupIds.Count);
        int wordGroupId = int.Parse(groupIds[index]);
        var cfgWordGroup = ConfigSystem.Instance.GetConfig<Language_WordGroup>(wordGroupId);

        return Random.Range(cfgWordGroup.WordIdsStart, cfgWordGroup.WordIdsEnd + 1);
    }

    //获取 冒险者名称 特殊名
    private int GetVenturerSpawnExtraNameId(Venturer_RaceClan cfg)
    {
        int index = Random.Range(0, cfg.ExtranameGroupIds.Count);
        int wordGroupId = cfg.ExtranameGroupIds[index];
        var cfgWordGroup = ConfigSystem.Instance.GetConfig<Language_WordGroup>(wordGroupId);

        return Random.Range(cfgWordGroup.WordIdsStart, cfgWordGroup.WordIdsEnd + 1);
    }

    #endregion

    #endregion

    #region 冒险者行动状态改变
    //消息 委托开始
    private void MsgEntrustStart(IMessage rMessage)
    {
        var info = (EntrustCommonInfo)rMessage.Data;
        var venturerIds = info.GetVenturerTeam();
        for (int i = 0; i < venturerIds.Length; i++)
        {
            //冒险者状态 设置为 执行委托中
            SetVenturerState(venturerIds[i], VenturerInfo.EVenturerState.Entrust);
        }
    }

    //消息 委托结算
    private void MsgEntrustStatement(IMessage rMessage)
    {
        var info = (EntrustCommonInfo)rMessage.Data;
        var venturerIds = info.GetVenturerTeam();
        for (int i = 0; i < venturerIds.Length; i++)
        {
            //冒险者状态 设置为 空闲
            SetVenturerState(venturerIds[i], VenturerInfo.EVenturerState.Idle);
        }
    }

    #endregion
    #endregion

    #region 执行回合

    /// <summary>
    /// 事件 执行下一回合
    /// </summary>
    private void MsgExecuteFinishTurn(IMessage rMessage)
    {
        ExecuteTurnVenturerPool(); //执行回合 冒险者池

        //更新所有冒险者信息
        foreach (var venturerInfo in m_MapVenturerInfoPool.Values)
        {
            //生命回合数
            venturerInfo.LifeTurnCur++;
            //是否到达生命回合数上限
            if (venturerInfo.LifeTurnCur > venturerInfo.LifeTurnLimit)
            {
                venturerInfo.State = VenturerInfo.EVenturerState.Dead;
            }
            MessageDispatcher.SendMessage(VenturerModelMsgType.VENTURERMODEL_INFO_LIFETURNCUR_CHANGE);

            //是否升级
            var cfgVenturerLevelNext = ConfigSystem.Instance.GetConfig<Venturer_Level>(venturerInfo.Level + 1);
            if (cfgVenturerLevelNext != null && venturerInfo.Exp >= cfgVenturerLevelNext.ExpRequire)
            {
                //提升等级
                venturerInfo.Level++;
                MessageDispatcher.SendMessage(VenturerModelMsgType.VENTURERMODEL_INFO_EXP_CHANGE);
                MessageDispatcher.SendMessage(VenturerModelMsgType.VENTURERMODEL_INFO_LEVEL_CHANGE);
            }
        }
    }

    #endregion
}