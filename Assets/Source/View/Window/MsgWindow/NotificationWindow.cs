using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class NotificationWindow : WindowBase
{
	/// <summary>
	/// 消息提示 Open参数
	/// </summary>
	public class NotificationWindowArg
    {
		/// <summary>
		/// 消息提示 类型
		/// </summary>
		public EMsgType MsgType = EMsgType.CenterTip;
		/// <summary>
		/// 文本
		/// </summary>
        public string Text = string.Empty;
		/// <summary>
		/// 显示时间
		/// </summary>
        public float ShowTime = 1f;
		/// <summary>
		/// 消息类型 参数
		/// </summary>
		public string MsgTypeParam = string.Empty;
		/// <summary>
		/// 显示结束回调
		/// </summary>
		public Action OnShowComplete;
    }

	public enum EMsgType
	{
		/// <summary>
		/// 中间
		/// 可配置字段 ShowTime, Msg1
		/// </summary>
		CenterTip = 1,
		/// <summary>
		/// 右上角
		/// 可配置字段 ShowTime, Msg1, Msg2, IconAddrs
		/// </summary>
		TopRightTip = 2,
		/// <summary>
		/// 全屏
		/// 可配置字段 ShowTime, Msg1, IconAddrs
		/// </summary>
		FullScreenTip = 3,
	}

	[SerializeField] private RectTransform m_RootTransCenterTip = null; //根节点 中间消息
	[SerializeField] private ItemMsgCenterTip m_ItemMsgCenterTip = null; //项目 中间消息
	[SerializeField] private List<ItemMsgTopRightTip> m_ListItemMsgTopRightTip = null; //项目列表 右上消息
	[SerializeField] private ItemMsgFullScreenTip m_ItemMsgFullScreenTip = null; //项目 全屏消息

	//中间消息 参数
	private Queue<NotificationWindowArg> m_CenterTipQueueMsgArg = new Queue<NotificationWindowArg>(); //消息队列
	private List<ItemMsgCenterTip> m_CenterTipListShowItem = new List<ItemMsgCenterTip>(); //当前显示的消息
    private int m_CenterTipMaxCount = 5; //最大显示数量
    private float m_CenterTipTimeGap = 0.1f; //显示间隔
    private bool m_CenterTipIsShow = false; //是否正在显示
	private float m_CenterTipFadeIntime = 0.3f; //淡入时间

	//右上角消息 参数
	private Queue<NotificationWindowArg> m_TopRightTipQueueMsgArg = new Queue<NotificationWindowArg>(); //消息队列
	private bool m_TopRightTipIsShow = false; //是否正在显示
	private float m_TopRightTipFadeIntime = 0.3f; //淡入时间

	//全屏消息 参数
	private Queue<NotificationWindowArg> m_FullScreenTipQueueMsgArg = new Queue<NotificationWindowArg>(); //消息队列
	private bool m_FullScreenTipIsShow = false; //是否正在显示

	public override void OnLoaded()
    {
        base.OnLoaded();
        MemoryLock = true;

		//缓存池初始化
        GameObjectPool.Instance.InitGameObject(m_ItemMsgCenterTip.gameObject, m_CenterTipMaxCount, true);
	}

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

		transform.SetAsFirstSibling();

		NotificationWindowArg arg = userData as NotificationWindowArg;
		if (arg == null) { return; }

		ShowMsg(arg);
	}

	/// <summary>
	/// 显示消息
	/// </summary>
	/// <param name="arg"></param>
	public void ShowMsg(NotificationWindowArg arg)
	{
		switch (arg.MsgType)
		{
			case EMsgType.CenterTip:
				AddArgQueueCenterTip(arg);
				break;
			case EMsgType.TopRightTip:
				AddArgQueueTopRightTip(arg);
				break;
			case EMsgType.FullScreenTip:
				AddArgQueueFullScreenTip(arg);
				break;
		}
	}

	/// <summary>
	/// 开始显示消息
	/// </summary>
	/// <param name="msgType"></param>
	public void StartShowMsg(EMsgType msgType)
	{
		switch (msgType)
		{
			case EMsgType.CenterTip:
				ShowMsgCenterTip();
				break;
			case EMsgType.TopRightTip:
				ShowMsgTopRightTip();
				break;
			case EMsgType.FullScreenTip:
				ShowMsgFullScreenTip();
				break;
		}
	}

	#region 中间消息
	private NotificationWindowArg m_ArgCenterTipCur;

	private void AddArgQueueCenterTip(NotificationWindowArg arg)
	{
		//添加进队列
		m_CenterTipQueueMsgArg.Enqueue(arg);
		ShowMsgCenterTip();
	}

	private void ShowMsgCenterTip()
	{
		if (!m_CenterTipIsShow)
		{
			OnShowMsgCenterTip();
		}
	}

	private void OnShowMsgCenterTip()
    {
        m_CenterTipIsShow = true;
        if (m_CenterTipQueueMsgArg.Count == 0)
        {
			//消息队列数量为0 结束显示
            m_CenterTipIsShow = false;
            return;
        }

		//获取一条消息数据 与 Item实例
		m_ArgCenterTipCur = m_CenterTipQueueMsgArg.Dequeue();
        GameObject msgItemGO = GameObjectPool.Instance.Get(m_ItemMsgCenterTip.gameObject, true);
		ItemMsgCenterTip msgItem = msgItemGO.GetComponent<ItemMsgCenterTip>();
        m_CenterTipListShowItem.Add(msgItem);
		//将消息数据设置进Item实例
		msgItem.Show(m_ArgCenterTipCur.Text, m_ArgCenterTipCur.ShowTime, ItemShowEndCenterTip, m_CenterTipFadeIntime, m_RootTransCenterTip);

		//重新设置Item的位移动画
		for (int i = 0; i < m_CenterTipListShowItem.Count; i++)
		{
			ItemMsgCenterTip item = m_CenterTipListShowItem[i];
			RectTransform rectTrans = item.transform as RectTransform;

			float toY = (m_CenterTipListShowItem.Count - i) * rectTrans.sizeDelta.y;

			DOTween.Kill(item.transform, false);
			rectTrans.DOAnchorPosY(toY, m_CenterTipFadeIntime);
		}

		//间隔时间后显示下一条消息
		DOVirtual.DelayedCall(m_CenterTipTimeGap, OnShowMsgCenterTip);
    }

    private void ItemShowEndCenterTip(ItemMsgCenterTip msgItem)
    {
		//显示完成 回调
		m_ArgCenterTipCur.OnShowComplete?.Invoke();

		m_CenterTipListShowItem.Remove(msgItem);
        GameObjectPool.Instance.Return(msgItem.gameObject);
    }
	#endregion

	#region 右上消息
	private void AddArgQueueTopRightTip(NotificationWindowArg arg)
	{
		//添加进队列
		m_TopRightTipQueueMsgArg.Enqueue(arg);
		ShowMsgTopRightTip();
	}

	private void ShowMsgTopRightTip()
	{
		if (!m_TopRightTipIsShow)
		{
			OnShowMsgTopRightTip();
		}
	}

	private void OnShowMsgTopRightTip()
	{
		m_TopRightTipIsShow = true;
		if (m_TopRightTipQueueMsgArg.Count == 0)
		{
			//消息队列数量为0 结束显示
			m_TopRightTipIsShow = false;
			return;
		}

		//获取tem实例
		ItemMsgTopRightTip msgItem = null;
		for (int i = 0; i < m_ListItemMsgTopRightTip.Count; i++)
		{
			var item = m_ListItemMsgTopRightTip[i];
			if (item.GameObjectGet.activeSelf == false)
			{
				msgItem = item;
				break;
			}
		}
		if (msgItem == null) { return; }
		//将消息数据设置进Item实例
		NotificationWindowArg arg = m_TopRightTipQueueMsgArg.Dequeue();
		var param = arg.MsgTypeParam.Split('|');
		msgItem.Show(arg.Text,int.Parse(param[0]), param[1], param[2], arg.ShowTime, EvtItemShowEndTopRightTip, m_TopRightTipFadeIntime);
	}

	//事件 消息显示结束
	private void EvtItemShowEndTopRightTip(ItemMsgTopRightTip msgItem)
	{
		//显示下一条 右上消息
		OnShowMsgTopRightTip();
	}
	#endregion

	#region 全屏消息
	private NotificationWindowArg m_ArgFullScreenCur;

	private void AddArgQueueFullScreenTip(NotificationWindowArg arg)
	{
		//添加进队列
		m_FullScreenTipQueueMsgArg.Enqueue(arg);

		ShowMsgFullScreenTip();
	}

	private void ShowMsgFullScreenTip()
	{
		if (!m_FullScreenTipIsShow)
		{
			OnShowMsgFullScreenTip();
		}
	}

	private void OnShowMsgFullScreenTip()
	{
		m_FullScreenTipIsShow = true;
		if (m_FullScreenTipQueueMsgArg.Count == 0)
		{
			//消息队列数量为0 结束显示
			m_FullScreenTipIsShow = false;
			return;
		}

		//正在使用
		if (m_ItemMsgFullScreenTip.GameObjectGet.activeSelf) { return; }

		//将消息数据设置进Item实例
		m_ArgFullScreenCur = m_FullScreenTipQueueMsgArg.Dequeue();
		if (string.IsNullOrEmpty(m_ArgFullScreenCur.Text))
		{
			//StoryDialague配置表 对话文本
			int id;
			if (int.TryParse(m_ArgFullScreenCur.MsgTypeParam, out id))
			{
				m_ItemMsgFullScreenTip.Show(id, EvtItemShowEndFullScreenTip);
			}
		}
		else
		{
			//显示单条文本
			m_ItemMsgFullScreenTip.Show(m_ArgFullScreenCur.Text, EvtItemShowEndFullScreenTip);
		}
	}

	//事件 消息显示结束
	private void EvtItemShowEndFullScreenTip()
	{
		//显示完成 回调
		m_ArgFullScreenCur.OnShowComplete?.Invoke();

		//显示下一条消息
		OnShowMsgFullScreenTip();
	}
	#endregion
}
