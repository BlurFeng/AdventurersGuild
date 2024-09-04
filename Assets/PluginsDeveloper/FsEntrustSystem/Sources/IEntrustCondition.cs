using System;

namespace EntrustSystem
{
    public struct EntrustConditionKey
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Type type;

        /// <summary>
        /// 参数
        /// </summary>
        public string param;

        public EntrustConditionKey(Type type, string param)
        {
            this.type = type;
            this.param = param;
        }

        public static bool operator ==(EntrustConditionKey a, EntrustConditionKey b)
        {
            return !(a != b);
        }

        public static bool operator !=(EntrustConditionKey a, EntrustConditionKey b)
        {
            return a.type != b.type || !a.param.Equals(b.param);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is not EntrustConditionKey) return false;

            return this == (EntrustConditionKey)obj;
        }

        public override int GetHashCode()
        {
            return type.GetHashCode() * param.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(type != null ? type.ToString() : "Null Type")} {(param != null ? param : "Null param")}";
        }
    }

    public interface IEntrustCondition
    {
        //项目实现接口，故事条件。用于配置到可用的位置，来判断内容是否可用
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
        /// <param name="handler">委托项目操作用类</param>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns></returns>
        public bool CheckCondition(EntrustItemHandler handler, object customData);

        /// <summary>
        /// 获取委托达成率
        /// </summary>
        /// <param name="handler">委托项目查询用类</param>
        /// <param name="customData">自定义数据，由项目需求决定。可能为空。</param>
        /// <returns></returns>
        public float GetAchievingRate(EntrustItemHandler handler, object customData);
    }
}