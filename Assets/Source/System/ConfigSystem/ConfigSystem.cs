using Deploy;
using FsGameFramework;
using FsGridCellSystem;
using FsStoryIncident;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 配置模块
/// </summary>
public class ConfigSystem : Singleton<ConfigSystem>, IDestroy
{
    #region LoadObjectContainer Class
    /// <summary>
    /// 加载容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoadObjectContainer<T> where T : UnityEngine.Object
    {
        private readonly Dictionary<string, T> m_Dic = new Dictionary<string, T>();

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public T Get(string keyName)
        {
            if (m_Dic.TryGetValue(keyName, out T config))
            {
                return config;
            }
            else
            {
                Debugx.LogNomWarning($"GetData: TryGetValue failed! KeyName:{keyName}");
                return null;
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="label"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public IEnumerator Load(string label, Addressables.MergeMode mode = Addressables.MergeMode.Union)
        {
            yield return Load(new List<string>() { label }, mode);
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public IEnumerator Load(List<string> labels, Addressables.MergeMode mode = Addressables.MergeMode.Union)
        {
            string typeName = typeof(T).Name;

            Debugx.LogNom($" ===== On load {typeName} : Start! =====");

            AsyncOperationHandle<IList<T>> asyncOperationHandleUWorldConfig = Addressables.LoadAssetsAsync<T>(labels, OnLoad, mode);
            while (!asyncOperationHandleUWorldConfig.IsDone) yield return null;

            Debugx.LogNom($" ===== On load {typeName} : Done! =====");
        }

        //Addressables加载回调
        private void OnLoad(T data)
        {
            if (data == null)
            {
                Debugx.LogNomWarning($"OnLoadData: Data is Null! TypeName:{typeof(T).Name}");
                return;
            }

            Debugx.LogNom($"OnLoadData: Load data Success! dataName:{data.name}");

            m_Dic[data.name] = data;
        }
    }
    #endregion

    /// <summary>
    /// 故事事件初始化配置
    /// </summary>
    public LoadObjectContainer<StoryIncidentInitConfig> StoryIncidentInitConfigContainer { get; private set; }

    /// <summary>
    /// 游戏框架，世界配置
    /// </summary>
    public LoadObjectContainer<UWorldConfig> UWorldConfigContainer { get; private set; }

    /// <summary>
    /// 初始化 配置表模块 (流程模块调用)
    /// </summary>
    /// <param name="action"></param>
    public void Init(Action action)
    {
        WindowSystem.Instance.StartCoroutine(LoadConfig(action));
    }

    private IEnumerator LoadConfig(Action action) //从本地加载配置表数据
    {
        //配置文件 Excel
        LogUtil.Log("===OnLoadConfig:Start!", Color.yellow);
        AsyncOperationHandle<IList<TextAsset>> asyncOperationHandle = Addressables.LoadAssetsAsync<TextAsset>(new List<string>() { "configexcel" }, ActLoadConfig, Addressables.MergeMode.Union);
        while (!asyncOperationHandle.IsDone)
        {
            yield return null;
        }
        if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
        {
            LogUtil.Log($"===OnLoadConfig:Done! Count-{asyncOperationHandle.Result.Count}", Color.yellow);
            Addressables.Release(asyncOperationHandle);
        }
        else
        {
            LogUtil.Log("===OnLoadConfig:Faild!", Color.red);
        }
        InitConfigSkillGroup(); //初始化 技能组配置

        //配置文件 ConfigBuildingGrid 建筑网格数据
        LogUtil.Log("===OnLoadConfig:Start!", Color.yellow);
        AsyncOperationHandle<IList<TextAsset>> asyncOperationHandleBuildingGrid = Addressables.LoadAssetsAsync<TextAsset>(new List<string>() { "configbuildinggrid" }, ActLoadConfigBuildingGrid, Addressables.MergeMode.Union);
        while (!asyncOperationHandleBuildingGrid.IsDone)
        {
            yield return null;
        }
        if (asyncOperationHandleBuildingGrid.Status == AsyncOperationStatus.Succeeded)
        {
            LogUtil.Log($"===OnLoadConfigBuildingGrid:Done! Count-{asyncOperationHandleBuildingGrid.Result.Count}", Color.yellow);
            Addressables.Release(asyncOperationHandleBuildingGrid);
        }
        else
        {
            LogUtil.Log("===OnLoadConfigBuildingGrid:Faild!", Color.red);
        }

        //着色器
        LogUtil.Log("===OnLoadShader:Start!", Color.yellow);
        AsyncOperationHandle<IList<Shader>> asyncOperationHandleShader = Addressables.LoadAssetsAsync<Shader>(new List<string>() { "shader" }, ActLoadShader, Addressables.MergeMode.Union);
        while (!asyncOperationHandleShader.IsDone)
        {
            yield return null;
        }
        LogUtil.Log("===OnLoadShader:Done!", Color.yellow);

        //加载游戏框架，世界配置
        UWorldConfigContainer = new LoadObjectContainer<UWorldConfig>();
        yield return UWorldConfigContainer.Load("configWorld");

        //加载故事事件初始化配置
        StoryIncidentInitConfigContainer = new LoadObjectContainer<StoryIncidentInitConfig>();
        yield return StoryIncidentInitConfigContainer.Load("configStoryIncidentInit");

        //加载配置完成 回调
        action?.Invoke();
    }

    #region ConfigExcel 配置文件
    private Dictionary<Type, IDictionary> m_ConfigMaps = new Dictionary<Type, IDictionary>(); //配置文件 Excel

    /// <summary>
    /// 获取 配置表 全部配置项
    /// </summary>
    /// <typeparam name="T">配置表类型</typeparam>
    /// <returns></returns>
    public Dictionary<int, T> GetConfigMap<T>() where T : Google.Protobuf.IMessage
    {
        IDictionary configs;
        if (m_ConfigMaps.TryGetValue(typeof(T), out configs))
        {
            var dic = new Dictionary<int, T>();
            foreach (var id in configs.Keys)
            {
                dic.Add((int)id, (T)configs[id]);
            }
            return dic;
        }
        else
        {
            LogUtil.Log($"===GetConfigMap:Null! Config-{typeof(T).Name}", Color.red);
            return null;
        }
    }

    /// <summary>
    /// 获取 配置表 单条配置项
    /// </summary>
    /// <typeparam name="T">配置表类型</typeparam>
    /// <param name="id">配置项ID</param>
    /// <param name="isShowNullLog"></param>
    /// <returns></returns>
    public T GetConfig<T>(int id, bool isShowNullLog = true) where T : Google.Protobuf.IMessage
    {
        IDictionary configs = null;
        m_ConfigMaps.TryGetValue(typeof(T), out configs);

        if (isShowNullLog && configs == null)
        {
            Debug.LogError($"CharacterBase.SetSpineSkinPartProp() Error! >> 无效的{typeof(T).Name}配置表ID-{id}");
            return default(T);
        }

        if (configs.Contains(id))
        {
            return (T)configs[id];
        }
        else
        {
            //LogUtil.Log($"===GetConfig:ID Null! Config-{typeof(T).Name} ID-{id}", Color.red);
            return default(T);
        }
    }

    private void ActLoadConfig(TextAsset textAsset) //回调 Addressables加载配置表
    {
        if (textAsset == null || textAsset.bytes.Length == 0)
        {
            LogUtil.Log($"↓↓↓OnLoadConfig:Faild! {textAsset.name}", Color.red);
            return;
        }

        LogUtil.Log($"↓↓↓OnLoadConfig:Success! {textAsset.name}", Color.cyan);

        //反序列化
        Type mapType = Type.GetType("Deploy." + textAsset.name + "_Map");
        MessageParser parser = mapType.GetProperty("Parser").GetMethod.Invoke(null, null) as MessageParser;
        IMessage mapInstance = parser.ParseFrom(textAsset.bytes) as IMessage;
        IDictionary dictionary = mapType.GetProperty("Items").GetMethod.Invoke(mapInstance, null) as IDictionary;
        Type itemType = Type.GetType("Deploy." + textAsset.name);

        m_ConfigMaps.Add(itemType, dictionary);
    }

    #region 技能组
    //字典 技能组:[阶级][技能ID列表]
    private Dictionary<int, Dictionary<int, List<int>>> m_DicConfigSkillGroupSkillIds = new Dictionary<int, Dictionary<int, List<int>>>();

    //初始化 技能组配置文件
    private void InitConfigSkillGroup()
    {
        m_DicConfigSkillGroupSkillIds.Clear();

        var cfgMapSkill = GetConfigMap<Skill_Config>();
        var cfgMapSkillGroup = GetConfigMap<Skill_Group>();
        foreach (var cfgSkill in cfgMapSkill.Values)
        {
            foreach (var cfgSkillGroup in cfgMapSkillGroup.Values)
            {
                //技能派系 比对
                if (cfgSkill.Faction != cfgSkillGroup.Faction) continue;
                //技能类型 比对
                if (cfgSkill.Type != cfgSkillGroup.Type) continue;
                //元素组合比对
                if (cfgSkill.Element.Count != cfgSkillGroup.Element.Count) continue;
                for (int i = 0; i < cfgSkill.Element.Count; i++)
                {
                    if (cfgSkill.Element[i] != cfgSkillGroup.Element[i]) continue;
                }

                //添加至技能组记录
                var skillIds = GetConfigSkillGroupSkillIds(cfgSkillGroup.Id, cfgSkill.Rank);
                skillIds.Add(cfgSkill.Id);
                break;
            }
        }
    }

    /// <summary>
    /// 获取 技能组 指定阶级的技能列表
    /// </summary>
    /// <param name="skillGroupId">技能组ID</param>
    /// <param name="rank">阶级</param>
    public List<int> GetConfigSkillGroupSkillIds(int skillGroupId, int rank)
    {
        Dictionary<int, List<int>> dicRankSkillIds = null;
        if (!m_DicConfigSkillGroupSkillIds.TryGetValue(skillGroupId, out dicRankSkillIds))
        {
            dicRankSkillIds = new Dictionary<int, List<int>>();
            m_DicConfigSkillGroupSkillIds.Add(skillGroupId, dicRankSkillIds);
        }
        List<int> skillIds = null;
        if (!dicRankSkillIds.TryGetValue(rank, out skillIds))
        {
            skillIds = new List<int>();
            dicRankSkillIds.Add(rank, skillIds);
        }

        return skillIds;
    }

    #endregion
    #endregion

    #region ConfigBuildingGrid
    /// <summary>
    /// 网格配置数据
    /// </summary>
    public class GridItemData
    {
        /// <summary>
        /// 网格坐标
        /// </summary>
        public GridCoord GridCoord;

        /// <summary>
        /// 可设置
        /// </summary>
        public bool CanSet;

        /// <summary>
        /// 可行走
        /// </summary>
        public bool CanWalk;

        /// <summary>
        /// 是地面
        /// </summary>
        public bool IsGround;

        /// <summary>
        /// 是墙面
        /// </summary>
        public bool IsWall;
    }

    private Dictionary<int, List<GridItemData>> m_DicConfigBuildingGrid = new Dictionary<int, List<GridItemData>>();

    /// <summary>
    /// 获取配置 建筑网格数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<GridItemData> GetConfigBuildingGrid(int id)
    {
        List<GridItemData> gridCellDataList = null;
        if (m_DicConfigBuildingGrid.TryGetValue(id, out gridCellDataList))
        {
            return gridCellDataList;
        }

        return new List<GridItemData>();
    }

    //回调 Addressables加载配置表
    private void ActLoadConfigBuildingGrid(TextAsset textAsset)
    {
        if (textAsset == null || textAsset.bytes.Length == 0)
        {
            LogUtil.Log($"↓↓↓OnLoadConfigBuildingGrid:Faild! {textAsset.name}", Color.red);
            return;
        }

        LogUtil.Log($"↓↓↓OnLoadConfigBuildingGrid:Success! {textAsset.name}", Color.cyan);

        //反序列化
        List<GridItemData> gridCellDataList = new List<GridItemData>();
        string strData = textAsset.text;
        strData = strData.TrimStart('{');
        strData = strData.TrimEnd('}');
        string[] strDatas = strData.Split(',');

        for (int i = 0; i < strDatas.Length; i++)
        {
            string[] dataAll = strDatas[i].Split(':');

            //网格状态
            string[] dataState = dataAll[0].Split('|');
            bool canSet = dataState[0] == "1";
            bool canWalk = dataState[1] == "1";
            bool isGround = dataState[2] == "1";
            bool isWall = dataState[3] == "1";

            string[] dataCoordAll = dataAll[1].Split('|');
            for (int j = 0; j < dataCoordAll.Length; j++)
            {
                var gridCellData = new GridItemData();

                //网格坐标
                var dataCoord = dataCoordAll[j].Split('-');
                gridCellData.GridCoord = new GridCoord(int.Parse(dataCoord[0]), int.Parse(dataCoord[1]), int.Parse(dataCoord[2]));
                //设置 网格状态
                gridCellData.CanSet = canSet;
                gridCellData.CanWalk = canWalk;
                gridCellData.IsGround = isGround;
                gridCellData.IsWall = isWall;

                gridCellDataList.Add(gridCellData);
            }
        }

        int id = int.Parse(textAsset.name);

        m_DicConfigBuildingGrid.Add(id, gridCellDataList);
    }
    #endregion

    #region Shader 着色器
    private Dictionary<string, Shader> m_ShaderMaps = new Dictionary<string, Shader>(); //着色器

    /// <summary>
    /// 获取 Shader
    /// </summary>
    /// <param name="shaderName"></param>
    /// <returns></returns>
    public Shader GetShader(string shaderName)
    {
#if UNITY_EDITOR
        //编辑器模式 AB包内的Shader格式为Android或其他平台时不支持 导致显示异常
        if (UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 2)
        {
            Shader shader1 = Shader.Find(shaderName);
            if (shader1 == null)
                LogUtil.Log($"===GetShader:Null! ShaderName-{shaderName}", Color.red);

            return shader1;
        }
#endif
        Shader shader = null;
        if (m_ShaderMaps.ContainsKey(shaderName))
            shader = m_ShaderMaps[shaderName];

        if (shader == null)
            LogUtil.Log($"===GetShader:Null! ShaderName-{shaderName}", Color.red);

        return shader;
    }

    private void ActLoadShader(Shader shader) //回调 Addressables加载Shader
    {
        if (shader == null)
        {
            LogUtil.Log($"↓↓↓OnLoadShader:Null! {shader.name}", Color.red);
            return;
        }

        if (!shader.isSupported)
            LogUtil.Log($"↓↓↓OnLoadShader:Error! Shader is not Supported! shaderName-{shader.name}", Color.red);
        else
            LogUtil.Log($"↓↓↓OnLoadShader:Success! {shader.name}", Color.cyan);

        m_ShaderMaps[shader.name] = shader;
    }

    /// <summary>
	/// 实例化 材质球
	/// </summary>
	/// <param name="shaderName"></param>
	/// <returns></returns>
	public Material CreateMaterial(string shaderName)
    {
        Material material = null;
        Shader shader = null;
#if UNITY_EDITOR
        shader = Shader.Find(shaderName);
#else
		shader = ConfigModel.Instance.GetShader(shaderName);
#endif
        if (shader == null)
            Debug.LogError($"材质球实例化错误：Material实例化失败 shaderName-{shaderName}");
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
        }

        return material;
    }
    #endregion

    #region 多语言
    /// <summary>
    /// 获取 多语言表 值
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetConfigLanguageValue(int id)
    {
        var cfg = GetConfig<Language_Config>(id);
        if (cfg == null) return string.Empty;

        //根据当前语言设置 返回对应语言文本
        return cfg.Chinese;
    }
    #endregion

    #region 获取配置表ID
    /// <summary>
    /// 获取 配置表ID Guild_BuildingLevel
    /// </summary>
    /// <param name="buildingId"></param>
    /// <param name="buildingLevel"></param>
    /// <returns></returns>
    public int GetConfigIdGuildBuildingLevel(int buildingId, int buildingLevel)
    {
        return buildingId * 100 + buildingLevel;
    }
    #endregion
}
