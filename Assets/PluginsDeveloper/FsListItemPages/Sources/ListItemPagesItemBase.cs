using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using com.ootii.Messages;
using System;

namespace FsListItemPages
{
    public class ListItemPagesItemBase : MonoBehaviour
    {
        [SerializeField] private GameObject m_BtnClick = null; //按钮 点击
        [SerializeField] private GameObject m_GoBgNormal; //物体 背景 正常
        [SerializeField] private GameObject m_GoBgHover; //物体 背景 悬停
        [SerializeField] private GameObject m_GoBgSelect; //物体 背景 选中

        /// <summary>
        /// 点击事件
        /// </summary>
        public Action<int, int> OnClick { get { return m_OnClick; } set { m_OnClick = value; } }
        private Action<int, int> m_OnClick;

        private GameObject GameObjectGet;
        private IItemPagesData m_Data;
        private int m_PageIndex;
        private int m_StripIndex;
        private bool m_IsSelect = false; //选中

        protected virtual void Awake()
        {
            GameObjectGet = gameObject;

            if (m_BtnClick != null)
            {
                ClickListener.Get(m_BtnClick).SetClickHandler(BtnClick);
                ClickListener.Get(m_BtnClick).SetPointerEnterHandler(BtnEnter);
                ClickListener.Get(m_BtnClick).SetPointerExitHandler(BtnExit);
            }

            SetSelect(false);
        }

        protected virtual void OnDestroy()
        {

        }

        /// <summary>
        /// 设置 信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pageIndex">第几页 下标</param>
        /// <param name="stripIndex">第几条 下标</param>
        public virtual void SetInfo(IItemPagesData data, int pageIndex, int stripIndex)
        {
            m_Data = data;
            m_PageIndex = pageIndex;
            m_StripIndex = stripIndex;
        }

        /// <summary>
        /// 设置 激活状态
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive(bool isActive)
        {
            if (GameObjectGet == null) { return; }

            GameObjectGet.SetActive(isActive);
        }

        /// <summary>
        /// 设置 选中
        /// </summary>
        /// <param name="isSelect"></param>
        public void SetSelect(bool isSelect)
        {
            m_IsSelect = isSelect;
            if (m_IsSelect)
            {
                SetBgType(3);
            }
            else
            {
                SetBgType(1);
            }
        }

        //按钮 鼠标点击
        protected virtual void BtnClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            m_OnClick?.Invoke(m_PageIndex, m_StripIndex);
        }

        //按钮 鼠标进入
        protected virtual void BtnEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (m_IsSelect) { return; }

            SetBgType(2);
        }

        //按钮 鼠标离开
        protected virtual void BtnExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (m_IsSelect) { return; }

            SetBgType(1);
        }

        //设置 背景类型
        private void SetBgType(int type)
        {
            if (m_GoBgNormal == null || m_GoBgHover == null || m_GoBgSelect == null) { return; }

            switch (type)
            {
                case 1:
                    m_GoBgNormal.SetActive(true);
                    m_GoBgHover.SetActive(false);
                    m_GoBgSelect.SetActive(false);
                    break;
                case 2:
                    m_GoBgNormal.SetActive(false);
                    m_GoBgHover.SetActive(true);
                    m_GoBgSelect.SetActive(false);
                    break;
                case 3:
                    m_GoBgNormal.SetActive(false);
                    m_GoBgHover.SetActive(false);
                    m_GoBgSelect.SetActive(true);
                    break;
            }
        }
    }
}

