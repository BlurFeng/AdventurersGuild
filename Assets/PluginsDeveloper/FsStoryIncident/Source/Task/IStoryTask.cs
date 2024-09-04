
namespace FsStoryIncident
{
    /// <summary>
    /// 故事工作接口
    /// 用于配置在故事事件配置中可配的位置，执行子类实现的具体工作内容
    /// </summary>
    public interface IStoryTask
    {
        //故事工作。由项目实现具体业务条件。用于配置到可用的位置，来执行自定义的工作内容（业务代码）
        //接口的每个实现类，根据类型和param参数不同，会创建对应的实例。每个Type不同的param都有一个对应的实例子类
        //使用接口而不是抽象类，是为了方便项目，在多个功能都要求有条件时，可以在一个类中实现，而不用重复实现多个功能相同的条件类

        /// <summary>
        /// 解析参数，只会在实例生成时调用一次
        /// 不同的参数都会生成一个对应的实例
        /// 参数解析只进行一次，应当缓存解析出的参数，提高效率。
        /// </summary>
        /// <param name="param">静态配置的参数</param>
        public void Parse(string param);

        /// <summary>
        /// 执行工作
        /// </summary>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns>是否成功执行</returns>
        public bool ExecuteTask(object customData);

#if UNITY_EDITOR
        /// <summary>
        /// 获取参数注释
        /// 注意必须在UNITY_EDITOR宏内实现此方法
        /// </summary>
        /// <returns></returns>
        public string GetParamsComment();
#endif
    }
}