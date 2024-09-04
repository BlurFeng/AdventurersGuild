using FsStoryIncident;
using System;
using System.Collections.Generic;

/// <summary>
/// 条件模块
/// 条件模块实际上使用FsStoryIncident插件的条件功能
/// </summary>
public class ConditionModel : Singleton<ConditionModel>, IDestroy
{

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init()
    {
        base.Init();
    }

    /// <summary>
    /// 条件是否满足
    /// </summary>
    /// <typeparam name="T">IStoryCondition条件接口实现类</typeparam>
    /// <param name="param">参数</param>
    /// <param name="customData">自定义数据</param>
    /// <returns></returns>
    public bool CheckCondition<T>(string param, object customData = null) where T : IStoryCondition
    {
        return StoryCondition.CheckCondition<T>(param, customData);
    }

    /// <summary>
    /// 条件是否满足
    /// </summary>
    /// <param name="type">条件类</param>
    /// <param name="param">参数</param>
    /// <param name="customData">自定义数据</param>
    /// <returns></returns>
    public bool CheckCondition(string type, string param, object customData = null)
    {
        return StoryCondition.CheckCondition(type, param, customData);
    }

    /// <summary>
    /// 条件是否满足
    /// </summary>
    /// <param name="type">条件类</param>
    /// <param name="param">参数</param>
    /// <param name="customData">自定义数据</param>
    /// <returns></returns>
    public bool CheckCondition(Type type, string param, object customData = null)
    {
        return StoryCondition.CheckCondition(type, param, customData);
    }
}