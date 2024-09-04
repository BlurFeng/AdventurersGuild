using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

public class ResourceSeeker : EditorWindow 
{
    class PrefabItem //Prefab项
    {
        public GameObject prefab;
        public List<UnityEngine.Object> dependObjs = new List<UnityEngine.Object>();
        public List<string> dependObjsHierarchy = new List<string>();
    }

    class SpriteItem //Sprite项
    {
        public Sprite sprite;
        public string name;
        public Texture image;
        public Dictionary<GameObject, PrefabItem> dependPrefabs = new Dictionary<GameObject, PrefabItem>();
    }

    //配置文件数据
    private string m_SeekAssetPath = string.Empty; //查询资源文件的Path
    private string m_SeekAssetName = string.Empty; //查询资源文件的Name
    private string m_SeekAssetGUID = string.Empty; //查询资源文件的GUID
    private UnityEngine.Object m_SeekAssetObj = null;
    private bool m_SeekAssetIsAtlas = false; //查询资源文件是否为图集
    private UnityEngine.Object[] m_SpriteAtlas; //图集Sprite列表

    //WindowUI 界面参数
    private int m_ActiveInspectType = 0; //当前选中的Label
    private string[] m_InspectLabels = { "Prefab", "SpriteAtlas" };
    private List<string> m_OperationTips = new List<string>();
    private Color m_DefaultColor;
    private Color m_TipsColor = Color.white;
    private static int MinWidth = 1000;

    //WindowUI Prefab下拉窗口
    private List<PrefabItem> m_AllSeekPrefabItem = new List<PrefabItem>(); //查询结果 所有Prefab
    private string m_PrefabViewDependObjsTitle = string.Empty; //选中的Prefab名
    private List<string> m_PrefabViewDependObjs = new List<string>(); //查询结果 当前打开的Prefab 所有引用的物体路径
    private Vector2 m_ListScrollPosPrefab1 = new Vector2(0, 0);
    private Vector2 m_ListScrollPosPrefab2 = new Vector2(0, 0);

    //WindowUI SpriteAtlas下拉窗口
    private Dictionary<string, SpriteItem> m_AllSeekSpriteItem = new Dictionary<string, SpriteItem>(); //图集中所有的Sprite 查询结果
    private string m_SpriteViewDependPrefabsTitle = string.Empty; //选中的Sprite名
    private Dictionary<GameObject, PrefabItem> m_SpriteViewDependPrefabs = new Dictionary<GameObject, PrefabItem>(); //查询结果 所有引用的Prefab
    private string m_SpriteViewDependPfbsTitle = string.Empty; //选中的Prefab名
    private List<string> m_SpriteViewDependPfbs = new List<string>(); //查询结果 当前打开的Prefab 所有引用的物体路径
    private List<string> m_EmptyStringList = new List<string>(); //查询结果 空
    private Vector2 m_ListScrollPosSprite1 = new Vector2(0, 0);
    private Vector2 m_ListScrollPosSprite2 = new Vector2(0, 0);
    private Vector2 m_ListScrollPosSprite3 = new Vector2(0, 0);

    [MenuItem("Window/Resource Seeker")]
    static void Init()
    {
        ResourceSeeker window = (ResourceSeeker)EditorWindow.GetWindow(typeof(ResourceSeeker));
        window.minSize = new Vector2(MinWidth, 500);
        window.m_OperationTips.Add("(1)从Project窗口拖拽需要查询的资源文件至ResourceSeeker窗口内");
        window.m_OperationTips.Add("(2)点击[Start Seek]按钮 开始查询");
    }

    void OnGUI()
    {
        m_DefaultColor = GUI.color;

        //提示信息
        GUILayout.BeginArea(new Rect(20, 15, 500, 120));
        GUI.color = m_TipsColor;
        int viewCount = m_OperationTips.Count <= 6 ? m_OperationTips.Count : 6;
        for (int i = 0; i < viewCount; i++)
        {
            GUILayout.Label(m_OperationTips[i]);
        }
        GUI.color = m_DefaultColor;
        GUILayout.EndArea();

        //查询参数输入框
        GUILayout.BeginArea(new Rect(position.width * 0.5f, 20, 450, 100));
        GUILayout.Label("[SeekAssetPath]", GUILayout.Width(100));
        GUILayout.Label(m_SeekAssetPath, GUILayout.Width(300));
        if (m_SeekAssetObj != null)
        {
            if (GUILayout.Button("Select In Project", GUILayout.Width(150)))
            {
                Selection.activeObject = m_SeekAssetObj;
            }
        }
        GUILayout.EndArea();

        //鼠标移到Window上
        if (mouseOverWindow == this)
        {
            if (Event.current.type == EventType.DragUpdated) //拖拽中
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic; //改变鼠标的外表
            }
            else if(Event.current.type == EventType.DragExited) //拖拽结束
            {
                //获取拖拽文件的Path
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                {
                    GetSeekAssetInfo(DragAndDrop.paths[0]);
                }
            }
        }

        //开始查询 按钮
        GUILayout.BeginArea(new Rect(position.width - 120, 20, 100, 65));
        GUI.color = Color.green;
        if (GUILayout.Button("Start Seek", GUILayout.Width(100), GUILayout.Height(60)))
        {
            OnSeekAllPrefabInDirectory();
        }
        GUI.color = m_DefaultColor;
        GUILayout.EndArea();

        //分类标签 Item数量
        GUILayout.Space(100);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Count-" + m_AllSeekPrefabItem.Count);
        GUILayout.Label("Count-" + m_AllSeekSpriteItem.Count);
        GUILayout.EndHorizontal();

        //分类标签 Label按钮
        m_ActiveInspectType = GUILayout.Toolbar(m_ActiveInspectType, m_InspectLabels);

        ShowItemScrollView();
    }

    void GetSeekAssetInfo(string path) //获取 查询文件信息
    {
        m_SeekAssetPath = path;
        m_SeekAssetObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(m_SeekAssetPath);
        m_SeekAssetName = Path.GetFileNameWithoutExtension(m_SeekAssetPath);
        m_SeekAssetGUID = AssetDatabase.AssetPathToGUID(m_SeekAssetPath);

        //判断是否为 SpriteAtlas
        m_SeekAssetIsAtlas = false;
        m_SpriteAtlas = null;
        if (path.EndsWith(".png") || path.EndsWith("jpg"))
        {
            m_SpriteAtlas = AssetDatabase.LoadAllAssetsAtPath(path);
            if (m_SpriteAtlas != null)
            {
                if (m_SpriteAtlas.Length > 2)
                {
                    int spriteCount = 0; //统计Sprite数量
                    for (int i = 0; i < m_SpriteAtlas.Length; i++)
                    {
                        var asset = m_SpriteAtlas[i];
                        var type = asset.GetType();
                        if (type == typeof(Sprite))
                        {
                            if (++spriteCount > 1)
                            {
                                m_SeekAssetIsAtlas = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    void ShowItemScrollView() //在下拉列表中显示所有查询结果
    {
        if (m_AllSeekPrefabItem.Count == 0)
        {
            return;
        }

        GUILayout.BeginHorizontal();

        if (m_ActiveInspectType == 0) //选中 Prefab标签
        {
            //所有PrefabItem
            GUILayout.BeginVertical();
            GUILayout.Label("【Depend Prefabs】 Count-" + m_AllSeekPrefabItem.Count);
            m_ListScrollPosPrefab1 = EditorGUILayout.BeginScrollView(m_ListScrollPosPrefab1, GUILayout.Width(position.width * 0.25f));
            for (int i = 0; i < m_AllSeekPrefabItem.Count; i++)
            {
                var item = m_AllSeekPrefabItem[i];

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(item.prefab.name, GUILayout.Width(150), GUILayout.Height(30)))
                {
                    Selection.activeObject = item.prefab;
                }

                if (GUILayout.Button("OpenPrefab", GUILayout.Width(80), GUILayout.Height(30)))
                {
                    //打开Prefab编辑器窗口
                    AssetDatabase.OpenAsset(item.prefab);
                    //显示当前Open的Prefab的所有依赖游戏物体路径
                    m_PrefabViewDependObjs = item.dependObjsHierarchy;
                    m_PrefabViewDependObjsTitle = item.prefab.name;
                    m_OperationTips.Clear();
                    m_OperationTips.Add("成功打开Prefab！！！请在下拉列表右侧查看所有引用资源的物体");
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.color = Color.black;
            GUILayout.Button(string.Empty, GUILayout.Width(2), GUILayout.Height(position.height - 145)); //分割线
            GUI.color = m_DefaultColor;

            //点击OpenPrefab 显示所有引用了查询资源文件的GameObject位置
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format("【Prefab-{0}】 Count-{1}", m_PrefabViewDependObjsTitle, m_PrefabViewDependObjs.Count));
            m_ListScrollPosPrefab2 = EditorGUILayout.BeginScrollView(m_ListScrollPosPrefab2, GUILayout.Width(position.width * 0.75f - 10));
            for (int i = 0; i < m_PrefabViewDependObjs.Count; i++)
            {
                var dependObjPath = m_PrefabViewDependObjs[i];
                GUILayout.TextField(dependObjPath);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        else if (m_ActiveInspectType == 1) //选中 SpriteAtlas标签
        {
            //所有SpriteItem
            GUILayout.BeginVertical();
            GUILayout.Label("【All Sprite】 Count-" + m_AllSeekSpriteItem.Count);
            m_ListScrollPosSprite1 = EditorGUILayout.BeginScrollView(m_ListScrollPosSprite1, GUILayout.Width(position.width * 0.25f));
            foreach (var item in m_AllSeekSpriteItem.Values)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(item.sprite.name, GUILayout.Width(130), GUILayout.Height(30)))
                {
                    Selection.activeObject = item.sprite;
                }

                string btn = "DepandPrefab";
                if (item.dependPrefabs.Count == 0)
                {
                    btn = "None";
                    GUI.color = Color.red;
                }
                if (GUILayout.Button(btn, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    m_SpriteViewDependPrefabs = item.dependPrefabs; //显示当前点击Sprite的引用Prefab列表
                    m_SpriteViewDependPfbsTitle = item.sprite.name;
                    m_SpriteViewDependPfbs = m_EmptyStringList;
                    m_SpriteViewDependPrefabsTitle = string.Empty;
                }
                GUI.color = m_DefaultColor;

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.color = Color.black;
            GUILayout.Button(string.Empty, GUILayout.Width(2), GUILayout.Height(position.height - 145)); //分割线
            GUI.color = m_DefaultColor;

            //选中SpriteItem的 所有引用的Prefab
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format("【Sprite-{0}】 Count-{1}", m_SpriteViewDependPfbsTitle, m_SpriteViewDependPrefabs.Count));
            m_ListScrollPosSprite2 = EditorGUILayout.BeginScrollView(m_ListScrollPosSprite2, GUILayout.Width(position.width * 0.25f));
            foreach (var item in m_SpriteViewDependPrefabs.Values)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(item.prefab.name, GUILayout.Width(150), GUILayout.Height(30)))
                {
                    Selection.activeObject = item.prefab;
                }

                if (GUILayout.Button("OpenPrefab", GUILayout.Width(80), GUILayout.Height(30)))
                {
                    //打开Prefab编辑器窗口
                    AssetDatabase.OpenAsset(item.prefab);
                    //显示当前Open的Prefab的所有依赖游戏物体路径
                    m_SpriteViewDependPfbs = item.dependObjsHierarchy;
                    m_SpriteViewDependPrefabsTitle = item.prefab.name;
                    m_OperationTips.Clear();
                    m_OperationTips.Add("成功打开Prefab！！！请在下拉列表右侧查看所有引用资源的物体");
                }

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUI.color = Color.black;
            GUILayout.Button(string.Empty, GUILayout.Width(2), GUILayout.Height(position.height - 145)); //分割线
            GUI.color = m_DefaultColor;

            //选中SpriteItem的 打开Prefab的 所有引用的物体
            GUILayout.BeginVertical();
            GUILayout.Label(string.Format("【Prefab-{0}】 Count-{1}", m_SpriteViewDependPrefabsTitle, m_SpriteViewDependPfbs.Count));
            m_ListScrollPosSprite3 = EditorGUILayout.BeginScrollView(m_ListScrollPosSprite3, GUILayout.Width(position.width * 0.5f - 20));
            for (int i = 0; i < m_SpriteViewDependPfbs.Count; i++)
            {
                var dependObjPath = m_SpriteViewDependPfbs[i];
                GUILayout.TextField(dependObjPath);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
    }

    void OnSeekAllPrefabInDirectory() //查询指定文件夹路径下的所有Prefab
    {
        m_OperationTips.Clear();

        //检查参数正确性
        if (m_SeekAssetPath.Equals(string.Empty))
        {
            m_TipsColor = Color.red;
            m_OperationTips.Add("SeekAsset-查询资源不能为空!!!");
            return;
        }

        ClearAllSeekAnswerData(); //清空上次搜索结果

        //获取图集文件的Sprite列表
        if (m_SeekAssetIsAtlas)
        {
            if (!GetSpriteAtlasItems())
            {
                return;
            }
        }

        var allFiles = AssetDatabase.GetAllAssetPaths(); //获取Asset下所有文件

        //查询Asset文件夹下所有Material 遍历检查是否引用了查询资源
        List<string> materialGUIDs = new List<string>();
        for (int i = 0; i < allFiles.Length; i++)
        {
            string filePath = allFiles[i];

            if (i % 200 == 0)
            {
                EditorUtility.DisplayProgressBar("Seek Material Files", filePath, (float)(i) / allFiles.Length);
            }

            if (!filePath.EndsWith(".mat"))
            {
                continue;
            }

            string fileText = File.ReadAllText(filePath);
            if (fileText.Contains(m_SeekAssetGUID))
            {
                string guid = AssetDatabase.AssetPathToGUID(filePath);
                if (!materialGUIDs.Contains(guid))
                {
                    materialGUIDs.Add(guid);
                }
            }
        }

        //查询Asset文件夹下所有Prefab 遍历检查是否引用了查询资源
        for (int i = 0; i < allFiles.Length; i++)
        {
            string filePath = allFiles[i];

            if (i % 200 == 0)
            {
                EditorUtility.DisplayProgressBar("Seek Prefab Files", filePath, (float)(i) / allFiles.Length);
            }
                
            if(!filePath.EndsWith(".prefab"))
            {
                continue;
            }

            bool isDepend = false;
            string fileText = File.ReadAllText(filePath);
            if (fileText.Contains(m_SeekAssetGUID)) //Prefab直接引用
            {
                isDepend = true;
            }
            else if (materialGUIDs.Count > 0) //Prefab引用Material
            {
                for (int j = 0; j < materialGUIDs.Count; j++)
                {
                    string guid = materialGUIDs[j];
                    if (fileText.Contains(guid))
                    {
                        isDepend = true;
                        break;
                    }
                }
            }

            if (isDepend)
            {
                PrefabItem prefabItem = new PrefabItem();
                prefabItem.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                m_AllSeekPrefabItem.Add(prefabItem);
            }
        }

        //遍历所有Prefab 检查Prefab内的引用了资源文件的GameObject
        for (int i = 0; i < m_AllSeekPrefabItem.Count; i++)
        {
            var prefabItem = m_AllSeekPrefabItem[i];

            EditorUtility.DisplayProgressBar("Seek depend GameObject", prefabItem.prefab.name, (float)i / m_AllSeekPrefabItem.Count);

            //遍历所有自定义脚本组件
            var allComp = GetAllComponent<MonoBehaviour>(prefabItem.prefab.transform);
            for (int j = 0; j < allComp.Count; j++)
            {
                var comp = allComp[j];
                var compType = comp.GetType();

                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance; // only public non-static fields are bound to by Unity.
                FieldInfo[] fields = compType.GetFields(flags);
                for (int k = 0; k < fields.Length; k++)
                {
                    var field = fields[k];
                    Type fieldType = field.FieldType;
                    if (fieldType == typeof(Sprite))
                    {
                        Sprite tSprite = field.GetValue(comp) as Sprite;
                        if (tSprite != null)
                        {
                            if (tSprite.texture.name.Equals(m_SeekAssetName))
                            {
                                AddDependObjInPrefabItem(comp.gameObject, prefabItem); //记录Prefab内引用的物体
                            }
                        }

                        //查询文件为图集时
                        if (m_SeekAssetIsAtlas)
                        {
                            if (tSprite != null)
                            {
                                if (tSprite.texture != null)
                                {
                                    if (tSprite.texture.name.Equals(m_SeekAssetName))
                                    {
                                        if (m_AllSeekSpriteItem.ContainsKey(tSprite.name))
                                        {
                                            //往SpriteItem中添加PrefabItem数据
                                            if (!m_AllSeekSpriteItem[tSprite.name].dependPrefabs.ContainsKey(prefabItem.prefab))
                                            {
                                                PrefabItem item = new PrefabItem();
                                                item.prefab = prefabItem.prefab;
                                                m_AllSeekSpriteItem[tSprite.name].dependPrefabs.Add(item.prefab, item);
                                            }
                                            //记录 引用物体Hierarchy
                                            m_AllSeekSpriteItem[tSprite.name].dependPrefabs[prefabItem.prefab].dependObjsHierarchy.Add(GetAllDependObjHierarchy(comp.transform));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (fieldType == typeof(Texture))
                    {
                        Texture tTexture = field.GetValue(comp) as Texture;
                        if (tTexture != null)
                        {
                            if (tTexture.name.Equals(m_SeekAssetName))
                            {
                                AddDependObjInPrefabItem(comp.gameObject, prefabItem);
                            }
                        }
                    }
                    if (fieldType == typeof(Material))
                    {
                        Material tMaterial = field.GetValue(comp) as Material;
                        if (tMaterial != null)
                        {
                            if (tMaterial.mainTexture != null)
                            {
                                if (tMaterial.mainTexture.name.Equals(m_SeekAssetName))
                                {
                                    AddDependObjInPrefabItem(comp.gameObject, prefabItem);
                                }
                            }
                        }
                    }
                    if (fieldType == typeof(Mesh))
                    {
                        Mesh tMesh = field.GetValue(comp) as Mesh;
                        if (tMesh != null)
                        {
                            if (tMesh.name.Equals(m_SeekAssetName))
                            {
                                AddDependObjInPrefabItem(comp.gameObject, prefabItem);
                            }
                        }
                    }
                }
            }

            //遍历所有Graphic组件
            var allGrap = GetAllComponent<Graphic>(prefabItem.prefab.transform);
            for (int j = 0; j < allGrap.Count; j++)
            {
                var graphic = allGrap[j];

                //纹理贴图 名字一致
                if (graphic.mainTexture != null)
                {
                    if (graphic.mainTexture.name.Equals(m_SeekAssetName))
                    {
                        //查询文件为图集时
                        if(m_SeekAssetIsAtlas)
                        {
                            if (graphic.GetType() == typeof(Image))
                            {
                                var image = graphic as Image;
                                if (m_AllSeekSpriteItem.ContainsKey(image.sprite.name))
                                {
                                    //往SpriteItem中添加PrefabItem数据
                                    if (!m_AllSeekSpriteItem[image.sprite.name].dependPrefabs.ContainsKey(prefabItem.prefab))
                                    {
                                        PrefabItem item = new PrefabItem();
                                        item.prefab = prefabItem.prefab;
                                        m_AllSeekSpriteItem[image.sprite.name].dependPrefabs.Add(item.prefab, item);
                                    }
                                    //记录 引用物体Hierarchy
                                    m_AllSeekSpriteItem[image.sprite.name].dependPrefabs[prefabItem.prefab].dependObjsHierarchy.Add(GetAllDependObjHierarchy(graphic.transform));
                                }
                            }
                        }

                        AddDependObjInPrefabItem(graphic.gameObject, prefabItem); //记录Prefab内引用的物体
                    }
                }
                //材质球
                if (graphic.materialForRendering)
                {
                    Material material = graphic.material;
                    if (material != null)
                    {
                        if (material.mainTexture != null)
                        {
                            if (material.mainTexture.name.Equals(m_SeekAssetName))
                            {
                                AddDependObjInPrefabItem(graphic.gameObject, prefabItem); //记录Prefab内引用的物体
                            }
                        }
                    }
                }

                //Missing的引用
                var sObj = new SerializedObject(graphic);
                var iter = sObj.GetIterator();
                while (iter.NextVisible(true))
                {
                    //如果这个属性类型是引用类型的
                    if (iter.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        //引用对象是null 并且 引用ID不是0 说明丢失了引用
                        if (iter.objectReferenceValue == null && iter.objectReferenceInstanceIDValue != 0)
                        {
                            if (iter.displayName.Equals("Sprite"))
                            {
                                AddDependObjInPrefabItem(graphic.gameObject, prefabItem, true); //记录Prefab内引用的物体
                                break;
                            }
                        }
                    }
                }
            }

            if (prefabItem.dependObjs.Count == 0)
            {
                prefabItem.dependObjsHierarchy.Add("定位所有引用物体功能 暂不支持 当前指定的查询资源文件的类型");
            }
        }
        EditorUtility.ClearProgressBar();

        //提示信息
        m_TipsColor = Color.green;
        m_OperationTips.Add("查询完成!!!请在下拉列表总查看查询结果");
        m_OperationTips.Add("点击[Open Prefab]后，下拉窗右侧会显示所有引用资源的物体");
    }

    bool GetSpriteAtlasItems() //获取当前图集文件 所有Sprite的信息
    {
        bool succeed = true;

        if (!m_SeekAssetIsAtlas)
            return succeed;

        for (int i = 1; i < m_SpriteAtlas.Length; i++)
        {
            var sprite = m_SpriteAtlas[i] as Sprite;
            if (sprite != null)
            {
                var spriteItem = new SpriteItem();
                spriteItem.sprite = sprite;
                spriteItem.name = sprite.name;
                spriteItem.image = sprite.associatedAlphaSplitTexture;

                if (m_AllSeekSpriteItem.ContainsKey(spriteItem.name))
                {
                    succeed = false;
                    m_TipsColor = Color.red;
                    m_OperationTips.Add("查询失败!!!图集中存在同名Sprite!!!请修改Sprite名称后重试!!! Sprite-" + spriteItem.name);
                }
                else
                {
                    m_AllSeekSpriteItem.Add(spriteItem.name, spriteItem);
                }
            }
        }

        return succeed;
    }

    void AddDependObjInPrefabItem(GameObject obj, PrefabItem prefabItem, bool isMissing = false) //添加Prefab内 引用资源的物体 与层级路径
    {
        if (!prefabItem.dependObjs.Contains(obj))
        {
            prefabItem.dependObjs.Add(obj); //记录GameObj

            if (isMissing) //是否为Missing的引用
            {
                prefabItem.dependObjsHierarchy.Add("[Missing]-" + GetAllDependObjHierarchy(obj.transform)); //记录层级路径
            }
            else
            {
                prefabItem.dependObjsHierarchy.Add(GetAllDependObjHierarchy(obj.transform));
            }
        }
    }

    List<T> GetAllComponent<T>(Transform root) //递归获取Prefab内的所有组件
    {
        List<Transform> allTrans = new List<Transform>();
        GetAllTransform(root, allTrans);

        List<T> allComp = new List<T>();
        for (int i = 0; i < allTrans.Count; i++)
        {
            var comps = allTrans[i].GetComponents<T>();
            for (int j = 0; j < comps.Length; j++)
            {
                allComp.Add(comps[j]);
            }
        }

        return allComp;
    }

    void GetAllTransform(Transform root, List<Transform> allTrans)
    {
        if (root.childCount > 0)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                GetAllTransform(root.GetChild(i), allTrans);
            }
        }

        allTrans.Add(root);
    }

    string GetAllDependObjHierarchy(Transform child)
    {
        StringBuilder sb = new StringBuilder();
        Transform nowTrans = child;
        sb.Append(nowTrans.gameObject.name);

        while (nowTrans.parent != null)
        {
            nowTrans = nowTrans.parent;
            sb.Insert(0, nowTrans.gameObject.name+">");
        }

        return sb.ToString();
    }

    void ClearAllSeekAnswerData() //清空 所有搜索结果
    {
        //Prefab 搜索结果
        m_AllSeekPrefabItem.Clear(); //查询结果 所有Prefab
        m_PrefabViewDependObjsTitle = string.Empty; //选中的Prefab名
        m_PrefabViewDependObjs.Clear(); //查询结果 当前打开的Prefab 所有引用的物体路径

        //Sprite 搜索结果
        m_AllSeekSpriteItem.Clear(); //图集中所有的Sprite 查询结果
        m_SpriteViewDependPrefabsTitle = string.Empty; //选中的Sprite名
        m_SpriteViewDependPrefabs.Clear(); //查询结果 所有引用的Prefab
        m_SpriteViewDependPfbsTitle = string.Empty; //选中的Prefab名
        m_SpriteViewDependPfbs.Clear(); //查询结果 当前打开的Prefab 所有引用的物体路径
    }
}
