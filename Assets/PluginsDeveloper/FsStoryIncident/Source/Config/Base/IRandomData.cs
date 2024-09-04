
namespace FsStoryIncident
{
    public interface IRandomData
    {
        /// <summary>
        /// 发生概率
        /// 范围0-10000
        /// </summary>
        /// <returns></returns>
        public ushort GetProbability();

        /// <summary>
        /// 优先级
        /// 范围-32,768到32,767。
        /// </summary>
        /// <returns></returns>
        public short GetPriority();

        /// <summary>
        /// 权重
        /// 范围0到32,767。
        /// </summary>
        /// <returns></returns>
        public ushort GetWeight();

        /// <summary>
        /// 确认自身条件是否符合
        /// </summary>
        /// <returns></returns>
        public bool CheckCondition(object CustomData = null);
    }
}