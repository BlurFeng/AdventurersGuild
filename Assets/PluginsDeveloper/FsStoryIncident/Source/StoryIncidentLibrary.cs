using System;
using System.Collections.Generic;
using UnityEngine;

namespace FsStoryIncident
{
    /// <summary>
    /// 故事事件方法库
    /// </summary>
    public static class StoryIncidentLibrary
    {
        /// <summary>
        /// 确认Int数组中是否存在某个值
        /// </summary>
        /// <param name="array"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IntArrayContains(int[] array, int target)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == target) return true;
            }

            return false;
        }

        #region Random 随机方法

        private static readonly List<int> m_RandomItemPriorityHeightCached = new List<int>();
        private static readonly List<IRandomData> m_randomItemsCached = new List<IRandomData>();
        private static readonly List<LinkOtherItem> linkOtherItemsGetCached = new List<LinkOtherItem>();

        /// <summary>
        /// 从传入的数组按发生概率，优先级和权重随机规则获取指定数量
        /// 先进行随机，人后按优先获取优先级高的，优先级相同的进行权重随机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getItem">获取到的对象</param>
        /// <param name="randomItems">用于随机的对象数组</param>
        /// <param name="customData">自定义数据，用于条件确认</param>
        /// <param name="checkCondition">是否确认条件</param>
        /// <returns></returns>
        public static bool RandomItemGet<T>(out T getItem, T[] randomItems, object customData = null, bool checkCondition = true) where T : class, IRandomData
        {
            getItem = null;
            if (randomItems == null || randomItems.Length == 0) return false;

            m_randomItemsCached.Clear();
            m_randomItemsCached.AddRange(randomItems);
            if (RandomItemGetInternal(m_randomItemsCached, customData, 1, checkCondition))
            {
                if (m_randomItemsCached.Count > 0) getItem = m_randomItemsCached[0] as T;
            }

            return getItem != null;
        }

        /// <summary>
        /// 从传入的数组按发生概率，优先级和权重随机规则获取指定数量
        /// 先进行随机，人后按优先获取优先级高的，优先级相同的进行权重随机
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getItem">获取到的对象</param>
        /// <param name="randomItems">用于随机的对象数组</param>
        /// <param name="customData">自定义数据，用于条件确认</param>
        /// <param name="checkCondition">是否确认条件</param>
        /// <returns></returns>
        public static bool RandomItemGet<T>(out T getItem, List<T> randomItems, object customData = null, bool checkCondition = true) where T : class, IRandomData
        {
            getItem = null;
            if (randomItems == null || randomItems.Count == 0) return false;

            m_randomItemsCached.Clear();
            m_randomItemsCached.AddRange(randomItems);
            if (RandomItemGetInternal(m_randomItemsCached, customData, 1, checkCondition))
            {
                if (m_randomItemsCached.Count > 0) getItem = m_randomItemsCached[0] as T;
            }

            return getItem != null;
        }

        /// <summary>
        /// 从传入的数组按发生概率，优先级和权重随机规则获取指定数量
        /// 先进行随机，人后按优先获取优先级高的，优先级相同的进行权重随机
        /// </summary>
        /// <param name="randomItems">用于随机的对象数组</param>
        /// <param name="customData">自定义数据，用于条件确认</param>
        /// <param name="getNum">想要获取的数量</param>
        /// <param name="checkCondition">是否确认条件</param>
        /// <returns></returns>
        public static bool RandomItemGet<T>(out T[] getItems, List<T> randomItems, object customData = null, int getNum = 1, bool checkCondition = true) where T : class, IRandomData
        {
            getItems = null;
            if (randomItems == null || randomItems.Count == 0) return false;
            
            m_randomItemsCached.Clear();
            m_randomItemsCached.AddRange(randomItems);
            if (RandomItemGetInternal(m_randomItemsCached, customData, getNum, checkCondition))
            {
                getItems = new T[m_randomItemsCached.Count];
                for (int i = 0; i < m_randomItemsCached.Count; i++)
                {
                    getItems[i] = m_randomItemsCached[i] as T;
                }
            }
            return getItems != null && getItems.Length > 0;
        }

        private static bool RandomItemGetInternal<T>(List<T> randomItems, object customData = null, int getNum = 1, bool checkCondition = true) where T : IRandomData
        {
            if (randomItems == null || randomItems.Count == 0) return false;
            if (randomItems[0] is not IRandomData) return false;

            //进行随机，确认是否会发生
            for (int i = randomItems.Count - 1; i >= 0; i--)
            {
                IRandomData item = randomItems[i] as IRandomData;

                //确认是否发生
                bool hap;
                if (item.GetProbability() == 0) { hap = false; }
                else if (item.GetProbability() < 10000)
                {
                    int num = Random(1, 10001);
                    hap = num <= item.GetProbability();
                }
                else hap = true;

                if (!hap)
                {
                    randomItems.RemoveAt(i);
                }
                else if(checkCondition)
                {
                    //确实条件是否符合
                    if (!item.CheckCondition(customData))
                    {
                        randomItems.RemoveAt(i);
                    }
                }
            }

            if (randomItems.Count <= getNum) return true;//数量符合要求

            //按优先级从高到低排序
            if (!IRandomDataSort(randomItems)) return false;

            m_RandomItemPriorityHeightCached.Clear();

            //倒序遍历，从优先级低的开始按规则移除，直到符合需要的数量
            //进行优先级和权重随机筛选
            short priorityCached = (randomItems[randomItems.Count - 1]).GetPriority();//最小权重记录
            for (int i = randomItems.Count - 1; i >= 0; i--)
            {
                IRandomData item = randomItems[i] as IRandomData;

                //根据优先级和权重筛选
                if (item.GetPriority() <= priorityCached)
                {
                    //收集优先级相同的
                    m_RandomItemPriorityHeightCached.Add(i);
                }
                //处理需要移除的对象
                else
                {
                    priorityCached = item.GetPriority();

                    //移除此优先级的所有事件后数量符合要求，那么直接移除
                    int cutAfterNum = randomItems.Count - m_RandomItemPriorityHeightCached.Count;
                    if (cutAfterNum >= getNum)
                    {
                        randomItems.RemoveRange(i + 1, m_RandomItemPriorityHeightCached.Count);
                    }
                    //进行多次权重随机，获取到需要的事件数量
                    else
                    {
                        int getNumWeightRandom = getNum - cutAfterNum;//需要移除的数量

                        int weightTotal = 0;
                        List<int> weights = new List<int>();
                        for (int j = 0; j < m_RandomItemPriorityHeightCached.Count; j++)
                        {
                            int weight = (randomItems[m_RandomItemPriorityHeightCached[j]] as IRandomData).GetPriority();
                            weights.Add(weight);
                            weightTotal += weight;
                        }

                        //权重随机获取到保留事件，并将事件从m_RandomItemPriorityHeightCached中移除
                        while (getNumWeightRandom > 0)
                        {
                            int index = StoryIncidentLibrary.RandomIndexByWeights(weights.ToArray(), weightTotal);

                            m_RandomItemPriorityHeightCached.RemoveAt(index);
                            weightTotal -= weights[index];
                            weights.Remove(index);

                            getNumWeightRandom--;
                        }

                        //进行移除
                        for (int j = 0; j < m_RandomItemPriorityHeightCached.Count; j++)
                        {
                            randomItems.RemoveAt(m_RandomItemPriorityHeightCached[j]);
                        }
                    }

                    m_RandomItemPriorityHeightCached.Clear();

                    //结束移除
                    if (randomItems.Count <= getNum)
                        break;
                }
            }

            return true;
        }

        private static bool IRandomDataSort<T>(List<T> randomItems) where T : IRandomData
        {
            if (randomItems == null) return false;

            bool dataValid = true;
            randomItems.Sort((T a, T b) =>
            {
                if (a is not IRandomData aR || b is not IRandomData bR)
                {
                    dataValid = false;
                    return 0;
                }

                return aR.GetPriority() > bR.GetPriority() ? 1 : -1;
            });

            return dataValid;
        }

        public static bool GetLinkOtherItemTargetPId(List<LinkOtherItem> linkOtherItems, int score, Func<NodeChooseLimitConfig, bool> checkNodeChooseFunc, out Guid targetGuid, object customData = null)
        {
            linkOtherItemsGetCached.Clear();
            targetGuid = System.Guid.Empty;

            //获取链接到其他事件信息
            if (linkOtherItems != null && linkOtherItems.Count > 0)
            {
                for (int i = 0; i < linkOtherItems.Count; i++)
                {
                    var item = linkOtherItems[i];

                    //确认分数符合要求
                    if (item.scoreLimit != ComparisonOperators.None)
                    {
                        if (!ScoreCompare(item.scoreLimit, score, item.scoreLimitNum)) continue;
                    }

                    //确认节点选择限制符合要求
                    if (item.HaveNodeChooseLimit && checkNodeChooseFunc != null)
                    {
                        for (int j = 0; j < item.nodeChooseLimit.Count; j++)
                        {
                            NodeChooseLimitConfig nodeLimit = item.nodeChooseLimit[j];
                            if (!checkNodeChooseFunc(nodeLimit)) continue;
                        }
                    }

                    linkOtherItemsGetCached.Add(item);
                }
            }

            //确认链接到哪个事件
            if (linkOtherItemsGetCached.Count > 0)
            {
                if (linkOtherItemsGetCached.Count == 1)
                {
                    targetGuid = linkOtherItemsGetCached[0].targetSGuid.Guid;
                }
                else
                {
                    if (RandomItemGet(out LinkOtherItem getItem, linkOtherItemsGetCached, customData))
                    {
                        targetGuid = getItem.targetSGuid.Guid;
                    }
                }
            }

            return targetGuid != System.Guid.Empty;
        }

        /// <summary>
        /// 随机选取一个权重数组中的某一段 并返回该段对应下标
        /// </summary>
        /// <param name="weights">权重数组</param>
        /// <param name="weightTotal">总权重值 不设置的话会自动根据输入的权重数组计算</param>
        /// <returns></returns>
        public static int RandomIndexByWeights(int[] weights, int weightTotal = -1)
        {
            //计算权重总值
            if (weightTotal <= 0)
            {
                weightTotal = 0;
                for (int i = 0; i < weights.Length; i++)
                {
                    weightTotal += weights[i];
                }
            }

            int randomNum = Random(1, weightTotal);//随机数

            int right = 0;
            //确认随机数命中了哪一段
            for (int i = 0; i < weights.Length; i++)
            {
                right += weights[i];

                //随机数条件有重合部分 但可以忽略
                if (randomNum <= right)
                {
                    return i;//返回命中段的权重下标
                }
            }

            return 0;
        }

        /// <summary>
        /// 随机数Float within [minInclusive..maxInclusive] (range is inclusive)
        /// 每次随机到的数字设置为种子
        /// </summary>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        public static float Random(float minInclusive, float maxInclusive)
        {
            float seedFloat = UnityEngine.Random.Range(minInclusive, maxInclusive);
            UnityEngine.Random.InitState(Mathf.CeilToInt(seedFloat * 10000f));

            return seedFloat;
        }

        /// <summary>
        /// 随机数Int within [minInclusive..maxExclusive)
        /// 每次随机到的数字设置为种子
        /// </summary>
        /// <param name="minInclusive"></param>
        /// <param name="maxInclusive"></param>
        /// <returns></returns>
        public static int Random(int minInclusive, int maxInclusive)
        {
            var seedInt = UnityEngine.Random.Range(minInclusive, maxInclusive);
            UnityEngine.Random.InitState(seedInt);

            return seedInt;
        }

        public static T Random<T>(T[] array) where T : struct
        {
            if (array == null || array.Length == 0) return default(T);
            var seedInt = UnityEngine.Random.Range(0, array.Length);
            UnityEngine.Random.InitState(seedInt);

            return array[seedInt];
        }

        #endregion

        #region Score 事件项目节点 分数对比方法

        /// <summary>
        /// 分数比较
        /// </summary>
        /// <param name="score1"></param>
        /// <param name="score2"></param>
        /// <returns>通过比较</returns>
        public delegate bool ScoreCompareD(int score1, int score2);

        /// <summary>
        /// 分数操作
        /// </summary>
        /// <param name="score1"></param>
        /// <param name="score2"></param>
        /// <returns></returns>
        public delegate int ScoreHandlerD(int score1, int score2);

        /// <summary>
        /// 分数比较
        /// </summary>
        /// <param name="type">比较方式</param>
        /// <param name="score">分数</param>
        /// <param name="scoreLimit">分数限制</param>
        /// <returns></returns>
        public static bool ScoreCompare(ComparisonOperators type, int score, int scoreLimit)
        {
            return GetScoreCompareFunc(type)(score, scoreLimit);
        }

        public static ScoreCompareD GetScoreCompareFunc(ComparisonOperators type)
        {
            switch (type)
            {
                case ComparisonOperators.GreaterThan:
                    return ScoreCompareGreaterThan;
                case ComparisonOperators.GreaterThanOrEqual:
                    return ScoreCompareGreaterThanOrEqual;
                case ComparisonOperators.Equal:
                    return ScoreCompareEqual;
                case ComparisonOperators.NotEqual:
                    return ScoreCompareNotEqual;
                case ComparisonOperators.LessThanOrEqual:
                    return ScoreCompareLessThanOrEqual;
                case ComparisonOperators.LessThan:
                    return ScoreCompareLessThan;
                default:
                    return ScoreCompareGreaterThanOrEqual;
            }
        }

        public static bool ScoreCompareGreaterThan(int score, int scoreLimit)
        {
            return score > scoreLimit;
        }

        public static bool ScoreCompareGreaterThanOrEqual(int score, int scoreLimit)
        {
            return score >= scoreLimit;
        }

        public static bool ScoreCompareEqual(int score, int scoreLimit)
        {
            return score == scoreLimit;
        }

        public static bool ScoreCompareNotEqual(int score, int scoreLimit)
        {
            return score != scoreLimit;
        }

        public static bool ScoreCompareLessThanOrEqual(int score, int scoreLimit)
        {
            return score <= scoreLimit;
        }

        public static bool ScoreCompareLessThan(int score, int scoreLimit)
        {
            return score < scoreLimit;
        }

        /// <summary>
        /// 分数处理，对分数进行加减乘除覆盖操作
        /// </summary>
        /// <param name="type">比较方式</param>
        /// <param name="scoreChange">变化分数</param>
        /// <param name="scoreBase">基础分数</param>
        /// <returns></returns>
        public static int ScoreHandler(ScoreHandlerType type, int scoreChange, int scoreBase)
        {
            return GetScoreHandlerFunc(type)(scoreChange, scoreBase);
        }

        public static ScoreHandlerD GetScoreHandlerFunc(ScoreHandlerType type)
        {
            switch (type)
            {
                case ScoreHandlerType.Add:
                    return ScoreUseAdd;
                case ScoreHandlerType.Multiply:
                    return ScoreUseMultiply;
                case ScoreHandlerType.Divide:
                    return ScoreUseDivide;
                case ScoreHandlerType.Override:
                    return ScoreUseOverride;
                default:
                    return ScoreUseAdd;
            }
        }

        public static int ScoreUseAdd(int score, int scoreBase)
        {
            return scoreBase + score;
        }

        public static int ScoreUseMultiply(int score, int scoreBase)
        {
            return scoreBase * score;
        }

        public static int ScoreUseDivide(int score, int scoreBase)
        {
            return scoreBase / score;
        }

        public static int ScoreUseOverride(int score, int scoreBase)
        {
            return score;
        }

        #endregion

        #region Param Parser 参数解析

        //可以用于条件配置中参数的规范化和使用时的方便解析

        public enum SepType
        {
            Sep1,
            Sep2,
            Sep3,
            Sep4,

            Length
        }

        private static bool GetSep(SepType sepType, out char sep)
        {
            switch (sepType)
            {
                case SepType.Sep1:
                    sep = paramSep1;
                    return true;
                case SepType.Sep2:
                    sep = paramSep2;
                    return true;
                case SepType.Sep3:
                    sep = paramSep3;
                    return true;
                case SepType.Sep4:
                    sep = paramSep4;
                    return true;
            }

            sep = '0';
            return false;
        }

        /// <summary>
        /// 参数数据项目
        /// 当项目的数据无法再切割时，会将参数存储到paramData。paramDataItems为null。
        /// 还能继续切割时paramData为null，paramDataItems中存储了切割后的数据。
        /// </summary>
        public struct ParamData
        {
            /// <summary>
            /// 参数数据
            /// </summary>
            public string data;

            /// <summary>
            /// 参数数据数组
            /// </summary>
            public ParamData[] items;

            /// <summary>
            /// 数据是否有效
            /// </summary>
            public bool IsValid => data != null || (items != null && items.Length > 0);

            /// <summary>
            /// 是最终数据
            /// 可以对paramData进行Parse到实际的数据类型
            /// </summary>
            public bool IsFinalData => IsValid && !string.IsNullOrEmpty(data);
        }

        public const char paramSep1 = '|';
        public const char paramSep2 = ';';
        public const char paramSep3 = ',';
        public const char paramSep4 = '-';

        /// <summary>
        /// 参数切割，并在获取到
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="parseFunc">参数处理方法，参数1=切割层数，从1开始。参数2=下标</param>
        /// <returns></returns>
        public static ParamData ParamsParseHandler(string paramsData, Action<int, int> parseFunc)
        {
            ParamsSplitSub(paramsData, out ParamData paramData, SepType.Sep1, parseFunc);
            return paramData;
        }

        private static void ParamsSplitSub(string dataStr, out ParamData paramData, SepType sepType, Action<int, int> paramHandler, int deep = 1)
        {
            paramData = new ParamData();

            bool final = true;
            if(sepType != SepType.Length)
            {
                if (SplitParams(dataStr, sepType, out string[] splits))
                {
                    final = false;
                    paramData.items = new ParamData[splits.Length];

                    for (int i = 0; i < splits.Length; i++)
                    {
                        string splitData = splits[i];

                        ParamsSplitSub(splitData, out paramData.items[i], (sepType + 1), paramHandler, deep + 1);
                    }
                }
            }
            
            if(final) paramData.data = dataStr;
        }

        /// <summary>
        /// 切割参数
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sepType"></param>
        /// <returns></returns>
        public static bool SplitParams(string paramsData, SepType sepType, out string[] strs)
        {
            strs = null;
            if (!GetSep(sepType, out char sep)) return false;
            return SplitParams(paramsData, sep, out strs);
        }

        /// <summary>
        /// 切割参数
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sepType"></param>
        /// <returns></returns>
        public static bool SplitParams(string paramsData, SepType sepType, out int[] ints)
        {
            ints = null;
            if (!GetSep(sepType, out char sep)) return false;
            return SplitParams(paramsData, sep, out ints);
        }

        /// <summary>
        /// 切割参数，输出int和float参数
        /// 先分析int，再分析float
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sepType"></param>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static bool SplitParamsIntFloat(string paramsData, SepType sepType, int intNum, int floatNum, out int[] ints, out float[] floats)
        {
            ints = null; floats = null;
            if (!GetSep(sepType, out char sep)) return false;
            return SplitParamsIntFloat(paramsData, sep, intNum, floatNum, out ints, out floats);
        }

        /// <summary>
        /// 切割参数
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sep"></param>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool SplitParams(string paramsData, char sep, out string[] strs)
        {
            strs = null;
            if (string.IsNullOrEmpty(paramsData)) return false;

            strs = paramsData.Split(sep);
            return strs != null && strs.Length > 0;
        }

        /// <summary>
        /// 切割参数
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sep"></param>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static bool SplitParams(string paramsData, char sep, out int[] ints)
        {
            ints = null;
            if (!SplitParams(paramsData, sep, out string[] strs)) return false;
            ints = new int[strs.Length];

            for (int i = 0; i < strs.Length; i++)
            {
                if (!int.TryParse(strs[i], out int value)) return false;
                ints[i] = value;
            }

            return true;
        }

        /// <summary>
        /// 切割参数，输出int和float参数
        /// 先分析int，再分析float
        /// </summary>
        /// <param name="paramsData"></param>
        /// <param name="sep"></param>
        /// <param name="ints"></param>
        /// <returns></returns>
        public static bool SplitParamsIntFloat(string paramsData, char sep, int intNum, int floatNum, out int[] ints, out float[] floats)
        {
            ints = null; floats = null;
            if (!SplitParams(paramsData, sep, out string[] strs)) return false;
            if(strs.Length != intNum + floatNum) return false;
            ints = new int[intNum];
            floats = new float[floatNum];

            int index = 0;
            for (int i = 0; i < intNum; i++)
            {
                if (!int.TryParse(strs[index], out int value)) return false;
                ints[index] = value;
                index++;
            }

            for (int i = 0; i < floatNum; i++)
            {
                if (!float.TryParse(strs[index], out float value)) return false;
                floats[index] = value;
                index++;
            }

            return true;
        }
        #endregion

        #region Extension

        /// <summary>
        /// 确认条件是否达成
        /// </summary>
        /// <param name="config">条件配置</param>
        /// <param name="customData">自定义数据，由项目需求决定。用于条件确认。</param>
        /// <returns></returns>
        public static bool CheckCondition(this ConditionConfig config, object customData = null)
        {
            if (!config.HaveCondition) return true;

            return StoryCondition.GetResult(config.logicExpression, config.conditionItems, customData);
        }

        public static bool ExecuteTask(this TaskConfig config, object customData = null)
        {
            if (!config.HaveTaskItems) return true;

            return StoryTask.ExecuteTaskConfig(config, customData);
        }
        #endregion
    }
}