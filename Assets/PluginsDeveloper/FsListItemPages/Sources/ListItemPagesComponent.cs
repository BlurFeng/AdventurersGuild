using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FsListItemPages
{
    public class ListItemPagesComponent : MonoBehaviour
    {
        [SerializeField] private List<ListItemPagesItemBase> m_ListItemOnePage = null; //列表 项目 单页
        [SerializeField] private GameObject m_BtnLastPage = null; //按钮 左翻页
        [SerializeField] private GameObject m_BtnNextPage = null; //按钮 右翻页
        [SerializeField] private TextMeshProUGUI m_TxtPageCur = null; //文本 页数 当前数
        [SerializeField] private TextMeshProUGUI m_TxtPageTotal = null; //文本 页数 总数

        /// <summary>
        /// 选中的项目改变
        /// </summary>
        public Action<IItemPagesData> OnSelectItemChange { get { return m_OnSelectItemChange; } set { m_OnSelectItemChange = value; } }
        private Action<IItemPagesData> m_OnSelectItemChange;

        private IItemPagesData[,] m_ArrayCfgItemData; //目录条目 配置表ID
        private int m_OnePageItemCount = 1; //单页 项目数量
        private int m_PageNumTotal; //页数 总数
        private int m_PageNumCur; //页数 当前
        private int m_SelectItemPageIndex; //选中的项目 第几页 下标
        private int m_SelectItemStripIndex = -1; //选中的项目 第几条 下标

        private void Awake()
        {
            ClickListener.Get(m_BtnLastPage).SetClickHandler(BtnLastPage);
            ClickListener.Get(m_BtnNextPage).SetClickHandler(BtnNextPage);

            m_OnePageItemCount = m_ListItemOnePage.Count; //单页项目数量
            //设置 所有项目的点击事件
            for (int i = 0; i < m_ListItemOnePage.Count; i++)
            {
                m_ListItemOnePage[i].OnClick = SelectItemIndex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listItemData">项目数据列表</param>
        /// <param name="onSelectItemChange">回调 选中项目</param>
        /// <param name="InitSelect">初始化时 默认选中第一个项目</param>
        public void Init(List<IItemPagesData> listItemData, Action<IItemPagesData> onSelectItemChange = null, bool InitSelect = true)
        {
            if (listItemData == null || listItemData.Count == 0)
            {
                //隐藏 所有项目
                for (int i = 0; i < m_ListItemOnePage.Count; i++)
                {
                    var item = m_ListItemOnePage[i];
                    item.SetActive(false);
                }
                //设置 UI信息
                m_TxtPageTotal.text = "1";
                m_TxtPageCur.text = "1";
                return;
            }

            //记录 选中项目回调
            m_OnSelectItemChange = onSelectItemChange;

            //页数 总数
            m_PageNumTotal = Mathf.CeilToInt((float)listItemData.Count / m_OnePageItemCount);
            m_ArrayCfgItemData = new IItemPagesData[m_PageNumTotal, m_OnePageItemCount];

            //记录 项目数据
            int pageCur = 0;
            int index = 0;
            for (int i = 0; i < listItemData.Count; i++)
            {
                var ItemData = listItemData[i];

                //记录 项目数据
                m_ArrayCfgItemData[pageCur, index] = ItemData;

                //记录数达到单页项目总数 翻页
                index++;
                if (index >= m_OnePageItemCount)
                {
                    index = 0;
                    pageCur++;
                }
            }

            SelectPageNum(1); //默认选中页数 第一页

            if (InitSelect)
                SelectItemIndex(0, 0); //默认选中项目

            //设置 UI信息
            m_TxtPageTotal.text = m_PageNumTotal.ToString();
        }

        //按钮 左翻页
        private void BtnNextPage(PointerEventData obj)
        {
            if (m_PageNumCur >= m_PageNumTotal) { return; }
            SelectPageNum(m_PageNumCur + 1);
        }

        //按钮 右翻页
        private void BtnLastPage(PointerEventData obj)
        {
            if (m_PageNumCur <= 1) { return; }
            SelectPageNum(m_PageNumCur - 1);
        }

        /// <summary>
        /// 选中 页数
        /// </summary>
        /// <param name="num"></param>
        public void SelectPageNum(int num)
        {
            if (num < 1 || num > m_PageNumTotal) { return; }
            m_PageNumCur = num;

            //UI信息
            m_TxtPageCur.text = m_PageNumCur.ToString();

            //刷新 Item信息
            int pageIndex = m_PageNumCur - 1;
            for (int i = 0; i < m_OnePageItemCount; i++)
            {
                var itemData = m_ArrayCfgItemData[pageIndex, i];
                var item = m_ListItemOnePage[i];

                if (itemData == null)
                    item.gameObject.SetActive(false);
                else
                {
                    item.gameObject.SetActive(true);
                    item.SetInfo(itemData, pageIndex, i);
                    bool isSelect = pageIndex == m_SelectItemPageIndex && i == m_SelectItemStripIndex;
                    item.SetSelect(isSelect);
                }
            }
        }

        /// <summary>
        /// 选中项目
        /// </summary>
        /// <param name="id">项目Id</param>
        public void SelectItem(int id)
        {
            if (id == 0) { return; }

            for (int i = 0; i < m_ArrayCfgItemData.GetLength(0); i++)
            {
                for (int j = 0; j < m_ArrayCfgItemData.GetLength(1); j++)
                {
                    var itemData = m_ArrayCfgItemData[i, j];
                    if (itemData == null) continue;

                    if (itemData.GetId() == id)
                    {
                        SelectItemIndex(i, j);
                        return;
                    }
                }
            }
        }

        //选中的项目 页数下标&项目下标
        private void SelectItemIndex(int pageIndex, int itemIndex)
        {
            //旧Item 取消选中
            if (m_SelectItemPageIndex == m_PageNumCur - 1 && m_SelectItemStripIndex >= 0)
            {
                m_ListItemOnePage[m_SelectItemStripIndex].SetSelect(false);
            }

            m_SelectItemPageIndex = pageIndex;
            m_SelectItemStripIndex = itemIndex;

            //新Item 选中
            if (m_SelectItemPageIndex == m_PageNumCur - 1)
            {
                m_ListItemOnePage[m_SelectItemStripIndex].SetSelect(true);
            }

            //回调
            IItemPagesData itemData = null;
            if (m_SelectItemPageIndex < m_ArrayCfgItemData.GetLength(0) && m_SelectItemStripIndex < m_ArrayCfgItemData.GetLength(1))
                itemData = m_ArrayCfgItemData[m_SelectItemPageIndex, m_SelectItemStripIndex];
            m_OnSelectItemChange?.Invoke(itemData);
        }

    }

    /// <summary>
    /// 翻页项目 数据
    /// </summary>
    public struct ItemPagesData : IItemPagesData
    {
        public ItemPagesData(int id)
        {
            Id = id;
            CustomString = string.Empty;
        }

        public ItemPagesData(int id, string customString)
        {
            Id = id;
            CustomString = customString;
        }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 自定义数据 int
        /// </summary>
        public string CustomString { get; set; }

        public int GetId()
        {
            return Id;
        }

        public string GetCustomString()
        {
            return CustomString;
        }
    }

    /// <summary>
    /// 接口 翻页项目 数据
    /// </summary>
    public interface IItemPagesData
    {
        /// <summary>
        /// 获取 ID
        /// </summary>
        /// <returns></returns>
        public int GetId();

        /// <summary>
        /// 获取 自定义数据（string）
        /// </summary>
        /// <returns></returns>
        public string GetCustomString();
    }
}
