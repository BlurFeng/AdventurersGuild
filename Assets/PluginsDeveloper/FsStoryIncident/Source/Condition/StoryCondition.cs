using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace FsStoryIncident
{
    public static class StoryCondition
    {
        //此类用于处理故事事件插件中的条件逻辑表达式
        //关于故事事件条件详见StoryIncidentCondition类
        //为提高效率，此逻辑分析器只处理：逻辑值（True False），与&&，或||，非!，等于==，不等于!=，小括号()。大于>小于<等比较符相对功能，应当在条件类中自行处理。
        //玩家在实际配置时，配置的是条件数组和一个逻辑表达式。逻辑表达式中用数字下标作为占位符，表示使用条件数组中某个条件的bool结果。
        //例：Comditions[c0,c1,c2,c3,c4]  0&&(1||2)||3&&!0||1==2&&3!=4

        #region Regex
        /// <summary>
        /// 最内部的小括号内内容
        /// </summary>
        private static readonly Regex reg_SBracket = new Regex(@"\([^(]*?\)");

        /// <summary>
        /// 将表达式以||切割
        /// </summary>
        private static readonly Regex reg_CutOr = new Regex(@"(\S.*？)([\|]{2})|(\S.*)");

        /// <summary>
        /// 将表达式以&&切割
        /// </summary>
        private static readonly Regex reg_CutAnd = new Regex(@"(\S.*？)([&]{2})|(\S.*)");

        /// <summary>
        /// 切割比较符或者单个数值
        /// 这是解析的最小单位切割
        /// </summary>
        private static readonly Regex reg_CutCompare = new Regex(@"(\d+)(==|!=)(\d+)|(\d+)");

        /// <summary>
        /// 用于确认表达式是否合法
        /// 包含任何表达式中不允许使用的符号则不合法 | 结尾不是数字 | 逻辑运算符的错误组合使用
        /// </summary>
        private static readonly Regex reg_ExpressionValid = new Regex(@"((?!(\d)|&|\||!|=).)+|[^\d]$|(?<=\||!|=)&|&(?=\||!|=)|(?<=&|!|=)\||\|(?=&|!|=)|(?<=&|\||=)!|!(?=&|\|)|(?<=&|\|)=|=(?=&|\||!)");

        private static readonly Regex reg_ExpressionRepAnd = new Regex(@"(?<!&)&(?!&)|&{3,}");
        private static readonly Regex reg_ExpressionRepOr = new Regex(@"(?<!\|)\|(?!\|)|\|{3,}");
        private static readonly Regex reg_ExpressionRepNot = new Regex(@"!{2,}");
        private static readonly Regex reg_ExpressionRepNotEquals = new Regex(@"!={2,}");
        private static readonly Regex reg_ExpressionRepEquals = new Regex(@"(?<!=|!)=(?!=)|={3,}");
        #endregion

        private static readonly string mTrueIntStr = "-1";
        private static readonly string mFalseIntStr = "-2";
        private static readonly int mTrueInt = -1;
        //private static readonly int m_FalseInt = -2;

        public static Type IStoryConditionType { get; private set; } = typeof(IStoryCondition);
        private static readonly Dictionary<string, Type> m_IStoryConditionTypeDic = new Dictionary<string, Type>();
        private static readonly Dictionary<StoryTypeParamKey, IStoryCondition> m_StoryConditionDic = new Dictionary<StoryTypeParamKey, IStoryCondition>();

        /// <summary>
        /// 确认逻辑表达式和逻辑值数组配置是否合理
        /// 要求逻辑表达式允许：0和正数(Index下标)，且&&，或||，非!，等于==，不等于!=，小括号()
        /// Index下标不能超过conditionItems的长度
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="conditionItems"></param>
        /// <returns></returns>
        public static bool CheckExpressionValid(ref string expression, List<ConditionItemConfig> conditionItems)
        {
            //数据通过工具静态配置，在配置时应当保证配置数据有效性
            //之后在运行时使用数据时，不会再做额外的判断产生开销

            if (string.IsNullOrEmpty(expression)) return true;

            bool valid = !reg_ExpressionValid.IsMatch(expression);

            //支持单个逻辑运算符 & | = ,规范或多的重复符号 &&& ||| !! === !===
            //自动规范书写格式
            if (valid)
            {
                expression = reg_ExpressionRepAnd.Replace(expression, "&&");
                expression = reg_ExpressionRepOr.Replace(expression, "||");
                expression = reg_ExpressionRepNot.Replace(expression, "!");
                expression = reg_ExpressionRepNotEquals.Replace(expression, "!=");
                expression = reg_ExpressionRepEquals.Replace(expression, "==");
            }

            return valid;
        }

        /// <summary>
        /// 获取逻辑表达式结果
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="conditionItems"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        public static bool GetResult(string expression, List<ConditionItemConfig> conditionItems, object customData = null)
        {
            if (string.IsNullOrEmpty(expression)) return true;//没有需要判断的内容时返回true
            if (conditionItems == null || conditionItems.Count == 0) return false;//没有有效的逻辑值时返回false

            bool result;
            StringBuilder sbExp = new StringBuilder(expression);
            string sbExpToStr = expression;
            //逻辑表达式中有小括号时，由内到外依次解析小括号内的内容为逻辑值并替换。直到没有小括号
            while (reg_SBracket.IsMatch(sbExpToStr))
            {
                MatchCollection mc = reg_SBracket.Matches(sbExpToStr);
                foreach (Match m in mc)
                {
                    //计算最靠内的小括号内逻辑，并替换
                    string bExp = m.Value;
                    result = LogicExpressionParse(bExp.Substring(1, bExp.Length - 2), conditionItems, customData);
                    sbExp.Replace(bExp, result ? mTrueIntStr : mFalseIntStr);
                }
                sbExpToStr = sbExp.ToString();
            }

            //最后一次解析
            result = LogicExpressionParse(expression, conditionItems, customData);

            return result;
        }

        /// <summary>
        /// 逻辑表达式解析
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="conditionItems"></param>
        /// <returns></returns>
        private static bool LogicExpressionParse(string expression, List<ConditionItemConfig> conditionItems, object customData)
        {
            MatchCollection mcOr = reg_CutOr.Matches(expression);

            int count1 = 0;
            foreach (Match mOr in mcOr)
            {
                //每一段||分割的内容解析
                count1++;
                string expItem1 = count1 == mcOr.Count ? mOr.Groups[3].Value : mOr.Groups[1].Value;//获取不包含||的内容

                int count2 = 0;
                bool subsectionBool = false;
                MatchCollection mcAnd = reg_CutAnd.Matches(expItem1);
                foreach (Match mAnd in mcAnd)
                {
                    //每一段&&分割的内容解析
                    count2++;
                    string expItem2 = count2 == mcAnd.Count ? mAnd.Groups[3].Value : mAnd.Groups[1].Value;//获取内容

                    MatchCollection mcCompare = reg_CutCompare.Matches(expItem2);
                    string indexSingleStr = mcCompare[0].Groups[4].Value;
                    bool cItemResult = false;
                    if (!string.IsNullOrEmpty(indexSingleStr))
                    {
                        //单个下标数字
                        cItemResult = GetResultConditionItem(int.Parse(indexSingleStr), conditionItems, customData);
                    }
                    else
                    {
                        //两个下标并带比较符，如1==2 3!=4
                        bool resultLeft = GetResultConditionItem(int.Parse(mcCompare[0].Groups[1].Value), conditionItems, customData);
                        bool resultRight = GetResultConditionItem(int.Parse(mcCompare[0].Groups[3].Value), conditionItems, customData);
                        string compareStr = mcCompare[0].Groups[2].Value;

                        switch (compareStr)
                        {
                            case "==":
                                cItemResult = resultLeft == resultRight;
                                break;
                            case "!=":
                                cItemResult = resultLeft != resultRight;
                                break;
                        }
                    }

                    //且分段任意为假就是假
                    if (!cItemResult)
                    {
                        subsectionBool = false;
                        break;
                    }
                    subsectionBool = true;
                }

                //或分段中任意为真就是真
                if(subsectionBool) return true;
            }

            //不会走到这
            return false;
        }

        /// <summary>
        /// 获取一个条件项目的结果
        /// 根据条件逻辑表达式，表达式中可能用到多个条件项目，根据所有条件项目结果和表达式得出最终逻辑结果
        /// </summary>
        /// <param name="index"></param>
        /// <param name="conditionItems"></param>
        /// <param name="customData"></param>
        /// <returns></returns>
        private static bool GetResultConditionItem(int index, List<ConditionItemConfig> conditionItems, object customData)
        {
            if (index >= 0 && index < conditionItems.Count)
            {
                ConditionItemConfig cItem = conditionItems[index];

                return CheckCondition(cItem.Type, cItem.param, customData);
            }
            else
            {
                if (index >= 0) return true;//不存在的Index默认为true
                else return index == mTrueInt ? true : false;//小括号内内容优先计算并替换，-1为True
            }
        }

        /// <summary>
        /// 条件是否满足
        /// </summary>
        /// <typeparam name="T">IStoryCondition条件接口实现类</typeparam>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据</param>
        /// <returns></returns>
        public static bool CheckCondition<T>(string param, object customData = null) where T : IStoryCondition
        {
            return CheckCondition(typeof(T), param, customData);
        }

        /// <summary>
        /// 条件是否满足
        /// </summary>
        /// <param name="type">条件类</param>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据</param>
        /// <returns></returns>
        public static bool CheckCondition(string type, string param, object customData = null)
        {
            //获取类型
            Type typeTemp;
            if (m_IStoryConditionTypeDic.ContainsKey(type)) typeTemp = m_IStoryConditionTypeDic[type];
            else
            {
                typeTemp = Type.GetType(type);
                if (typeTemp.GetInterfaces().Contains(IStoryConditionType))
                    m_IStoryConditionTypeDic.Add(type, typeTemp);
                else
                    typeTemp = null;
            }

            if (typeTemp == null) return false;

            return CheckCondition(typeTemp, param, customData);
        }

        /// <summary>
        /// 条件是否满足
        /// </summary>
        /// <param name="type">条件类</param>
        /// <param name="param">参数</param>
        /// <param name="customData">自定义数据</param>
        /// <returns></returns>
        public static bool CheckCondition(Type type, string param, object customData = null)
        {
            if (!GetStoryCondition(type, param, out IStoryCondition iStoryCondition)) return false;

            return iStoryCondition.CheckCondition(customData);
        }

        public static bool GetStoryConditionDefault(Type type, out IStoryCondition iStoryCondition)
        {
            return GetStoryCondition(type, "", out iStoryCondition);
        }

        /// <summary>
        /// 获取一个故事条件接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="param"></param>
        /// <param name="iStoryCondition"></param>
        /// <returns></returns>
        public static bool GetStoryCondition(Type type, string param, out IStoryCondition iStoryCondition)
        {
            iStoryCondition = null;

            if (type == null || !type.GetInterfaces().Contains(IStoryConditionType)) return false;

            StoryTypeParamKey key = new StoryTypeParamKey(type, param);

            //获取接口类
            if (m_StoryConditionDic.ContainsKey(key))
            {
                iStoryCondition = m_StoryConditionDic[key];
            }
            else
            {
                iStoryCondition = System.Activator.CreateInstance(type) as IStoryCondition;

                if (iStoryCondition == null) return false;
                if(!string.IsNullOrEmpty(param)) iStoryCondition.Parse(param);
                m_StoryConditionDic.Add(key, iStoryCondition);
            }

            return true;
        }
    }
}