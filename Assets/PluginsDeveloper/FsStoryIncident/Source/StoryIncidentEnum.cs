using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 章类型
    /// </summary>
    public enum ChapterType
    {
        /// <summary>
        /// 零散事件
        /// </summary>
        Prose,

        /// <summary>
        /// 线型
        /// </summary>
        Line,

        /// <summary>
        /// 层型
        /// </summary>
        Tier,
    }

    /// <summary>
    /// 分数使用方式
    /// </summary>
    public enum ScoreHandlerType
    {
        /// <summary>
        /// 增加
        /// </summary>
        Add,

        /// <summary>
        /// 乘
        /// </summary>
        Multiply,

        /// <summary>
        /// 除
        /// </summary>
        Divide,

        /// <summary>
        /// 重写
        /// </summary>
        Override,
    }

    /// <summary>
    /// 比较运算符
    /// </summary>
    public enum ComparisonOperators
    {
        None,

        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan,

        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// 等于
        /// </summary>
        Equal,

        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual,

        /// <summary>
        /// 小于等于
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// 小于
        /// </summary>
        LessThan,
    }

    public enum StoryIncidentConfigType
    {
        StoryIncidentInit,
        StoryGather,
        Story,
        ChapterConfig,
        Incident,
        IncidentItem,
        Node,
        Choose,
        LinkNode,

        IncidentPackGather,
        IncidentPack,
    }
}