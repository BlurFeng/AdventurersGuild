using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FsNotificationSystem
{
    public class NotificationComponentBase : MonoBehaviour
    {
        #region Unity MonoBehaviour Extension
        public RectTransform RectTransformGet
        {
            get
            {
                if (mCachedRectTransform == null && null != this)
                    mCachedRectTransform = GetComponent<RectTransform>();
                return mCachedRectTransform;
            }
        }
        RectTransform mCachedRectTransform;

        public GameObject GameObjectGet
        {
            get
            {
                if (mCachedGameObject == null && null != this)
                    mCachedGameObject = gameObject;
                return mCachedGameObject;
            }
        }
        GameObject mCachedGameObject;
        #endregion

        [SerializeField] private RectTransform m_RectTransBgFrame; //RectTrans 背景外框
        [SerializeField] private float m_LineWidthLimitPixel = 50f; //行宽度上限 像素单位
        [SerializeField] private TMP_Text m_TxtContent = null; //文本 内容
        [SerializeField] private RectTransform m_RectTxtContent; //RectTrans 文本内容
        [SerializeField] private bool m_AdaptiveFrameSize = true; //是否 自适应外框尺寸
        [SerializeField] private GameObject m_RootNotification = null; //根节点 消息气泡
        [SerializeField] private bool m_IsAutoClose = true; //是否 自动关闭
        [SerializeField] private float m_AnimFadeSeconds = 0.05f; //动画淡入淡出 秒数
        [SerializeField] private bool m_EnableFadeAnim = true; //开启 淡入淡出动画

        /// <summary>
        /// 播放完成 回调
        /// </summary>
        public Action OnPlayComplete { get { return m_OnPlayComplete; } set { m_OnPlayComplete = value; } }
        private Action m_OnPlayComplete; 

        private string m_TextContent; //文本 内容
        private float m_AnimSpeed = 12f; //动画播放速度
        private Transform m_RootNotificationTrans; //根节点 消息气泡 Trans
        private Coroutine m_CorAnimTyper; //协程 动画 打字机

        /// <summary>
        /// 像素缩放单位
        /// </summary>
        protected float m_PixelsPerUnit = 1f;
        /// <summary>
        /// TMP_Text 文本缩放尺寸
        /// </summary>
        protected float m_FontSizePerUnit = 1f;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            m_RootNotification.SetActive(false);
            m_RootNotificationTrans = m_RootNotification.transform;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void Init()
        {
            
        }

        /// <summary>
        /// 播放 文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="isPlayAnim">是否播放动画</param>
        public void PlayTextContent(string text, bool isPlayAnim = true)
        {
            if (m_TxtContent == null) { return; }

            m_TextContent = text;
            
            //是否播放 打字机动画
            if (isPlayAnim)
            {
                if (m_CorAnimTyper != null)
                    StopCoroutine(m_CorAnimTyper);

                m_CorAnimTyper = StartCoroutine(CorAnimTyper());
            }
            else
                m_TxtContent.text = m_TextContent;
        }

        /// <summary>
        /// 设置 动画参数
        /// </summary>
        /// <param name="isAutoClose">播放完成自动关闭</param>
        /// <param name="animPlaySpeed">动画速度 字数/秒</param>
        public void SetAnimParam(bool isAutoClose = true, float animPlaySpeed = 12f)
        {
            m_IsAutoClose = isAutoClose;
            m_AnimSpeed = animPlaySpeed;
        }

        //协程 动画 打字机
        IEnumerator CorAnimTyper()
        {
            m_TxtContent.text = string.Empty;

            //设置激活
            m_RootNotification.SetActive(true);
            //自适应外框尺寸
            if (m_AdaptiveFrameSize)
            {
                SetTextFrameSize("  ");
            }

            //淡入
            if (m_EnableFadeAnim)
            {
                m_RootNotificationTrans.localScale = Vector3.zero;
                m_RootNotificationTrans.DOScale(Vector3.one, m_AnimFadeSeconds);
                yield return new WaitForSeconds(m_AnimFadeSeconds);
            }

            //文字动画
            var waitSeconds = new WaitForSeconds(1f / m_AnimSpeed); //单字等待秒数
            for (int length = 1; length <= m_TextContent.Length; length++)
            {
                yield return waitSeconds;

                //设置 文本内容
                string showText = m_TextContent.Substring(0, length);
                m_TxtContent.text = showText;
                //自适应外框尺寸
                if (m_AdaptiveFrameSize)
                {
                    SetTextFrameSize(showText);
                }
            }

            //自动关闭
            if (m_IsAutoClose)
            {
                yield return new WaitForSeconds(1f); //等待1秒 阅读文本

                //淡出
                if (m_EnableFadeAnim)
                {
                    m_RootNotificationTrans.DOScale(Vector3.zero, m_AnimFadeSeconds);
                    yield return new WaitForSeconds(m_AnimFadeSeconds);
                    m_RootNotificationTrans.localScale = Vector3.one;
                }

                //设置非激活
                m_RootNotification.SetActive(false);
            }

            //播放完成 回调
            m_OnPlayComplete?.Invoke();
        }

        //设置 文本框
        private void SetTextFrameSize(string showText)
        {
            //文本组件的参数
            var fontSize = m_TxtContent.fontSize * m_FontSizePerUnit; //文字尺寸
            var frameSizePadding = m_RectTxtContent.offsetMin + m_RectTxtContent.offsetMax * -1f; //外框间隔

            //计算 外框尺寸
            var textWidth = GetTextWidth(showText, fontSize) + 0.01f; //总文本宽度 + 预留宽度 防止文字错误换行
            float lineHeigth = fontSize + fontSize * m_TxtContent.lineSpacing * 0.01f; //单行高度+行间距(数值为FontSize的百分比)
            float frameWidth; //外框最终宽度
            float frameHeight; //外框最终高度
            //对话框宽度 是否超过宽度限制
            float widthLimit = m_LineWidthLimitPixel * m_PixelsPerUnit;
            if (textWidth > widthLimit)
            {
                frameWidth = widthLimit;
                //计算行数
                int lineCount = (int)Math.Ceiling(textWidth / widthLimit);
                frameHeight = lineHeigth * lineCount;
            }
            else
            {
                frameWidth = textWidth;
                frameHeight = lineHeigth;
            }

            //计算最终尺寸
            var size = new Vector2(frameWidth, frameHeight) + frameSizePadding; //四周间隔
            //设置 外框尺寸
            m_RectTransBgFrame.sizeDelta = size;
            
            //子类重写 设置其他对象尺寸
            SetSize(size);
        }

        //设置尺寸
        protected virtual void SetSize(Vector2 size)
        {

        }

        //获取 文本宽度
        private float GetTextWidth(string text, float fontSize)
        {
            float fontWidth = fontSize + fontSize * m_TxtContent.characterSpacing * m_PixelsPerUnit; //单字宽度

            //计算消息字符串宽度
            float textWidth = 0; //记录消息字符串宽度
            for (int i = 0; i < text.Length; i++)
            {
                Char font = text[i];
                if (font >= 0x4E00 && font <= 0x9FA5)
                    textWidth += fontWidth; //中文
                else
                    textWidth += fontWidth * 0.58f; //非中文
            }

            return textWidth;
        }

#if UNITY_EDITOR
        [ContextMenu("PlayText")]
        protected void EditorPlayText()
        {
            if (m_TxtContent == null) { return; }

            Init();

            var text = m_TxtContent.text;
            PlayTextContent(text);
        }
#endif
    }
}
