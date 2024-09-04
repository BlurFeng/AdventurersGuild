using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FsGameFramework.InputSystem;
using Deploy;
using UnityEngine.EventSystems;
using System;
using FsGridCellSystem;
using FsGameFramework;

/// <summary>
/// 网格区域项目
/// </summary>
public class AreaBaseActor : GridItemActor
{
    private int m_AreaGroupInfoId; //区域组信息 ID

    public override bool Init(object owner = null)
    {
        bool succeed = base.Init(owner);

        return true;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        Init(FWorldContainer.GetWorld());
    }

    protected override void OnStart()
    {
        base.OnStart();

    }
}
