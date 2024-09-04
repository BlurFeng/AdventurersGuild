using UnityEngine;
using EntrustSystem;

/// <summary>
/// 条件项目
/// 手脚利索，属性要求，满足的属性越多达成率越高。
/// </summary>
public class EC_Adroitly : IEntrustCondition
{
    //项目委托成功率系数为0-3，0-1为失败和成功结果区间，1-2为大成功结果区间，2-3为极大成功结果区间
    //如果委托有条件配置时，将通过配置的条件计算此系数
    //委托条件返回的完成系数在0-3的区间。一般达成条件时系数为1，超出条件时系数上浮。

    private int corporeity;//要求体质
    private int dexterity;//要求敏捷

    public void Parse(string param)
    {
        string[] paramArr = param.Split('-');
        corporeity = int.Parse(paramArr[0]);
        dexterity = int.Parse(paramArr[1]);
    }

    public bool CheckCondition(EntrustItemHandler handler, object customData)
    {
        //没有硬性要求
        return true;
    }

    public float GetAchievingRate(EntrustItemHandler handler, object customData)
    {
        float achievingRate = 0;

        //确认参与冒险者的属性

        //根据委托等级和参与者等级进行结果权重计算
        int[] venturerTeam = handler.GetVenturerTeam();
        for (int i = 0; i < venturerTeam.Length; i++)
        {
            VenturerInfo venturerInfo = VenturerModel.Instance.GetVenturerInfo(venturerTeam[i]);
            if (venturerInfo == null) continue;
            float corporeityRate = venturerInfo.GetAttributeValue(1001) * 1f / corporeity;
            float dexterityRate = venturerInfo.GetAttributeValue(1003) * 1f / dexterity;

            achievingRate += (corporeityRate + dexterityRate) * 0.5f / handler.VenturerNumMust
                                * (i < handler.VenturerNumMust ? EntrustModelStaticData.Condition_AchievingRate_VenturerMust_EffectCoefficient : EntrustModelStaticData.Condition_AchievingRate_VenturerOptional_EffectCoefficient);
        }

        return Mathf.Clamp(achievingRate, 0f, 3f);
    }
}