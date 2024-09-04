using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntrustModelStaticData
{
    #region Condition
    //委托条件。计算条件达成率相关静态数据定义

    /// <summary>
    /// 必要冒险者效果系数
    /// 默认为1
    /// </summary>
    public const float Condition_AchievingRate_VenturerMust_EffectCoefficient = 1f;

    /// <summary>
    /// 可选冒险者效果系数
    /// 一般低于必要冒险者效果系数
    /// </summary>
    public const float Condition_AchievingRate_VenturerOptional_EffectCoefficient = 0.8f;

    #endregion
}
