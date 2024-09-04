using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FsConditionParser
{
    public class ConditionParserComplex
    {
        #region Fields
        /// <summary>
        /// 将表达式以&&或者||切割
        /// group[1]和group[3]为单个逻辑表达式 group[2]为右边的运算符【&&或||】
        /// </summary>
        private static readonly Regex regCutItem = new Regex(@"(.*?)([&|\|]{2})|(\S.*)");
        /// <summary>
        /// 将每个单独的item匹配出小分子 例子:3>2 => [3,>,2]
        /// </summary>
        private static readonly Regex regCutMolecule = new Regex(@"(.*?)([>|<]=?|=)(.*)");
        /// <summary>
        /// 验证每个单独的item是否合法
        /// </summary>
        private static readonly Regex regValidMolecult = new Regex(@"^([\w\[\]]*?)([>|<]=?|=)([\w\[\]]*)$");
        /// <summary>
        /// 提取表达式中的小括号内容 (如有小括号嵌套 需重复匹配;最先获取最内部括号内容,由内向外获取)
        /// </summary>
        private static readonly Regex regSmallBracket = new Regex(@"\([^(]*?\)");
        /// <summary>
        /// 提取单个单个小分子的字段值
        /// </summary>
        private static readonly Regex regGetField = new Regex(@"^\s*?\[(\S+)\]\s*?$");

        private static readonly string trueText = "1=1";
        private static readonly string falseText = "1=2";
        #endregion

        #region Methods
        /// <summary>
        /// 判断表达式的有效性
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool JudgeValid(string expression)
        {
            if (string.IsNullOrEmpty(expression)) new ArgumentException("is null or empty", "expression");
            bool haveBracketite = regSmallBracket.IsMatch(expression);
            //匹配出所有括号内的内容验证是否合法,合法则remove掉
            while (regSmallBracket.IsMatch(expression))
            {
                MatchCollection collection = regSmallBracket.Matches(expression);
                foreach (Match bracketite in collection)
                {
                    int index = 0; string opL = string.Empty, opR = string.Empty;
                    //匹配到的值 带括号(8>5&&7>3.....)
                    string expBracketite = bracketite.Value;
                    index = expression.IndexOf(expBracketite);
                    //括号内容前的连接运算符
                    if (index >= 2) opL = expression.Substring(index - 2, 2);
                    //括号内容后的连接运算符
                    if (expression.Length > index + expBracketite.Length + 2) opR = expression.Substring(index + expBracketite.Length, 2);
                    //如果连接符不为&&或||则不符合表达式规则
                    if (!string.IsNullOrEmpty(opL) && opL != "&&" && opL != "||") return false;
                    if (!string.IsNullOrEmpty(opR) && opR != "&&" && opR != "||") return false;
                    MatchCollection items = regCutItem.Matches(expBracketite.Substring(1, expBracketite.Length - 2));
                    //如果集合数小于1 则括号之间内容为空或者内容不符合规则
                    if (items.Count < 1) return false;
                    int indexOfItem1 = 0;
                    foreach (Match item in items)
                    {
                        indexOfItem1++;
                        //获取表达式
                        string expItem = indexOfItem1 == items.Count ? item.Groups[3].Value : item.Groups[1].Value;
                        //验证表达式的有效性
                        if (!regValidMolecult.IsMatch(expItem)) return false;
                    }
                    //remove括号内的表达式以及括号前的连接运算符
                    expression = expression.Remove(index < 2 ? index : index - 2, expBracketite.Length + (index < 2 ? index : 2));
                }
            }
            //验证去除括号内容后剩下表达式的有效性
            MatchCollection expItems = regCutItem.Matches(expression);
            //如果表达式没有括号且匹配结果数小于1 则表达式内容不符合规则
            if (!haveBracketite && expItems.Count < 1) return false;
            int indexOfItem = 0;
            foreach (Match item in expItems)
            {
                indexOfItem++;
                //获取表达式
                string expItem = indexOfItem == expItems.Count ? item.Groups[3].Value : item.Groups[1].Value;
                //验证表达式的有效性
                if (!regValidMolecult.IsMatch(expItem)) return false;
            }
            return true;
        }

        /// <summary>
        /// 获取表达式结果
        /// 表达式支持逻辑操作符和比较符
        /// 例1：[field]>3&&[field2]<78  ;  传入Dic{[field,23],[field2,66]}
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="prm">替换数据Dic</param>
        /// <returns></returns>
        public static bool GetResult(string expression, Dictionary<string, object> prm)
        {
            if (prm == null) throw new Exception("invalid prm");
            if (!JudgeValid(expression)) throw new Exception("invalid expression");

            //匹配出所有括号内的内容计算结果并根据结果替换对应的值
            while (regSmallBracket.IsMatch(expression))
            {
                MatchCollection collection = regSmallBracket.Matches(expression);
                foreach (Match bracketite in collection)
                {
                    //表达式内容赋值
                    string expBracketite = bracketite.Value;
                    int index = expression.IndexOf(expBracketite);
                    //获取表达式计算后的结果
                    bool result = MosaicLambda(expBracketite.Substring(1, expBracketite.Length - 2), prm);
                    //根据结果将括号内表达式替换成不同字符串
                    expression = expression.Remove(index, expBracketite.Length).Insert(index, result ? trueText : falseText);
                }
            }
            return MosaicLambda(expression, prm);
        }

        /// <summary>
        /// 获取表达式结果
        /// 表达式支持逻辑操作符和比较符
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        private static bool GetResult(string expression)
        {
            return GetResult(expression, new Dictionary<string, object>());
        }

        /// <summary>
        /// 拼接动态Lambda并获取结果
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        private static bool MosaicLambda(string expression, Dictionary<string, object> prm)
        {
            //Fields
            Expression lambda = null;
            string connector = null;
            MatchCollection items = regCutItem.Matches(expression);
            int indexOfItem = 0;
            foreach (Match item in items)
            {
                indexOfItem++;
                Expression left = null, right = null;
                BinaryExpression binary = null;
                //获取表达式
                string expItem = indexOfItem == items.Count ? item.Groups[3].Value : item.Groups[1].Value;
                Match Molecule = regCutMolecule.Matches(expItem)[0];
                //判断左边是否是字段
                if (regGetField.IsMatch(Molecule.Groups[1].Value))
                {
                    string field = regGetField.Matches(Molecule.Groups[1].Value)[0].Groups[1].Value;
                    if (!prm.ContainsKey(field)) throw new Exception($"未提供必须字段[{field}]");
                    left = Expression.Constant(prm[field]);
                }
                else
                {
                    object v = null;
                    Type t = TryGetType(Molecule.Groups[1].Value, out v);
                    left = Expression.Constant(v, t);
                }
                //判断右边是否是字段
                if (regGetField.IsMatch(Molecule.Groups[3].Value))
                {
                    string field = regGetField.Matches(Molecule.Groups[3].Value)[0].Groups[1].Value;
                    if (!prm.ContainsKey(field)) throw new Exception($"未提供必须字段[{field}]");
                    right = Expression.Constant(prm[field]);
                }
                else
                {
                    object v = null;
                    Type t = TryGetType(Molecule.Groups[3].Value, out v);
                    right = Expression.Constant(v, t);
                }
                if (left.Type != right.Type) throw new Exception("尝试比较两个不同数据类型的值");
                //根据操作符选择合适的方法进行装载
                switch (Molecule.Groups[2].Value)
                {
                    case ">":
                        binary = Expression.GreaterThan(left, right);
                        break;
                    case ">=":
                        binary = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "=":
                        binary = Expression.Equal(left, right);
                        break;
                    case "<=":
                        binary = Expression.LessThanOrEqual(left, right);
                        break;
                    case "<":
                        binary = Expression.LessThan(left, right);
                        break;
                    default: throw new Exception("invalid charactor");
                }
                //根据上一个item设置的连接符进行相应的操作
                if (connector == null) lambda = binary;
                else
                {
                    switch (connector)
                    {
                        case "&&":
                            lambda = Expression.And(lambda, binary);
                            break;
                        case "||":
                            lambda = Expression.Or(lambda, binary);
                            break;
                        default: throw new Exception("invalid charactor");
                    }
                }
                //And|Or赋值
                connector = item.Groups[2].Value;
            }
            return Expression.Lambda<Func<bool>>(lambda).Compile().Invoke();
        }

        private static Type TryGetType(string v, out object obj)
        {
            int i = default(int); float f = default(float); double d = default(double);
            if (int.TryParse(v, out i)) { obj = i; return typeof(int); }
            if (float.TryParse(v, out f)) { obj = f; return typeof(float); }
            if (double.TryParse(v, out d)) { obj = d; return typeof(double); }
            obj = v;
            return typeof(string);
        }
        #endregion
    }
}