using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using FsListItemPages;

public class PropModel : Singleton<PropModel>, IDestroy
{
    /// <summary>
    /// 道具类型
    /// </summary>
    public enum EPropType
    {
        None,
        /// <summary>
        /// 资产
        /// </summary>
        Property,
        /// <summary>
        /// 材料
        /// </summary>
        Material,
        /// <summary>
        /// 功能
        /// </summary>
        Function,
        /// <summary>
        /// 身体(Skin)
        /// </summary>
        Body,
        /// <summary>
        /// 装备(Skin)
        /// </summary>
        Equip,
        /// <summary>
        /// 家具
        /// </summary>
        Furniture,
        /// <summary>
        /// 独特
        /// </summary>
        Unique,
    }

    /// <summary>
    /// 家具类型
    /// </summary>
    public enum EFurnitureType
    {
        None,
        /// <summary>
        /// 其他
        /// </summary>
        Other,
        /// <summary>
        /// 桌
        /// </summary>
        Table,
        /// <summary>
        /// 椅
        /// </summary>
        Chair,
        /// <summary>
        /// 前台
        /// </summary>
        Desk,
        /// <summary>
        /// 委托板
        /// </summary>
        EntrustBoard,
        /// <summary>
        /// 装饰
        /// </summary>
        Decorate,
    }

    public override void Init()
    {
        base.Init();

    }
}
