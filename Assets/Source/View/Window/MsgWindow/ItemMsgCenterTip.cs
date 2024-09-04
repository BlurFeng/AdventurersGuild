using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class ItemMsgCenterTip : MonoBehaviour
{
	[SerializeField] private TMP_Text m_TxtMsg = null;
	[SerializeField] private CanvasGroup m_CanvasGroup = null;

	/// <summary>
	/// 文本 消息
	/// </summary>
	public string TxtMsg { get { return m_TxtMsg?.text; } }

	private GameObject m_GameObj;
	private RectTransform m_RectTrans;

	private Sequence m_Sequence;

	private void OnEnable()
	{
		m_RectTrans = GetComponent<RectTransform>();
	}

	private void Awake()
	{
		m_GameObj = gameObject;
		m_RectTrans = GetComponent<RectTransform>();
	}

	public void Show(string msg, float showTime, Action<ItemMsgCenterTip> callBack, float fadetime = 0.3f, Transform parent = null)
    {
		//设置数据
		m_TxtMsg.text = msg;

		m_GameObj.SetActive(true);
		if (parent != null)
		{
			m_RectTrans.SetParent(parent);
		}
		m_RectTrans.SetAsLastSibling();
		m_RectTrans.anchoredPosition = Vector3.zero;

		//动画队列
		if (m_Sequence != null)
		{
			m_Sequence.Kill(false);
		}
		m_CanvasGroup.alpha = 0;
		m_Sequence = DOTween.Sequence();
        m_Sequence.Append(m_CanvasGroup.DOFade(1, fadetime));
        m_Sequence.AppendInterval(showTime - fadetime * 2);
        m_Sequence.Append(m_CanvasGroup.DOFade(0, fadetime)).OnComplete(() =>
		{
			m_GameObj.SetActive(false);
			callBack?.Invoke(this);
		});
    }
}
