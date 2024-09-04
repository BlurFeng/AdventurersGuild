using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FsConditionParser
{
    public class ConditionParser
    {
        /// <summary>
        /// 解析逻辑表达式 表达式使用占位符(允许相同的占位符 但会影响效率) 和对应逻辑值替换
        /// </summary>
        /// <param name="logicExpression">表达式 如 " ( A && B || ( C || !D )) || E && !F "  支持双符号和单符号 如 &&和& ||和|</param>
        /// <param name="logicVauleArray">表达式占位符替换对应逻辑值 true false</param>
        /// <returns></returns>
        public static bool ConditionParserByString(string logicExpression, bool[] logicVauleArray)
        {
            //判空
            if(logicVauleArray.Length == 0)
            {
                Debug.LogError("logicVauleArray length is zero!");
                return false;
            }

            //提取占位符 保留!非符号
            string[] variableArray = ExtractPlaceholder(logicExpression, true);

            if (variableArray.Length == 0)
            {
                Debug.LogError("logicExpression extract placeholder error!");
                return false;
            }

            //表达式占位符数量和传入逻辑数组长度对比
            if (variableArray.Length != logicVauleArray.Length)
            {
                Debug.LogError("FsConditionParser : logicExpression placeholder length not equal logicVauleArray length!");
                return false;
            }

            StringBuilder sbLogicExpression = new StringBuilder(logicExpression);

            //确认是否包含了相同的占位符 相同占位符会被替换 但这会降低效率 请尽量不要使用相同的占位符
            List<int> sameIndexs = new List<int>();//相同字符在variableArray中的Index 每次i变化后清空
            List<int> skipIndexs = new List<int>();//不用再检查直接跳过的variableArray中的Index
            for (int i = 0; i < variableArray.Length; i++)
            {
                if (skipIndexs.Contains(i)) continue;

                int counter = 1;//有几个相同的占位符
                string s = variableArray[i];//这次对比的占位符内容
                sameIndexs.Clear();//相同占位符的下标
                sameIndexs.Add(i);//先添加自己
                skipIndexs.Add(i);//在之后对比中可以跳过的Index

                //从i之后开始对比
                for (int j = i + 1; j < variableArray.Length; j++)
                {
                    if (skipIndexs.Contains(j)) continue;

                    //找到了相同的占位符
                    if (s.Equals(variableArray[j]))
                    {
                        counter++;
                        sameIndexs.Add(j);
                        skipIndexs.Add(j);
                        Debug.Log("logicExpression have the same placeholders!This slows down parsing!");
                    }
                }

                //有相同的占位符
                if(counter > 1)
                {
                    int subLength = s.Length;//占位符长度
                    int num = 0;//相同占位符编号

                    for (int k = 0; k < logicExpression.Length; k++)
                    {
                        //查看这一段是不是相同的占位符
                        if (logicExpression.Substring(k, subLength).Equals(s))
                        {
                            //检查对比的内容左边和右边是不是逻辑运算符 确保不会替换掉其他一长串字符中恰好和占位符相同的位置
                            bool allow = true;
                            if(k > 0 && k + subLength < logicExpression.Length)
                            {
                                int ascL = (int)logicExpression.Substring(k - 1, 1).ToCharArray()[0];
                                int ascR = (int)logicExpression.Substring(k + subLength, 1).ToCharArray()[0];
                                allow = (ascL == 33 || ascL == 38 || ascL == 124) && (ascR == 38 || ascR == 124);
                            }
                            
                            if (allow)
                            {
                                //替换掉旧的占位符
                                string replaceStr = s + num;
                                //startIndex为k + num 因为每一次替换后实际上sbLogicExpression的长度也增加了1 比logicExpression长
                                sbLogicExpression.Replace(s, replaceStr, k + num, subLength);
                                variableArray[sameIndexs[num]] = replaceStr;//替换掉占位符数组中对应内容
                                num++;//序号自增
                                counter--;//相同数量自减
                            }
                        }

                        //全部被替换掉了
                        if (counter == 0)
                            break;
                    }

                    logicExpression = sbLogicExpression.ToString();//更新逻辑表达式字符串
                }
            }

            //将占位符替换为对应的逻辑值
            for (int i = 0; i < variableArray.Length; i++)
            {
                if (variableArray[i].Contains("!"))
                    sbLogicExpression.Replace(variableArray[i], (!logicVauleArray[i]).ToString());
                else
                    sbLogicExpression.Replace(variableArray[i], logicVauleArray[i].ToString());
            }

            return ConditionParserByString(sbLogicExpression.ToString());
        }

        /// <summary>
        /// 解析逻辑表达式
        /// </summary>
        /// <param name="logicExpression">表达式 如 " ( true && false || ( true || !true )) || false && !true "  支持双符号和单符号 如 &&和& ||和|</param>
        /// <returns></returns>
        public static bool ConditionParserByString(string logicExpression)
        {
            //传入参数判空
            if (string.IsNullOrEmpty(logicExpression))
            {
                Debug.LogError("logicExpression is null or empty!");
                return false;
            }

            logicExpression = logicExpression.ToLower();
            //判断是否是合法的表达式
            if (!logicExpression.Contains("true") && !logicExpression.Contains("false"))
            {
                Debug.LogError("LogicExpression is not contains true or false!");
                return false;
            }

            //替换为单个符号
            logicExpression = logicExpression.Replace("&&", "&");
            logicExpression = logicExpression.Replace("||", "|");
            //根据()括号多次运算括号内逻辑 并替换()括号内容 直到没有()
            while (logicExpression.Contains("("))
            {
                int lasttLeftBracketIndex = -1;
                int firstRightBracketIndex = -1;

                //找到最后一个左括号
                for (int i = 0; i < logicExpression.Length; i++)
                {
                    string tempChar = logicExpression.Substring(i, 1);
                    if (tempChar == "(")
                        lasttLeftBracketIndex = i;
                }

                //找到与最后第一个左括号对应的右括号
                for (int i = lasttLeftBracketIndex; i < logicExpression.Length; i++)
                {
                    string tempChar = logicExpression.Substring(i, 1);
                    if (tempChar == ")" && firstRightBracketIndex == -1)
                        firstRightBracketIndex = i;
                }

                //确认括号数量是否正确
                if (lasttLeftBracketIndex == -1 || firstRightBracketIndex == -1)
                {
                    Debug.LogError("logicExpression incorrect number of parentheses please check!");
                    return false;
                }

                //获取括号中的逻辑表达式并进行解析操作
                string calculateExpression = logicExpression.Substring(lasttLeftBracketIndex + 1, firstRightBracketIndex - lasttLeftBracketIndex - 1);
                bool logicResult = LogicOperate(calculateExpression);

                //将括号内容替换为逻辑值 如果有完全一样的多组内容 那么也会直接替换 这不仅是正确的且能提高效率
                logicExpression = logicExpression.Replace("(" + calculateExpression + ")", logicResult.ToString());
            }

            //确认括号数量是否正确
            if (logicExpression.Contains(")"))
            {
                Debug.LogError("logicExpression incorrect number of parentheses please check!");
                return false;
            }

            //没有括号了 进行最后一次逻辑运算
            return LogicOperate(logicExpression);
        }

        /// <summary>
        /// 提取逻辑表达式中的占位符 移除 && || ! ( ) 支持单符号 & |
        /// </summary>
        /// <param name="logicExpression">逻辑表达式</param>
        /// <param name="retainNon">保留非!</param>
        /// <returns></returns>
        public static string[] ExtractPlaceholder(string logicExpression, bool retainNon = true)
        {
            //传入参数判空
            if (string.IsNullOrEmpty(logicExpression))
            {
                Debug.LogError("logicExpression is null or empty!");
                return null;
            }

            StringBuilder sb = new StringBuilder(logicExpression);

            //替换为单符号
            sb.Replace("&&", "&");
            sb.Replace("||", "|");
            //去除空格和括号
            sb.Replace(" ", "");
            sb.Replace("(", "");
            sb.Replace(")", "");
            //将逻辑运算符用逗号替换
            sb.Replace("&", ",");
            sb.Replace("|", ",");
            if (!retainNon)
            {
                sb.Replace("!", ",");
            }
            //获取逻辑表达式中的占位符数组
            string[] outArray = sb.ToString().Split(',');

            return outArray;
        }

        /// <summary>
        /// 运算逻辑表达式
        /// </summary>
        /// <param name="logicExpression"></param>
        /// <returns></returns>
        private static bool LogicOperate(string logicExpression)
        {
            bool haveAnd = logicExpression.Contains("&");
            bool haveOr = logicExpression.Contains("|");

            if (!haveAnd && !haveOr)
                return bool.Parse(logicExpression);//不包含逻辑符号 直接返回

            //去除空格
            logicExpression = logicExpression.Replace(" ", "");

            //用"|"切割
            string[] arrLogicValue = logicExpression.Split('|');
            for (int i = 0; i < arrLogicValue.Length; i++)
            {
                string logicStr = arrLogicValue[i];
                if (logicStr.Contains("&"))
                {
                    logicStr = (!logicStr.Contains("false")).ToString();
                }

                //有或时 
                if (haveOr)
                {
                    //任一为true就是true
                    if (bool.Parse(logicStr))
                        return true;
                }
                //只有且时
                else
                {
                    //任一为false就是false
                    if (!bool.Parse(logicStr))
                        return false;
                }

                arrLogicValue[i] = logicStr;
            }

            //走到这
            //如果有或 则代表所有都为false
            //如果只有且 则代表所有都为true
            return !haveOr;
        }
    }
}