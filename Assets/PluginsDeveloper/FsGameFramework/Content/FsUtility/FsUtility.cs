using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FsUtility
{
    #region Confirm Distance Direction Position 方向方位

    /// <summary>
    /// 确认目标和自身发方位
    /// </summary>
    /// <param name="selfPos">自身位置</param>
    /// <param name="selfForward">自身正面方向</param>
    /// <param name="targetPos">目标位置</param>
    /// <param name="disLimit">距离限制</param>
    /// <param name="dotLimit">角度限制</param>
    /// <param name="forward">确认在正面还是背面</param>
    /// <returns>true=在有效范围内</returns>
    public static bool ConfirmLocation(Vector3 selfPos, Vector3 selfForward, Vector3 targetPos, float disLimit, float dotLimit, bool forward = true)
    {
        float dis = Vector3.Distance(targetPos, selfPos);
        Vector3 diff = targetPos - selfPos;
        diff.y = 0;
        float dot = Vector3.Dot(selfForward, diff.normalized);

        if (dis < disLimit)
        {
            if (forward && dot > dotLimit)
            {
                return true;
            }
            else if (dot < dotLimit)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 限制向量currentDir和向量targetDir的夹角 并返回currentDir修改之后的向量
    /// </summary>
    /// <param name="currentDir">当前向量</param>
    /// <param name="targetDir">目标向量</param>
    /// <param name="limitAngle">角度限制</param>
    /// <returns></returns>
    public static Vector3 DirectionRotateLimit(Vector3 currentDir, Vector3 targetDir, float limitAngle)
    {
        float angleDif = Vector3.Angle(currentDir, targetDir);
        if (angleDif > limitAngle)
        {
            float angleTemp = angleDif - limitAngle;
            currentDir = RotateTowards(currentDir, targetDir, angleTemp);

            //Debug.Log("方向修正" + angleDif + "   " + Vector3.Angle(currentDir, targetDir));
        }

        return currentDir;
    }

    /// <summary>
    /// 向量currentDir 向向量targetDir旋转 最多旋转到和目标相同
    /// </summary>
    /// <param name="currentDir">当前向量</param>
    /// <param name="targetDir">目标向量</param>
    /// <param name="maxAngle">最大旋转角度限制</param>
    /// <returns></returns>
    public static Vector3 RotateTowards(Vector3 currentDir, Vector3 targetDir, float maxAngle)
    {
        currentDir = Vector3.RotateTowards(currentDir, targetDir, maxAngle * Mathf.Deg2Rad, 0f);
        return currentDir;
    }

    /// <summary>
    /// 向量dir2在向量dir1的左边或右边
    /// </summary>
    /// <param name="dir1"></param>
    /// <param name="dir2"></param>
    /// <returns>true=左边 false=右边</returns>
    public static bool DirectionLeftOrRight(Vector3 dir1, Vector3 dir2)
    {
        dir1.y = 0;
        dir2.y = 0;

        Vector3 cross = Vector3.Cross(dir1, dir2);
        if (cross.y < 0)
        {
            //Debug.Log("left");
            return true;
        }
        else
        {
            //Debug.Log("Right");
            return false;
        }
    }

    /// <summary>
    /// 向量dir2在向量dir1的左边或右边
    /// </summary>
    /// <param name="dir1"></param>
    /// <param name="dir2"></param>
    /// <returns>true=前方 false=后方</returns>
    public static bool DirectionForwardOrBack(Vector3 dir1, Vector3 dir2)
    {
        dir1.y = 0;
        dir2.y = 0;

        float dot = Vector3.Dot(dir1, dir2);
        if (dot > 0f)
        {
            //Debug.Log("left");
            return true;
        }
        else
        {
            //Debug.Log("Right");
            return false;
        }
    }
    #endregion

    /// <summary>
    /// 通过名字寻找跟节点和跟节点的子节点中名字一致的节点
    /// </summary>
    /// <param name="findName">寻找对象名称</param>
    /// <param name="rootTs">寻找范围的根节点</param>
    /// <returns></returns>
    public static Transform FindChildByName(string findName, Transform rootTs)
    {
        Transform ReturnObj;

        if (rootTs.name == findName)
            return rootTs.transform;

        foreach (Transform child in rootTs)
        {
            ReturnObj = FindChildByName(findName, child);

            if (ReturnObj != null)
                return ReturnObj;
        }

        return null;
    }

    /// <summary>
    /// 数值积累直到溢出值 达到溢出点后返回true并重置积累值（重置时保留多余的值）
    /// </summary>
    /// <param name="accumulateValue">积累用float字段</param>
    /// <param name="addValue">本次增加值</param>
    /// <param name="spillPoint">溢出点</param>
    /// <returns>是否达到溢出点</returns>
    public static bool OverflowValue(ref float accumulateValue, float addValue, float spillPoint)
    {
        return OverflowValue(ref accumulateValue, out float changedVaule, addValue, spillPoint, true);
    }

    /// <summary>
    /// 数值积累直到溢出值 达到溢出点后返回true并重置积累值
    /// </summary>
    /// <param name="accumulateValue">积累用float字段</param>
    /// <param name="changedVaule">改变后的积累值</param>
    /// <param name="addValue">本次增加值</param>
    /// <param name="spillPoint">溢出点</param>
    ///  <param name="ResetKeepExtra">重置时保留多余的部分</param>
    /// <returns>是否达到溢出点</returns>
    public static bool OverflowValue(ref float accumulateValue, out float changedVaule, float addValue, float spillPoint, bool ResetKeepExtra = true)
    {
        accumulateValue += addValue;
        changedVaule = accumulateValue;
        if (accumulateValue >= spillPoint)
        {
            changedVaule = accumulateValue;
            if (!ResetKeepExtra || accumulateValue == spillPoint)
                accumulateValue = 0f;
            else
                accumulateValue -= spillPoint;

            return true;
        }

        return false;
    }

    public static System.Type[] GetTypes(Type typeBase)
    {
        System.Type[] allTypes = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
        System.Type[] types = (from System.Type type in allTypes where type.IsSubclassOf(typeBase) select type).ToArray();

        return types;
    }
}
