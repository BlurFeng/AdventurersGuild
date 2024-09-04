using UnityEngine;
using UnityEditor;
using System.IO;

namespace FsGameFramework
{
    public delegate bool CreatePrefabGameobjectProcessor(GameObject obj);

    public class EditorUtility
    {
        public static void CreatePrefab(CreatePrefabGameobjectProcessor processorMethod, string prefabName)
        {
            //获取当前选中对象的路径
            string folderPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if (string.IsNullOrEmpty(folderPath)) return;

            //截取到文件夹路径
            if (!AssetDatabase.IsValidFolder(folderPath))
                folderPath = Path.GetDirectoryName(folderPath);

            //不检查浪费时间啦 这个方法也就程序员自己会用到 取名字的时候麻烦注意一下！
            ////检查prefabName合法性
            //if (prefabName.Contains("/"))
            //    prefabName.Replace("/", "");

            //设置资源存储路径
            string savePath = folderPath + string.Format("/{0}.Prefab", prefabName);
            //防止重名文件覆盖
            if (System.IO.File.Exists(savePath))
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
            
            //对新建的GameObject进行处理
            processorMethod(obj);

            //存储预制体到目标路径
            var objOut = PrefabUtility.SaveAsPrefabAsset(obj, savePath);

            //删除操作的GameObject对象 即Scene窗口中的GameObject
            Editor.DestroyImmediate(obj);

            //选中创建的预制体 DOTO：20210506 选中了但不是重命名的模式 需要查找怎么实现
            Selection.activeObject = objOut;
        }
    }
}