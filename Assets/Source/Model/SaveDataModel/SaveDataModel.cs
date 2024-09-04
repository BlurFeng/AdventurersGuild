using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using Deploy;
using com.ootii.Messages;
using Google.Protobuf;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 存档模块
/// </summary>
public class SaveDataModel : Singleton<SaveDataModel>, IDestroy
{
    /// <summary>
    /// 存档信息
    /// </summary>
    [Serializable]
    public class SaveDataInfo
    {
        /// <summary>
        /// 存档序号
        /// </summary>
        public int Num;
        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName;
        /// <summary>
        /// 游戏纪年日期
        /// </summary>
        public string GameTimeDate;
        /// <summary>
        /// 游玩时间 秒
        /// </summary>
        public uint PlayTimeSeconds;
    }

    /// <summary>
    /// 存档数据
    /// </summary>
    public ES3File SaveDataCur
    {
        get
        {
            if (m_SaveDataCur == null)
                m_SaveDataCur = new ES3File();

            return m_SaveDataCur;
        }
    }
    private ES3File m_SaveDataCur; //当前正在使用的存档数据
    private int m_SaveDataNumCur = -1; //当前存档的序号

    /// <summary>
    /// 存档信息
    /// </summary>
    public Dictionary<int, SaveDataInfo> DicSaveDataInfo { get { return m_DicSaveDataInfo; } }
    private Dictionary<int, SaveDataInfo> m_DicSaveDataInfo = new Dictionary<int, SaveDataInfo>(); //字典 存档下标:存档信息

    private static string m_SaveDataInfoFileName = "SaveDataInfo.bytes"; //文件名 格式
    private static string m_SaveDataFileNameFormat = "SaveData_{0}.data"; //文件名 格式
    private static string m_SaveDatasDirRelativePath = "Able Games/Ablegaea - Guild of Otherworld/SaveDatas"; //文件夹 相对路径
    private static string m_SaveDatasDirPath; //存档文件夹

    public override void Init()
    {
        base.Init();

        //获取 存档文件夹
        m_SaveDatasDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), m_SaveDatasDirRelativePath);
        if (!Directory.Exists(m_SaveDatasDirPath))
        {
            Directory.CreateDirectory(m_SaveDatasDirPath);
        }

        //加载 存档信息文件
        LoadSaveDataInfo();
    }

    /// <summary>
    /// 加载 存档数据 当前
    /// </summary>
    public void LoadSaveDataCur(int num)
    {
        string fileName = string.Format(m_SaveDataFileNameFormat, num);
        string filePath = Path.Combine(m_SaveDatasDirPath, fileName);

        if (File.Exists(filePath))
        {
            m_SaveDataCur = new ES3File(filePath);
            m_SaveDataNumCur = num;
            ReloadModelData();
        }

        //if (File.Exists(filePath))
        //{
        //    using (FileStream fs = new FileStream(filePath, FileMode.Open))
        //    {
        //        byte[] savaDataBytes = new byte[fs.Length];
        //        fs.Read(savaDataBytes, 0, (int)fs.Length);
        //        m_SaveDataCur = Save_Data.Parser.ParseFrom(savaDataBytes);
        //        m_SaveDataNumCur = num;
        //        ReloadModelData();
        //    }
        //}
    }

    /// <summary>
    /// 保存 存档数据 当前
    /// </summary>
    public void SaveSaveDataCur(int num = -1)
    {
        //存档序号
        if (num == -1)
        {
            if (m_SaveDataNumCur == -1)
            {
                //检查当前已有存档 使用下一序号
                for (int i = 1; i <= 3; i++)
                {
                    num = i;
                    string filePathCheck = Path.Combine(m_SaveDatasDirPath, string.Format(m_SaveDataFileNameFormat, num));
                    if (!File.Exists(filePathCheck)) { break; }
                }

                m_SaveDataNumCur = num;
            }
            else
            {
                //当前载入的正在使用的存档序号
                num = m_SaveDataNumCur;
            }
        }

        ES3File saveData = new ES3File();

        //遍历执行Model接口
        var listSingleton = SingletonModel.GetSingletons<ISaveData>();
        for (int i = 0; i < listSingleton.Count; i++)
        {
            var singleton = listSingleton[i];
            singleton.SaveData(saveData);
        }

        string fileName = string.Format(m_SaveDataFileNameFormat, num);
        string filePath = Path.Combine(m_SaveDatasDirPath, fileName);

        saveData.Sync(filePath);

        //using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
        //{
        //    MessageExtensions.WriteTo(saveData, fs);
        //}

        //更新 存档列表信息
        SaveSaveDataListInfo(num);
    }

    /// <summary>
    /// 删除 存档文件
    /// </summary>
    /// <param name="num"></param>
    public void DeleteSaveData(int num)
    {
        string fileName = string.Format(m_SaveDataFileNameFormat, num);
        string filePath = Path.Combine(m_SaveDatasDirPath, fileName);

        //删除 存档文件
        ES3.DeleteFile(filePath);

        //更新 存档列表信息
        m_DicSaveDataInfo.Remove(num);
        SaveSaveDataListInfo();
    }

    /// <summary>
    /// 清除 存档数据
    /// </summary>
    public void ClearData()
    {
        m_SaveDataCur = null;
        m_SaveDataNumCur = -1;
        ReloadModelData();
    }

    //重新加载模块数据
    private void ReloadModelData()
    {
        //获取所有实现了SaveData的Model并移除
        var listSingleton = SingletonModel.GetSingletons<ISaveData>();
        for (int i = 0; i < listSingleton.Count; i++)
        {
            var singleton = listSingleton[i];
            SingletonModel.ClearSingleton(singleton);
        }

        //需要加载数据的Model
        TimeModel.Instance.Create();
        EntrustModel.Instance.Create();
        PathfindingModel.Instance.Create();
    }

    //加载 存档信息
    private void LoadSaveDataInfo()
    {
        string filePath = Path.Combine(m_SaveDatasDirPath, m_SaveDataInfoFileName);
        var dicSaveDataInfo = PlayerPrefsUtil.LoadDataFilePath<Dictionary<int, SaveDataInfo>>(filePath);
        if (dicSaveDataInfo != null)
        {
            m_DicSaveDataInfo = dicSaveDataInfo;
            //检查存档文件是否存在
            List<int> listInvalidSaveDataNum = new List<int>();
            foreach (var kv in m_DicSaveDataInfo)
            {
                var num = kv.Key;
                string filePathCheck = Path.Combine(m_SaveDatasDirPath, string.Format(m_SaveDataFileNameFormat, num));
                if (!File.Exists(filePathCheck))
                {
                    listInvalidSaveDataNum.Add(num);
                }
            }
            //移除无效的存档信息
            for (int i = 0; i < listInvalidSaveDataNum.Count; i++)
            {
                m_DicSaveDataInfo.Remove(listInvalidSaveDataNum[i]);
            }
        }
    }

    //保存 存档列表信息
    private void SaveSaveDataListInfo(int num = -1)
    {
        if (num != -1)
        {
            var saveDataInfo = new SaveDataInfo();
            saveDataInfo.Num = num;
            saveDataInfo.PlayerName = PlayerModel.Instance.PlayerInfo.Nickname;
            saveDataInfo.GameTimeDate = TimeModel.Instance.TimeInfoCur.GameTimeDateFormat;
            saveDataInfo.PlayTimeSeconds = TimeModel.Instance.RealTimePlaySeconds;

            if (m_DicSaveDataInfo.ContainsKey(num))
            {
                m_DicSaveDataInfo[num] = saveDataInfo;
            }
            else
            {
                m_DicSaveDataInfo.Add(num, saveDataInfo);
            }
        }

        //保存至本地
        string filePath = Path.Combine(m_SaveDatasDirPath, m_SaveDataInfoFileName);
        PlayerPrefsUtil.SaveDataFilePath(filePath, m_DicSaveDataInfo);
    }
}

public interface ISaveData
{
    /// <summary>
    /// 保存 存档数据
    /// </summary>
    void SaveData(ES3File saveData);

    /// <summary>
    /// 加载 存档数据
    /// 在单例构造时调用，在Init之后调用。
    /// </summary>
    void LoadData(ES3File saveData);
}