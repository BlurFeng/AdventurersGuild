using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FInteractionSystem;

/// <summary>
/// �ɽ�������ľ�� ������
/// </summary>
public class IObj_Barrel : FInteractionObjectBase
{
    public override bool OnDistanceClose(Component other, bool isOn)
    {
        if(!base.OnDistanceClose(other, isOn)) return false;

        Debug.Log(other.name + (isOn ? " ������ " : " Զ���� ") + GameObjectGet.name);

        return true;
    }

    public override bool OnDistanceVeryClose(Component other, bool isOn)
    {
        if (!base.OnDistanceVeryClose(other, isOn)) return false;

        Debug.Log(other.name + (isOn ? " �ǳ����� " : " ���ǳ����� ") + GameObjectGet.name);

        return true;
    }

    public override bool OnOutline(Component other, bool isOn, out bool conditionAllowed)
    {
        if (!base.OnOutline(other, isOn, out conditionAllowed)) return false;

        Debug.Log(other.name + (isOn ? " ����� " : " �ر���� ") + GameObjectGet.name);

        return true;
    }
}
