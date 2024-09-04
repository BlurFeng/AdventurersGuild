using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace FsGridCellSystem
{
    /// <summary>
    /// 网格物体工具窗口
    /// 可以创建整体预制件（包含一个或多个GridItem）和批量创建GridItem，以及对GridItem进行处理。
    /// 项目可以继承GridItemToolsWindowProcessor类，然后挂在到工具窗口。重写GridItemToolsWindowProcessor中的方法来实现项目业务逻辑处理接入到工具流程中。
    /// 比如创建GridItem预制体时，根据项目需求自动化进行额外的操作，配置GridItem类的参数等。
    /// </summary>
    public class GridItemToolsWindow : EditorWindow
    {
        /// <summary>
        /// 创建的窗口自身
        /// </summary>
        public static EditorWindow m_Window;

        private GridItemToolsWindowConfig m_GridItemToolsWindowConfig;//当前的窗口配置
        private SerializedObject m_GridItemToolsWindowConfig_Ser;//网格物品工具窗口配置类序列化数据
        string m_ConfigPath_Cur;//当前窗口配置路径
        string m_ConfigPath_Saved;//窗口配置存储文件路径

        GridItemToolsWindowProcessor m_GridItemToolsWindowProcessor;//处理器脚本

        //生成预制件及批量网格物品
        public List<GridItemToolsWindowConfig.GridItemScriptSetting> m_GridItemScriptSettings_Cached = new List<GridItemToolsWindowConfig.GridItemScriptSetting>();//网格物品脚本类配置数组缓存，用于对比是否进行修改
        private SerializedProperty m_GridItemScriptSettings_SerP;//网格物品脚本配置序列化数据
        private Dictionary<string, MonoScript> m_GridItemScriptDic_CreatePrefab = new Dictionary<string, MonoScript>();//网格物品脚本配置字典，方便查询
        private AsepriteHeaderData m_AsepriteHeaderData_CreatePrefab;//读取出的Aseprite头部数据
        private Dictionary<string, AsepriteGridItemData> m_GridItemDataDic_CreatePrefab = new Dictionary<string, AsepriteGridItemData>();//网格物品Aseprite导入数据
        private Dictionary<string, Sprite> m_ImageDic_CreatePrefab = new Dictionary<string, Sprite>();//图片字典，Key=ImageName
        private Dictionary<string, GameObject> m_CreateGridItemPrefabDic = new Dictionary<string, GameObject>();//创建的网格物品预制体 Key=PrefabName, Vaule=Prefab
        private Dictionary<string, string> m_CreatePreformedUnitGridItemsDic = new Dictionary<string, string>(); //预制件下所有的GridItem，Key=KeyName, Vaule=PrefabName
        private string m_CreateGridItemKeyName;
        private Component m_CreateGridItemScriptClass;
        private GameObject m_CreateGridItemViewRoot;
        private GameObject m_CreateGridItemColliderRoot;
        private Dictionary<string, GameObject> m_GridItemPrefabDepot = new Dictionary<string, GameObject>();//网格物品预制体仓库，已经存在的网格物品预制体 Key=PrefabName, Vaule=Prefab
        private Dictionary<string, string> m_GridItemPrefabDepot_Path = new Dictionary<string, string>();
        private string m_GridItemPrefabSearchPath_CreatePrefab_Cached;
        private string m_ImagesFolderPath_CreatePrefab_Cached;


        //额外功能
        private static bool m_ExtraFoldout;

        [MenuItem("Tools/FsGridCellSystem/GridItemToolsWindow")]
        public static void ShowWindow()
        {
            m_Window = EditorWindow.GetWindow(typeof(GridItemToolsWindow));
            m_ExtraFoldout = false;
            Vector2 size = new Vector2(400, 300);
            m_Window.minSize = size;
        }

        //重新链接m_GridItemToolsWindowConfig
        public void LinkGridItemToolsWindowConfig()
        {
            m_GridItemToolsWindowConfig_Ser = new SerializedObject(m_GridItemToolsWindowConfig);
            m_GridItemScriptSettings_SerP = m_GridItemToolsWindowConfig_Ser.FindProperty("gridItemScriptSettings");

            CheckProcessorScript();
        }

        private void OnEnable()
        {
            //获取配置文件缓存
            string configFolderPath = GridSystemEditorLibrary.GetConfigPath();
            m_ConfigPath_Cur = configFolderPath + "/" + "GridItemToolsWindowConfig" + ".asset";
            m_ConfigPath_Saved = configFolderPath + "/" + "GridItemToolsWindowConfig_Saved" + ".asset";
            m_GridItemToolsWindowConfig = AssetDatabase.LoadAssetAtPath<GridItemToolsWindowConfig>(m_ConfigPath_Cur);
            if (!m_GridItemToolsWindowConfig)
            {
                m_GridItemToolsWindowConfig = ScriptableObject.CreateInstance(typeof(GridItemToolsWindowConfig)) as GridItemToolsWindowConfig;
                AssetDatabase.CreateAsset(m_GridItemToolsWindowConfig, m_ConfigPath_Cur);
            }

            LinkGridItemToolsWindowConfig();
        }

        private void OnDisable()
        {
            EditorUtility.SetDirty(m_GridItemToolsWindowConfig);
            AssetDatabase.SaveAssetIfDirty(new GUID(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_GridItemToolsWindowConfig))));
        }

        private void OnGUI()
        {
            m_GridItemToolsWindowConfig_Ser.Update();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(); EditorGUILayout.Space();

            //一级标题风格
            GUIStyle title1GUIStyle = new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = new Color(1f, 1f, 1f)
                }
            };
            EditorGUILayout.LabelField("网格物品工具", title1GUIStyle);
            EditorGUILayout.Space();

            //二级标题风格
            GUIStyle title2GUIStyle = new GUIStyle
            {
                fontSize = 14,
                alignment = TextAnchor.UpperCenter,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = new Color(1f, 1f, 1f)
                }
            };

            #region Processor Script 处理器脚本配置
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("工作流处理器脚本配置", title2GUIStyle);

            EditorGUI.BeginChangeCheck();
            m_GridItemToolsWindowConfig.processorScript = EditorGUILayout.ObjectField(
               new GUIContent("工作流处理器脚本", "项目想要介入工作流进行处理，需要继承GridItemToolsWindowProcessor类，并将脚本配置到此。"),
               m_GridItemToolsWindowConfig.processorScript, typeof(MonoScript), false) as MonoScript;

            if (EditorGUI.EndChangeCheck())
            {
                CheckProcessorScript();
            }

            #endregion

            #region 生成预制件及批量网格物品

            //根据提供的GridItemsData批量生成网格物品预制体
            //网格物品碰撞器：物品占用Cell和物品实际碰撞器形状没有必然关系，物品的碰撞器*需要手动配置*
            //网格物品类脚本：根据网格物品功能定义的不同，项目可能为网格物品提供了不同的子类。如果没有则使用配置的基类
            //如果没有配置网格物品类型脚本的参数，那么将不会添加脚本，*可能需要手动配置*
            //GridItemComponent Data Import：如果成功添加了网格物品类脚本，我们会向网格物品组件类中导入所有必要的数据。否则 *可能需要手动配置*
            //ViewRoot：我们会生成ViewRoot，之后根据配置的图片根目录下KeyName同名文件夹下的图片数量，我们将在ViewRoot下生成对应数量的Image子节点，并添加SpriteRenderer脚本且设置Image

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("生成预制件及批量网格物品", title2GUIStyle);

            EditorGUILayout.LabelField("完成必要配置后，选中资源生成的文件夹路径并点击生成。");

            //获取网格物品图片存放路径
            m_GridItemToolsWindowConfig.imagesFolderPath_CreatePrefab = DragTextField(
                m_GridItemToolsWindowConfig.imagesFolderPath_CreatePrefab,
                new GUIContent("网格物品图片文件夹", "请将GridItemsDataTxt中使用到的图片存放在此文件夹下，创建预制体自动配置Image时会从此文件夹中查找。（右键文件夹后点击CopyPath复制文件夹路径，或者拖拽文件夹到文件夹配置范围内）"));

            //获取网格物品预制体存放根文件夹
            m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab = DragTextField(
                m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab,
                new GUIContent("网格物品预制体检索文件夹", "请将存放所有GridItem预制体的根文件夹配置在此。我们检索此文件夹防止创建重复的预制体。"));

            EditorGUILayout.Space(); EditorGUILayout.Space();

            //获取GridItemsDataTxt文件
            m_GridItemToolsWindowConfig.gridItemsDataTxt_CreatePrefab = EditorGUILayout.ObjectField(
                new GUIContent("网格物品数据文本", Texture2D.whiteTexture, "数据文本由Aseprite资源导出，请将GridItemDataTransverter脚本安装到Aseprite并按照规定配置Aseprite数据。"),
                m_GridItemToolsWindowConfig.gridItemsDataTxt_CreatePrefab, typeof(TextAsset), false) as TextAsset;

            //设置GridItem的预制体导出文件夹
            m_GridItemToolsWindowConfig.gridItemsExportFolder_CreatePrefab = DragTextField(
                m_GridItemToolsWindowConfig.gridItemsExportFolder_CreatePrefab,
                new GUIContent("网格物品预制体导出文件夹", "点击生成后，将导出建筑预制件和批量网格物品到此文件夹中。这是根节点文件夹，我们还会创建子文件夹在此文件夹下。（右键文件夹后点击CopyPath复制文件夹路径，或者拖拽文件夹到文件夹配置范围内）"));

            //使用已经存在的预制体
            m_GridItemToolsWindowConfig.useExistedPrefab_CreatePrefab = GUILayout.Toggle(
                m_GridItemToolsWindowConfig.useExistedPrefab_CreatePrefab,
                new GUIContent("使用已有的预制体", "如果已经存在数据表中同名的预制体，那么我们就会使用这个预制体。否则，我们会创建新的预制体，如果旧预制体路径和我们创建的路径相同，旧的预制体将被覆盖。"));
            if (m_GridItemToolsWindowConfig.useExistedPrefab_CreatePrefab)
            {
                //更新预制体
                m_GridItemToolsWindowConfig.gridItemPrefabUpdate_CreatePrefab = GUILayout.Toggle(
                    m_GridItemToolsWindowConfig.gridItemPrefabUpdate_CreatePrefab,
                    new GUIContent("更新预制体", "是否更新已有预制体的数据。(即使没有勾选任何子项目，更新后依旧会调用到OnUpdateGridItemPrefab通知更新。)"));
                if(m_GridItemToolsWindowConfig.gridItemPrefabUpdate_CreatePrefab)
                {
                    m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemMonoScript = GUILayout.Toggle(
                        m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemMonoScript,
                        new GUIContent("更新网格物品脚本类", "此类型脚本由项目创建，此类中必须拥有GridItemComponent字段。"));

                    m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemComponent_CreatePrefab = GUILayout.Toggle(
                        m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemComponent_CreatePrefab,
                        new GUIContent("更新网格物品组件", "更新GridItemComponent网格物品组件，这会更新网格物品单位化尺寸等数据。"));

                    m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ViewRoot_CreatePrefab = GUILayout.Toggle(
                        m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ViewRoot_CreatePrefab,
                        new GUIContent("更新显示节点", "更新网格物品下显示节点内容，包括显示节点配置的图片，显示子节点位置等。"));

                    m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ColliderRoot_CreatePrefab = GUILayout.Toggle(
                        m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ColliderRoot_CreatePrefab,
                        new GUIContent("更新碰撞器节点", "更新网格物品下碰撞器节点，这会删除已有的碰撞器内容并根据Aseprite导入数据重新生成。谨慎使用，这可能覆盖掉已经经过手工调整的碰撞器内容！"));

                    //更新碰撞体前必须保证GridItemComponent的数据是最新的
                    if (m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ColliderRoot_CreatePrefab) m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemComponent_CreatePrefab = true;
                }
            }

            //配置GridItem的脚本基类
            m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab = EditorGUILayout.ObjectField(
               new GUIContent("网格物品脚本基类", "此类型脚本由项目创建，此类中必须拥有GridItemComponent字段。"),
               m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab, typeof(MonoScript), false) as MonoScript;
            //配置GridItem挂载脚本
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                m_GridItemScriptSettings_SerP,
                new GUIContent("网格物品脚本配置", "根据网格物品数据的ScriptTag来确认挂载哪个脚本。可不配置，那么默认会挂载配置的GridItem基类脚本。"),
                true
                );
            //对比长度条件，在开启窗口和导入数据后会刷新一次数据
            if (EditorGUI.EndChangeCheck() || m_GridItemScriptSettings_Cached.Count != m_GridItemToolsWindowConfig.gridItemScriptSettings.Count)
            {
                m_GridItemToolsWindowConfig_Ser.ApplyModifiedProperties();
                bool changed = false;

                if (m_GridItemScriptSettings_Cached.Count != m_GridItemToolsWindowConfig.gridItemScriptSettings.Count)
                    changed = true;
                else
                {
                    for (int i = 0; i < m_GridItemToolsWindowConfig.gridItemScriptSettings.Count; i++)
                    {
                        if (m_GridItemToolsWindowConfig.gridItemScriptSettings[i] != m_GridItemScriptSettings_Cached[i])
                        {
                            changed = true;
                            break;
                        }
                    }
                }

                if (changed)
                {
                    //记录到Dic方便查询
                    m_GridItemScriptDic_CreatePrefab.Clear();
                    for (int i = 0; i < m_GridItemToolsWindowConfig.gridItemScriptSettings.Count; i++)
                    {
                        if (string.IsNullOrEmpty(m_GridItemToolsWindowConfig.gridItemScriptSettings[i].scriptTag) 
                            || m_GridItemToolsWindowConfig.gridItemScriptSettings[i].gridItemScript == null
                            || m_GridItemScriptDic_CreatePrefab.ContainsKey(m_GridItemToolsWindowConfig.gridItemScriptSettings[i].scriptTag))
                            continue;

                        m_GridItemScriptDic_CreatePrefab.Add(m_GridItemToolsWindowConfig.gridItemScriptSettings[i].scriptTag, m_GridItemToolsWindowConfig.gridItemScriptSettings[i].gridItemScript);
                    }

                    m_GridItemScriptSettings_Cached = new List<GridItemToolsWindowConfig.GridItemScriptSetting>(m_GridItemToolsWindowConfig.gridItemScriptSettings);
                }
            }

            if (GUILayout.Button("生成网格物品组预制件"))
            {
                //确认需要的数据是否准备好
                bool configDataValid = true;

                if (!m_GridItemToolsWindowConfig.gridItemsDataTxt_CreatePrefab
                    || !m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab
                    || string.IsNullOrEmpty(m_GridItemToolsWindowConfig.imagesFolderPath_CreatePrefab)
                    || string.IsNullOrEmpty(m_GridItemToolsWindowConfig.gridItemsExportFolder_CreatePrefab))
                {
                    configDataValid = false;
                    EditorUtility.DisplayDialog("GridItemTools Tips", "无法执行生成预制件及批量网格物品，因为参数未正确配置。", "确认");
                }

                if (configDataValid) configDataValid = CheckGridItemImages(); //获取图片目录中所有图片
                if (configDataValid && m_GridItemToolsWindowConfig.useExistedPrefab_CreatePrefab) configDataValid = CheckGridItemPrefabDepot(); //获取预制体仓库中已有预制体

                if (configDataValid && GridSystemEditorLibrary.GetAsepriteGridItemsData(m_GridItemToolsWindowConfig.gridItemsDataTxt_CreatePrefab, out m_AsepriteHeaderData_CreatePrefab, out m_GridItemDataDic_CreatePrefab))
                {
                    //bool newPrefabCreate = false;
                    string rootPath = m_GridItemToolsWindowConfig.gridItemsExportFolder_CreatePrefab + "/" + m_AsepriteHeaderData_CreatePrefab.spriteName;
                    string folderPath = $"{rootPath}/{m_AsepriteHeaderData_CreatePrefab.spriteName}_GridItems";

                    //生成批量网格物品
                    m_CreateGridItemPrefabDic.Clear(); //本次执行创建预制件组预制体使用到的预制体，包括新创建的，和从仓库获取的
                    m_CreatePreformedUnitGridItemsDic.Clear(); //本次创建的预制件组预制体
                    foreach (var gridItemData in m_GridItemDataDic_CreatePrefab.Values)
                    {
                        bool succeed = true;
                        GameObject gridItemPrefab;
                        string prefabPath;

                        //本次执行，已经创建过的网格物品预制体，直接获取
                        if (m_CreateGridItemPrefabDic.ContainsKey(gridItemData.prefabName))
                        {
                            gridItemPrefab = m_CreateGridItemPrefabDic[gridItemData.prefabName];
                        }
                        else
                        {
                            //搜索网格物品预制体仓库中已经存在的预制体
                            bool findPrefab = false;
                            if (m_GridItemToolsWindowConfig.useExistedPrefab_CreatePrefab && m_GridItemPrefabDepot.ContainsKey(gridItemData.prefabName))
                            {
                                GameObject prefabFind = m_GridItemPrefabDepot[gridItemData.prefabName];

                                if (prefabFind)
                                {
                                    findPrefab = true;
                                    //更新已存在预制体数据
                                    if (m_GridItemToolsWindowConfig.gridItemPrefabUpdate_CreatePrefab)
                                    {
                                        //这里复制实例化一个新的GameObject进行操作，操作结束后覆盖保存旧的预制体
                                        //因为无法直接对预制体对象进行操作
                                        GameObject prefabFindNew = GameObject.Instantiate(prefabFind);

                                        if (GridItemUpdate(
                                            gridItemData.keyName,
                                            prefabFindNew,
                                            m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemMonoScript,
                                            m_GridItemToolsWindowConfig.gridItemPrefabUpdate_GridItemComponent_CreatePrefab, 
                                            m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ViewRoot_CreatePrefab, 
                                            m_GridItemToolsWindowConfig.gridItemPrefabUpdate_ColliderRoot_CreatePrefab,
                                            m_AsepriteHeaderData_CreatePrefab,
                                            gridItemData, folderPath))
                                        {
                                            PrefabUtility.SaveAsPrefabAsset(prefabFindNew, m_GridItemPrefabDepot_Path[gridItemData.prefabName]);
                                            GameObject.DestroyImmediate(prefabFindNew);

                                            //更新预制体引用缓存
                                            GameObject prefabNew = AssetDatabase.LoadAssetAtPath<GameObject>(m_GridItemPrefabDepot_Path[gridItemData.prefabName]);
                                            m_GridItemPrefabDepot[gridItemData.prefabName] = prefabNew;
                                        }
                                        else
                                        {
                                            Debug.Log(string.Format("批量创建预制体时更新已存在的预制体资源失败。更新失败的预制体为{0}", gridItemData.prefabName));
                                        }
                                    }

                                    m_CreateGridItemPrefabDic.Add(gridItemData.prefabName, m_GridItemPrefabDepot[gridItemData.prefabName]);
                                }
                                else
                                {
                                    m_GridItemPrefabDepot.Remove(gridItemData.prefabName);
                                    m_GridItemPrefabDepot_Path.Remove(gridItemData.prefabName);
                                }
                            }

                            if (!findPrefab)
                            {
                                m_CreateGridItemKeyName = gridItemData.keyName;
                                //创建预制体
                                prefabPath = GridSystemEditorLibrary.CreatePrefab(
                                CreatGridItemPrefabProcessor,
                                folderPath,
                                gridItemData.prefabName,
                                true,
                                out gridItemPrefab);

                                if (!string.IsNullOrEmpty(prefabPath))
                                {
                                    //newPrefabCreate = true;
                                    m_CreateGridItemPrefabDic.Add(gridItemData.prefabName, gridItemPrefab);
                                    m_CreateGridItemScriptClass = gridItemPrefab.GetComponent(m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab.GetClass());
                                    m_CreateGridItemViewRoot = gridItemPrefab.transform.Find(GridSystemConfig.viewRootName).gameObject;
                                    m_CreateGridItemColliderRoot = gridItemPrefab.transform.Find(GridSystemConfig.colliderRootName).gameObject;
                                    if (m_GridItemToolsWindowProcessor != null && !m_GridItemToolsWindowProcessor.OnCreateGridItemPrefab(gridItemPrefab, m_CreateGridItemScriptClass, m_CreateGridItemViewRoot, m_CreateGridItemColliderRoot))
                                        Debug.Log(string.Format("项目定义的工作流处理器执行失败。失败方法为{0}。失败的类为{1}", "OnCreateGridItemPrefab", m_CreateGridItemScriptClass.ToString()));
                                }
                                else
                                {
                                    succeed = false;
                                    Debug.Log(string.Format("批量创建预制体时失败。创建失败的预制体为{0}", gridItemData.prefabName));
                                }
                            }
                        }

                        if (succeed)
                        {
                            m_CreatePreformedUnitGridItemsDic.Add(gridItemData.keyName, gridItemData.prefabName);
                        }
                    }

                    //生成预制件并将网格物品放入，导入网格物品数据
                    GridSystemEditorLibrary.CreatePrefab(
                            CreateGridItemsPreformedUnitPrefabProcessor,
                            rootPath,
                            m_AsepriteHeaderData_CreatePrefab.spriteName,
                            true,
                            out GameObject preformedUnitPrefab);

                    //刷新显示层结构节点位置
                    GridSystemEditorLibrary.EditorRefreshViewRootPosition(preformedUnitPrefab, m_AsepriteHeaderData_CreatePrefab.asepriteGridConfig.cellSizePixel);

                    if (m_GridItemToolsWindowProcessor != null && !m_GridItemToolsWindowProcessor.OnCreatePreformedUnitPrefab(preformedUnitPrefab))
                        Debug.Log(string.Format("项目定义的工作流处理器执行失败。失败方法为{0}", "preformedUnitPrefab"));

                    //创建了新的预制体，那么更新一次预制体仓库Dic
                    //if (newPrefabCreate)
                    //    CheckGridItemPrefabDepot(true);

                    CheckGridItemPrefabDepot(true);

                    //TODO : 如果我们正在一个此次会被修改的预制体编辑场景。更新预制体后，不知道怎应重载预制体编辑场景。当前预制体编辑场景还是旧的数据。希望能否重新载入当前所在的预制体编辑场景。
                    //现在我们必须退出当前预制体编辑场景再进入才能刷新显示的内容。
                    //UnityEditor.SceneManagement.EditorSceneManager.OpenScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
                    //UnityEngine.SceneManagement.SceneManager.GetActiveScene().isSubScene

                    Debug.Log(string.Format("生成预制体成功，预制件组预制体名称{0}", preformedUnitPrefab.name));
                    AssetDatabase.OpenAsset(preformedUnitPrefab);
                }
            }

            #region 更新单个预制体

            EditorGUILayout.Space(); EditorGUILayout.Space();
            EditorGUILayout.LabelField("更新单个网格物品预制体", title2GUIStyle);
            EditorGUILayout.Space();

            //获取GridItem预制件预制体
            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab = EditorGUILayout.ObjectField(
                new GUIContent("网格物品组装件预制体", "Unity的预制体，要求预制体根节点挂载了定义为GridItem的脚本，此脚本内必须有GridItemComponent字段。"),
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab, typeof(GameObject), false) as GameObject;

            //获取GridItemsDataTxt文件
            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_DataText = EditorGUILayout.ObjectField(
                new GUIContent("网格物品数据文本", Texture2D.whiteTexture, "数据文本由Aseprite资源导出，请将GridItemDataTransverter脚本安装到Aseprite并按照规定配置Aseprite数据。"),
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_DataText, typeof(TextAsset), false) as TextAsset;

            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemMonoScript = GUILayout.Toggle(
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemMonoScript,
                new GUIContent("更新网格物品脚本类", "此类型脚本由项目创建，此类中必须拥有GridItemComponent字段。"));

            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemComponent = GUILayout.Toggle(
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemComponent,
                new GUIContent("更新网格物品组件", "更新GridItemComponent网格物品组件，这会更新网格物品单位化尺寸等数据。"));

            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ViewRoot = GUILayout.Toggle(
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ViewRoot,
                new GUIContent("更新显示节点", "更新网格物品下显示节点内容，包括显示节点配置的图片，显示子节点位置等。"));

            m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ColliderRoot = GUILayout.Toggle(
                m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ColliderRoot,
                new GUIContent("更新碰撞器节点", "更新网格物品下碰撞器节点，这会删除已有的碰撞器内容并根据Aseprite导入数据重新生成。谨慎使用，这可能覆盖掉已经经过手工调整的碰撞器内容！"));

            //更新碰撞体前必须保证GridItemComponent的数据是最新的
            if (m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ColliderRoot) m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemComponent = true;

            if (GUILayout.Button("更新单个网格物品预制体") && m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab != null && m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_DataText != null)
            {
                bool configDataValid = true;
                if (configDataValid) configDataValid = CheckGridItemImages(); //获取图片目录中所有图片
                if (configDataValid) configDataValid = CheckGridItemPrefabDepot(); //获取预制体仓库中已有预制体

                if (configDataValid
                    && GridSystemEditorLibrary.GetAsepriteGridItemDataSingle(
                    m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_DataText,
                    m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab.name,
                    out AsepriteHeaderData singleGridItemPrefabUpdate_AsepriteHeaderData,
                    out AsepriteGridItemData singleGridItemPrefabUpdate_GridItemData,
                    true))
                {
                    //这里复制实例化一个新的GameObject进行操作，操作结束后覆盖保存旧的预制体
                    //因为无法直接对预制体对象进行操作
                    GameObject prefabFindNew = GameObject.Instantiate(m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab);

                    string rootPath = m_GridItemToolsWindowConfig.gridItemsExportFolder_CreatePrefab + "/" + m_AsepriteHeaderData_CreatePrefab.spriteName;
                    string folderPath = $"{rootPath}/{m_AsepriteHeaderData_CreatePrefab.spriteName}_GridItems";

                    if (GridItemUpdate(
                        singleGridItemPrefabUpdate_GridItemData.keyName,
                        prefabFindNew,
                        m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemMonoScript,
                        m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_GridItemComponent,
                        m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ViewRoot,
                        m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_ColliderRoot,
                        singleGridItemPrefabUpdate_AsepriteHeaderData,
                        singleGridItemPrefabUpdate_GridItemData,
                        folderPath)) 
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabFindNew, m_GridItemPrefabDepot_Path[singleGridItemPrefabUpdate_GridItemData.prefabName]);
                        GameObject.DestroyImmediate(prefabFindNew);

                        //更新预制体引用缓存
                        GameObject prefabNew = AssetDatabase.LoadAssetAtPath<GameObject>(m_GridItemPrefabDepot_Path[singleGridItemPrefabUpdate_GridItemData.prefabName]);
                        m_GridItemPrefabDepot[singleGridItemPrefabUpdate_GridItemData.prefabName] = prefabNew;

                        AssetDatabase.OpenAsset(prefabNew);

                        Debug.Log(string.Format("更新单个网格物品预制体成功。预制体为{0}", singleGridItemPrefabUpdate_GridItemData.prefabName));
                    }
                    else
                    {
                        Debug.Log(string.Format("更新单个网格物品预制体失败。预制体为{0}", singleGridItemPrefabUpdate_GridItemData.prefabName));
                    }
                }
                else
                {
                    Debug.Log(string.Format("更新单个网格物品预制体失败。预制体为{0}", m_GridItemToolsWindowConfig.singleGridItemPrefabUpdate_Prefab.name));
                }
            }

            #endregion

            #endregion

            #region 导出和导入工具面板配置

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("导入导出网格物品工具配置", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("读取配置"))
            {
                if (EditorUtility.DisplayDialog("GridItemTools Tips", "是否读取配置？这会覆盖现有的配置。", "确定", "取消"))
                {
                    GridItemToolsWindowConfig m_GridItemToolsWindowConfig_Saved = AssetDatabase.LoadAssetAtPath<GridItemToolsWindowConfig>(m_ConfigPath_Saved);
                    if (m_GridItemToolsWindowConfig_Saved)
                    {
                        AssetDatabase.DeleteAsset(m_ConfigPath_Cur);
                        m_GridItemToolsWindowConfig = Instantiate(m_GridItemToolsWindowConfig_Saved);
                        AssetDatabase.CreateAsset(m_GridItemToolsWindowConfig, m_ConfigPath_Cur);
                        LinkGridItemToolsWindowConfig();

                        m_GridItemScriptSettings_Cached.Clear();//清除网格物品类型配置List缓存，这样能更新相关数据
                    }
                }
            }
            else if (GUILayout.Button("保存配置"))
            {
                if (EditorUtility.DisplayDialog("GridItemTools Tips", "是否保存配置？这会覆盖上次保存的配置内容。", "确定", "取消"))
                {
                    GridItemToolsWindowConfig m_GridItemToolsWindowConfig_Saved = AssetDatabase.LoadAssetAtPath<GridItemToolsWindowConfig>(m_ConfigPath_Saved);

                    //移除旧的
                    if (m_GridItemToolsWindowConfig_Saved)
                        AssetDatabase.DeleteAsset(m_ConfigPath_Saved);

                    m_GridItemToolsWindowConfig_Saved = Instantiate(m_GridItemToolsWindowConfig);
                    AssetDatabase.CreateAsset(m_GridItemToolsWindowConfig_Saved, m_ConfigPath_Saved);

                    //如果已经存在Saved配置，直接通过m_GridItemToolsWindowConfig_Saved = Instantiate(m_GridItemToolsWindowConfig)会导致崩溃。
                    //也没有找到方便的复制GridItemToolsWindowConfig类的方法
                }
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            //额外功能
            EditorGUILayout.Space();
            m_ExtraFoldout = EditorGUILayout.Foldout(m_ExtraFoldout, "额外功能");
            if (m_ExtraFoldout)
            {
                #region 网格物品整体组装件更新

                //设置GridItemSize网格物品单位化尺寸
                //设置ViewRoot下显示用图片节点的位置

                EditorGUILayout.Space(); EditorGUILayout.Space();
                EditorGUILayout.LabelField("网格物品整体组装件更新", title2GUIStyle);
                EditorGUILayout.Space();

                //获取GridItem预制件预制体
                m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_Update_Prefab = EditorGUILayout.ObjectField(
                    new GUIContent("网格物品组装件预制体", "Unity的预制体，要求预制体根节点挂载了定义为GridItem的脚本，此脚本内必须有GridItemComponent字段。"),
                    m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_Update_Prefab, typeof(GameObject), false) as GameObject;

                //获取GridItemsDataTxt文件
                m_GridItemToolsWindowConfig.gridItemsDataTxt_Update_DataText = EditorGUILayout.ObjectField(
                    new GUIContent("网格物品数据文本", Texture2D.whiteTexture, "数据文本由Aseprite资源导出，请将GridItemDataTransverter脚本安装到Aseprite并按照规定配置Aseprite数据。"),
                    m_GridItemToolsWindowConfig.gridItemsDataTxt_Update_DataText, typeof(TextAsset), false) as TextAsset;

                if (GUILayout.Button("整体预制件更新"))
                {
                    //确认输入数据有效性
                    if (m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_Update_Prefab && m_GridItemToolsWindowConfig.gridItemsDataTxt_Update_DataText)
                    {
                        //获取数据
                        if (GridSystemEditorLibrary.GetAsepriteGridItemsData(
                            m_GridItemToolsWindowConfig.gridItemsDataTxt_Update_DataText,
                            out AsepriteHeaderData asepriteHeaderData_DataImport,
                            out Dictionary<string, AsepriteGridItemData> gridItemDataDic_DataImport))
                        {
                            if (!GridSystemEditorLibrary.SetPreformedUnitPrefab(gridItemDataDic_DataImport, m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_Update_Prefab.transform))
                            {
                                EditorUtility.DisplayDialog("GridItemTools Tips", "无法执行导入数据到预制件，请确认GridSystemEditorLibrary.SetPreformedUnitPrefab方法执行是否顺利。", "确认");
                            }

                            PrefabUtility.SavePrefabAsset(m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_Update_Prefab);
                        }
                    }
                    else
                        EditorUtility.DisplayDialog("GridItemTools Tips", "无法执行导入数据到预制件，因为参数未正确配置。", "确认");
                }
                #endregion

                #region 批量设置网格物品组装件内所有网格物品的ViewRoot的Position

                //在编辑器模式中设置ViewRoot位置，以达到预览显示效果的目的
                //实际游戏运行时ViewRoot会根据运行时情况设置

                EditorGUILayout.Space(); EditorGUILayout.Space();
                EditorGUILayout.LabelField("网格物品组装件内所有网格物品ViewRoot设置", title2GUIStyle);
                EditorGUILayout.Space();

                //获取GridItem预制件预制体
                m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_ViewRootSet = EditorGUILayout.ObjectField("网格物品组装件预制体", m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_ViewRootSet, typeof(GameObject), false) as GameObject;
                //获取GridItemsDataTxt文件
                m_GridItemToolsWindowConfig.gridItemsDataTxt_ViewRootSet = EditorGUILayout.ObjectField(
                    new GUIContent("网格物品数据文本", Texture2D.whiteTexture, "数据文本由Aseprite资源导出，请将GridItemDataTransverter脚本安装到Aseprite并按照规定配置Aseprite数据。"),
                    m_GridItemToolsWindowConfig.gridItemsDataTxt_ViewRootSet, typeof(TextAsset), false) as TextAsset;

                if (GUILayout.Button("设置ViewRoot") && m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_ViewRootSet)
                {
                    if (GridSystemEditorLibrary.GetAsepriteHeaderData(m_GridItemToolsWindowConfig.gridItemsDataTxt_ViewRootSet, out AsepriteHeaderData asepriteHeaderData_ViewRootSet))
                        GridSystemEditorLibrary.EditorRefreshViewRootPosition(m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_ViewRootSet, asepriteHeaderData_ViewRootSet.asepriteGridConfig.cellSizePixel);
                    else
                        GridSystemEditorLibrary.EditorRefreshViewRootPosition(m_GridItemToolsWindowConfig.gridItemPreformedUnitPrefab_ViewRootSet);
                }
                #endregion
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 创建网格物品预制体时处理方法
        /// </summary>
        /// <param name="GObj"></param>
        /// <returns></returns>
        public bool CreatGridItemPrefabProcessor(GameObject GObj, string folderPath)
        {
            if (!m_GridItemDataDic_CreatePrefab.ContainsKey(m_CreateGridItemKeyName)) return false;

            AsepriteGridItemData data = m_GridItemDataDic_CreatePrefab[m_CreateGridItemKeyName];
            Transform rootTs = GObj.transform;

            //添加GridItem脚本，并导入数据
            Type scriptType = m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab.GetClass();
            if (!string.IsNullOrEmpty(data.scriptTag))
            {
                if (m_GridItemScriptDic_CreatePrefab.ContainsKey(data.scriptTag))
                    scriptType = m_GridItemScriptDic_CreatePrefab[data.scriptTag].GetClass();
                else
                    Debug.LogWarning(string.Format("GridItemTools Tips : 生成网格物品时，无法获取到{0}的脚本标记{1}对应的脚本，请确认网格物品脚本配置是否正确。", data.keyName, data.scriptTag));
            }

            m_CreateGridItemScriptClass = rootTs.gameObject.AddComponent(scriptType);
            GridSystemEditorLibrary.SetGridItemComponent(m_CreateGridItemScriptClass, m_AsepriteHeaderData_CreatePrefab, data, out GridItemComponent gridItemComponent);

            //生成ViewRoot
            m_CreateGridItemViewRoot = GridSystemEditorLibrary.CreateViewRoot(rootTs, true);
            //设置ViewRoot下的MeshRender
            GenerateViewRootMeshRender(m_CreateGridItemViewRoot.transform, data, folderPath);

            //生成默认碰撞器，碰撞器可根据需求自行调整
            GridSystemEditorLibrary.CreateGridItemCollider(GObj, data, gridItemComponent, m_AsepriteHeaderData_CreatePrefab, out m_CreateGridItemColliderRoot);

            if (m_GridItemToolsWindowProcessor != null && !m_GridItemToolsWindowProcessor.OnCreateGridItemAfter(GObj, m_CreateGridItemScriptClass, m_CreateGridItemViewRoot, m_CreateGridItemColliderRoot))
                Debug.Log(string.Format("项目定义的工作流处理器执行失败。失败方法为{0}", "OnCreateGridItemAfter"));

            return true;
        }

        /// <summary>
        /// 生成 显示物体的MeshRender
        /// </summary>
        /// <param name="viewRoot"></param>
        /// <param name="data"></param>
        /// <param name="folderPath"></param>
        private void GenerateViewRootMeshRender(Transform viewRoot, AsepriteGridItemData data, string folderPath)
        {
            //清空旧的渲染物体
            for (int i = 0; i < viewRoot.childCount; i++)
                GameObject.Destroy(viewRoot.GetChild(i).gameObject);

            int childIndex = 0;
            Dictionary<string, Transform> dicSubMeshRoot = new Dictionary<string, Transform>(); //多MeshRender组合的根节点
            foreach (var imageData in data.imageDataDic.Values)
            {
                //生成子节点物体
                Transform nodeTs = new GameObject().transform;
                nodeTs.SetParent(viewRoot);

                //多MeshRender组合
                if (imageData.imageName.Contains('-'))
                {
                    var subMeshRootName = imageData.imageName.Split('-')[0];
                    //多Mesh的根节点
                    if (!dicSubMeshRoot.ContainsKey(subMeshRootName))
                    {
                        Transform subMeshRootNew = new GameObject().transform;
                        subMeshRootNew.SetParent(viewRoot);
                        subMeshRootNew.name = subMeshRootName;
                        dicSubMeshRoot.Add(subMeshRootName, subMeshRootNew);
                    }

                    Transform subMeshRoot = dicSubMeshRoot[subMeshRootName];
                    nodeTs.SetParent(subMeshRoot);
                }

                //重命名
                nodeTs.name = imageData.imageName;

                //创建MeshRender
                if (m_ImageDic_CreatePrefab.ContainsKey(imageData.imageName))
                    GridSystemEditorLibrary.CreateGridItemMeshRender(nodeTs.gameObject, data, imageData, m_AsepriteHeaderData_CreatePrefab, m_ImageDic_CreatePrefab[imageData.imageName].texture, childIndex, folderPath);
                else
                    Debug.LogWarning(string.Format("GridItemTools Tips : 生成网格物品时，无法获取到{0}的图片{1}，请确认图片资源文件夹下是否存在此名称的图片。", data.keyName, imageData.imageName));

                childIndex++;
            }
        }

        #region 临时代码 Mesh生成
        [MenuItem("Tools/FsGridCellSystem/GenerateMesh")]
        static void Init()
        {
            var location = -new Vector3(0.5f, 0.5f, 0.5f);
            var mesh = new Mesh();
            Vector3[] vertices = null;
            int[] triangles = null;
            Vector2[] uvs = null;
            //顶点
            vertices = new Vector3[6];
            vertices[0] = new Vector3(0, 0, 0) + location;
            vertices[1] = new Vector3(1, 0, 0) + location;
            vertices[2] = new Vector3(0, 0, 1) + location;
            vertices[3] = new Vector3(1, 0, 1) + location;
            vertices[4] = new Vector3(0, 1, 1) + location;
            vertices[5] = new Vector3(1, 1, 1) + location;
            //三角面
            triangles = new int[24];
            triangles[0] = 1; triangles[1] = 2; triangles[2] = 0;
            triangles[3] = 1; triangles[4] = 3; triangles[5] = 2;
            triangles[6] = 3; triangles[7] = 4; triangles[8] = 2;
            triangles[9] = 3; triangles[10] = 5; triangles[11] = 4;
            triangles[12] = 0; triangles[13] = 4; triangles[14] = 5;
            triangles[15] = 0; triangles[16] = 5; triangles[17] = 1;
            triangles[18] = 0; triangles[19] = 2; triangles[20] = 4;
            triangles[21] = 1; triangles[22] = 5; triangles[23] = 3;
            //UV
            uvs = new Vector2[6];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(0, 0.5f);
            uvs[3] = new Vector2(1, 0.5f);
            uvs[4] = new Vector2(0, 1);
            uvs[5] = new Vector2(1, 1);
            mesh.SetVertices(vertices);
            mesh.triangles = triangles;
            mesh.uv = uvs;
            //法线方向
            mesh.RecalculateNormals();

            string folderPath = "Assets/ProductAssets";
            //确认文件夹是否存在，否则创建
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //保存Mesh
            AssetDatabase.CreateAsset(mesh, $"{folderPath}/Mesh_Slope.asset");
            return;
        }
        #endregion

        //更新网格物品数据
        public bool GridItemUpdate(string keyName, GameObject GObj, bool updateGridItemMonoScript, bool updateGridItemComponent,
            bool updateViewRoot, bool updateColliderRoot, AsepriteHeaderData headerData, AsepriteGridItemData data, string folderPath)
        {
            Transform rootTs = GObj.transform;

            //更新GridItemComponent
            GridItemComponent gridItemComponent = null;
            var gridItemScript = GObj.GetComponent(m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab.GetClass());
            if (updateGridItemMonoScript)
            {
                //获取需要创建的脚本类
                Type scriptType = m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab.GetClass();
                if (!string.IsNullOrEmpty(data.scriptTag))
                {
                    if (m_GridItemScriptDic_CreatePrefab.ContainsKey(data.scriptTag))
                        scriptType = m_GridItemScriptDic_CreatePrefab[data.scriptTag].GetClass();
                    else
                        Debug.LogWarning(string.Format("GridItemTools Tips : 生成网格物品时，无法获取到{0}的脚本标记{1}对应的脚本，请确认网格物品脚本配置是否正确。", data.keyName, data.scriptTag));
                }

                if (scriptType != null && gridItemScript.GetType() != scriptType)
                {
                    DestroyImmediate(gridItemScript);

                    //添加GridItem脚本，并导入数据
                    gridItemScript = rootTs.gameObject.AddComponent(scriptType);
                }
            }

            if (!gridItemScript) return false;
            if (updateGridItemComponent)
            {
                GridSystemEditorLibrary.SetGridItemComponent(gridItemScript, headerData, data, out gridItemComponent);
            }

            //更新ViewRoot显示节点
            var viewRootTs = rootTs.Find(GridSystemConfig.viewRootName);
            GenerateViewRootMeshRender(viewRootTs, data, folderPath);

            var colliderRootTs = rootTs.Find(GridSystemConfig.colliderRootName);
            GameObject colliderRoot = colliderRootTs.gameObject;
            if (updateColliderRoot)
            {
                if (gridItemComponent == null)
                    gridItemComponent = GridSystemEditorLibrary.GetGridItemComponent(GObj);

                if (gridItemComponent != null)
                    GridSystemEditorLibrary.CreateGridItemCollider(GObj, data, gridItemComponent, headerData, out colliderRoot);
            }

            if (m_GridItemToolsWindowProcessor != null && !m_GridItemToolsWindowProcessor.OnUpdateGridItemPrefab(GObj, gridItemScript, viewRootTs.gameObject, colliderRoot))
                Debug.Log(string.Format("项目定义的工作流处理器执行失败。失败方法为{0}", "OnUpdateGridItemPrefab"));

            return true;
        }

        /// <summary>
        /// 生成网格物品预制组预制体时
        /// </summary>
        /// <param name="GObj"></param>
        /// <returns></returns>
        public bool CreateGridItemsPreformedUnitPrefabProcessor(GameObject GObj, string folderPath)
        {
            if (m_AsepriteHeaderData_CreatePrefab.spriteName != GObj.name) return false;

            Transform PreformedUnitTs = GObj.transform;

            foreach (var item in m_CreatePreformedUnitGridItemsDic)
            {
                //此方法无效，会报错路径错误，但检查过路径没有问题
                //GameObject gridItemGObj = PrefabUtility.LoadPrefabContents(m_CreateGridItemPrefabPaths[i]); 
                //GameObject prefabGObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject prefabGObj = m_CreateGridItemPrefabDic[item.Value];
                GameObject gridItemGObj = GameObject.Instantiate(prefabGObj);
                gridItemGObj.transform.SetParent(PreformedUnitTs);
                PrefabUtility.ConnectGameObjectToPrefab(gridItemGObj, prefabGObj);
                PreformedUnitTs.GetChild(PreformedUnitTs.childCount - 1).name = item.Key;
            }

            bool succeed = GridSystemEditorLibrary.SetPreformedUnitPrefab(m_GridItemDataDic_CreatePrefab, PreformedUnitTs);

            if (m_GridItemToolsWindowProcessor != null && !m_GridItemToolsWindowProcessor.OnCreatePreformedUnitAfter(GObj))
                Debug.Log(string.Format("项目定义的工作流处理器执行失败。失败方法为{0}", "OnCreatePreformedUnitAfter"));

            return succeed;
        }

        public string DragTextField(string path, GUIContent label)
        {
            bool needCheckPathValid = false;
            Rect pathRect = EditorGUILayout.GetControlRect();

            //输入框直接输入
            EditorGUI.BeginChangeCheck();
            path = EditorGUI.TextField(pathRect, label, path);
            if (EditorGUI.EndChangeCheck())
                needCheckPathValid = true;

            //注意，EditorGUILayout.PropertyField方法会导致Event鼠标事件的丢失，不明原因。不要在此方法后再调用此方法。
            //鼠标移到输入框范围内
            if (pathRect.Contains(Event.current.mousePosition)
            && (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited))
            {
                if (Event.current.type == EventType.DragUpdated) //拖拽中
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic; //改变鼠标的外表
                }
                else if (Event.current.type == EventType.DragExited) //拖拽结束
                {
                    //获取拖拽文件的Path
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        path = DragAndDrop.paths[0];
                        needCheckPathValid = true;
                    }
                }
            }
            //确认图片文件夹路径是否合法
            if (needCheckPathValid)
            {
                bool isImagePathValid = true;
                if (!Directory.Exists(path)) isImagePathValid = false;

                if (!isImagePathValid)
                {
                    path = string.Empty;
                    //EditorUtility.DisplayDialog("GridItemTools Tips", "拖拽进来的不是有效的文件夹", "确认");
                }
            }

            return path;
        }

        /// <summary>
        /// 设置处理器脚本
        /// 根据配置生成处理器脚本实例，用于接入工作流
        /// </summary>
        public void CheckProcessorScript()
        {
            if (m_GridItemToolsWindowConfig.processorScript != null)
            {
                if (m_GridItemToolsWindowConfig.processorScript.GetClass().IsSubclassOf(typeof(GridItemToolsWindowProcessor)))
                {
                    if (m_GridItemToolsWindowProcessor == null || m_GridItemToolsWindowProcessor.GetType() != m_GridItemToolsWindowConfig.processorScript.GetClass())
                    {
                        m_GridItemToolsWindowProcessor = Activator.CreateInstance(m_GridItemToolsWindowConfig.processorScript.GetClass()) as GridItemToolsWindowProcessor;
                        m_GridItemToolsWindowProcessor.Init();
                    }
                }
                else
                {
                    m_GridItemToolsWindowConfig.processorScript = null;
                    Debug.Log("工作流处理器脚本必须是GridItemToolsWindowProcessor的子类！");
                }
            }
        }

        /// <summary>
        /// 确认获取网格物品图片Dic
        /// </summary>
        /// <returns></returns>
        public bool CheckGridItemImages()
        {
            //确认图片文件夹有效性
            if (GridSystemEditorLibrary.GetFiles<Sprite>(m_GridItemToolsWindowConfig.imagesFolderPath_CreatePrefab, out Sprite[] outImages, "*.png"))
            {
                //更新ImageDic方便查询
                m_ImageDic_CreatePrefab.Clear();
                for (int i = 0; i < outImages.Length; i++)
                {
                    Sprite image = outImages[i];
                    if(!m_ImageDic_CreatePrefab.ContainsKey(image.name))
                        m_ImageDic_CreatePrefab.Add(image.name, image);
                    else
                    {
                        Debug.Log(string.Format("获取图片文件夹下名称为{0}的图片有多个，请确认。", image.name));
                    }
                }

                return true;
            }
            else
            {
                EditorUtility.DisplayDialog("GridItemTools Tips", "无法执行生成预制件及批量网格物品，因为图片文件夹下没有任何有效的图片。", "确认");
                return false;
            }
        }

        /// <summary>
        /// 确认获取网格物品预制体仓库
        /// 从设定的目录中读取所有网格物品预制体
        /// </summary>
        /// <param name="skipPathConfirmation"></param>
        public bool CheckGridItemPrefabDepot(bool skipPathConfirmation = false)
        {
            if (string.IsNullOrEmpty(m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab))
            {
                //设置目录为空
                EditorUtility.DisplayDialog("GridItemTools Tips", "开启了预制体复用（不创建重复预制体）功能，但没有配置搜索目录。", "确认");
                return false;
            }
            if (!skipPathConfirmation && m_GridItemPrefabSearchPath_CreatePrefab_Cached == m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab) return true; //没有更新必要

            AssetDatabase.Refresh();

            m_GridItemPrefabDepot.Clear();
            m_GridItemPrefabDepot_Path.Clear();
            if (GridSystemEditorLibrary.GetFiles<GameObject>(m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab, out GameObject[] prefabs, out string[] prefabPaths, "*.Prefab"))
            {
                for (int i = 0; i < prefabs.Length; i++)
                {
                    var prefab = prefabs[i];
                    //获取GridItem基类脚本，作为对GridItem的认定
                    if (!prefab.GetComponent(m_GridItemToolsWindowConfig.gridItemMonoScript_CreatePrefab.GetClass())) continue;

                    if (!m_GridItemPrefabDepot.ContainsKey(prefab.name))
                    {
                        m_GridItemPrefabDepot.Add(prefab.name, prefab);
                        m_GridItemPrefabDepot_Path.Add(prefab.name, prefabPaths[i]);
                    }
                    else
                        Debug.Log(string.Format("指定的预制体仓库文件夹下有名称为{0}的多个预制体，请确认。", prefab.name));
                }

                m_GridItemPrefabSearchPath_CreatePrefab_Cached = m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab;
            }
            else
            {
                //可能此目录是新目录，还未创建任何GridItem预制体，仅仅做提示
                Debug.Log(string.Format("无法从预制体搜索文件夹中获取任何预制体。检索路径{0}", m_GridItemToolsWindowConfig.gridItemPrefabSearchPath_CreatePrefab));
                return true;
            }

            return true;
        }
    }
}
