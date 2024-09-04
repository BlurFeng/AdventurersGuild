using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FInteractionSystem;

/// <summary>
/// 可交互物体木箱 测试用
/// </summary>
public class IObj_Barrel : FInteractionObjectBase
{
    public override bool OnDistanceClose(Component other, bool isOn)
    {
        if(!base.OnDistanceClose(other, isOn)) return false;

        Debug.Log(other.name + (isOn ? " 靠近了 " : " 远离了 ") + GameObjectGet.name);

        return true;
    }

    public override bool OnDistanceVeryClose(Component other, bool isOn)
    {
        if (!base.OnDistanceVeryClose(other, isOn)) return false;

        Debug.Log(other.name + (isOn ? " 非常靠近 " : " 不非常靠近 ") + GameObjectGet.name);

        return true;
    }

    public override bool OnOutline(Component other, bool isOn, out bool conditionAllowed)
    {
        if (!base.OnOutline(other, isOn, out conditionAllowed)) return false;

        Debug.Log(other.name + (isOn ? " 打开描边 " : " 关闭描边 ") + GameObjectGet.name);

        return true;
    }
}
