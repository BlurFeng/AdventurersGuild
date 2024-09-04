using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FsGameFramework
{
    /// <summary>
    /// 世界类型
    /// </summary>
    public enum WorldType
    {
        Default,//默认类型 同时只存在一个活动的Level
        BigWorld,//大世界 支持多个Level组合 分区域加载 20210505：未完成的功能
    }

    /// <summary>
    /// 世界类 管理属于自己的关卡ULevel
    /// </summary>
    [System.Serializable]
    public class UWorld : UObjectLife
    {
        //TODO：大世界无缝衔接Level的功能未完成

        public WorldType WorldType { get; private set; }

        private string m_Name;
        /// <summary>
        /// 世界名称
        /// </summary>
        public string Name { get { return m_Name; } }

        /// <summary>
        /// 世界中的所有ULevel关卡 一个World可以包含多个Level，由多个Level组成大世界，但必须包含起码一个Level
        /// WorldType.Default模式时，此Dic中只会有一个Level
        /// Key = ULevelConfig.m_LevelName (有同名时将添加_Index)
        /// </summary>
        private Dictionary<string, ULevel> m_LevelDic;

        /// <summary>
        /// 当前关卡
        /// </summary>
        public ULevel CurrentLevel
        {
            get { return m_CurrentLevel; }
            private set
            {
                m_CurrentLevel = value;
            }
        }

        /// <summary>
        /// 世界管理的所有的Actor 一般只管理玩家控制器或玩家控制的角色等不特定从属于某个Level的对象
        /// </summary>
        private Dictionary<string, List<AActor>> m_ActorsDic;

        private List<APlayerController> m_PlayerControllers;
        private List<APawn> m_PlayerPawns;

        /// <summary>
        /// 当前的关卡
        /// </summary>
        private ULevel m_CurrentLevel;

        /// <summary>
        /// 当前世界配置的游戏模式
        /// </summary>
        private UGameMode m_GameMode;

        /// <summary>
        /// 世界配置数据，关卡配置数据也在其中
        /// </summary>
        public UWorldConfig WorldConfig { get; private set; }

        /// <summary>
        /// 配置在世界配置中的关卡配置的快速查询字典
        /// Key = ULevelConfig.m_LevelName, Vaule = WorldConfig.levelConfigs的Index
        /// </summary>
        public Dictionary<string, int> m_LevelConfigIndexDic;

        /// <summary>
        /// 世界根节点GameObject
        /// </summary>
        public GameObject RootGameObject { get; private set; }

        /// <summary>
        /// 世界根节点Transform 用于挂载属于此世界的GameObject内容
        /// </summary>
        public Transform RootTs { get; private set; }


        /// <summary>
        /// 实例化UWorld对象
        /// </summary>
        /// <param name="worldConfig">世界配置信息文件</param>
        public UWorld(UWorldConfig worldConfig)
        {
            //在此初始化没有GameObject的 纯数据或逻辑成员


            //实例化成员
            m_LevelDic = new Dictionary<string, ULevel>();
            m_ActorsDic = new Dictionary<string, List<AActor>>();
            m_PlayerControllers = new List<APlayerController>();
            m_PlayerPawns = new List<APawn>();
            m_LevelConfigIndexDic = new Dictionary<string, int>();

            //根据传入的世界配置文件 生成数据
            WorldConfig = worldConfig;
            if (WorldConfig == null || !WorldConfig.CheckDataValid()) return;

            //配置世界信息
            WorldType = worldConfig.type;
            m_Name = string.IsNullOrEmpty(worldConfig.worldName)? "World" : worldConfig.worldName;

            //处理levelConfigs相关信息
            //确认并移除无效的LevelConfig
            for (int i = WorldConfig.levelConfigs.Count - 1; i >=0 ; i--)
            {
                if(WorldConfig.levelConfigs[i] == null || !WorldConfig.levelConfigs[i].CheckDataValid())
                {
                    WorldConfig.levelConfigs.RemoveAt(i);
                }
            }
            //缓存LevelConfig的Name对应在levelConfigs中的缓存，方便之后查阅
            for (int i = 0; i < WorldConfig.levelConfigs.Count; i++)
            {
                string key = WorldConfig.levelConfigs[i].name;
                GetNonRepeatingKey(m_LevelConfigIndexDic, ref key);
                m_LevelConfigIndexDic.Add(key, i);
            }

            switch (WorldType)
            {
                case WorldType.Default:
                    //m_PersistentLevel = new ULevel(worldConfig.persistentLevel, this);
                    break;
                case WorldType.BigWorld:
                    ////初始化ULevel关卡
                    //if (worldConfig.levelConfigs.Count > 0)
                    //{
                    //    for (int i = 0; i < worldConfig.levelConfigs.Count; i++)
                    //    {
                    //        var level = new ULevel(worldConfig.levelConfigs[i], this);

                    //        if (i == 0) m_PersistentLevel = level;
                    //    }
                    //}
                    //else
                    //{
                    //    throw new System.Exception(
                    //        "The WorldConfig's levelConfigs needs at least one levelConfig!"
                    //        + "世界配置信息中的关卡配置信息数组起码需要一个关卡配置信息！");
                    //}
                    break;
            }

            #region GameMode create
            var gameModeConfig = worldConfig.gameModeConfig;
            if (null != gameModeConfig)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                //初始化GamoMode类
                m_GameMode = assembly.CreateInstance(gameModeConfig.GameModeClass) as UGameMode;
                if (null == m_GameMode)
                    throw new System.Exception("WorldConfig.GameModeConfig.GameModeClass is bad! 游戏模式配置中的游戏模式类名不正确！不是继承于UGameMode的类型！");

                //初始化GameState类
                var gameState = assembly.CreateInstance(gameModeConfig.GameStateClass) as UGameState;
                if (null != gameState)
                    m_GameMode.GameState = gameState;
                else
                    throw new System.Exception("WorldConfig.GameModeConfig.GameStateClass is bad! 游戏模式配置中的游戏数据类名不正确！不是继承于UGameState的类型！");
            }
            #endregion
        }

        public override void Init()
        {
            if (WorldConfig == null) return;
            //在此初始化和GameObject相关联的内容

            //实例化世界根节点
            RootGameObject = new GameObject();
            RootGameObject.name = m_Name;
            RootTs = RootGameObject.transform;

            //切换到主关卡
            SwitchLevel(WorldConfig.persistentLevel);

            #region Create PlayerController and DefaultPawn

            if (WorldConfig.gameModeConfig != null)
            {
                var gameModeConfig = WorldConfig.gameModeConfig;

                //初始化玩家控制器和玩家数据
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var playerControllerObj = GameObject.Instantiate(gameModeConfig.PlayerController);
                APlayerController playerController;
                if (null != playerControllerObj)
                {
                    playerController = playerControllerObj.GetComponent<APlayerController>();
                    if (null != playerController)
                    {
                        //玩家控制器从属于世界 因为关卡会频繁切换而控制器是持久性的
                        //切换世界时需要按需转移此数据
                        playerController.Init(this);

                        m_PlayerControllers.Add(playerController);

                        playerController.PlayerState = assembly.CreateInstance(gameModeConfig.PlayerStateClass) as UPlayerState;
                        if (null == playerController.PlayerState)
                            throw new System.Exception("WorldConfig.GameModeConfig.PlayerStateClass is bad!游戏模式配置中的玩家数据类名不正确！不是继承于UPlayerState的类型！");
                    }
                    else
                    {
                        GameObject.Destroy(playerController);
                        throw new System.Exception("WorldConfig.GameModeConfig.PlayerControllerClass is bad!游戏模式配置中的玩家控制器预制体没有APlayerController类挂载！");
                    }
                }
                else
                    throw new System.Exception("WorldConfig.GameModeConfig.PlayerController is bad!游戏模式配置中的玩家控制器预制体无法加载！");

                //初始化默认Pawn
                var defaultPawnObj = GameObject.Instantiate(gameModeConfig.DefaultPawn);
                if (defaultPawnObj != null)
                {
                    var defaultPawn = defaultPawnObj.GetComponent<APawn>();
                    if (null != defaultPawn)
                    {
                        //玩家默认Pawn从属于世界 因为关卡会频繁切换而默认Pawn是持久性的
                        //切换世界时需要按需转移此数据
                        defaultPawn.Init(this);
                        m_PlayerPawns.Add(defaultPawn);

                        //关卡配置信息可能为空
                        if (null != m_CurrentLevel.LevelTerrainConfig)
                        {
                            //设置玩家角色到出生位置
                            if (m_CurrentLevel.LevelTerrainConfig.PlayerStart != null)
                            {
                                defaultPawn.TransformGet.position = m_CurrentLevel.LevelTerrainConfig.PlayerStart.TransformGet.position;
                            }
                        }

                        playerController.Possess(defaultPawn);
                    }
                    else
                    {
                        GameObject.Destroy(defaultPawnObj);
                        throw new System.Exception("WorldConfig.GameModeConfig.DefaultPawn is bad!游戏模式配置中的默认Pawn预制体没有APawn类脚本挂载！");
                    }
                }
                else
                    throw new System.Exception("WorldConfig.GameModeConfig.DefaultPawn is bad!游戏模式配置中的默认Pawn预制体无法加载！");
            }
            else
            {
                throw new System.Exception("WorldConfig.GameModeConfig is null!世界的游戏配置为空！");
            }
            #endregion
        }

        public override void ResetSelf()
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.ResetSelf();
                    break;
                case WorldType.BigWorld:
                    foreach (var level in m_LevelDic.Values)
                    {
                        level.ResetSelf();
                    }
                    break;
            }

            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].ResetSelf();
                }
            }
        }

        public override void DestroySelf()
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.DestroySelf();
                    m_CurrentLevel = null;
                    m_LevelDic.Clear();
                    break;
                case WorldType.BigWorld:
                    ULevel[] levelsTemp = new ULevel[m_LevelDic.Values.Count];
                    int index = 0;
                    //销毁所有的Levels
                    foreach (var level in m_LevelDic.Values)
                    {
                        levelsTemp[index] = level;
                        index++;
                    }
                    for (int i = 0; i < levelsTemp.Length; i++)
                    {
                        levelsTemp[i].DestroySelf();
                    }
                    m_LevelDic.Clear();
                    break;
            }

            //销毁所有世界管理的Actor
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].DestroySelf();
                }
            }
            m_ActorsDic.Clear();

            //销毁世界根节点
            GameObject.Destroy(RootGameObject);

            m_ActorsDic.Clear();
        }

        public override void Begin()
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.Begin();
                    break;
                case WorldType.BigWorld:
                    foreach (var level in m_LevelDic.Values)
                    {
                        level.Begin();
                    }
                    break;
            }

            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].Begin();
                }
            }
        }

        public override void Tick(float deltaTime)
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.Tick(deltaTime);
                    break;
                case WorldType.BigWorld:
                    foreach (var level in m_LevelDic.Values)
                    {
                        level.Tick(deltaTime);
                    }
                    break;
            }
            

            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].Tick(deltaTime);
                }
            }
        }

        public override void LateTick(float deltaTime)
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.LateTick(deltaTime);
                    break;
                case WorldType.BigWorld:
                    foreach (var level in m_LevelDic.Values)
                    {
                        level.LateTick(deltaTime);
                    }
                    break;
            }

            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].LateTick(deltaTime);
                }
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            switch (WorldType)
            {
                case WorldType.Default:
                    m_CurrentLevel.FixedTick(fixedDeltaTime);
                    break;
                case WorldType.BigWorld:
                    foreach (var level in m_LevelDic.Values)
                    {
                        level.FixedTick(fixedDeltaTime);
                    }
                    break;
            }

            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].FixedTick(fixedDeltaTime);
                }
            }
        }

        #region Level
        /// <summary>
        /// 添加关卡ULevel到世界中
        /// </summary>
        /// <param name="level"></param>
        public void AddLevel(ULevel level)
        {
            string key = level.Name;

            //确保key不会重复 新建关卡Config时请确保同一个世界中没有添加名称相同的关卡
            //因为切换到某个关卡时会需要使用到名称进行查询
            GetNonRepeatingKey(m_LevelDic, ref key);

            m_LevelDic.Add(key, level);
        }

        /// <summary>
        /// 获取一个不重复的Key
        /// </summary>
        /// <typeparam name="T">字典Vaule的类型</typeparam>
        /// <param name="dic">想要存入的字典</param>
        /// <param name="key">确认的Key，如果已经包含，会改变此值</param>
        /// <returns>传入的Key是否被改变</returns>
        private bool GetNonRepeatingKey<T>(Dictionary<string,T> dic, ref string key)
        {
            if (dic.ContainsKey(key))
            {
                int index = 1;
                string keyTemp = key + "_" + index;
                while (dic.ContainsKey(keyTemp))
                {
                    index += 1;
                    keyTemp = key + "_" + index;
                }

                key = keyTemp;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除关卡ULevel从世界中
        /// </summary>
        /// <param name="level">移除的关卡</param>
        public void RemoveLevel(ULevel level)
        {
            RemoveLevel(level.Name);
        }

        /// <summary>
        /// 移除关卡ULevel从世界中
        /// </summary>
        /// <param name="LevelName">移除的关卡名称</param>
        public void RemoveLevel(string LevelName)
        {
            m_LevelDic.Remove(LevelName);
        }

        /// <summary>
        /// 切换关卡，仅在世界类型为默认时可用此方法切换当前关卡
        /// </summary>
        /// <param name="levelConfig">切换目标关卡</param>
        /// <param name="passagewayIndex">玩家传送都目标关卡的哪个通道 关卡配置的通道数组index</param>
        /// <param name="force">不对比现在的关卡和目标关卡是否一直 强制执行切换</param>
        public bool SwitchLevel(ULevelConfig levelConfig, int passagewayIndex = -1, bool force = false)
        {
            //世界类型为默认时可用此方法进行当前关卡的切换
            //默认类型的世界同时只存在一个大概的关卡
            if (WorldType != WorldType.Default) return false;

            if (m_CurrentLevel != null)
            {
                if (m_CurrentLevel.LevelTerrainConfig == levelConfig && !force) return false;

                //销毁旧关卡
                m_CurrentLevel.DestroySelf();
            }

            //实例化新关卡
            m_CurrentLevel = new ULevel(levelConfig, this);
            m_CurrentLevel.Init();

            //设置玩家角色到目标位置
            Vector3 pos = Vector3.zero;
            if(passagewayIndex >= 0 && passagewayIndex < m_CurrentLevel.LevelTerrainConfig.passageways.Count)
            {
                pos = m_CurrentLevel.LevelTerrainConfig.passageways[passagewayIndex].position;
            }
            else
            {
                if(m_CurrentLevel.LevelTerrainConfig.PlayerStart != null)
                {
                    pos = m_CurrentLevel.LevelTerrainConfig.PlayerStart.TransformGet.position;
                }
            }

            for (int i = 0; i < m_PlayerPawns.Count; i++)
            {
                m_PlayerPawns[i].TransformGet.position = pos;
            }

            return true;
        }

        /// <summary>
        /// 切换关卡，仅在世界类型为默认时可用此方法切换当前关卡
        /// </summary>
        /// <param name="levelName">关卡名称</param>
        /// <returns></returns>
        public bool SwitchLevel(string levelName)
        {
            if (!GetLevelConfigByName(levelName, out ULevelConfig levelConfig)) return false;

            return SwitchLevel(levelConfig);
        }

        /// <summary>
        /// 根据关卡名称获取观关卡配置
        /// 此方法只能获取配置到当前UWorldConfig列表中的关卡
        /// </summary>
        /// <param name="levelName">关卡名称，ULevelConfig文件中配置</param>
        /// <param name="levelConfig">关卡配置</param>
        /// <returns>是否查找到此名称的关卡配置</returns>
        public bool GetLevelConfigByName(string levelName, out ULevelConfig levelConfig)
        {
            levelConfig = null;
            if (!m_LevelConfigIndexDic.ContainsKey(levelName)) return false;

            levelConfig = WorldConfig.levelConfigs[m_LevelConfigIndexDic[levelName]];
            return true;
        }
        #endregion

        #region Actor
        /// <summary>
        /// 添加Actor到世界容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        /// <returns></returns>
        public void AddActor(AActor actor, string typeTag)
        {
            if (!m_ActorsDic.ContainsKey(typeTag))
            {
                m_ActorsDic.Add(typeTag, new List<AActor>());
            }

            if (!m_ActorsDic[typeTag].Contains(actor))
                m_ActorsDic[typeTag].Add(actor);
        }

        /// <summary>
        /// 移除Actor从世界容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        public void RemoveActor(AActor actor, string typeTag)
        {
            if (m_ActorsDic.ContainsKey(typeTag))
            {
                m_ActorsDic[typeTag].Remove(actor);
            }
        }

        /// <summary>
        /// 获取所有的某一类AActor
        /// </summary>
        /// <typeparam name="T">继承自AActor类的类型</typeparam>
        /// <returns></returns>
        public List<T> GetActors<T>() where T : AActor
        {
            string type = typeof(T).ToString();
            List<T> returnActors = new List<T>();

            foreach (var level in m_LevelDic.Values)
            {
                var tempActors = level.GetActors<T>();
                if (null != tempActors)
                {
                    returnActors.AddRange(tempActors);
                }
            }

            //搜索并添加自身管理的Actor
            if (m_ActorsDic.ContainsKey(type))
                returnActors.AddRange(m_ActorsDic[type].ConvertAll(x => x as T));

            if (returnActors.Count > 0) return returnActors;

            return null;
        }
        #endregion
    }
}