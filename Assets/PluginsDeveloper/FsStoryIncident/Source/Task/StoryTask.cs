using System;
using System.Collections.Generic;
using System.Linq;

namespace FsStoryIncident
{
    public static class StoryTask
    {
        public static Type IStoryTaskType { get; private set; } = typeof(IStoryTask);
        private static readonly Dictionary<string, Type> m_IStoryTaskTypeDic = new Dictionary<string, Type>();
        private static readonly Dictionary<StoryTypeParamKey, IStoryTask> m_StoryTaskDic = new Dictionary<StoryTypeParamKey, IStoryTask>();

        /// <summary>
        /// 执行工作配置中的工作项目
        /// </summary>
        /// <param name="config"></param>
        /// <param name="customData"></param>
        /// <returns>有任何工作执行失败时，返回false</returns>
        public static bool ExecuteTaskConfig(TaskConfig config, object customData = null)
        {
            bool succeed = true;

            if (config.HaveTaskItems)
            {
                for (int i = 0; i < config.taskItems.Count; i++)
                {
                    var item = config.taskItems[i];
                    if (!ExecuteTask(item.Type, item.param, customData)) succeed = false;
                }
            }

            return succeed;
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空</param>
        /// <returns></returns>
        public static bool ExecuteTask<T>(string param, object customData = null) where T : IStoryCondition
        {
            return ExecuteTask(typeof(T), param, customData);
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空</param>
        /// <returns>是否成功执行</returns>
        public static bool ExecuteTask(string type, string param, object customData = null)
        {
            //获取类型
            Type typeTemp;
            if (m_IStoryTaskTypeDic.ContainsKey(type)) typeTemp = m_IStoryTaskTypeDic[type];
            else
            {
                typeTemp = Type.GetType(type);
                if (typeTemp.GetInterfaces().Contains(IStoryTaskType))
                    m_IStoryTaskTypeDic.Add(type, typeTemp);
                else
                    typeTemp = null;
            }

            if (typeTemp == null) return false;

            return ExecuteTask(typeTemp, param, customData);
        }

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns>是否成功执行</returns>
        public static bool ExecuteTask(Type type, string param, object customData = null)
        {
            if (!GetStoryTask(type, param, out IStoryTask iStoryTask)) return false;

            return iStoryTask.ExecuteTask(customData);
        }


        public static bool GetStoryTaskDefault(Type type, out IStoryTask iStoryCondition)
        {
            return GetStoryTask(type, "", out iStoryCondition);
        }

        /// <summary>
        /// 获取一个故事条件接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <param name="iStoryCondition"></param>
        /// <returns></returns>
        public static bool GetStoryTask(Type type, string param, out IStoryTask iStoryCondition)
        {
            iStoryCondition = null;

            if (type == null || !type.GetInterfaces().Contains(IStoryTaskType)) return false;

            StoryTypeParamKey key = new StoryTypeParamKey(type, param);

            //获取接口类
            if (m_StoryTaskDic.ContainsKey(key))
            {
                iStoryCondition = m_StoryTaskDic[key];
            }
            else
            {
                iStoryCondition = System.Activator.CreateInstance(type) as IStoryTask;

                if (iStoryCondition == null) return false;
                if (!string.IsNullOrEmpty(param)) iStoryCondition.Parse(param);
                m_StoryTaskDic.Add(key, iStoryCondition);
            }

            return true;
        }
    }
}