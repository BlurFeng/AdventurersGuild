
namespace FsStoryIncident
{
    /// <summary>
    /// 故事条件接口
    /// 用于配置在故事事件的各种位置，来确认事件能否触发，或者某些节点的选择是否可选等
    /// </summary>
    public interface IStoryCondition
    {
        //故事条件。由项目实现具体业务条件。用于配置到可用的位置，来判断内容是否可用
        //接口的每个实现类，根据类型和param参数不同，会创建对应的实例。每个Type不同的param都有一个对应的实例子类
        //使用接口而不是抽象类，是为了方便项目，在多个功能都要求有条件时，可以在一个类中实现，而不用重复实现多个功能相同的条件类

        //使用场景
        //例1：一个事件在触发时，根据条件选择不同的事件项目（不同的人遇到一个事件触发不同的事件项目）
        //例2：一个节点，根据玩家属性，有些不可选择

        /// <summary>
        /// 解析参数，只会在实例生成时调用一次
        /// 不同的参数都会生成一个对应的实例
        /// 参数解析只进行一次，应当缓存解析出的参数，提高效率。
        /// </summary>
        /// <param name="param">静态配置的参数</param>
        public void Parse(string param);

        /// <summary>
        /// 确认条件是否达成
        /// </summary>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns>是否满足条件</returns>
        public bool CheckCondition(object customData);

        /// <summary>
        /// 获取达成率
        /// </summary>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns>完成度0-1</returns>
        public float GetAchievingRate(object customData);

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