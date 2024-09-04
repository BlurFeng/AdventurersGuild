using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class DeveloperWindow : WindowBase
{
    [SerializeField] private List<Image> m_ListImgLogos = null; //列表 贴图 标志图

    public override void OnLoaded()
    {
        base.OnLoaded();
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        //开发跳过 直接进入主界面
        WindowSystem.Instance.OpenWindow(WindowEnum.MainMenuWindow); //主选单界面
        WindowSystem.Instance.OpenWindow(WindowEnum.CursorWindow); //光标界面
        WindowSystem.Instance.OpenWindow(WindowEnum.NotificationWindow); //通用消息界面
        WindowSystem.Instance.CloseWindow(WindowEnum.DeveloperWindow);

        //StartCoroutine(CorAnim());
    }

    IEnumerator CorAnim()
    {
        //隐藏所有Logo
        for (int i = 0; i < m_ListImgLogos.Count; i++)
        {
            var imgLoga = m_ListImgLogos[i];
            imgLoga.color = new Color(1, 1, 1, 0);
        }

        //依次展示所有Logo
        for (int i = 0; i < m_ListImgLogos.Count; i++)
        {
            yield return new WaitForSeconds(2f);

            var imgLoga = m_ListImgLogos[i];
            imgLoga.DOFade(1, 0.6f);
            yield return new WaitForSeconds(3f);

            imgLoga.DOFade(0, 0.6f);
        }

        yield return new WaitForSeconds(2f);

        AsyncLoadWindow.FadeIn(() =>
        {
            WindowSystem.Instance.OpenWindow(WindowEnum.MainMenuWindow); //主选单界面
            WindowSystem.Instance.OpenWindow(WindowEnum.CursorWindow); //光标界面
            WindowSystem.Instance.OpenWindow(WindowEnum.NotificationWindow); //通用消息界面
            WindowSystem.Instance.CloseWindow(WindowEnum.DeveloperWindow);
        }, false, false, 0f, 0f);
    }
}
