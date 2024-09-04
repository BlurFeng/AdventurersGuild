using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntrustSystem
{
    /// <summary>
    /// 委托项目容器
    /// </summary>
    public class EntrustItemContent
    {
        /// <summary>
        /// 所有委托项目字典
        /// Key=Id Vaule=委托项目
        /// </summary>
        private Dictionary<int, EntrustItem> m_EntrustItemDic;

        /// <summary>
        /// 所有委托项目
        /// </summary>
        private List<EntrustItem> m_EntrustItems;

        /// <summary>
        /// 所有委托项目数组
        /// 在获取时更新
        /// </summary>
        private EntrustItem[] m_EntrustItemArray;

        private EntrustItemHandler[] m_EntrustItemHandlerArray;

        /// <summary>
        /// 确认所有委托项目数组是否需要更新
        /// </summary>
        private bool m_EntrustItemArrayIsDirty;

        /// <summary>
        /// 委托的总数
        /// </summary>
        public int Count { get { return m_EntrustItemDic.Count; } }

        public EntrustItemContent()
        {
            m_EntrustItemDic = new Dictionary<int, EntrustItem>();
            m_EntrustItems = new List<EntrustItem>();
        }

        /// <summary>
        /// 添加项目
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        public bool Add(int id, EntrustItem item)
        {
            if (Contains(id)) return false;

            m_EntrustItemDic.Add(id, item);
            m_EntrustItems.Add(item);

            m_EntrustItemArrayIsDirty = true;

            return true;
        }

        /// <summary>
        /// 移除项目
        /// </summary>
        /// <param name="id"></param>
        public bool Remove(int id)
        {
            if (!Contains(id)) return false;

            m_EntrustItems.Remove(m_EntrustItemDic[id]);
            m_EntrustItemDic.Remove(id);

            m_EntrustItemArrayIsDirty = true;

            return true;
        }

        /// <summary>
        /// 是否包含某个Id的委托项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Contains(int id)
        {
            return m_EntrustItemDic.ContainsKey(id);
        }

        /// <summary>
        /// 获取委托项目 通过Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EntrustItem Get(int id)
        {
            if (!Contains(id)) return null;

            return m_EntrustItemDic[id];
        }

        /// <summary>
        /// 获取委托项目数组
        /// </summary>
        /// <param name="isDirty">委托数组是否被更新</param>
        /// <returns></returns>
        public EntrustItem[] GetEntrustItems(out bool isDirty)
        {
            isDirty = false;

            if (m_EntrustItems.Count == 0)
                return null;

            if (m_EntrustItemArrayIsDirty)
            {
                isDirty = true;
                m_EntrustItemArray = m_EntrustItems.ToArray();
            }

            return m_EntrustItemArray;
        }

        /// <summary>
        /// 获取委托项目数组
        /// </summary>
        /// <param name="isDirty">委托数组是否被更新</param>
        /// <returns></returns>
        public EntrustItemHandler[] GetEntrustItemHandlers(out bool isDirty)
        {
            var items = GetEntrustItems(out isDirty);

            if (isDirty)
            {
                if (items != null)
                {
                    m_EntrustItemHandlerArray = new EntrustItemHandler[items.Length];
                    for (int i = 0; i < items.Length; i++)
                    {
                        m_EntrustItemHandlerArray[i] = items[i].EntrustItemHandler;
                    }
                }
                else
                    m_EntrustItemHandlerArray = null;
            }

            return m_EntrustItemHandlerArray;
        }
    }

    /// <summary>
    /// 委托系统管理器配置
    /// </summary>
    public struct EntrustSystemManagerConfig
    {
        /// <summary>
        /// 冒险者只能在一个委托中工作
        /// </summary>
        public bool venturerWorkingOnlyOneEntrust;

        /// <summary>
        /// 放弃委托时，此委托返回未受理池中或者销毁
        /// true=返回未受理，false=销毁
        /// </summary>
        public bool abortEntrust_ReturnUnacceptedOrDestroy;
    }
}
