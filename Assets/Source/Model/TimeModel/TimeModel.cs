using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.ootii.Messages;
using System.Timers;
using FsWeatherSystem;

public class TimeModel : MonoBehaviourSingleton<TimeModel>, ISaveData
{
    #region 游戏时间信息
    /// <summary>
    /// 游戏时间信息
    /// </summary>
    public struct TimeInfo
    {
        public static TimeInfo operator +(TimeInfo timeInfoA, TimeInfo timeInfoB)
        {
            timeInfoA.GameTimeEraYear += timeInfoB.GameTimeEraYear;
            timeInfoA.GameTimeMonth += timeInfoB.GameTimeMonth;
            timeInfoA.GameTimeDay += timeInfoB.GameTimeDay;

            //天数跨月
            if (timeInfoA.GameTimeDay > m_GameMonthUnitDayCount)
            {
                //增加月数
                int monthCount = timeInfoA.GameTimeDay / m_GameMonthUnitDayCount;
                timeInfoA.GameTimeMonth += monthCount;
                //减少天数
                timeInfoA.GameTimeDay -= monthCount * m_GameMonthUnitDayCount;
            }

            //月数跨年
            if (timeInfoA.GameTimeMonth > m_GameYeatUnitMonthCount)
            {
                //增加年数
                int yearCount = timeInfoA.GameTimeMonth / m_GameYeatUnitMonthCount;
                timeInfoA.GameTimeEraYear += yearCount;
                //减少月数
                timeInfoA.GameTimeMonth -= yearCount * m_GameYeatUnitMonthCount;
            }

            return timeInfoA;
        }

        public static TimeInfo operator -(TimeInfo timeInfoA, TimeInfo timeInfoB)
        {
            timeInfoA.GameTimeEraYear -= timeInfoB.GameTimeEraYear;
            timeInfoA.GameTimeMonth -= timeInfoB.GameTimeMonth;
            timeInfoA.GameTimeDay -= timeInfoB.GameTimeDay;

            //天数跨月
            if (timeInfoA.GameTimeDay < 1)
            {
                //减少月数
                int monthCount = -timeInfoA.GameTimeDay / m_GameMonthUnitDayCount + 1;
                timeInfoA.GameTimeMonth -= monthCount;
                //增加天数
                timeInfoA.GameTimeDay += monthCount * m_GameMonthUnitDayCount;
            }

            //月数跨年
            if (timeInfoA.GameTimeMonth < 1)
            {
                //减少年数
                int yearCount = -timeInfoA.GameTimeMonth / m_GameYeatUnitMonthCount + 1;
                timeInfoA.GameTimeEraYear -= yearCount;
                //增加月数
                timeInfoA.GameTimeMonth += yearCount * m_GameYeatUnitMonthCount;
            }

            return timeInfoA;
        }

        /// <summary>
        /// 游戏时间 日期
        /// </summary>
        public string GameTimeDateFormat
        {
            get
            {
                return $"{GameTimeEraYear}年{GameTimeMonth}月{GameTimeDay}日 {GameTimeHours}:{GameTimeMinutes}";
            }
        }

        /// <summary>
        /// 真实时间 今日 当前秒数 带小数
        /// </summary>
        public float RealTimeSecondsFloat;
        /// <summary>
        /// 真实时间 今日 当前秒数
        /// </summary>
        public int RealTimeSeconds;
        /// <summary>
        /// 真实时间 今日 当前秒数
        /// </summary>
        public int GameTimeSeconds;
        /// <summary>
        /// 游戏时间 今日 当前分钟数
        /// </summary>
        public int GameTimeMinutes;
        /// <summary>
        /// 游戏时间 今日 当前小时数
        /// </summary>
        public int GameTimeHours;
        /// <summary>
        /// 游戏时间 当前日数
        /// </summary>
        public int GameTimeDay;
        /// <summary>
        /// 游戏时间 当前月数
        /// </summary>
        public int GameTimeMonth;
        /// <summary>
        /// 游戏时间 当前纪元年数
        /// </summary>
        public int GameTimeEraYear;

        /// <summary>
        /// 游戏时间 今日 时间进度(0.0 - 1.0)
        /// </summary>
        public float GameTimeDayProgress
        {
            get
            {
                return GameTimeSeconds / 86400f;
            }
        }
    }

    /// <summary>
    /// 获取 游戏时间信息
    /// </summary>
    /// <param name="allDayCount">总天数</param>
    public TimeInfo GetTimeInfo(int allDayCount)
    {
        //年数
        int yearUnitCount = m_GameYeatUnitMonthCount * m_GameMonthUnitDayCount; //1年的回合数
        int yearCount = allDayCount / yearUnitCount;
        //月数
        int monthUnitCount = m_GameMonthUnitDayCount; //1月的回合数
        int monthCount = (allDayCount - yearCount * yearUnitCount) / monthUnitCount;
        //天数
        int dayCount = allDayCount - yearCount * yearUnitCount - monthCount * monthUnitCount;

        var timeInfo = new TimeInfo();
        timeInfo.GameTimeEraYear = yearCount;
        timeInfo.GameTimeMonth = monthCount;
        timeInfo.GameTimeDay = dayCount;

        return timeInfo;
    }

    /// <summary>
    /// 获取 游戏总天数
    /// </summary>
    /// <param name="timeInfo"></param>
    public int GetAllDayCount(TimeInfo timeInfo)
    {
        int allDayCount = 0;
        //年数 累计回合数
        int yearUnitCount = m_GameYeatUnitMonthCount * m_GameMonthUnitDayCount; //1年的回合数
        allDayCount += timeInfo.GameTimeEraYear * yearUnitCount;
        //月数 累计回合数
        int monthUnitCount = m_GameMonthUnitDayCount; //1月的回合数
        allDayCount += timeInfo.GameTimeMonth * monthUnitCount;
        //天数 累计回合数
        allDayCount += timeInfo.GameTimeDay;

        return allDayCount;
    }
    #endregion

    /// <summary>
    /// 游戏时间/真实时间 比例
    /// </summary>
    public int GameTimeScale { get { return m_GameTimeScale; } }
    private int m_GameTimeScale = 120;
    private static int m_GameYeatUnitMonthCount = 12; //游戏时间 年 单位月数
    private static int m_GameMonthUnitDayCount = 30; //游戏时间 月 单位天数
    private int m_InitEraYearCount = 376; //初始 纪元年
    private int m_InitMonthCount = 2; //初始 月
    private int m_InitDayCount = 23; //初始 日

    private Coroutine m_CorTimer; //协程 计时器
    private float m_RealTimeSecondsFloatCur; //真实时间 本回合 当前的秒数 带小数
    private int m_RealTimeSecondsCur; //真实时间 本回合 当前的秒数 
    private int m_GameTimeMinutesCur; //游戏时间 本回合 当前的分钟数
    private int m_GameTimeHoursCur; //游戏时间 本回合 当前的小时数

    /// <summary>
    /// 真实时间 游玩总时长 秒数
    /// </summary>
    public uint RealTimePlaySeconds { get { return m_RealTimePlaySeconds + (uint)m_RealTimeSecondsCur; } }
    private uint m_RealTimePlaySeconds;

    /// <summary>
    /// 本回合 当前的 时间信息
    /// </summary>
    public TimeInfo TimeInfoCur 
    { 
        get
        {
            var timeInfo = m_GameTimeInfoCur;
            timeInfo.RealTimeSecondsFloat = m_RealTimeSecondsFloatCur;
            timeInfo.RealTimeSeconds = m_RealTimeSecondsCur;
            timeInfo.GameTimeSeconds = (int)(m_RealTimeSecondsFloatCur * m_GameTimeScale);
            timeInfo.GameTimeMinutes = m_GameTimeMinutesCur;
            timeInfo.GameTimeHours = m_GameTimeHoursCur;
            return timeInfo;
        }
    }
    private TimeInfo m_GameTimeInfoCur; //游戏时间 当前

    /// <summary>
    /// 总天数 当前
    /// </summary>
    public int AllDayCountCur { get { return m_AllDayCountCur; } set { m_AllDayCountCur = value; } }
    private int m_AllDayCountCur;

    private string m_SaveKeyTimeModelAllDayCountCur = "TimeModel_AllDayCount"; //总天数
    private string m_SaveKeyTimeModelRealTimeDaySecondsFloat = "TimeModel_RealTimeDaySecondsFloat"; //真实时间 当天总秒数
    private string m_SaveKeyTimeModelRealTimePlaySeconds = "TimeModel_RealTimePlaySeconds"; //真实时间 游玩总秒数

    public void SaveData(ES3File saveData)
    {
        saveData.Save(m_SaveKeyTimeModelAllDayCountCur, m_AllDayCountCur);
        saveData.Save(m_SaveKeyTimeModelRealTimeDaySecondsFloat, m_RealTimeSecondsFloatCur);
        saveData.Save(m_SaveKeyTimeModelRealTimePlaySeconds, m_RealTimePlaySeconds);
    }

    public void LoadData(ES3File saveData)
    {
        //游戏回合
        m_AllDayCountCur = saveData.Load(m_SaveKeyTimeModelAllDayCountCur, 0);

        //游戏时间
        m_GameTimeInfoCur = new TimeInfo();
        m_GameTimeInfoCur.GameTimeEraYear = m_InitEraYearCount;
        m_GameTimeInfoCur.GameTimeMonth = m_InitMonthCount;
        m_GameTimeInfoCur.GameTimeDay = m_InitDayCount;
        //已经过的时间
        var timeInfoAdd = GetTimeInfo(m_AllDayCountCur);
        m_GameTimeInfoCur += timeInfoAdd;

        //当日计时器 重新开始
        TimerRestart(saveData.Load(m_SaveKeyTimeModelRealTimeDaySecondsFloat, 200));

        //游玩总时长 秒数
        m_RealTimePlaySeconds = saveData.Load<uint>(m_SaveKeyTimeModelRealTimePlaySeconds, 0);
    }

    /// <summary>
    /// 执行 结束今日
    /// </summary>
    public void ExecuteFinishCurDay()
    {
        //回合数+1
        AllDayCountCur++;

        //天数+1
        m_GameTimeInfoCur.GameTimeDay++;

        //是否跨月
        if (m_GameTimeInfoCur.GameTimeDay > m_GameMonthUnitDayCount)
        {
            //月数+1
            m_GameTimeInfoCur.GameTimeDay = 1;
            m_GameTimeInfoCur.GameTimeMonth++;
            MessageDispatcher.SendMessageData(TimeModelMsgType.TIMEMODEL_SEASON_CHANGE, TimeInfoCur);

            //是否跨年
            if (m_GameTimeInfoCur.GameTimeMonth > m_GameYeatUnitMonthCount) 
            {
                //年数+1
                m_GameTimeInfoCur.GameTimeMonth = 1;
                m_GameTimeInfoCur.GameTimeEraYear++;
                MessageDispatcher.SendMessageData(TimeModelMsgType.TIMEMODEL_ERAYEAR_CHANGE, TimeInfoCur);
            }
        }

        //计时器 重新开始
        TimerRestart();

        MessageDispatcher.SendMessageData(TimeModelMsgType.TIMEMODEL_DAYCOUNT_CHANGE, TimeInfoCur);

        //自动保存
        if (AllDayCountCur % 3 == 0)
            SaveDataModel.Instance.SaveSaveDataCur();
    }

    //计时器 重新开始
    private void TimerRestart(float realSecondsCur = 0)
    {
        if (m_CorTimer != null)
        {
            StopCoroutine(m_CorTimer);
            m_RealTimePlaySeconds += (uint)m_RealTimeSecondsCur; //累计 游玩总时长 秒数
        }
        m_CorTimer = StartCoroutine(CorTimerTick(realSecondsCur));
    }

    //协程 计时器
    private IEnumerator CorTimerTick(float realSecondsCur = 0)
    {
        m_RealTimeSecondsFloatCur = realSecondsCur; //真实时间 今日当前累计时间
        float gameTurnDeltaTime = m_RealTimeSecondsFloatCur * m_GameTimeScale; //游戏时间 今日当前累计时间

        m_RealTimeSecondsCur = (int)realSecondsCur; //真实时间 当前的秒数
        int gameSecondsCur = m_RealTimeSecondsCur * m_GameTimeScale; //游戏时间 当前的秒数
        m_GameTimeMinutesCur = gameSecondsCur / 60; //游戏时间 当前的分钟数
        m_GameTimeHoursCur = gameSecondsCur / 3600; //游戏时间 当前的小时数

        while (true)
        {
            //固定帧更新
            yield return new WaitForFixedUpdate();

            //累计当前时间
            m_RealTimeSecondsFloatCur += Time.fixedDeltaTime;
            //根据比例计算 游戏时间当前秒数
            gameTurnDeltaTime = m_RealTimeSecondsFloatCur * m_GameTimeScale;

            //真实时间 跨秒 调用委托
            if (m_RealTimeSecondsFloatCur > m_RealTimeSecondsCur)
                m_RealTimeSecondsCur = (int)m_RealTimeSecondsFloatCur;

            //游戏时间 跨分钟 调用委托
            int gameMinutesCur = (int)(gameTurnDeltaTime / 60);
            if (gameMinutesCur > m_GameTimeMinutesCur)
                m_GameTimeMinutesCur = gameMinutesCur;

            //游戏时间 跨小时 调用委托
            int gameHoursCur = (int)(gameTurnDeltaTime / 3600);
            if (gameHoursCur > m_GameTimeHoursCur)
            {
                m_GameTimeHoursCur = gameHoursCur;
                MessageDispatcher.SendMessageData(TimeModelMsgType.TIMEMODEL_GAMETIME_HOURS_CHANGE, TimeInfoCur);

                //累计小时数达到24小时 跨天
                if (m_GameTimeHoursCur >= 24)
                    ExecuteFinishCurDay();
            }
        }
    }
}
