using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using Spine.Unity;
using Spine;
using UnityEngine.EventSystems;
using FsNotificationSystem;
using Deploy;

public class ItemMsgFullScreenTip : MonoBehaviour
{
	[SerializeField] private Image m_ImgBgFade = null; //贴图 背景 淡入淡出
	[SerializeField] private GameObject m_BtnClick = null; //按钮 点击
	[SerializeField] private NotificationUIComponent m_NotificationUIComponent = null; //组件 通知

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

	private Color m_ColorImgFadeDefault; //颜色 贴图淡入 默认
	private Queue<string> m_QueueTextContent = new Queue<string>(); //队列 文本内容
	private Action m_ShowEndcallBack; //显示完成 回调

	private void Awake()
	{
		ClickListener.Get(m_BtnClick).SetClickHandler(BtnClick);

		m_ColorImgFadeDefault = m_ImgBgFade.color;
		m_NotificationUIComponent.OnPlayComplete = EvtPlayComplete;
	}

	//按钮 点击
	private void BtnClick(PointerEventData obj)
	{
		//播放 下一句文本
		if (!PlayNextText())
		{
			//文本播放完毕 淡出
			OnFadeOut();
		}
	}

	//事件 播放完成
	private void EvtPlayComplete()
	{
		//显示 点击按钮
		m_BtnClick.SetActive(true);
	}

	/// <summary>
	/// 显示文本
	/// </summary>
	/// <param name="text"></param>
	/// <param name="showEndcallBack"></param>
	public void Show(string text, Action showEndcallBack)
    {
		GameObjectGet.SetActive(true);
		m_ShowEndcallBack = showEndcallBack;
		m_NotificationUIComponent.PlayTextContent(string.Empty, false);

		//添加 需要显示的文本
		m_QueueTextContent.Enqueue(text);

		OnFadeIn();
	}

	/// <summary>
	/// 显示文本
	/// </summary>
	/// <param name="storyDialogueIdStart">剧情对话配置表 ID</param>
	/// <param name="showEndcallBack"></param>
	public void Show(int storyDialogueIdStart, Action showEndcallBack)
	{
		GameObjectGet.SetActive(true);
		m_ShowEndcallBack = showEndcallBack;
		m_NotificationUIComponent.PlayTextContent(string.Empty, false);

		//顺序添加需要显示的文本
		int id = storyDialogueIdStart;
		for (int i = 0; i < 1000; i++)
		{
			//获取配置
			var cfg = ConfigSystem.Instance.GetConfig<Story_Dialogue>(id);
			if (cfg == null) break;

			//添加需要显示的文本
			m_QueueTextContent.Enqueue(cfg.Content);

			//下一句话的ID
			id = cfg.NextID;
			if (id == -1) { break; } //对话是否结束
		}

		OnFadeIn();
	}
	
	//协程 开始播放文本
	IEnumerator CorPlayTextStart(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);

		PlayNextText(); //播放文本
	}

	//播放 下一个文本
	private bool PlayNextText()
	{
		if (m_QueueTextContent.Count == 0) { return false; }
		var text = m_QueueTextContent.Dequeue();

		//隐藏 点击按钮
		m_BtnClick.SetActive(false);
		//播放文本
		m_NotificationUIComponent.PlayTextContent(text);

		return true;
	}

	//淡入
	private void OnFadeIn(float fadeInSeconds = 1.2f)
	{
		//隐藏 点击按钮
		m_BtnClick.SetActive(false);

		DOTween.Kill(m_ImgBgFade);
		m_ImgBgFade.color = new Color(m_ColorImgFadeDefault.r, m_ColorImgFadeDefault.g, m_ColorImgFadeDefault.b, 0);
		//淡入 背景
		m_ImgBgFade.DOFade(m_ColorImgFadeDefault.a, fadeInSeconds).OnComplete(() =>
		{
			StartCoroutine(CorPlayTextStart(1f));
		});
	}

	//淡出
	private void OnFadeOut(float fadeOutSeconds = 1.2f)
	{
		//隐藏 点击按钮
		m_BtnClick.SetActive(false);

		//淡出 背景
		m_ImgBgFade.DOFade(0, fadeOutSeconds).OnComplete(() => 
		{
			GameObjectGet.SetActive(false);
			m_ShowEndcallBack?.Invoke();
		});
	}
}
