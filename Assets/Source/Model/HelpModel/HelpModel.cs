using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deploy;
using FsListItemPages;

public class HelpModel : Singleton<HelpModel>, IDestroy
{
    /// <summary>
    /// 帮助信息类型
    /// </summary>
    public enum EHelpType
    {
        None = 0,
        /// <summary>
        /// 基础
        /// </summary>
        Basic,
        /// <summary>
        /// 公会运营
        /// </summary>
        Guild,
        /// <summary>
        /// 其他
        /// </summary>
        Other,
    }

    private Dictionary<EHelpType, List<int>> m_DicShowHelpCfgId = new Dictionary<EHelpType, List<int>>(); //字典 显示的帮助信息 配置表ID

    public override void Init()
    {
        base.Init();

        m_DicShowHelpCfgId.Clear();
        var cfgMap = ConfigSystem.Instance.GetConfigMap<Notebook_Help>();
        foreach (var cfg in cfgMap.Values)
        {
            var helpType = (EHelpType)cfg.HelpType;
            if (!m_DicShowHelpCfgId.ContainsKey(helpType))
            {
                m_DicShowHelpCfgId.Add(helpType, new List<int>());
            }

            m_DicShowHelpCfgId[helpType].Add(cfg.Id);
        }
    }

    /// <summary>
    /// 获取 帮助信息配置表ID
    /// </summary>
    /// <param name="helpType"></param>
    /// <returns></returns>
    public List<IItemPagesData> GetListHelpCfgId(EHelpType helpType)
    {
        var listData = new List<IItemPagesData>();

        List<int> listCfgId = null;
        if (m_DicShowHelpCfgId.TryGetValue(helpType, out listCfgId))
        {
            for (int i = 0; i < listCfgId.Count; i++)
            {
                listData.Add(new ItemPagesData(listCfgId[i]));
            }
        }

        return listData;
    }
}
