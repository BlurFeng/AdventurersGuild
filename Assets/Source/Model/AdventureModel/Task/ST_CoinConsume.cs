using FsStoryIncident;

/// <summary>
/// 金币消耗
/// </summary>
public class ST_CoinConsume : IStoryTask
{
    private int m_ConsumeCoin;
    private bool m_Force;

    public void Parse(string param)
    {
        if (StoryIncidentLibrary.SplitParams(param, StoryIncidentLibrary.SepType.Sep1, out string[] paramAry))
        {
            m_ConsumeCoin = 1; if (paramAry.Length > 0) m_ConsumeCoin = int.Parse(paramAry[0]);
            m_Force = false; if (paramAry.Length > 1) m_Force = bool.Parse(paramAry[1]);
        }
    }

    public bool ExecuteTask(object customData)
    {
        if (AdventureLibrary.GetCCD_Entrust(customData, out CCD_Entrust data))
        {
            ////TODO：确认冒险者并消耗冒险者身上的金钱
            //for (int i = evt.PassVenturerInfo.Count; i >= 0; i--)
            //{
            //    int num = 1000;
            //    CheckCondition(num);
            //}
#if GAME_DEBUG
            Debugx.Log(1, $"金钱消耗: {m_ConsumeCoin}");
#endif

            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    public string GetParamsComment()
    {
        return "示例方法；参1：消耗的金币数量 参2：不足时是否消耗可消耗数量 0=否 1=是";
    }
#endif
}
