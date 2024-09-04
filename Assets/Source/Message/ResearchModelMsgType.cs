using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResearchModelMsgType
{
    /// <summary>
    /// 研究 载入 成功
    /// </summary>
    public const string RESEARCH_LOAD_SUCCESS = "RESEARCH_LOAD_SUCCESS";

    /// <summary>
    /// 研究 载入 失败
    /// </summary>
    public const string RESEARCH_LOAD_ERROR = "RESEARCH_LOAD_ERROR";

    /// <summary>
    /// 研究项目 变更
    /// </summary>
    public const string RESEARCH_INFO_RESEARCHITEM_CHANGE = "RESEARCH_INFO_RESEARCHITEM_IDCHANGE";

    /// <summary>
    /// 研究项目 更新
    /// </summary>
    public const string RESEARCH_INFO_RESEARCHITEM_UPDATE = "RESEARCH_INFO_RESEARCHITEM_UPDATE";
}
