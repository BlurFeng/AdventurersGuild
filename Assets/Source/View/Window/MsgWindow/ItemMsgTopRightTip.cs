using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;
using Spine;

public class ItemMsgTopRightTip: MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI m_TxtMsg1 = null;
	[SerializeField] private List<GameObject> m_ListGoType = null;
	[SerializeField] private List<Image> m_ListImgTypeIcon = null;

	public GameObject GameObjectGet
	{
		get
		{
			if (m_GameObj == null)
			{
				m_GameObj = gameObject;
			}
			return m_GameObj;
		}
	}
	private GameObject m_GameObj;
	public RectTransform RectTransformGet
	{
		get
		{
			if (m_RectTrans == null)
			{
				m_RectTrans = GetComponent<RectTransform>();
			}
			return m_RectTrans;
		}
	}
	private RectTransform m_RectTrans;

	private Sequence m_Sequence;

	/// <summary>
	/// 显示消息
	/// </summary>
	/// <param name="msg1"></param>
	/// <param name="msg2"></param>
	/// <param name="showType">显示类型 Icon 0:双环背景 1:单环背景 2:无Icon</param>
	/// <param name="iconfolder"></param>
	/// <param name="iconName"></param>
	/// <param name="showTime"></param>
	/// <param name="showEndcallBack"></param>
	/// <param name="fadetime"></param>
	public void Show(string msg1,
		int showType, string iconfolder, string iconName,
		float showTime, Action<ItemMsgTopRightTip> showEndcallBack, float fadetime = 0.5f)
    {
		GameObjectGet.SetActive(true);
		RectTransformGet.SetAsLastSibling();

		//设置数据
		m_TxtMsg1.text = msg1;

		//根据文本数量自适应宽度
		float msg1Width = GetTextWidth(m_TxtMsg1.text, m_TxtMsg1.fontSize);
		float iconSize = showType == 2 ? 0 : 280;
		float width = msg1Width + iconSize;
		float ratio = width / 1350f;

		//设置对应类型的图标
		for (int i = 0; i < m_ListGoType.Count; i++)
		{
			var go = m_ListGoType[i];
			bool showIcon = showType == i;
			go.SetActive(showIcon);
			if (showIcon && i < m_ListImgTypeIcon.Count)
			{
				var imgIcon = m_ListImgTypeIcon[i];
				AssetIconSystem.Instance.SetIcon(imgIcon, iconfolder, iconName, true, () => { imgIcon.enabled = true; });
			}
		}

		//动画队列
		if (m_Sequence != null)
		{
			m_Sequence.Kill(false);
		}
		RectTransformGet.localPosition = new Vector3(width, -150f, 0);
		m_Sequence = DOTween.Sequence();
		m_Sequence.Append(RectTransformGet.DOLocalMoveY(0, fadetime));
		m_Sequence.Join(RectTransformGet.DOLocalMoveX(0, fadetime * 0.6f));
		m_Sequence.AppendInterval(showTime);
		m_Sequence.AppendCallback(() => { showEndcallBack?.Invoke(this); });
		m_Sequence.Join(RectTransformGet.DOLocalMoveX(width, fadetime)).OnComplete(() => { GameObjectGet.SetActive(false); });
	}

	//获取 文本宽度
	private float GetTextWidth(string text, float fontSize)
	{
		float textWidth = 0; //记录消息字符串宽度
		for (int i = 0; i < text.Length; i++)
		{
			Char font = text[i];
			if (font >= 0x4E00 && font <= 0x9FA5)
				textWidth += fontSize;
			else
				textWidth += fontSize * 0.58f;
		}

		return textWidth;
	}
}
