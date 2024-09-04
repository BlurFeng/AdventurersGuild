using System.Collections;
using System.Collections.Generic;

namespace Deploy
{
    public partial class Skill_Group
    {
        Dictionary<int, Dictionary<int, int>> m_DicRankAttributeRequire = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// 获取 指定阶级的属性值需求
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public Dictionary<int, int> GetRankAttributeRequire(int rank)
        {
            if (m_DicRankAttributeRequire.Count == 0)
                InitDicRankAttributeRequire();

            m_DicRankAttributeRequire.TryGetValue(rank, out Dictionary<int, int> dicAttrValue);
            return dicAttrValue;
        }

        //初始化 阶级属性需求
        private void InitDicRankAttributeRequire()
        {
            m_DicRankAttributeRequire.Clear();
            foreach (var kv in RankAttributeRequire)
            {
                int rankId = kv.Key; //阶级ID
                var dicAttrValue = new Dictionary<int, int>(); // 属性:值要求
                string[] attrValueArray = kv.Value.Split('|');
                for (int i = 0; i < attrValueArray.Length; i++)
                {
                    string[] attrValue = attrValueArray[i].Split('=');
                    dicAttrValue.Add(int.Parse(attrValue[0]), int.Parse(attrValue[1]));
                }
                m_DicRankAttributeRequire.Add(rankId, dicAttrValue);
            }
        }
    }
}
  
