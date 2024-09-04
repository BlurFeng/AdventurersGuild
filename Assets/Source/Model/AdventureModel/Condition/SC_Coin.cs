using FsStoryIncident;

/// <summary>
/// 条件,金币
/// </summary>
public class SC_Coin : IStoryCondition
{
    private int mCoinNum;
    private int mOperatorNum;

    public void Parse(string param)
    {
        if (string.IsNullOrEmpty(param)) return;

        //解析参数只进行一次，缓存参数使用，提高效率。
        if(StoryIncidentLibrary.SplitParams(param, StoryIncidentLibrary.SepType.Sep1, out string[] paramAry))
        {
            mCoinNum = 1; if (paramAry.Length > 0) mCoinNum = int.Parse(paramAry[0]);
            mOperatorNum = 3; if (paramAry.Length > 1) mOperatorNum = int.Parse(paramAry[1]);
        }
    }

    public bool CheckCondition(object customData)
    {
        if(AdventureLibrary.GetCCD_Entrust(customData, out CCD_Entrust data))
        {
            ////TODO：确认冒险者中，身上金钱数量符合要求的，不符合的从evt.PassVenturerInfo中移除
            //for (int i = evt.PassVenturerInfo.Count; i >= 0; i--)
            //{
            //    int num = 1000;
            //    CheckCondition(num);
            //}

            int num = 1000;

#if GAME_DEBUG
            string operatorDes = "";
            switch (mOperatorNum)
            {
                case 1:
                    operatorDes = "等于";
                    break;
                case 2:
                    operatorDes = "大于";
                    break;
                case 3:
                    operatorDes = "大于等于";
                    break;
                case 4:
                    operatorDes = "小于";
                    break;
                case 5:
                    operatorDes = "小于等于";
                    break;
                case 6:
                    operatorDes = "不等于";
                    break;
            }
            Debugx.Log(1, $"金钱确认: {num} {operatorDes} {mCoinNum}");
#endif

            return CheckCondition(num);
        }

        return false;
    }

    public float GetAchievingRate(object customData)
    {
        return 1f;
    }

    private bool CheckCondition(int num)
    {
        switch (mOperatorNum)
        {
            case 1:
                return num == mCoinNum;
            case 2:
                return num > mCoinNum;
            case 3:
                return num >= mCoinNum;
            case 4:
                return num < mCoinNum;
            case 5:
                return num <= mCoinNum;
            case 6:
                return num != mCoinNum;
        }

        return false;
    }

#if UNITY_EDITOR
    public string GetParamsComment()
    {
        return "参1：金币数量；参2：比较符 1=等于 2=大于 3=大于等于 4=小于 5=小于等于 6=不等于";
    }
#endif
}