using EntrustSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FsGridCellSystem
{
    public delegate bool CreatePrefabGameobjectProcessor(GameObject obj, string folderPath);

    public class GridSystemConfig
    {
        public const string viewRootName = "ViewRoot";
        public const string colliderRootName = "ColliderRoot";
        public const float pixelSizeToU3DSizeCoefficient = 0.01f;
        public const string gridItemToolsConfigName = "gridItemToolsConfig";

        public static char dataSep1 = ';';
        public static char dataSep2 = '|';
        public static char dataSep3 = ',';
        public static char dataSep4 = ':';
    }

    #region Aseprite Grid Item Data

    /// <summary>
    /// Aseprite头文件数据
    /// </summary>
    public struct AsepriteHeaderData
    {
        /// <summary>
        /// Aseprite的文件名称
        /// </summary>
        public string spriteName;

        /// <summary>
        /// Aseprite中配置的网格配置
        /// </summary>
        public AsepriteGridConfig asepriteGridConfig;
    }

    /// <summary>
    /// Aseprite网格配置
    /// </summary>
    public struct AsepriteGridConfig
    {
        /// <summary>
        /// Cell单元格像素尺寸
        /// </summary>
        public GridCoord cellSizePixel;
    }

    public struct AsepriteGridItemData
    {
        /// <summary>
        /// key名称，也是U3D预制体的备用名称
        /// </summary>
        public string keyName;

        /// <summary>
        /// 网格物体单位化尺寸
        /// </summary>
        public GridCoord gridItemSizeUnit;

        /// <summary>
        /// U3d中的位置数据
        /// 这个数据取决于Aseprite文件中原点位置的设置
        /// </summary>
        public GridCoordFloat u3dGridItemLocation;

        /// <summary>
        /// 真实体积数据
        /// </summary>
        public AsepriteRealVolumeData[] realVolumes;

        /// <summary>
        /// 用于确认生成的GridItem预制体挂载哪个脚本
        /// 此数据可以为空
        /// </summary>
        public string scriptTag;

        /// <summary>
        /// 预制体名称
        /// 此数据可能为空，为空时获取keyName作为预制体名称
        /// 当Aseprite的一个场景中，有多个相同的GridItem时(结构和图片完全一致)，我们可以设置预制体名称一致，那么导入到U3D中时将使用相同的预制体。
        /// </summary>
        public string prefabName;

        /// <summary>
        /// 图片数据数组
        /// Key=imageName, Vaule=GridItemImageData
        /// </summary>
        public Dictionary<string, AsepriteGridItemImageData> imageDataDic;

        /// <summary>
        /// 数据是否合法
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(keyName)) return false;
            if (gridItemSizeUnit.isNoVolume()) return false;
            //u3dGridItemLocation位置数据不做检查
            if (imageDataDic.Count == 0) return false;

            return true;
        }
    }

    public struct AsepriteRealVolumeData
    {
        /// <summary>
        /// 名称
        /// 导出的名称保证在一个GridItem下不重复
        /// </summary>
        public string keyName;

        /// <summary>
        /// 尺寸，整体包围盒的尺寸
        /// 像素单位
        /// </summary>
        public GridCoord size;

        /// <summary>
        /// 位置，整体包围盒左下角位置。
        /// 原点为GridItem的Size范围的左下角
        /// 像素单位
        /// </summary>
        public GridCoord location;
         
        /// <summary>
        /// 旋转，欧拉角。
        /// </summary>
        public GridCoordFloat rotate;

        /// <summary>
        /// 裁剪用方块数据
        /// 用于自动化生成碰撞器Collider
        /// </summary>
        public Square[] volumeCutSquares;
    }

    /// <summary>
    /// 网格物品体积裁剪用数据
    /// </summary>
    public struct Square
    {
        /// <summary>
        /// 裁剪尺寸，像素单位
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// 裁剪位置，像素单位
        /// </summary>
        public Vector2 pos;

        /// <summary>
        /// 有面积
        /// </summary>
        public bool haveArea { get { return size.x > 0 && size.y > 0; } }
    }

    /// <summary>
    /// 网格物品图片数据
    /// </summary>
    public struct AsepriteGridItemImageData
    {
        public enum EMeshType 
        {
            /// <summary>
            /// 方块
            /// </summary>
            Cube,

            /// <summary>
            /// 斜坡
            /// </summary>
            Slope,
        }

        public static EMeshType MeshTypeParse(string str)
        {
            return str switch
            {
                "Cube" => EMeshType.Cube,
                "Slope" => EMeshType.Slope,
                _ => EMeshType.Cube,
            };
        }

        /// <summary>
        /// 图片名称，不包含类型尾缀
        /// </summary>
        public string imageName;

        /// <summary>
        /// 图片在ViewRoot子节点下的位置
        /// U3D单位
        /// </summary>
        public Vector2 imagePos;

        /// <summary>
        /// 图片旋转
        /// </summary>
        public Vector3 imageRotate;

        /// <summary>
        /// Mesh类型
        /// </summary>
        public EMeshType meshType;

        /// <summary>
        /// 是否拥有Mesh数据
        /// </summary>
        public bool haveMeshData;

        /// <summary>
        /// Mesh信息，尺寸
        /// 像素单位
        /// </summary>
        public GridCoord meshSize;

        /// <summary>
        /// Mesh信息，位置，整体左下角位置。
        /// 本地坐标，原点为GridItem的Size范围的左下角
        /// 像素单位
        /// </summary>
        public GridCoord meshLocalLocation;
    }

    #endregion

    public class GridSystemEditorLibrary
    {
        #region Aseprite Grid Item Data

        //从Aseprite导出的数据文本解析并获取数据
        //对数据进行使用


        /// <summary>
        /// 获取网格物体数据字典，根据网格物体组数据Str
        /// </summary>
        /// <param name="gridItemsDataText">gridItemsDataText数据由Aseprite的lua工具脚本导出为.txt类型文件</param>
        /// <param name="asepriteHeaderData">头部数据</param>
        /// <param name="gridItemDataDic">GridItemData字典，Key=KeyName（预制体名称），Vaule=GridItemData</param>
        /// <returns>是否成功解析</returns>
        public static bool GetAsepriteGridItemsData(
            TextAsset gridItemsDataText,
            out AsepriteHeaderData asepriteHeaderData,
            out Dictionary<string, AsepriteGridItemData> gridItemDataDic)
        {
            asepriteHeaderData = new AsepriteHeaderData();
            gridItemDataDic = new Dictionary<string, AsepriteGridItemData>();

            if (gridItemsDataText == null || string.IsNullOrEmpty(gridItemsDataText.text)) return false;

            string[] gridItemsDataTextArr = gridItemsDataText.text.Split(GridSystemConfig.dataSep1);

            //解析头部数据
            if (!ParseAsepriteHeaderData(gridItemsDataTextArr, out asepriteHeaderData)) return false;

            //从1开始，因为第0个数据是GridConfig信息等头部数据
            for (int i = 1; i < gridItemsDataTextArr.Length; i++)
            {
                string[] gridItemDatas = gridItemsDataTextArr[i].Split(GridSystemConfig.dataSep2);

                AsepriteGridItemData gridItemData = CreateAsepriteGridItemData(gridItemDatas);

                gridItemDataDic.Add(gridItemData.keyName, gridItemData);
            }

            return true;
        }

        /// <summary>
        /// 获取Aseprite头部数据
        /// </summary>
        /// <param name="gridItemsDataText">gridItemsDataText数据由Aseprite的lua工具脚本导出为.txt类型文件</param>
        /// <param name="asepriteHeaderData">头部数据</param>
        /// <returns>是否成功解析</returns>
        public static bool GetAsepriteHeaderData(TextAsset gridItemsDataText, out AsepriteHeaderData asepriteHeaderData)
        {
            if (gridItemsDataText == null)
            {
                asepriteHeaderData = new AsepriteHeaderData();
                return false;
            }

            string[] gridItemsDataTextArr = gridItemsDataText.text.Split(GridSystemConfig.dataSep1);

            return ParseAsepriteHeaderData(gridItemsDataTextArr, out asepriteHeaderData);
        }

        /// <summary>
        /// 解析数据获取Aseprite头部数据
        /// </summary>
        /// <param name="gridItemsDataTextArr"></param>
        /// <param name="asepriteHeaderData"></param>
        /// <returns></returns>
        private static bool ParseAsepriteHeaderData(string[] gridItemsDataTextArr, out AsepriteHeaderData asepriteHeaderData)
        {
            if (gridItemsDataTextArr == null || gridItemsDataTextArr.Length < 2)
            {
                asepriteHeaderData = new AsepriteHeaderData();
                return false;
            }

            //解析头部数据
            string[] headerDatas = gridItemsDataTextArr[0].Split(GridSystemConfig.dataSep2);
            string[] cellSizePixelDatas = headerDatas[1].Split(GridSystemConfig.dataSep3);
            asepriteHeaderData = new AsepriteHeaderData
            {
                spriteName = headerDatas[0],
                asepriteGridConfig = new AsepriteGridConfig
                {
                    cellSizePixel = new GridCoord(int.Parse(cellSizePixelDatas[0]), int.Parse(cellSizePixelDatas[1]), int.Parse(cellSizePixelDatas[2]))
                }
            };

            return true;
        }

        /// <summary>
        /// 获取网格物体数据，根据网格物体组数据Str
        /// </summary>
        /// <param name="gridItemsDataText">gridItemsDataText数据由Aseprite的lua工具脚本导出为.txt类型文件</param>
        /// <param name="keyName">导出数据的查询名称，预制体名称</param>
        /// <param name="gridItemData">网格物品数据</param>
        /// <returns></returns>
        public static bool GetAsepriteGridItemDataSingle(TextAsset gridItemsDataText, string keyName, out AsepriteHeaderData asepriteHeaderData, out AsepriteGridItemData gridItemData, bool checkPrefabName = false)
        {
            gridItemData = new AsepriteGridItemData();
            if (gridItemsDataText == null || string.IsNullOrEmpty(gridItemsDataText.text) || string.IsNullOrEmpty(keyName))
            {
                asepriteHeaderData = new AsepriteHeaderData();
                return false;
            }

            string[] gridItemsDataTextArr = gridItemsDataText.text.Split(GridSystemConfig.dataSep1);

            if (!ParseAsepriteHeaderData(gridItemsDataTextArr, out asepriteHeaderData)) return false;

            for (int i = 1; i < gridItemsDataTextArr.Length; i++)
            {
                string[] gridItemDatas = gridItemsDataTextArr[i].Split(GridSystemConfig.dataSep2);

                if (checkPrefabName)
                {
                    var tempData = CreateAsepriteGridItemData(gridItemDatas);
                    if (keyName != tempData.prefabName) continue;
                    gridItemData = tempData;
                }
                else
                {
                    if (keyName != gridItemDatas[0]) continue;
                    gridItemData = CreateAsepriteGridItemData(gridItemDatas);
                }

                break;
            }

            return true;
        }

        /// <summary>
        /// 创建一份网格物品数据
        /// </summary>
        /// <param name="gridItemDatas"></param>
        /// <returns></returns>
        private static AsepriteGridItemData CreateAsepriteGridItemData(string[] gridItemDatas)
        {
            string keyName = gridItemDatas[0]; //键名称
            string[] gridItemSizeUnitArr = gridItemDatas[1].Split(GridSystemConfig.dataSep3); //网格物品单位化尺寸数据
            string[] gridItemLocationArr = gridItemDatas[2].Split(GridSystemConfig.dataSep3); //网格物品U3D位置数据
            string[] realVolumeDatas = gridItemDatas[3].Split(GridSystemConfig.dataSep3); //网格物品体积裁剪数据
            string scriptTag = gridItemDatas[4]; //脚本标记
            string prefabName = string.IsNullOrEmpty(gridItemDatas[5]) ? keyName : gridItemDatas[5]; //预制体名称
            string[] imageDatasStrArr = gridItemDatas[6].Split(GridSystemConfig.dataSep3); //图片数据组数据

            //真实体积数据
            AsepriteRealVolumeData[] realVolumes = null;
            if (realVolumeDatas.Length > 0 && !string.IsNullOrEmpty(realVolumeDatas[0]))
            {
                realVolumes = new AsepriteRealVolumeData[realVolumeDatas.Length];

                for (int i = 0; i < realVolumeDatas.Length; i++)
                {
                    var realVolumeDataArr = realVolumeDatas[i].Split(GridSystemConfig.dataSep4);

                    // 生成真实体积裁剪用数据
                    int volumeCutDataLength = Mathf.FloorToInt(realVolumeDataArr.Length / 4f) - 2;
                    Square[] volumeCutSquares = null;
                    if (volumeCutDataLength > 0)
                    {
                        volumeCutSquares = new Square[volumeCutDataLength];
                        for (int j = 0; j < volumeCutDataLength; j++)
                        {
                            var volumeCutData = realVolumeDatas[i].Split(GridSystemConfig.dataSep4);
                            volumeCutSquares[j] = new Square
                            {
                                size = new Vector2(int.Parse(volumeCutData[j * 4 + 10]), int.Parse(volumeCutData[j * 4 + 11])),
                                pos = new Vector2(int.Parse(volumeCutData[j * 4 + 12]), int.Parse(volumeCutData[j * 4 + 13])),
                            };
                        }
                    }

                    realVolumes[i] = new AsepriteRealVolumeData()
                    {
                        keyName = realVolumeDataArr[0],
                        size = new GridCoord(int.Parse(realVolumeDataArr[1]), int.Parse(realVolumeDataArr[2]), int.Parse(realVolumeDataArr[3])),
                        location = new GridCoord(int.Parse(realVolumeDataArr[4]), int.Parse(realVolumeDataArr[5]), int.Parse(realVolumeDataArr[6])),
                        rotate = new GridCoordFloat(float.Parse(realVolumeDataArr[7]), float.Parse(realVolumeDataArr[8]), float.Parse(realVolumeDataArr[9])),
                        volumeCutSquares = volumeCutSquares
                    };
                }
            }

            //生成图片相关数据
            Dictionary<string, AsepriteGridItemImageData> imageDataDic = new Dictionary<string, AsepriteGridItemImageData>();
            for (int j = 0; j < imageDatasStrArr.Length; j++)
            {
                string[] imageDataArr = imageDatasStrArr[j].Split(GridSystemConfig.dataSep4); //图片数据
                AsepriteGridItemImageData imageData;
                if(imageDataArr.Length == 4)
                {
                    imageData = new AsepriteGridItemImageData
                    {
                        imageName = imageDataArr[0],
                        imagePos = new Vector2(float.Parse(imageDataArr[1]), float.Parse(imageDataArr[2])) * 0.01f, //数据为像素单位，转换为U3D单位
                        meshType = AsepriteGridItemImageData.MeshTypeParse(imageDataArr[3]),
                    };
                }
                else if(imageDataArr.Length == 7)
                {
                    imageData = new AsepriteGridItemImageData
                    {
                        imageName = imageDataArr[0],
                        imagePos = new Vector2(float.Parse(imageDataArr[1]), float.Parse(imageDataArr[2])) * 0.01f, //数据为像素单位，转换为U3D单位
                        meshType = AsepriteGridItemImageData.MeshTypeParse(imageDataArr[3]),
                        imageRotate = new Vector3(float.Parse(imageDataArr[4]), float.Parse(imageDataArr[5]), float.Parse(imageDataArr[6]))
                    };
                }
                else if (imageDataArr.Length == 10)
                {
                    imageData = new AsepriteGridItemImageData
                    {
                        imageName = imageDataArr[0],
                        imagePos = new Vector2(float.Parse(imageDataArr[1]), float.Parse(imageDataArr[2])) * 0.01f, //数据为像素单位，转换为U3D单位
                        meshType = AsepriteGridItemImageData.MeshTypeParse(imageDataArr[3]),
                        haveMeshData = true,
                        meshSize = new GridCoord(int.Parse(imageDataArr[4]), int.Parse(imageDataArr[5]), int.Parse(imageDataArr[6])),
                        meshLocalLocation = new GridCoord(int.Parse(imageDataArr[7]), int.Parse(imageDataArr[8]), int.Parse(imageDataArr[9]))
                    };
                }
                else if (imageDataArr.Length == 13)
                {
                    imageData = new AsepriteGridItemImageData
                    {
                        imageName = imageDataArr[0],
                        imagePos = new Vector2(float.Parse(imageDataArr[1]), float.Parse(imageDataArr[2])) * 0.01f, //数据为像素单位，转换为U3D单位
                        meshType = AsepriteGridItemImageData.MeshTypeParse(imageDataArr[3]),
                        imageRotate = new Vector3(float.Parse(imageDataArr[4]), float.Parse(imageDataArr[5]), float.Parse(imageDataArr[6])),
                        haveMeshData = true,
                        meshSize = new GridCoord(int.Parse(imageDataArr[7]), int.Parse(imageDataArr[8]), int.Parse(imageDataArr[9])),
                        meshLocalLocation = new GridCoord(int.Parse(imageDataArr[10]), int.Parse(imageDataArr[11]), int.Parse(imageDataArr[12]))
                    };
                }
                else
                {
                    imageData = new AsepriteGridItemImageData();
                }
                
                imageDataDic.Add(imageDataArr[0], imageData);
            }

            AsepriteGridItemData gridItemData = new AsepriteGridItemData
            {
                keyName = keyName,
                gridItemSizeUnit = new GridCoord(int.Parse(gridItemSizeUnitArr[0]), int.Parse(gridItemSizeUnitArr[1]), int.Parse(gridItemSizeUnitArr[2])),
                u3dGridItemLocation = new GridCoordFloat(float.Parse(gridItemLocationArr[0]), float.Parse(gridItemLocationArr[2]), float.Parse(gridItemLocationArr[1])) * 0.01f, //数据为像素单位，转换为U3D单位，U3DY为上，Z为前，需要替换
                realVolumes = realVolumes,
                scriptTag = scriptTag,
                prefabName = prefabName,
                imageDataDic = imageDataDic
            };

            return gridItemData;
        }

        /// <summary>
        /// 设置一个GridItemU3DComponent中的GridItemComponent类中的数据，根据gridItemData
        /// </summary>
        /// <param name="gridItemU3DComponent">U3DComponent</param>
        /// <param name="gridItemData">网格物品数据</param>
        /// <returns>成功执行，且改变了GridItemComponent类中的数据</returns>
        static public bool SetGridItemComponent(Component gridItemU3DComponent, AsepriteHeaderData asepriteHeaderData, AsepriteGridItemData gridItemData)
        {
            return SetGridItemComponent(gridItemU3DComponent, asepriteHeaderData, gridItemData, out GridItemComponent gridItemComponent);
        }

        /// <summary>
        /// 设置一个GridItemU3DComponent中的GridItemComponent类中的数据，根据gridItemData
        /// </summary>
        /// <param name="gridItemU3DComponent">U3DComponent</param>
        /// <param name="gridItemData">网格物品数据</param>
        /// <param name="gridItemComponent">网格物品组件</param>
        /// <returns>成功执行，且改变了GridItemComponent类中的数据</returns>
        static public bool SetGridItemComponent(Component gridItemU3DComponent, AsepriteHeaderData asepriteHeaderData, AsepriteGridItemData gridItemData, out GridItemComponent gridItemComponent)
        {
            gridItemComponent = null;
            if (gridItemU3DComponent == null || !gridItemData.IsValid()) return false;

            //从传入U3D的Component上寻找GridItemComponent字段
            bool findGridItemComponent = false;
            bool isComponentDataChange = false;
            FieldInfo[] fieldInfos = gridItemU3DComponent.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in fieldInfos)
            {
                if (item.FieldType != typeof(GridItemComponent)) continue;
                gridItemComponent = item.GetValue(gridItemU3DComponent) as GridItemComponent;
                if (gridItemComponent == null) continue;

                //设置GridItemComponent的参数
                findGridItemComponent = true;

                //设置GridItemSize
                if (gridItemComponent.GridItemSize != gridItemData.gridItemSizeUnit)
                {
                    isComponentDataChange = true;
                    gridItemComponent.GridItemSize = gridItemData.gridItemSizeUnit;
                }

                //设置真实碰撞盒数据
                if (gridItemData.realVolumes != null && gridItemData.realVolumes.Length > 0)
                {
                    RealVolume[] realVolumes = new RealVolume[gridItemData.realVolumes.Length];

                    for (int i = 0; i < gridItemData.realVolumes.Length; i++)
                    {
                        var realVolumeData = gridItemData.realVolumes[i];

                        RealVolume newRealVolume = new RealVolume()
                        {
                            Name = realVolumeData.keyName,
                            BoundingBoxSize = realVolumeData.size / asepriteHeaderData.asepriteGridConfig.cellSizePixel,
                            BoundingBoxLocalLocation = realVolumeData.location / asepriteHeaderData.asepriteGridConfig.cellSizePixel,
                        };

                        //新旧相等时，使用旧的。因为此时ColliderNode还未设置，要等到创建碰撞器时设置。
                        //如果数据改变了，才清空ColliderNode，因为ColliderNode需要更新！
                        if (gridItemComponent.GetRealVolume(i, out RealVolume oldRealVolume))
                        {
                            if (oldRealVolume.EqualsPart1(newRealVolume))
                                newRealVolume = oldRealVolume;
                        }

                        realVolumes[i] = newRealVolume;
                    }

                    gridItemComponent.SetRealVolume(realVolumes);
                }
                else
                {
                    gridItemComponent.AddRealVolume(
                        new RealVolume()
                        {
                            Name = GridSystemConfig.colliderRootName,
                            BoundingBoxSize = gridItemComponent.GridItemSize,
                            BoundingBoxLocalLocation = new GridCoordFloat(),
                        });
                }

                break;
            }

            if (!findGridItemComponent)
                Debug.LogWarning(string.Format("无法在{0}类中获取到GridItemComponent成员字段实例，请确保此类中有此成员字段且默认值不为空。", gridItemU3DComponent.GetType().Name));

            return isComponentDataChange;
        }

        #region 生成 渲染根节点
        /// <summary>
        /// 在GridItem预制体中，根节点下创建一个显示层结构ViewRoot
        /// </summary>
        /// <param name="rootTs"></param>
        /// <param name="nodeNum"></param>
        /// <param name="destroyOldViewRoot"></param>
        public static GameObject CreateViewRoot(Transform rootTs, bool destroyOldViewRoot = false)
        {
            GameObject viewRoot;

            var viewRootFind = rootTs.Find(GridSystemConfig.viewRootName);
            if (viewRootFind)
            {
                viewRoot = viewRootFind.gameObject;

                if (destroyOldViewRoot)
                    GameObject.Destroy(viewRootFind);
                else
                    //提示已经有名称为ViewRoot的节点，需要手动删除。我们不直接自动删除因为这不安全。
                    EditorUtility.DisplayDialog("GridItem Inspector Tips", "无法创建。已经有ViewRoot节点，想要创建新的显示层级结构ViewRoot节点，请先手动删除旧的。", "确认");
            }
            else
            {
                //创建显示层结构根节点
                viewRoot = new GameObject
                {
                    name = GridSystemConfig.viewRootName
                };
                Transform viewRootTs = viewRoot.transform;
                viewRoot.transform.SetParent(rootTs);
            }

            return viewRoot;
        }
        #endregion

        #region 生成MeshRender
        /// <summary>
        /// 生成MeshRender
        /// </summary>
        /// <param name="root"></param>
        /// <param name="gridItemData"></param>
        /// <param name="imageData"></param>
        /// <param name="headerData"></param>
        /// <param name="texture"></param>
        /// <param name="childIndex"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static bool CreateGridItemMeshRender(GameObject root, AsepriteGridItemData gridItemData, AsepriteGridItemImageData imageData,
            AsepriteHeaderData headerData, Texture2D texture, int childIndex, string folderPath)
        {
            //尺寸与相对位移
            GridCoordFloat size = GridCoordFloat.one;
            GridCoordFloat location = GridCoordFloat.zero;
            if (imageData.haveMeshData)
            {
                //使用绘制的Mesh数据
                size = new GridCoordFloat(imageData.meshSize);
                location = new GridCoordFloat(imageData.meshLocalLocation);
            }
            else
            {
                //使用真实碰撞器尺寸
                if (gridItemData.realVolumes != null && gridItemData.realVolumes.Length > 0)
                {
                    var volume = gridItemData.realVolumes[0];
                    if (childIndex < gridItemData.realVolumes.Length)
                        volume = gridItemData.realVolumes[childIndex];

                    size = new GridCoordFloat(volume.size);
                    location = new GridCoordFloat(volume.location);
                }
                else
                    size = new GridCoordFloat(gridItemData.gridItemSizeUnit * headerData.asepriteGridConfig.cellSizePixel);
            }
            //像素纯转为Unity尺寸
            size *= GridSystemConfig.pixelSizeToU3DSizeCoefficient;
            location *= GridSystemConfig.pixelSizeToU3DSizeCoefficient;
            //相对位移
            location -= new GridCoordFloat(gridItemData.gridItemSizeUnit * headerData.asepriteGridConfig.cellSizePixel) * 0.005f; //-占地网格尺寸(使基准点位移至中心点)
            //坐标轴方向转换 Aseprite中坐标系Z轴朝上
            var tempY = location.Y;
            location.Y = location.Z;
            location.Z = tempY;
            tempY = size.Y;
            size.Y = size.Z;
            size.Z = tempY;
            //Mesh位移 移至中心点
            var meshCenterOffset = size * 0.5f;
            location += meshCenterOffset;

            //在Transform上应用参数
            var trans = root.transform;
            //位移
            if (location != GridCoordFloat.zero)
                trans.position = location;
            //旋转
            if (imageData.imageRotate != Vector3.zero)
                trans.rotation = Quaternion.Euler(imageData.imageRotate.x, imageData.imageRotate.z, imageData.imageRotate.y);

            //通过Z轴旋转判断朝向
            int direction = 0; //朝下
            if (0f < imageData.imageRotate.z && imageData.imageRotate.z <= 90f)
                direction = 1; //朝左
            else if (90f < imageData.imageRotate.z && imageData.imageRotate.z <= 180f)
                direction = 2; //朝上
            else if (180f < imageData.imageRotate.z && imageData.imageRotate.z <= 270f)
                direction = 3; //朝右
            //生成MeshFilter与Mesh
            var meshFilter = root.GetOrAddComponent<MeshFilter>();
            var mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
            //根据形状类型 生成对应的Mesh
            Vector3[] vertices = null;
            int[] triangles = null;
            Vector2[] uvs = null;
            switch (imageData.meshType)
            {
                case AsepriteGridItemImageData.EMeshType.Cube:
                    #region 方块
                    //顶点
                    vertices = new Vector3[6];
                    vertices[0] = new Vector3(0, 0, 0) - meshCenterOffset;
                    vertices[1] = new Vector3(size.X, 0, 0) - meshCenterOffset;
                    vertices[2] = new Vector3(0, size.Y, 0) - meshCenterOffset;
                    vertices[3] = new Vector3(size.X, size.Y, 0) - meshCenterOffset;
                    vertices[4] = new Vector3(0, size.Y, size.Z) - meshCenterOffset;
                    vertices[5] = new Vector3(size.X, size.Y, size.Z) - meshCenterOffset;
                    //三角面
                    triangles = new int[12];
                    triangles[0] = 1; triangles[1] = 0; triangles[2] = 2;
                    triangles[3] = 1; triangles[4] = 2; triangles[5] = 3;
                    triangles[6] = 3; triangles[7] = 2; triangles[8] = 4;
                    triangles[9] = 3; triangles[10] = 4; triangles[11] = 5;
                    //UV
                    uvs = new Vector2[6];
                    float yTurn = size.Y / (size.Y + size.Z);
                    uvs[0] = new Vector2(0, 0);
                    uvs[1] = new Vector2(1, 0);
                    uvs[2] = new Vector2(0, yTurn);
                    uvs[3] = new Vector2(1, yTurn);
                    uvs[4] = new Vector2(0, 1);
                    uvs[5] = new Vector2(1, 1);
                    break;
                #endregion
                case AsepriteGridItemImageData.EMeshType.Slope:
                    #region 斜面
                    //顶点
                    vertices = new Vector3[4];
                    vertices[0] = new Vector3(0, 0, 0) - meshCenterOffset;
                    vertices[1] = new Vector3(size.X, 0, 0) - meshCenterOffset;
                    vertices[2] = new Vector3(0, size.Y, size.Z) - meshCenterOffset;
                    vertices[3] = new Vector3(size.X, size.Y, size.Z) - meshCenterOffset;

                    //三角面
                    triangles = new int[12];
                    triangles[0] = 1; triangles[1] = 0; triangles[2] = 2;
                    triangles[3] = 1; triangles[4] = 2; triangles[5] = 3;

                    //UV
                    uvs = new Vector2[4];
                    switch(direction)
                    {
                        case 0: //朝下
                            uvs[0] = new Vector2(0, 0);
                            uvs[1] = new Vector2(1, 0);
                            uvs[2] = new Vector2(0, 1);
                            uvs[3] = new Vector2(1, 1);
                            break;
                        case 2: //朝上
                            uvs[3] = new Vector2(0, 0);
                            uvs[2] = new Vector2(1, 0);
                            uvs[1] = new Vector2(0, 1);
                            uvs[0] = new Vector2(1, 1);
                            break;
                        case 1: //朝左
                        case 3: //朝右
                            var width = texture.width;
                            var height = texture.height;
                            //读取贴图像素颜色
                            RenderTexture rt = RenderTexture.GetTemporary(width, height);
                            Graphics.Blit(texture, rt);
                            Texture2D textureRead = new Texture2D(width, height);
                            textureRead.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                            textureRead.Apply();

                            //检查贴图边缘非透明像素位置 作为UV点
                            var colors = textureRead.GetPixels();
                            Vector2 leftUp = Vector2.zero;
                            Vector2 leftDown = Vector2.zero;
                            Vector2 rightUp = Vector2.zero;
                            Vector2 rightDown = Vector2.zero;
                            for (int y = height - 1; y >= 0; y--)
                            {
                                var color = colors[y * width];
                                if (color.a > 0.004f)
                                {
                                    leftUp = new Vector2(0, (float)(y + 1) / height);
                                    break;
                                }
                            }
                            for (int y = 0; y < height; y++)
                            {
                                var color = colors[y * width];
                                if (color.a > 0.004f)
                                {
                                    leftDown = new Vector2(0, (float)y / height);
                                    break;
                                }
                            }
                            for (int y = height - 1; y >= 0; y--)
                            {
                                var color = colors[y * width + width - 1];
                                if (color.a > 0.004f)
                                {
                                    rightUp = new Vector2(1, (float)(y + 1) / height);
                                    break;
                                }
                            }
                            for (int y = 0; y < height; y++)
                            {
                                var color = colors[y * width + width - 1];
                                if (color.a > 0.004f)
                                {
                                    rightDown = new Vector2(1, (float)y / height);
                                    break;
                                }
                            }

                            //根据朝向设置UV点
                            if (direction == 1)
                            {
                                uvs[0] = leftUp;
                                uvs[1] = leftDown;
                                uvs[2] = rightUp;
                                uvs[3] = rightDown;
                            }
                            else if (direction == 2)
                            {
                                uvs[3] = leftDown;
                                uvs[2] = leftUp;
                                uvs[1] = rightDown;
                                uvs[0] = rightUp;
                            }

                            //释放贴图数据
                            RenderTexture.ReleaseTemporary(rt);
                            GameObject.Destroy(textureRead);
                            break;
                    }
                    break;
                #endregion
            }
            mesh.SetVertices(vertices);
            mesh.triangles = triangles;
            mesh.uv = uvs;
            //法线方向
            mesh.RecalculateNormals();

            //生成MeshRenderer与Material
            var meshRender = root.GetOrAddComponent<MeshRenderer>();
            var material = ConfigSystem.Instance.CreateMaterial("Able/Lit-Alpha");
            material.mainTexture = texture;
            meshRender.sharedMaterial = material;

            //确认文件夹是否存在，否则创建
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //保存Mesh
            AssetDatabase.CreateAsset(mesh, $"{folderPath}/{root.name}_Mesh.asset");
            //保存材质球
            material.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(material, $"{folderPath}/{root.name}_Mat.mat");

            return true;
        }
        #endregion

        #region 生成 碰撞器
        /// <summary>
        /// 创建网格物品碰撞器
        /// 这会创建一个碰撞器节点，并在此节点上添加Collider组件
        /// </summary>
        /// <param name="colliderParentGObj"></param>
        /// <param name="asepriteGridItemData"></param>
        /// <param name="gridItemComponent"></param>
        /// <param name="asepriteHeaderData"></param>
        /// <param name="colliderRootGObj"></param>
        /// <returns></returns>
        public static bool CreateGridItemCollider(GameObject colliderParentGObj, AsepriteGridItemData asepriteGridItemData, GridItemComponent gridItemComponent, AsepriteHeaderData asepriteHeaderData, out GameObject colliderRootGObj)
        {
            Transform colliderParentTs = colliderParentGObj.transform;
            Transform colliderRootTs = colliderParentTs.Find(GridSystemConfig.colliderRootName);

            //删除旧的碰撞器物体
            if (colliderRootTs) GameObject.DestroyImmediate(colliderRootTs.gameObject);

            //创建一个碰撞器节点
            colliderRootGObj = new GameObject() { name = GridSystemConfig.colliderRootName };
            colliderRootGObj.transform.SetParent(colliderParentTs);

            colliderRootTs = colliderRootGObj.transform;

            //获取数据
            var gridItemSizePixel = gridItemComponent.GridItemSize * asepriteHeaderData.asepriteGridConfig.cellSizePixel;
            
            //没有配置真实体积范围数据
            if (asepriteGridItemData.realVolumes == null || asepriteGridItemData.realVolumes.Length == 0)
            {
                if (asepriteGridItemData.imageDataDic.Count > 0)
                {
                    //使用图片数据size 生成碰撞器
                    #region 
                    Dictionary<string, GameObject> dicColliderRoot = new Dictionary<string, GameObject>(); //多Collider组合的根节点
                    foreach (var imageData in asepriteGridItemData.imageDataDic.Values)
                    {
                        var param = imageData.imageName.Split('-');
                        var imageName = param[0];
                        var index = "0";
                        if (param.Length > 1)
                            index = param[1];

                        //生成新的Collider的根节点
                        if (!dicColliderRoot.ContainsKey(imageName))
                        {
                            var colliderRootNew = new GameObject();
                            colliderRootNew.transform.SetParent(colliderRootTs);
                            colliderRootNew.name = imageName;
                            dicColliderRoot.Add(imageName, colliderRootNew);
                        }

                        //生成Collider
                        var colliderRoot = dicColliderRoot[imageName];
                        var size = new GridCoordFloat(imageData.meshSize) * GridSystemConfig.pixelSizeToU3DSizeCoefficient; //尺寸
                        var location = new GridCoordFloat(imageData.meshLocalLocation) * GridSystemConfig.pixelSizeToU3DSizeCoefficient; //坐标
                        location -= new GridCoordFloat(asepriteGridItemData.gridItemSizeUnit * asepriteHeaderData.asepriteGridConfig.cellSizePixel) * 0.005f; //-占地网格尺寸(使基准点位移至中心点)
                        location += size * 0.5f; //坐标基准点修正

                        //坐标轴方向转换
                        var tempY = location.Y;
                        location.Y = location.Z;
                        location.Z = tempY;
                        tempY = size.Y;
                        size.Y = size.Z;
                        size.Z = tempY;

                        switch (imageData.meshType)
                        {
                            case AsepriteGridItemImageData.EMeshType.Cube:
                                #region 方块
                                //生成BoxCollider
                                BoxCollider boxCollider = colliderRoot.AddComponent<BoxCollider>();
                                boxCollider.size = size;
                                boxCollider.center = location;
                                #endregion
                                break;
                            case AsepriteGridItemImageData.EMeshType.Slope:
                                #region 斜面
                                //生成 子物体
                                var subColliderRoot = new GameObject().transform;
                                subColliderRoot.SetParent(colliderRoot.transform);
                                subColliderRoot.name = $"ColliderSlope-{index}";
                                //生成 MeshCollider
                                var meshCollider = subColliderRoot.gameObject.AddComponent<MeshCollider>();
                                meshCollider.convex = true;
                                meshCollider.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/PluginsDeveloper/FsGridCellSystem/Resource/Mesh_Slope.asset");
                                subColliderRoot.localScale = size;
                                subColliderRoot.localPosition = location;
                                subColliderRoot.localRotation = Quaternion.Euler(imageData.imageRotate.x, imageData.imageRotate.z, imageData.imageRotate.y);
                                #endregion
                                break;
                        }
                    }

                    foreach (var kv in dicColliderRoot)
                    {
                        //绑定自身相应的真实体积数据
                        gridItemComponent.SetRealVolumeColliderNode(kv.Key, kv.Value);
                    }
                    #endregion
                }
                else
                {
                    //使用网格尺寸size 生成碰撞器
                    BoxCollider boxCollider = colliderRootGObj.AddComponent<BoxCollider>();
                    boxCollider.size = GridCoordCSToU3DCS(gridItemSizePixel) * GridSystemConfig.pixelSizeToU3DSizeCoefficient;
                    //绑定自身相应的真实体积数据
                    gridItemComponent.SetRealVolumeColliderNode(GridSystemConfig.colliderRootName, colliderRootGObj);
                }

                return true;
            }

            //配置了真实体积数据，根据数据会生成一个或多个真实体积碰撞内容节点
            for (int i = 0; i < asepriteGridItemData.realVolumes.Length; i++)
            {
                //获取真实体积数据，生成子节点
                AsepriteRealVolumeData realVolume = asepriteGridItemData.realVolumes[i];
                GameObject realVolumeNodeGObj = new GameObject() { name = realVolume.keyName };
                var realVolumeNodeTs = realVolumeNodeGObj.transform;
                realVolumeNodeTs.SetParent(colliderRootTs);
                var realVolumeNodePosWithCenter = (realVolume.location + realVolume.size * 0.5f) - (gridItemSizePixel * 0.5f);
                //设置位置
                realVolumeNodeTs.position =
                    new Vector3(
                        realVolumeNodePosWithCenter.X,
                        realVolumeNodePosWithCenter.Z,
                        realVolumeNodePosWithCenter.Y)
                    * GridSystemConfig.pixelSizeToU3DSizeCoefficient;
                //设置旋转
                realVolumeNodeTs.rotation = Quaternion.Euler(realVolume.rotate.X, realVolume.rotate.Y, realVolume.rotate.Z);

                //绑定自身相应的真实体积数据
                gridItemComponent.SetRealVolumeColliderNode(realVolumeNodeGObj.name, realVolumeNodeGObj);

                //生成realVolum节点碰撞器
                if (realVolume.volumeCutSquares != null && realVolume.volumeCutSquares.Length > 0)
                {
                    #region 裁切碰撞器
                    //需要进行体积裁剪

                    //裁剪方形的位置原点是真实体积立面方形的左下角
                    //整体方形，我们在立面的方形上进行裁剪，这里高度取Z
                    var squareEntirety = new Square()
                    {
                        size = new Vector2(realVolume.size.X, realVolume.size.Z),
                        pos = Vector2.zero
                    };
                    Vector2 squareEntiretyCenterPos = new Vector2(squareEntirety.size.x * 0.5f, squareEntirety.size.y * 0.5f);//整体方形中心点位置

                    Square[] createColliderSquares;
                    //确认是否需要进行裁剪
                    if (realVolume.volumeCutSquares.Length > 0)
                    {
                        //分别进行垂直和水平裁切，取总数少的那个进行创建Collider
                        List<Square> createColliderSquares_Ver = new List<Square>();
                        createColliderSquares_Ver.Add(squareEntirety);
                        RealVolumeCutting(realVolume, ref createColliderSquares_Ver, true);
                        List<Square> createColliderSquares_Hor = new List<Square>();
                        createColliderSquares_Hor.Add(squareEntirety);
                        RealVolumeCutting(realVolume, ref createColliderSquares_Hor, false);

                        createColliderSquares = createColliderSquares_Ver.Count < createColliderSquares_Hor.Count ? createColliderSquares_Ver.ToArray() : createColliderSquares_Hor.ToArray();
                    }
                    else
                        createColliderSquares = new Square[] { squareEntirety };

                    //参见方形完成后，根据数据来生成碰撞器Collider
                    for (int j = 0; j < createColliderSquares.Length; j++)
                    {
                        var square = createColliderSquares[j];

                        BoxCollider boxCollider = realVolumeNodeGObj.AddComponent<BoxCollider>();
                        //这里Collider的尺寸深度是realVolume.size.y
                        boxCollider.size = new GridCoordFloat(square.size.x, square.size.y, realVolume.size.Y) * GridSystemConfig.pixelSizeToU3DSizeCoefficient;
                        //这里设置的位置原点是中心点，碰撞器锚点也是中心点
                        boxCollider.center = new GridCoordFloat(square.pos.x - squareEntiretyCenterPos.x + square.size.x * 0.5f, square.pos.y - squareEntiretyCenterPos.y + square.size.y * 0.5f, 0f) * GridSystemConfig.pixelSizeToU3DSizeCoefficient;
                    }

                    gridItemComponent.SetIsHaveMultipleCollider(createColliderSquares.Length > 1);
                    #endregion
                }
                else
                {
                    #region 单碰撞器
                    //不需要进行体积裁剪，生成一个完整体积碰撞器
                    //坐标系转换 Y~Z
                    BoxCollider boxCollider = realVolumeNodeGObj.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(realVolume.size.X, realVolume.size.Z, realVolume.size.Y) * GridSystemConfig.pixelSizeToU3DSizeCoefficient;
                    gridItemComponent.SetIsHaveMultipleCollider(false);
                    #endregion
                }
            }

            return true;
        }

        #region 碰撞器裁切拼装
        /// <summary>
        /// 对真实体积数据中的体积进行裁剪
        /// </summary>
        /// <param name="realVolume">真实体积数据</param>
        /// <param name="createColliderSquares"></param>
        /// <param name="cutMode">裁剪模式 true=垂直 false=水平</param>
        /// <returns>裁剪完的方形数据数组</returns>
        public static void RealVolumeCutting(AsepriteRealVolumeData realVolume, ref List<Square> createColliderSquares, bool cutMode = true)
        {
            for (int j = 0; j < realVolume.volumeCutSquares.Length; j++)
            {
                //获取裁剪数据
                var squareCut = realVolume.volumeCutSquares[j];

                //确认现有所有的Collider数据，是否会被裁剪数据影响
                for (int k = createColliderSquares.Count - 1; k >= 0; k--)
                {
                    //确认裁剪数据方形和当前Collider数据方形关系。相离不处理，相交和重合时需要处理
                    var square = createColliderSquares[k];
                    bool isSeparation = squareCut.pos.x - square.pos.x >= square.size.x //裁剪方形在此方形的右侧
                       || squareCut.pos.x - square.pos.x + squareCut.size.x <= 0 //裁剪方形在此方形的左侧
                       || squareCut.pos.y - square.pos.y >= square.size.y //裁剪方形在此方形的上侧
                       || squareCut.pos.y - square.pos.y + squareCut.size.y <= 0; //裁剪方形在此方形的下侧

                    if (isSeparation) continue;//想离或相切不需要处理

                    //判断此方形是否被裁剪方形覆盖了，被覆盖时直接移除
                    if (SquareCover_Unilateral(squareCut, square))
                    {
                        createColliderSquares.RemoveAt(k);
                        continue;
                    }

                    //裁剪切割方形，裁剪后最多会有四个新的方形
                    if (cutMode)
                        SquareCutting_Vertical(square, squareCut, createColliderSquares);
                    else
                        SquareCutting_Horizontal(square, squareCut, createColliderSquares);

                    //裁切完此方形后，原有的完整方形需要移除
                    createColliderSquares.RemoveAt(k);
                }
            }
        }

        /// <summary>
        /// 方形裁剪，将square减去squareCut的面积后剩余面积切割成多个square，并添加到squares中
        /// 我们进行竖向切割
        /// </summary>
        /// <param name="square">被裁剪方形</param>
        /// <param name="squareCut">裁剪用方形</param>
        /// <param name="squares">裁剪分割后的方形添加到此列表</param>
        public static void SquareCutting_Vertical(Square square, Square squareCut, List<Square> squares = null)
        {
            //左侧方形
            Square square1 = new Square()
            {
                size = new Vector2(squareCut.pos.x - square.pos.x, square.size.y),
                pos = square.pos
            };
            //有面积的才添加到创建碰撞器数据中。这里包含了两个方形边相切的情况，所以需要判断是否有面积。
            if (squares != null && square1.haveArea)
                squares.Add(square1);

            //右侧方形
            Square square2 = new Square()
            {
                size = new Vector2(square.size.x - squareCut.size.x - square1.size.x, square.size.y),
                pos = new Vector2(squareCut.pos.x + squareCut.size.x, square.pos.y)
            };
            if (squares != null && square2.haveArea)
                squares.Add(square2);

            //下侧方形
            Square square3 = new Square()
            {
                size = new Vector2(squareCut.size.x, squareCut.pos.y - square.pos.y),
                pos = new Vector2(squareCut.pos.x, square.pos.y)
            };
            if (squares != null && square3.haveArea)
                squares.Add(square3);

            //上侧方形
            Square square4 = new Square()
            {
                size = new Vector2(squareCut.size.x, square.size.y - squareCut.size.y - square3.size.y),
                pos = new Vector2(squareCut.pos.x, squareCut.pos.y + squareCut.size.y)
            };
            if (squares != null && square4.haveArea)
                squares.Add(square4);
        }

        /// <summary>
        /// 方形裁剪，将square减去squareCut的面积后剩余面积切割成多个square，并添加到squares中
        /// 我们进行横向切割
        /// </summary>
        /// <param name="square">被裁剪方形</param>
        /// <param name="squareCut">裁剪用方形</param>
        /// <param name="squares">裁剪分割后的方形添加到此列表</param>
        public static void SquareCutting_Horizontal(Square square, Square squareCut, List<Square> squares = null)
        {
            //左侧方形
            Square square1 = new Square()
            {
                size = new Vector2(squareCut.pos.x - square.pos.x, squareCut.size.y),
                pos = new Vector2(square.pos.x, squareCut.pos.y)
            };
            //有面积的才添加到创建碰撞器数据中。这里包含了两个方形边相切的情况，所以需要判断是否有面积。
            if (squares != null && square1.haveArea)
                squares.Add(square1);

            //右侧方形
            Square square2 = new Square()
            {
                size = new Vector2(square.size.x - squareCut.size.x - square1.size.x, squareCut.size.y),
                pos = new Vector2(squareCut.pos.x + squareCut.size.x, squareCut.pos.y)
            };
            if (squares != null && square2.haveArea)
                squares.Add(square2);

            //下侧方形
            Square square3 = new Square()
            {
                size = new Vector2(square.size.x, squareCut.pos.y - square.pos.y),
                pos = square.pos
            };
            if (squares != null && square3.haveArea)
                squares.Add(square3);

            //上侧方形
            Square square4 = new Square()
            {
                size = new Vector2(square.size.x, square.size.y - squareCut.size.y - square3.size.y),
                pos = new Vector2(square.pos.x, squareCut.pos.y + squareCut.size.y)
            };
            if (squares != null && square4.haveArea)
                squares.Add(square4);
        }

        /// <summary>
        /// 正方形覆盖，单方面覆盖。当Big覆盖Small时，返回true;
        /// </summary>
        /// <param name="squareBig"></param>
        /// <param name="squareSmall"></param>
        /// <returns></returns>
        public static bool SquareCover_Unilateral(Square squareBig, Square squareSmall)
        {
            return squareSmall.pos.x >= squareBig.pos.x
                            && squareSmall.pos.y >= squareBig.pos.y
                            && squareSmall.pos.x + squareSmall.size.x <= squareBig.pos.x + squareBig.size.x
                            && squareSmall.pos.y + squareSmall.size.y <= squareBig.pos.y + squareBig.size.y;
        }
        #endregion
        #endregion

        /// <summary>
        /// 设置预制件
        /// </summary>
        /// <param name="gridItemDataDic">网格物品数据字典</param>
        /// <param name="preformedUnitPrefabTs">预制件Transform</param>
        /// <returns></returns>
        public static bool SetPreformedUnitPrefab(Dictionary<string, AsepriteGridItemData> gridItemDataDic, Transform preformedUnitPrefabTs)
        {
            if (gridItemDataDic == null || gridItemDataDic.Count == 0 || preformedUnitPrefabTs == null || preformedUnitPrefabTs.childCount == 0) return false;

            for (int i = 0; i < preformedUnitPrefabTs.childCount; i++)
            {
                Transform nodeTs = preformedUnitPrefabTs.GetChild(i);

                if (!gridItemDataDic.ContainsKey(nodeTs.name))
                {
                    Debug.LogWarning(
                        string.Format(
                            "SetPreformedUnitPrefab : 设置预制件时，无法找到预制件{0}中GridItem节点{1}对应的AsepriteGridItemData数据。请确认传入gridItemDataDic是否正确，或者预制件子节点的GrIdItem名称是否规范。",
                            preformedUnitPrefabTs.name, nodeTs.name));
                    continue;
                }

                AsepriteGridItemData data = gridItemDataDic[nodeTs.name];

                //设置GridItem子节点数据
                nodeTs.transform.position = data.u3dGridItemLocation;//设置位置
            }

            return true;
        }

        /// <summary>
        /// 获取GridItemComponent类型
        /// 这是GridCellSystem提供的组件类型，将此字段添加到项目中的定义为GridItemBase（父类为UnityEngine.MonoBehaviour，挂载在GameObject上)的类中，此类将拥有GridItem的功能
        /// </summary>
        /// <param name="gridItemObj">项目定义的GridItem类脚本挂载到的GameObject</param>
        /// <param name="gridItemType">项目定义的GridItem的类型Type</param>
        /// <returns></returns>
        public static GridItemComponent GetGridItemComponent(GameObject gridItemObj)
        {
            //Type GridItemType = System.Reflection.Assembly.Load("Assembly-CSharp").GetType(m_GridItemClassType); //通过字符串获取类型的方式
            UnityEngine.MonoBehaviour gridItem = gridItemObj.GetComponent<MonoBehaviour>();
            if (gridItem)
            {
                FieldInfo[] fieldInfos = gridItem.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                foreach (var item in fieldInfos)
                {
                    if (item.FieldType == typeof(GridItemComponent))
                    {
                        GridItemComponent gridItemComponent = item.GetValue(gridItem) as GridItemComponent;

                        return gridItemComponent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取GameObject下所有的GetGridItemComponent
        /// </summary>
        /// <param name="gridItemObjRoot"></param>
        /// <returns></returns>
        public static GridItemComponent[] GetGridItemComponents(GameObject gridItemObjRoot)
        {
            List<GridItemComponent> gridItemComponents = new List<GridItemComponent>();
            MonoBehaviour[] monoBehaviours = gridItemObjRoot.GetComponentsInChildren<MonoBehaviour>();

            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var mono = monoBehaviours[i];
                var gridItemComponent = GetGridItemComponent(mono.gameObject);

                //调用设置ViewPort的接口
                if (gridItemComponent != null)
                {
                    gridItemComponents.Add(gridItemComponent);
                }
            }

            return gridItemComponents.ToArray();
        }

        /// <summary>
        /// 网格坐标到U3D坐标系转换
        /// 这换调换Z和Y的值，因为U3D中Y为上
        /// </summary>
        /// <param name="gridCoord"></param>
        /// <returns></returns>
        public static Vector3 GridCoordCSToU3DCS(GridCoord gridCoord)
        {
            return new Vector3(gridCoord.X, gridCoord.Z, gridCoord.Y);
        }

        private static GridCellSystemManager m_GridCellSystemManager; //网格系统管理器 编辑器模式 用于排序队列

        /// <summary>
        /// 刷新预制件中所有GridItem的ViewRoot的显示位置，为Editor下提供预览。
        /// ViewRoot的位置在运行时会重新被设置
        /// </summary>
        /// <param name="gridItemPreformedUnitPrefab"></param>
        public static void EditorRefreshViewRootPosition(GameObject gridItemPreformedUnitPrefab, GridCoord gridCellSizePixel = default)
        {
            if (m_GridCellSystemManager == null)
            {
                m_GridCellSystemManager = new GridCellSystemManager();
                //单位单元格尺寸
                var unitCellSizePixel = gridCellSizePixel == default ? new GridCoord(20, 20, 10) : gridCellSizePixel;
                Vector3 cellUnitSize = new Vector3(unitCellSizePixel.X * 0.01f, unitCellSizePixel.Y * 0.01f, unitCellSizePixel.Z * 0.01f);
                //自定义Layer初始化的单元格数据 TODO
                m_GridCellSystemManager.Init(cellUnitSize.x, cellUnitSize.y, cellUnitSize.z,
                    200, 200, 30, 1);
            }

            m_GridCellSystemManager.ClearAllViewSortGridItem();

            MonoBehaviour[] monoBehaviours = gridItemPreformedUnitPrefab.GetComponentsInChildren<MonoBehaviour>();

            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var mono = monoBehaviours[i];
                var gridItemComponent = GetGridItemComponent(mono.gameObject);

                //调用设置ViewPort的接口
                if (gridItemComponent != null)
                {

                    gridItemComponent.ViewRootTrans = mono.transform.Find(GridSystemConfig.viewRootName);
                    gridItemComponent.EditorRefreshViewRootPosition(m_GridCellSystemManager);
                }
            }

            PrefabUtility.SavePrefabAsset(gridItemPreformedUnitPrefab);
        }
        #endregion

        /// <summary>
        /// 创建一个预制体
        /// </summary>
        /// <param name="createPrefabGameobjectProcessor">允许自定义对创建的预制体GameObject进行操作的方法</param>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="prefabName">预制体名称</param>
        /// <param name="OverwriteFilesSameNames">重名时是否覆盖</param>
        /// <returns>新建预制体路径，为空则创建失败</returns>
        public static string CreatePrefab(CreatePrefabGameobjectProcessor createPrefabGameobjectProcessor, string folderPath, string prefabName, bool OverwriteFilesSameNames)
        {
            return CreatePrefab(createPrefabGameobjectProcessor, folderPath, prefabName, OverwriteFilesSameNames, out GameObject prefab);
        }

        /// <summary>
        /// 创建一个预制体
        /// </summary>
        /// <param name="createPrefabGameobjectProcessor">允许自定义对创建的预制体GameObject进行操作的方法</param>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="prefabName">预制体名称</param>
        /// <param name="OverwriteFilesSameNames">重名时是否覆盖</param>
        /// <param name="prefab">创建的预制体</param>
        /// <returns>新建预制体路径，为空则创建失败</returns>
        public static string CreatePrefab(CreatePrefabGameobjectProcessor createPrefabGameobjectProcessor, string folderPath, string prefabName, bool OverwriteFilesSameNames, out GameObject prefab)
        {
            //检查prefabName合法性，并处理
            if (prefabName.Contains("/"))
                prefabName.Replace("/", "");

            //确认文件夹是否存在，否则创建
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //设置资源存储路径
            string savePath = folderPath + string.Format("/{0}.Prefab", prefabName);

            //不覆盖模式时，防止重名文件覆盖
            if (!OverwriteFilesSameNames && System.IO.File.Exists(savePath))
            {
                int index = 1;
                string savePathTemp = folderPath + string.Format("/{0} {1}.Prefab", prefabName, index);

                while (System.IO.File.Exists(savePathTemp))
                {
                    index++;
                    savePathTemp = folderPath + string.Format("/{0} {1}.Prefab", prefabName, index);
                }

                savePath = savePathTemp;
            }

            //新建GameObject 即预制体的实际内容
            GameObject obj = new GameObject();
            obj.name = prefabName;

            if (createPrefabGameobjectProcessor == null || createPrefabGameobjectProcessor(obj, folderPath))
            {
                //存储预制体到目标路径
                prefab = PrefabUtility.SaveAsPrefabAsset(obj, savePath);
            }
            else
            {
                prefab = null;
                savePath = string.Empty;
            }

            //删除操作的GameObject对象 即Scene窗口中的GameObject
            Editor.DestroyImmediate(obj);

            return savePath;
        }

        /// <summary>
        /// 加载文件夹下指定类型的文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullPath">文件夹路径</param>
        /// <param name="outFiles">输出文件</param>
        /// <param name="searchPattern">查找方式</param>
        /// <returns></returns>
        public static bool GetFiles<T>(string fullPath, out T[] outFiles, string searchPattern = "*") where T : UnityEngine.Object
        {
            string[] outFilePaths;
            return GetFiles<T>(fullPath, out outFiles, out outFilePaths, searchPattern);
        }

        /// <summary>
        /// 加载文件夹下指定类型的文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullPath">文件夹路径</param>
        /// <param name="outFiles">输出文件</param>
        /// <param name="searchPattern">查找方式</param>
        /// <returns></returns>
        public static bool GetFiles<T>(string fullPath, out T[] outFiles, out string[] outFilePaths, string searchPattern = "*") where T : UnityEngine.Object
        {
            outFiles = null;
            outFilePaths = null;

            if (!Directory.Exists(fullPath)) return false;

            List<T> outFilesList = new List<T>();
            List<string> outFilePathsList = new List<string>();

            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles(searchPattern, SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                //忽略.meta文件
                if (files[i].Name.EndsWith(".meta")) continue;
                string path = files[i].FullName;
                path = path.Replace("\\", "/");
                path = path.Replace(Application.dataPath, "Assets");
                T item = AssetDatabase.LoadAssetAtPath<T>(path);

                if (!item) continue;

                outFilesList.Add(item);
                outFilePathsList.Add(path);
            }

            outFiles = outFilesList.ToArray();
            outFilePaths = outFilePathsList.ToArray();

            return outFiles.Length > 0;
        }

        /// <summary>
        /// 获取网格单元格系统配置文件夹位置
        /// </summary>
        /// <returns></returns>
        public static string GetConfigPath()
        {
            string configPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("GridSystemEditorLibrary")[0]).Replace((@"/" + "GridSystemEditorLibrary" + ".cs"), "");
            configPath = configPath + "/Config";

            //确认文件夹是否存在，否则创建
            if (!Directory.Exists(configPath))
                Directory.CreateDirectory(configPath);

            return configPath;
        }

        /// <summary>
        /// 获取选中的资源文件夹路径
        /// </summary>
        /// <returns></returns>
        public static string GetSelectionAssetDirectory()
        {
            foreach (var obj in Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets))
            {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (System.IO.Directory.Exists(path))
                    return path;
                else if (System.IO.File.Exists(path))
                    return System.IO.Path.GetDirectoryName(path);
            }

            return string.Empty;
        }
    }
}