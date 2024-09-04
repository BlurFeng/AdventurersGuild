using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FsGameFramework
{
    /// <summary>
    /// 世界容器 管理其他所有的
    /// </summary>
    public static class FWorldContainer
    {
        private static UWorld m_CurrentWorld;

        /// <summary>
        /// 当前UWorld世界
        /// </summary>
        public static UWorld CurrentWorld { get { return m_CurrentWorld; } }
        public static UWorld GetWorld() { return m_CurrentWorld; }

        /// <summary>
        /// 获取当前关卡地图
        /// </summary>
        /// <returns></returns>
        public static ULevel GetLevelCur()
        {
            if (m_CurrentWorld != null)
            {
                return m_CurrentWorld.CurrentLevel;
            }

            return null;
        }

        /// <summary>
        /// 获取当前关切地图名称
        /// </summary>
        /// <returns></returns>
        public static string GetLevelCurName()
        {
            var level = GetLevelCur();
            return level != null ? level.Name : string.Empty;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            m_ActorSceneIDTracer = new Dictionary<string, uint>();
            m_Pawns = new List<APawn>();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public static void ResetSelf()
        {
            m_CurrentWorld.ResetSelf();

            m_ActorSceneIDTracer.Clear();
        }

        /// <summary>
        /// 启动 在初始化之后
        /// </summary>
        public static void Begin()
        {
            m_CurrentWorld?.Begin();
        }

        /// <summary>
        /// 每帧执行，被WorldContainer管理进行。意义同Update
        /// </summary>
        public static void Tick(float deltaTime)
        {
            m_CurrentWorld?.Tick(deltaTime);
        }

        /// <summary>
        /// 在所有Update执行结束后执行，被WorldContainer管理进行。意义同LateUpdate
        /// </summary>
        /// <param name="deltaTime"></param>
        public static void LateTick(float deltaTime)
        {
            m_CurrentWorld?.LateTick(deltaTime);
        }

        /// <summary>
        /// 固定时间间隔执行，被WorldContainer管理进行。意义同FixedUpdate
        /// </summary>
        /// <param name="fixedDeltaTime"></param>
        public static void FixedTick(float fixedDeltaTime)
        {
            m_CurrentWorld?.FixedTick(fixedDeltaTime);
        }

        /// <summary>
        /// 销毁自身
        /// </summary>
        public static void DestroySelf()
        {
            if(m_CurrentWorld != null)
                m_CurrentWorld.DestroySelf();

            m_Pawns.Clear();
        }

        /// <summary>
        /// 切换到目标世界
        /// </summary>
        /// <param name="worldConfig">目标世界配置文件</param>
        /// <param name="force">目标世界和当前世界相同时 是否强制切换</param>
        /// <returns>是否成功执行</returns>
        public static bool SwitchWorld(UWorldConfig worldConfig, bool force = false)
        {
            if (worldConfig == null) return false;

            //销毁当前世界
            if (m_CurrentWorld != null)
            {
                if (worldConfig == m_CurrentWorld.WorldConfig && !force) return false;

                m_CurrentWorld.DestroySelf();
            }

            //初始化新世界
            var world = new UWorld(worldConfig);
            if (!world.WorldConfig.CheckDataValid()) return false;

            world.Init();
            m_CurrentWorld = world;

            if (m_OnLoadLevel != null) m_OnLoadLevel.Invoke(m_CurrentWorld.CurrentLevel);

            return true;
        }

        /// <summary>
        /// 切换到某个关卡 在世界类型为默认时可用
        /// </summary>
        /// <param name="levelConfig">切换目标关卡</param>
        /// <param name="passagewayIndex">玩家传送都目标关卡的哪个通道 关卡配置的通道数组index</param>
        /// <param name="force">不对比现在的关卡和目标关卡是否一直 强制执行切换</param>
        public static void SwitchLevel(ULevelConfig levelConfig, int passagewayIndex = -1, bool force = false)
        {
            if (null == m_CurrentWorld) return;

            m_CurrentWorld.SwitchLevel(levelConfig, passagewayIndex, force);

            if (m_OnLoadLevel != null) m_OnLoadLevel.Invoke(m_CurrentWorld.CurrentLevel);
        }

        #region Actor Manager 管理Actor演员类

        //AActor的SceneID追踪器
        static Dictionary<string, uint> m_ActorSceneIDTracer;

        //所有APawn的引用 不是最根源的缓存 只是为了方便获取在此缓存
        static List<APawn> m_Pawns;
        
        //public static List<APawn> Pawns { get { return m_Pawns; } }

        /// <summary>
        /// 添加Actor到Level
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string AddActor<T>(AActor actor, string typeTag, T owner)
        {
            if (string.IsNullOrEmpty(typeTag)) return string.Empty;

            System.Object ownerTemp;
            //如果传入对象是Actor 那么获取这个Actor的依存对象
            if (owner is AActor)
                ownerTemp = (owner as AActor).GetOuter;
            else
                ownerTemp = owner;

            //依存对象是Level或者World
            if (ownerTemp is ULevel)
            {
                return AddActor(actor, typeTag, ownerTemp as ULevel);
            }
            else if(ownerTemp is UWorld)
            {
                return AddActor(actor, typeTag, ownerTemp as UWorld);
            }
            else
            {
                Debug.LogWarning("Actor is not registered with Owner.");
                return string.Empty;
            }
        }

        /// <summary>
        /// 移除Actor从Level容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        public static void RemoveActor<T>(AActor actor, string typeTag, T owner)
        {
            if (string.IsNullOrEmpty(typeTag)) return;

            System.Object ownerTemp;
            //如果传入对象是Actor 那么获取这个Actor的拥有者作为新添加的Actor的拥有者
            if (owner is AActor)
            {
                ownerTemp = (owner as AActor).GetOuter;
            }
            else
            {
                ownerTemp = owner;
            }


            if (ownerTemp is ULevel)
            {
                RemoveActor(actor, typeTag, ownerTemp as ULevel);
            }
            else if (ownerTemp is UWorld)
            {
                RemoveActor(actor, typeTag, ownerTemp as UWorld);
            }
            else
            {
                Debug.LogWarning("Actor is not registered with Owner.");
            }
        }

        /// <summary>
        /// 添加Actor到Level
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        static string AddActor(AActor actor, string typeTag, ULevel level)
        {
            //添加到所属的Level中
            level.AddActor(actor, typeTag);
            //actor.TransformGet.SetParent(level.RootTs);

            //额外缓存所有的APawn 方便获取
            AddPawn(actor);

            return GetActorSceneID(typeTag);
        }

        /// <summary>
        /// 移除Actor从Level容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        static void RemoveActor(AActor actor, string typeTag, ULevel level)
        {
            level.RemoveActor(actor, typeTag);
            RemovePawn(actor);
        }

        /// <summary>
        /// 添加Actor到World 主要用于缓存玩家控制器和玩家控制的角色
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        static string AddActor(AActor actor, string typeTag, UWorld world)
        {
            //添加到所属的Level中
            world.AddActor(actor, typeTag);
            //actor.TransformGet.SetParent(world.RootTs);

            //额外缓存所有的APawn 方便获取
            AddPawn(actor);

            return GetActorSceneID(typeTag);
        }

        /// <summary>
        /// 移除Actor从World容器
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="typeTag"></param>
        static void RemoveActor(AActor actor, string typeTag, UWorld world)
        {
            world.RemoveActor(actor, typeTag);
            RemovePawn(actor);
        }

        /// <summary>
        /// 获取所有的某一类AActor
        /// </summary>
        /// <typeparam name="T">继承自AActor类的类型</typeparam>
        /// <returns></returns>
        public static List<T> GetActors<T>() where T : AActor
        {
            if (null == m_CurrentWorld) return null;

            string type = typeof(T).ToString();
            List<T> returnActors = m_CurrentWorld.GetActors<T>();

            return returnActors;
        }

        static void AddPawn(AActor actor)
        {
            if (actor is APawn)
            {
                APawn pawn = actor as APawn;
                if (!m_Pawns.Contains(pawn))
                    m_Pawns.Add(actor as APawn);
            }
        }

        static void RemovePawn(AActor actor)
        {
            if (actor is APawn)
                m_Pawns.Remove(actor as APawn);
        }

        static string GetActorSceneID(string typeTag)
        {
            if (!m_ActorSceneIDTracer.ContainsKey(typeTag))
            {
                m_ActorSceneIDTracer.Add(typeTag, 0);
            }

            //AActor场景ID生成
            m_ActorSceneIDTracer[typeTag]++;
            return typeTag + "_" + m_ActorSceneIDTracer[typeTag].ToString("D10");
        }

        #endregion

        #region Action

        private static Action<ULevel> m_OnLoadLevel;
        public static void BindActionWithOnLoadLevel(Action<ULevel> bindDelegate, bool bind = true)
        {
            if (bind)
            {
                m_OnLoadLevel += bindDelegate;
            }
            else
            {
                m_OnLoadLevel -= bindDelegate;
            }
        }


        #endregion
    }
}