using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 关卡 每个关卡有一个UGameMode
    /// 关卡可以理解为一个陆地区域（在大世界时） 或者一个小的房间区域（在不同房间间切换）
    /// 关卡管理着在自己范围内的所有AActor
    /// </summary>
    public class ULevel : UObjectLife
    {
        private string m_Name;
        /// <summary>
        /// 关卡名称 也是所属世界的Level字典中的Key
        /// </summary>
        public string Name { get { return m_Name; } set { m_Name = value; } }

        /// <summary>
        /// 关卡中所有的Actor
        /// </summary>
        private Dictionary<string, List<AActor>> m_ActorsDic;
        private Dictionary<string, List<string>> m_ActorsSubclassDic;
        private Dictionary<string, System.Type> m_ActorTypesDic;

        /// <summary>
        /// 关卡地形预制体
        /// </summary>
        private GameObject m_Terrain;

        /// <summary>
        /// 关卡配置信息
        /// </summary>
        public FMLevelTerrainConfig LevelTerrainConfig { get; private set; }

        public UWorld OwnerWorld { get; private set; }

        public Transform RootTs { get; private set; }


        /// <summary>
        /// 实例化ULevel对象
        /// </summary>
        /// <param name="levelConfig">关卡配置信息文件</param>
        /// <param name="ownerWorld">自己所属的世界对象</param>
        public ULevel(ULevelConfig levelConfig, UWorld ownerWorld)
        {
            if (null == levelConfig) return;

            //实例化成员
            m_ActorsDic = new Dictionary<string, List<AActor>>();
            m_ActorsSubclassDic = new Dictionary<string, List<string>>();
            m_ActorTypesDic = new Dictionary<string, Type>();

            //配置关卡信息
            m_Name = levelConfig.m_LevelName;
            m_Terrain = levelConfig.m_TerrainPrefab;//这里m_Terrain指向的是预制体资源 之后会指向 新实例化的预制体的Clone

            //推送到所属世界容器
            OwnerWorld = ownerWorld;
            OwnerWorld.AddLevel(this);
        }

        public override void Init()
        {
            //实例化关卡预制体
            if (null != m_Terrain)
            {
                m_Terrain = GameObject.Instantiate(m_Terrain, Vector3.zero, Quaternion.identity);
                RootTs = m_Terrain.transform;
                RootTs.SetParent(OwnerWorld.RootGameObject.transform);

                LevelTerrainConfig = m_Terrain.GetComponent<FMLevelTerrainConfig>();

                //获取关卡地形预制体中的AActor并初始化
                var tempActors = m_Terrain.GetComponentsInChildren<AActor>();
                for (int i = 0; i < tempActors.Length; i++)
                {
                    tempActors[i].Init(this);
                }
            }
            else
            {
                throw new System.Exception("Terrain prefab is nll!");
            }
        }

        public override void ResetSelf()
        {
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
            //销毁所有管理的Actor
            foreach (var Actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < Actors.Count; i++)
                {
                    Actors[i].DestroySelf();
                }
            }
            m_ActorsDic.Clear();

            //销毁地形预制体
            if(m_Terrain != null)
                GameObject.Destroy(m_Terrain);

            //从所属的世界容器移除
            OwnerWorld.RemoveLevel(this);
        }

        public override void Begin()
        {
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
            //执行Actor生命周期
            foreach (var actors in m_ActorsDic.Values)
            {
                for (int i = 0; i < actors.Count; i++)
                {
                    actors[i].FixedTick(fixedDeltaTime);
                }
            }
        }

        /// <summary>
        /// 添加Actor到关卡容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        /// <returns></returns>
        internal void AddActor(AActor actor, string typeTag)
        {
            //创建新类型Actor的缓存列表
            if (!m_ActorsDic.ContainsKey(typeTag))
            {
                var actorType = actor.GetType();
                m_ActorTypesDic.Add(typeTag, actorType);

                //新添加的类型 确认和其他已经存储的Actor类型的父子关系
                foreach (var item in m_ActorTypesDic)
                {
                    var type = item.Value;
                    if (actorType.IsSubclassOf(type))
                    {
                        AddSubClassMap(type.ToString(), typeTag);
                    }
                    else if (type.IsSubclassOf(actorType))
                    {
                        AddSubClassMap(typeTag, type.ToString());
                    }
                }

                //获取父类并存储关心信息 直到AActor类
                var baseType = actorType;
                while (baseType != typeof(AActor))
                {
                    baseType = baseType.BaseType;
                    string baseTypeTag = baseType.ToString();
                    if (!m_ActorsDic.ContainsKey(baseTypeTag))
                    {
                        m_ActorsDic.Add(baseTypeTag, new List<AActor>());
                        AddSubClassMap(baseTypeTag, typeTag);
                    }
                }

                m_ActorsDic.Add(typeTag, new List<AActor>());
            }

            //新建时添加 不会有重复
            //if(!m_ActorsDic[typeTag].Contains(actor))

            m_ActorsDic[typeTag].Add(actor);
            actor.Begin();
        }

        /// <summary>
        /// 移除Actor从关卡容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        internal void RemoveActor(AActor actor, string typeTag)
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
        public List<T> GetActors<T>(bool containsSubclass = true) where T : AActor
        {
            string typeStr = typeof(T).ToString();
            List<T> outList = new List<T>();
            if (m_ActorsDic.ContainsKey(typeStr))
            {
                if(m_ActorsDic[typeStr].Count > 0)
                    outList = m_ActorsDic[typeStr].ConvertAll(x => x as T);

                //获取子类
                if (containsSubclass && m_ActorsSubclassDic.ContainsKey(typeStr))
                {
                    for (int i = 0; i < m_ActorsSubclassDic[typeStr].Count; i++)
                    {
                        var list = m_ActorsDic[m_ActorsSubclassDic[typeStr][i]];
                        if (list.Count > 0)
                            outList.AddRange(list.ConvertAll(x => x as T));
                    }
                }

                return outList;
            }

            return null;
        }

        /// <summary>
        /// 添加存储的Actor的父子类关系
        /// </summary>
        /// <param name="superclass"></param>
        /// <param name="subclass"></param>
        internal void AddSubClassMap(string superclass, string subclass)
        {
            if (!m_ActorsSubclassDic.ContainsKey(superclass))
            {
                m_ActorsSubclassDic.Add(superclass, new List<string>());
            }

            if (!m_ActorsSubclassDic[superclass].Contains(subclass))
            {
                m_ActorsSubclassDic[superclass].Add(subclass);
            }
        }
    }
}