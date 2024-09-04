using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FsStoryIncident
{
    public class StoryIncidentEditorLibrary
    {
        public static string Name { get; private set; } = "StoryIncidentEditorLibrary";

        private static string m_RootPath;
        /// <summary>
        /// FsStoryIncident文件夹根节点
        /// </summary>
        public static string RootPath
        {
            get
            {
                if(m_RootPath == null)
                {
                    m_RootPath = GetAssetsPathBySelfFolder(Name).Replace(("/Source/Editor"), "");
                }

                return m_RootPath;
            }
        }

        private static string m_EditorConfigPath;
        /// <summary>
        /// 编辑器用配置存储文件夹
        /// </summary>
        public static string EditorConfigPath
        {
            get
            {
                if(m_EditorConfigPath == null)
                {
                    m_EditorConfigPath = RootPath + "/Source/Editor/Config";
                }

                //确认文件夹是否存在，否则创建
                if (!Directory.Exists(m_EditorConfigPath))
                    Directory.CreateDirectory(m_EditorConfigPath);

                return m_EditorConfigPath;
            }
        }

        /// <summary>
        /// 获取自身插件文件夹下某个名称资源的路径
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getDirectoryName">获取文件夹路径，false时获取到文件路径</param>
        /// <returns></returns>
        public static string GetAssetsPathBySelfFolder(string name, bool getDirectoryName = true)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            string[] assetGUIDs = AssetDatabase.FindAssets(name);
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                if (path.Contains("/FsStoryIncident/"))
                {
                    if (getDirectoryName)
                        return Path.GetDirectoryName(path).Replace('\\','/');
                    else
                        return path;
                }
            }

            return string.Empty;
        }

        public static T[] GetAssets<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();

            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{typeof(T)}");
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                if (string.IsNullOrEmpty(path)) continue;
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if(asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets.ToArray();
        }

        /// <summary>
        /// 获取资源所在文件夹路径
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetFolderPath(UnityEngine.Object obj)
        {
            if (obj == null) return string.Empty;
            return Path.GetDirectoryName(AssetDatabase.GetAssetPath(obj));
        }

        /// <summary>
        /// 获取资源GUID
        /// 必须是持久化的资源，运行时资源没有有效的GUID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GUID GetGUIDFromAsset(UnityEngine.Object obj)
        {
            if (obj == null) return new GUID();
            string path = AssetDatabase.GetAssetPath(obj);
            return AssetDatabase.GUIDFromAssetPath(path);
        }

        /// <summary>
        /// 创建一个ScriptableObject类资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameBase">基础名称，重名时会自动添加序号后缀</param>
        /// <param name="folderPath">存储文件夹名称</param>
        /// <returns></returns>
        public static T CreateAsset_ScriptableObject<T>(string nameBase, string folderPath) where T : ScriptableObject
        {
            T config = ScriptableObject.CreateInstance<T>();
            string savePath = CreateSavePath_Asset(nameBase, folderPath);
            //防止重名文件覆盖
            if (System.IO.File.Exists(savePath))
            {
                int index = 1;
                string savePathTemp = CreateSavePath_Asset(nameBase, folderPath, index.ToString());

                while (System.IO.File.Exists(savePathTemp))
                {
                    index++;
                    savePathTemp = CreateSavePath_Asset(nameBase, folderPath, index.ToString());
                }

                savePath = savePathTemp;
            }

            AssetDatabase.CreateAsset(config, savePath);
            return AssetDatabase.LoadMainAssetAtPath(savePath) as T;
        }

        private static string CreateSavePath_Asset(string name, string folderPath, string suffix = "")
        {
            if(string.IsNullOrEmpty(suffix))
                return string.Format("{0}/{1}.asset", folderPath, name);
            else
                return string.Format("{0}/{1}_{2}.asset", folderPath, name, suffix);
        }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">全路径</param>
        /// <param name="createOnNotFound">未找到时自动创建</param>
        /// <returns></returns>
        public static T GetConfigDefault<T>(string path, bool createOnNotFound = true) where T : ScriptableObject
        {
            T config = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!config && createOnNotFound)
            {
                config = ScriptableObject.CreateInstance(typeof(T)) as T;
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
            }

            return config;
        }

        public static void CreateSampleSprite(Texture2D texture2D, string folderPath, string name)
        {
            //保存图片
            byte[] dataBytes = texture2D.EncodeToPNG();
            string savePath = $"path/{name}.png";
            FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);
            fileStream.Write(dataBytes, 0, dataBytes.Length);
            fileStream.Close();
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        /// <summary>
        /// 获取所有某种类型
        /// </summary>
        /// <param name="typeBase"></param>
        /// <returns></returns>
        public static System.Type[] GetTypes(Type typeBase)
        {
            System.Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.IsSubclassOf(typeBase) || t.GetInterfaces().Contains(typeBase)))
                        .ToArray();

            //获取的类型可以通过Activator.CreateInstance(type)方法实例化

            return types;
        }

        /// <summary>
        /// 获取所有某种类型
        /// </summary>
        /// <param name="typeBase"></param>
        /// <returns></returns>
        public static void GetTypes(Type typeBase, out System.Type[] types, out string[] typeStrs, string insertHeadStr = null)
        {
            types = GetTypes(typeBase);
            typeStrs = GetTypeStrs(typeBase, insertHeadStr);
        }

        /// <summary>
        /// 获取所有某种类型
        /// </summary>
        /// <param name="typeBase"></param>
        /// <returns></returns>
        public static string[] GetTypeStrs(Type typeBase, string insertHeadStr = null)
        {
            var types = GetTypes(typeBase);

            if (types == null || types.Length == 0) return new string[0];

            string[] typeStrs;
            if (string.IsNullOrEmpty(insertHeadStr))
            {
                typeStrs = new string[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    typeStrs[i] = types[i].ToString();
                }
            }
            else
            {
                typeStrs = new string[types.Length + 1];
                typeStrs[0] = insertHeadStr;
                for (int i = 0; i < types.Length; i++)
                {
                    typeStrs[i + 1] = types[i].ToString();
                }
            }

            return typeStrs;
        }
    }
}