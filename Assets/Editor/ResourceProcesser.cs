using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ResourceProcesser
{
    #region 设置像素纹理贴图的Inspector面板属性
    static private TextureImporterType m_TextureImporterType;
    static private FilterMode m_FilterMode;
    static private TextureImporterCompression m_TextureImporterCompression;
    static private bool m_GenerateMipmaps;
    static private bool m_ReadWriteEnabled;

    [MenuItem("Assets/ResourceProcesser/SetPixelTextureSprite(设置像素贴图-精灵)")]
    public static void SetPixelTextureSprite()
    {
        m_TextureImporterType = TextureImporterType.Sprite;
        m_FilterMode = FilterMode.Point;
        m_TextureImporterCompression = TextureImporterCompression.CompressedHQ;
        m_GenerateMipmaps = false;
        m_ReadWriteEnabled = false;

        SetPixelTextureForeachSelection();
    }

    [MenuItem("Assets/ResourceProcesser/SetPixelTextureSpriteSpineSkin(设置像素贴图-精灵-Spine皮肤)")]
    public static void SetPixelTextureSpriteSpineSkin()
    {
        m_TextureImporterType = TextureImporterType.Sprite;
        m_FilterMode = FilterMode.Point;
        m_TextureImporterCompression = TextureImporterCompression.CompressedHQ;
        m_GenerateMipmaps = false;
        m_ReadWriteEnabled = true;

        SetPixelTextureForeachSelection();
    }

    //获取所有选择的文件
    public static void SetPixelTextureForeachSelection()
    {
        //获取项目路径长度 用于剔除Assets文件夹之前的路径
        m_DataPathLength = Application.dataPath.Length;

        var objects = Selection.objects;
        try
        {
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                string path = AssetDatabase.GetAssetPath(obj);

                //进度条
                EditorUtility.DisplayProgressBar("生成场景物体的预制体", path, (float)i / objects.Length);

                if (Directory.Exists(path))
                {
                    ProcessPixelTextureInspectorDir(path);
                }
                else
                {
                    ProcessPixelTextureInspectorFile(path);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    //处理 目录
    private static void ProcessPixelTextureInspectorDir(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        //处理目录下的所有文件
        var filesInfo = dirInfo.GetFiles();
        for (int i = 0; i < filesInfo.Length; i++)
        {
            var fileInfo = filesInfo[i];
            var filePath = "Assets" + fileInfo.FullName.Remove(0, m_DataPathLength);
            ProcessPixelTextureInspectorFile(filePath);
        }

        //处理所有子目录
        var subDirInfos = dirInfo.GetDirectories();
        for (int i = 0; i < subDirInfos.Length; i++)
        {
            var subDirInfo = subDirInfos[i];
            ProcessPixelTextureInspectorDir(subDirInfo.FullName);
        }
    }

    //处理 文件
    private static void ProcessPixelTextureInspectorFile(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { return; }

        importer.textureType = m_TextureImporterType;
        importer.filterMode = m_FilterMode;
        importer.textureCompression = m_TextureImporterCompression;
        importer.mipmapEnabled = m_GenerateMipmaps;
        importer.isReadable = m_ReadWriteEnabled;

        importer.SaveAndReimport();
    }
    #endregion

    #region 创建场景物体的预制体
    private static string m_FolderSrcFurnitureSprite  = "Assets/ProductAssets/Texture/GridItem/Furniture/"; //源文件夹 家具的贴图
    private static string m_FolderDestFurniturePrefab = "Assets/ProductAssets/Prefab/GridItem/Furniture/"; //目标文件夹 家具的预制体
    private static int m_DataPathLength; //项目路径长度

    [MenuItem("Tools/ResourceProcesser/CreatePrefabFurniture(创建家具的预制体)")]
    public static void CreatePrefabSceneObject()
    {
        //源文件夹 场景物体的贴图 获取目录
        DirectoryInfo dirSrcSceneObjectSprite = new DirectoryInfo(m_FolderSrcFurnitureSprite);
        if (dirSrcSceneObjectSprite == null)
            Debug.LogError($"ResourceProcesser.CreatePrefabSceneObject() Error! >> 源文件夹不存在-{m_FolderSrcFurnitureSprite}");

        //目标文件夹 场景物体的预制体 若无文件夹则创建
        if (!Directory.Exists(m_FolderDestFurniturePrefab))
            Directory.CreateDirectory(m_FolderDestFurniturePrefab);

        //获取项目路径长度 用于剔除Assets文件夹之前的路径
        m_DataPathLength = Application.dataPath.Length;

        //开始处理
        try
        {
            //处理所有子目录
            var subDirInfos = dirSrcSceneObjectSprite.GetDirectories();
            for (int i = 0; i < subDirInfos.Length; i++)
            {
                var subDirInfo = subDirInfos[i];
                ProcessPrefabSceneObjectDir(subDirInfo);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        //刷新编辑器资源
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    //处理 目录
    private static void ProcessPrefabSceneObjectDir(DirectoryInfo dirInfo)
    {
        //创建文件夹
        string dirSrcPath = "Assets" + dirInfo.FullName.Remove(0, m_DataPathLength); //源文件夹
        dirSrcPath = dirSrcPath.Replace("\\", "/");
        string dirDestPath = dirSrcPath.Replace(m_FolderSrcFurnitureSprite, m_FolderDestFurniturePrefab); //目标文件夹
        if (!Directory.Exists(dirDestPath))
            Directory.CreateDirectory(dirDestPath);

        //处理目录下的所有文件
        var filesInfo = dirInfo.GetFiles();
        for (int i = 0; i < filesInfo.Length; i++)
        {
            var fileInfo = filesInfo[i];
            var filePath = fileInfo.FullName;

            //进度条
            EditorUtility.DisplayProgressBar("生成场景物体的预制体", filePath, (float)i / filesInfo.Length);

            //过滤文件类型
            if (!filePath.EndsWith(".png")) continue;

            //加载纹理
            var assetPath = Path.Combine(dirSrcPath, fileInfo.Name);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
            {
                Debug.LogError($"ResourceProcesser.ProcessPrefabSceneObjectDir() Error! >> 文件加载Sprite失败-{assetPath}");
                continue;
            }

            //预制体已存在 不覆盖
            var prefabPath = Path.Combine(dirDestPath, fileInfo.Name);
            prefabPath = prefabPath.Replace(".png", ".prefab");
            if (File.Exists(prefabPath)) return;

            //创建预制体
            GameObject prefab = new GameObject(sprite.name);
            //添加Box碰撞器
            var boxCollider = prefab.AddComponent<BoxCollider>();
            float sizeX = sprite.texture.width * 0.01f; //计算纹理尺寸
            float sizeY = sprite.texture.height * 0.01f;
            boxCollider.size = new Vector3(sizeX, sizeY * 0.2f, sizeY); //设置Size
            boxCollider.center = new Vector3(0f, sizeY * 0.1f - sizeY * 0.5f, sizeY * 0.5f); //设置Center 紧贴底部 不超过Z轴0位置
            //添加子物体 SpriteRender
            GameObject subGo = new GameObject("SpriteRender");
            var sr = subGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            var trans = sr.transform;
            trans.SetParent(prefab.transform);
            //保存预制体至 目标文件夹
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            GameObject.DestroyImmediate(prefab);
        }

        //处理所有子目录
        var subDirInfos = dirInfo.GetDirectories();
        for (int i = 0; i < subDirInfos.Length; i++)
        {
            var subDirInfo = subDirInfos[i];
            ProcessPrefabSceneObjectDir(subDirInfo);
        }
    }
    #endregion
}

public class AssetProcesser : AssetPostprocessor
{
    public void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;

        //Spine角色皮肤图片
        if (assetPath.StartsWith("Assets/ProductAssets/Texture/Character/"))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = false;
            importer.isReadable = true;
        }
        //精灵图片
        else if 
            (
            assetPath.StartsWith("Assets/ProductAssets/Texture/")||
            assetPath.StartsWith("Assets/TileMap/")
            )
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
        }
    }
}
