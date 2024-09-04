using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//部分枚举为世界地图专用
namespace WorldMap
{
    /// <summary>
    /// 地形类型
    /// </summary>
    public enum ELandformType
    {
        None,

        /// <summary>
        /// 平原
        /// </summary>
        Plains,

        /// <summary>
        /// 丘陵
        /// </summary>
        Hills,

        /// <summary>
        /// 山地
        /// </summary>
        Mountain,

        /// <summary>
        /// 高原
        /// </summary>
        Plateau,

        /// <summary>
        /// 冰层
        /// </summary>
        IceCover,

        /// <summary>
        /// 火山
        /// </summary>
        Volcano,

        /// <summary>
        /// 洞穴
        /// </summary>
        Cave,

        /// <summary>
        /// 地底
        /// </summary>
        Underground,

        /// <summary>
        /// 海洋
        /// </summary>
        Ocean,

        /// <summary>
        /// 海底
        /// </summary>
        Seabed,

        /// <summary>
        /// 空岛
        /// </summary>
        SkyeIsle,

        /// <summary>
        /// 破碎之地
        /// </summary>
        Broken,
    }

    /// <summary>
    /// 地貌类型
    /// </summary>
    public enum EPhysiognomyType
    {
        None,

        /// <summary>
        /// 沙漠
        /// </summary>
        Desert,

        /// <summary>
        /// 沙地
        /// </summary>
        Sandy,

        /// <summary>
        /// 荒地
        /// </summary>
        Wasteland,

        /// <summary>
        /// 草地
        /// </summary>
        Grassland,

        /// <summary>
        /// 冻土
        /// </summary>
        Tundra,

        /// <summary>
        /// 雪地
        /// </summary>
        Snowfield,

        /// <summary>
        /// 岩地
        /// </summary>
        Rockland,

        /// <summary>
        /// 水
        /// </summary>
        Waterland,

        /// <summary>
        /// 火山灰烬
        /// </summary>
        Cinerite,

        /// <summary>
        /// 菌地
        /// </summary>
        Fungiland,
    }

    /// <summary>
    /// 气候类型
    /// </summary>
    public enum EClimateType
    {
        None,

        /// <summary>
        /// 热带雨林
        /// </summary>
        TropicalRainy,

        /// <summary>
        /// 热带季风
        /// </summary>
        TropicalMonsoon,

        /// <summary>
        /// 热带草原
        /// </summary>
        TropicalSavanna,

        /// <summary>
        /// 热带沙漠
        /// </summary>
        TropicalDesert,

        /// <summary>
        /// 亚热带季风
        /// </summary>
        SubtropicalMonsoon,

        /// <summary>
        /// 温带海洋
        /// </summary>
        TemperateOceanic,

        /// <summary>
        /// 温带大陆
        /// </summary>
        TemperateContinental,

        /// <summary>
        /// 温带季风
        /// </summary>
        TemperateMonsoon,

        /// <summary>
        /// 高原山地气候
        /// </summary>
        PlateauMontane,

        /// <summary>
        /// 地中海
        /// </summary>
        Mediterranean,

        /// <summary>
        /// 寒带
        /// </summary>
        Frigid,

        /// <summary>
        /// 地底
        /// </summary>
        Underground,

        /// <summary>
        /// 海底
        /// </summary>
        Seabed,
    }

    /// <summary>
    /// 特性类型
    /// </summary>
    public enum EPeculiarityType
    {
        None,

        //100段 温度描述 仅存在一种

        /// <summary>
        /// 极寒
        /// </summary>
        ExtremelyCold = 100,

        /// <summary>
        /// 温暖
        /// </summary>
        Warmth,

        /// <summary>
        /// 炎热
        /// </summary>
        Torridity,


        //200段 湿度描述 仅存在一种

        /// <summary>
        /// 潮湿
        /// </summary>
        Moist = 200,

        /// <summary>
        /// 干燥
        /// </summary>
        Desiccation,


        //300段 自然环境描述 仅存在一种

        /// <summary>
        /// 针叶林
        /// </summary>
        ConiferousForest = 300,

        /// <summary>
        /// 阔叶林
        /// </summary>
        BroadleafForest,

        /// <summary>
        /// 热带雨林
        /// </summary>
        Rainforest,

        /// <summary>
        /// 枯木林
        /// </summary>
        WitheredForest,

        /// <summary>
        /// 芦苇丛
        /// </summary>
        Phragmites,

        /// <summary>
        /// 灌木丛
        /// </summary>
        Shrub,

        /// <summary>
        /// 荒草地
        /// </summary>
        WildGrass,

        /// <summary>
        /// 花丛
        /// </summary>
        Flowers,

        /// <summary>
        /// 水晶簇丛
        /// </summary>
        crystalThicket,

        /// <summary>
        /// 石林
        /// </summary>
        StoneForest,


        //400段 其他 可同时存在

        /// <summary>
        /// 沿江河
        /// </summary>
        Rivers = 400,

        /// <summary>
        /// 沿海
        /// </summary>
        Coastal,

        /// <summary>
        /// 岩浆
        /// </summary>
        Magma,

        /// <summary>
        /// 悬崖
        /// </summary>
        Cliff,

        /// <summary>
        /// 天空
        /// </summary>
        Welkin,

        /// <summary>
        /// 迷雾
        /// </summary>
        Fog,

        /// <summary>
        /// 珊瑚礁
        /// </summary>
        coralReef,

        /// <summary>
        /// 幽暗
        /// </summary>
        Dim,


        //500段 特殊

        /// <summary>
        /// 绿洲
        /// </summary>
        Oasis = 500,

        /// <summary>
        /// 大裂痕
        /// </summary>
        BigCracks,

        /// <summary>
        /// 巨坑
        /// </summary>
        Endocrator,

        /// <summary>
        /// 世界树
        /// </summary>
        WoeldTree,

        /// <summary>
        /// 地下城
        /// </summary>
        Dungeons = 300,
    }

    /// <summary>
    /// 建筑类型
    /// </summary>
    public enum EBuildingType
    {
        None,

        //100段 建筑集群规模 唯一

        /// <summary>
        /// 都市
        /// </summary>
        Metropolis = 100,

        /// <summary>
        /// 城市
        /// </summary>
        City,

        /// <summary>
        /// 城镇
        /// </summary>
        Township,

        /// <summary>
        /// 村庄
        /// </summary>
        Village,

        /// <summary>
        /// 部落
        /// </summary>
        Tribe,

        /// <summary>
        /// 房屋
        /// </summary>
        House,

        /// <summary>
        /// 营地
        /// </summary>
        Campsite,

        /// <summary>
        /// 农场
        /// </summary>
        Farm,

        /// <summary>
        /// 堡垒
        /// </summary>
        Fortress,


        //200段 单个建筑物 可多个

        /// <summary>
        /// 岗哨
        /// </summary>
        Sentry = 200,

        /// <summary>
        /// 港口
        /// </summary>
        Port,

        /// <summary>
        /// 灯塔
        /// </summary>
        Lighthouse,

        /// <summary>
        /// 破屋
        /// </summary>
        PoorHouse,

        /// <summary>
        /// 神庙
        /// </summary>
        Hieron,

        /// <summary>
        /// 高塔
        /// </summary>
        HighTower,
    }
}

//有些枚举主要用于世界地图 但也是公共的定义

/// <summary>
/// 国家类型
/// </summary>
public enum ECountryType
{
    None,
}

/// <summary>
/// 所属区域类型
/// </summary>
public enum EAreaType
{
    None,
}

/// <summary>
/// 所属大陆类型
/// </summary>
public enum EContinentType
{
    None,
}

/// <summary>
/// 所属星球类型
/// </summary>
public enum EStellarType
{
    /// <summary>
    /// 艾伯盖亚
    /// </summary>
    AbleGaea,
}