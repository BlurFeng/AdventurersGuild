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

        [SerializeField] private RectTransform m_RectTransBgFrame; //RectTrans �������
        [SerializeField] private float m_LineWidthLimitPixel = 50f; //�п������ ���ص�λ
        [SerializeField] private TMP_Text m_TxtContent = null; //�ı� ����
        [SerializeField] private RectTransform m_RectTxtContent; //RectTrans �ı�����
        [SerializeField] private bool m_AdaptiveFrameSize = true; //�Ƿ� ����Ӧ���ߴ�
        [SerializeField] private GameObject m_RootNotification = null; //���ڵ� ��Ϣ����
        [SerializeField] private bool m_IsAutoClose = true; //�Ƿ� �Զ��ر�
        [SerializeField] private float m_AnimFadeSeconds = 0.05f; //�������뵭�� ����
        [SerializeField] private bool m_EnableFadeAnim = true; //���� ���뵭������

        /// <summary>
        /// ������� �ص�
        /// </summary>
        public Action OnPlayComplete { get { return m_OnPlayComplete; } set { m_OnPlayComplete = value; } }
        private Action m_OnPlayComplete; 

        private string m_TextContent; //�ı� ����
        private float m_AnimSpeed = 12f; //���������ٶ�
        private Transform m_RootNotificationTrans; //���ڵ� ��Ϣ���� Trans
        private Coroutine m_CorAnimTyper; //Э�� ���� ���ֻ�

        /// <summary>
        /// �������ŵ�λ
        /// </summary>
        protected float m_PixelsPerUnit = 1f;
        /// <summary>
        /// TMP_Text �ı����ųߴ�
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
        /// ��ʼ��
        /// </summary>
        protected virtual void Init()
        {
            
        }

        /// <summary>
        /// ���� �ı�
        /// </summary>
        /// <param name="text">�ı�����</param>
        /// <param name="isPlayAnim">�Ƿ񲥷Ŷ���</param>
        public void PlayTextContent(string text, bool isPlayAnim = true)
        {
            if (m_TxtContent == null) { return; }

            m_TextContent = text;
            
            //�Ƿ񲥷� ���ֻ�����
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
        /// ���� ��������
        /// </summary>
        /// <param name="isAutoClose">��������Զ��ر�</param>
        /// <param name="animPlaySpeed">�����ٶ� ����/��</param>
        public void SetAnimParam(bool isAutoClose = true, float animPlaySpeed = 12f)
        {
            m_IsAutoClose = isAutoClose;
            m_AnimSpeed = animPlaySpeed;
        }

        //Э�� ���� ���ֻ�
        IEnumerator CorAnimTyper()
        {
            m_TxtContent.text = string.Empty;

            //���ü���
            m_RootNotification.SetActive(true);
            //����Ӧ���ߴ�
            if (m_AdaptiveFrameSize)
            {
                SetTextFrameSize("  ");
            }

            //����
            if (m_EnableFadeAnim)
            {
                m_RootNotificationTrans.localScale = Vector3.zero;
                m_RootNotificationTrans.DOScale(Vector3.one, m_AnimFadeSeconds);
                yield return new WaitForSeconds(m_AnimFadeSeconds);
            }

            //���ֶ���
            var waitSeconds = new WaitForSeconds(1f / m_AnimSpeed); //���ֵȴ�����
            for (int length = 1; length <= m_TextContent.Length; length++)
            {
                yield return waitSeconds;

                //���� �ı�����
                string showText = m_TextContent.Substring(0, length);
                m_TxtContent.text = showText;
                //����Ӧ���ߴ�
                if (m_AdaptiveFrameSize)
                {
                    SetTextFrameSize(showText);
                }
            }

            //�Զ��ر�
            if (m_IsAutoClose)
            {
                yield return new WaitForSeconds(1f); //�ȴ�1�� �Ķ��ı�

                //����
                if (m_EnableFadeAnim)
                {
                    m_RootNotificationTrans.DOScale(Vector3.zero, m_AnimFadeSeconds);
                    yield return new WaitForSeconds(m_AnimFadeSeconds);
                    m_RootNotificationTrans.localScale = Vector3.one;
                }

                //���÷Ǽ���
                m_RootNotification.SetActive(false);
            }

            //������� �ص�
            m_OnPlayComplete?.Invoke();
        }

        //���� �ı���
        private void SetTextFrameSize(string showText)
        {
            //�ı�����Ĳ���
            var fontSize = m_TxtContent.fontSize * m_FontSizePerUnit; //���ֳߴ�
            var frameSizePadding = m_RectTxtContent.offsetMin + m_RectTxtContent.offsetMax * -1f; //�����

            //���� ���ߴ�
            var textWidth = GetTextWidth(showText, fontSize) + 0.01f; //���ı���� + Ԥ����� ��ֹ���ִ�����
            float lineHeigth = fontSize + fontSize * m_TxtContent.lineSpacing * 0.01f; //���и߶�+�м��(��ֵΪFontSize�İٷֱ�)
            float frameWidth; //������տ��
            float frameHeight; //������ո߶�
            //�Ի����� �Ƿ񳬹��������
            float widthLimit = m_LineWidthLimitPixel * m_PixelsPerUnit;
            if (textWidth > widthLimit)
            {
                frameWidth = widthLimit;
                //��������
                int lineCount = (int)Math.Ceiling(textWidth / widthLimit);
                frameHeight = lineHeigth * lineCount;
            }
            else
            {
                frameWidth = textWidth;
                frameHeight = lineHeigth;
            }

            //�������ճߴ�
            var size = new Vector2(frameWidth, frameHeight) + frameSizePadding; //���ܼ��
            //���� ���ߴ�
            m_RectTransBgFrame.sizeDelta = size;
            
            //������д ������������ߴ�
            SetSize(size);
        }

        //���óߴ�
        protected virtual void SetSize(Vector2 size)
        {

        }

        //��ȡ �ı����
        private float GetTextWidth(string text, float fontSize)
        {
            float fontWidth = fontSize + fontSize * m_TxtContent.characterSpacing * m_PixelsPerUnit; //���ֿ��

            //������Ϣ�ַ������
            float textWidth = 0; //��¼��Ϣ�ַ������
            for (int i = 0; i < text.Length; i++)
            {
                Char font = text[i];
                if (font >= 0x4E00 && font <= 0x9FA5)
                    textWidth += fontWidth; //����
                else
                    textWidth += fontWidth * 0.58f; //������
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
