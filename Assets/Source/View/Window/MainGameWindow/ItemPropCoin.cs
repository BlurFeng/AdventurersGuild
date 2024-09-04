using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Deploy;
using com.ootii.Messages;

public class ItemPropCoin : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_TxtPropertyCoinCopperCount = null; //文本 铜币 数量
    [SerializeField] private TextMeshProUGUI m_TxtPropertyCoinSilverCount = null; //文本 银币 数量
    [SerializeField] private TextMeshProUGUI m_TxtPropertyCoinGoldCount = null; //文本 金币 数量

    private GameObject m_GoCoinCopper;
    private GameObject m_GoCoinSilver;
    private GameObject m_GoCoinGold;

    private void Awake()
    {
        m_GoCoinCopper = m_TxtPropertyCoinCopperCount.gameObject;
        m_GoCoinSilver = m_TxtPropertyCoinSilverCount.gameObject;
        m_GoCoinGold = m_TxtPropertyCoinGoldCount.gameObject;

        MessageDispatcher.AddListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_PROPERTY, MsgCoinPropInfoChange);
    }

    private void OnDestroy()
    {
        MessageDispatcher.RemoveListener(PlayerModelMsgType.PROP_BACKPACK_CHANGE_PROPERTY, MsgCoinPropInfoChange);
    }

    /// <summary>
    /// 设置 钱币数量
    /// </summary>
    /// <param name="count"></param>
    public void SetCoinCount(int count)
    {
        //铜币数量
        int copperCount = (count % 100);
        if (copperCount <= 0)
        {
            //m_GoCoinCopper.SetActive(false);
            m_TxtPropertyCoinCopperCount.text = "00";
        }
        else
        {
            //m_GoCoinCopper.SetActive(true);
            m_TxtPropertyCoinCopperCount.text = copperCount.ToString();
        }
        //银币数量
        int silverCount = (count % 10000 / 100);
        if (silverCount <= 0)
        {
            //m_GoCoinSilver.SetActive(false);
            m_TxtPropertyCoinSilverCount.text = "00";
        }
        else
        {
            //m_GoCoinSilver.SetActive(true);
            m_TxtPropertyCoinSilverCount.text = silverCount.ToString();
        }
        //金币数量
        int goldCount = (count / 10000);
        if (goldCount <= 0)
        {
            //m_GoCoinGold.SetActive(false);
            m_TxtPropertyCoinGoldCount.text = "00";
        }
        else
        {
            //m_GoCoinGold.SetActive(true);
            m_TxtPropertyCoinGoldCount.text = goldCount.ToString();
        }
    }

    //消息 钱币信息改变
    private void MsgCoinPropInfoChange(IMessage msg)
    {
        var propInfo = msg.Data as PropInfo;
        if (propInfo == null) { return; }

        SetCoinCount(propInfo.Count);
    }
}
