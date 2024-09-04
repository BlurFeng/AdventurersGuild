using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class AsyncLoadWindow : WindowBase
{
    struct OpenArgs
    {
        /// <summary>
        /// 淡入后回调
        /// </summary>
        public Action FadeInCallback;
        /// <summary>
        /// 自动淡出
        /// </summary>
        public bool AutoFadeOut;
        /// <summary>
        /// 显示 加载中动画
        /// </summary>
        public bool ShowLoadingAnim;
        /// <summary>
        /// 淡入秒数
        /// </summary>
        public float FadeInSeconds;
        /// <summary>
        /// 最小等待秒数
        /// </summary>
        public float MinWaitSeconds;
    }

    public static void FadeIn(Action fadeInCallback = null, bool autoFadeOut = true, bool showLoadingAnim = true, float fadeInSeconds = 0.3f, float minWaitSeconds = 3f)
    {
        OpenArgs args = new OpenArgs();
        args.FadeInCallback = fadeInCallback;
        args.AutoFadeOut = autoFadeOut;
        args.ShowLoadingAnim = showLoadingAnim;
        args.FadeInSeconds = fadeInSeconds;
        args.MinWaitSeconds = minWaitSeconds;
        WindowSystem.Instance.OpenWindow(WindowEnum.AsyncLoadWindow, args);
    }

    public static void FadeOut(float fadeOutSeconds = 1.2f)
    {
        AsyncLoadWindow asyncLoadWindow = WindowSystem.Instance.GetWindow(WindowEnum.AsyncLoadWindow) as AsyncLoadWindow;
        if (asyncLoadWindow != null)
        {
            asyncLoadWindow.IsLoadComplete = true;
            asyncLoadWindow.OnFadeOut(fadeOutSeconds);
        }
    }

    [SerializeField] private Image m_ImgFade = null; //贴图 淡入淡出
    [SerializeField] private CanvasGroup m_CGLoadingAnim = null; //画布组 载入中动画

    /// <summary>
    /// 加载完成
    /// </summary>
    public bool IsLoadComplete { get { return m_IsLoadComplete; } set { m_IsLoadComplete = value; } }
    private bool m_IsLoadComplete; //加载完成
    private bool m_IsMinWaitSecondsComplete; //最小等待秒数 完成
    private Color m_ColorImgFadeDefault; //颜色 贴图淡入 默认

    public override void OnLoaded()
    {
        base.OnLoaded();
        MemoryLock = true;

        m_ColorImgFadeDefault = m_ImgFade.color;
    }

    public override void OnOpen(object userData = null)
    {
        base.OnOpen(userData);

        var args = (OpenArgs)userData;
        IsLoadComplete = false;
        OnFadeIn(args);
    }

    private void OnFadeIn(OpenArgs args)
    {
        DOTween.Kill(m_ImgFade);
        m_CGLoadingAnim.alpha = 0f;
        m_ImgFade.color = new Color(m_ColorImgFadeDefault.r, m_ColorImgFadeDefault.g, m_ColorImgFadeDefault.b, 0);
        //淡入 背景
        m_ImgFade.DOFade(m_ColorImgFadeDefault.a, args.FadeInSeconds).OnComplete(() =>
        {
            if (args.ShowLoadingAnim)
            {
                //淡入 载入中动画
                m_CGLoadingAnim.DOFade(1, 0.2f);
            }

            args.FadeInCallback?.Invoke();
        });

        //最小等待秒数
        StartCoroutine(CorMinWaitSeconds(args.MinWaitSeconds, args.AutoFadeOut));
    }

    private void OnFadeOut(float fadeOutSeconds = 1.2f)
    {
        if (!m_IsLoadComplete || !m_IsMinWaitSecondsComplete) { return; }

        DOTween.Kill(m_ImgFade, true);

        //淡出 载入中动画
        m_CGLoadingAnim.DOFade(0, 0.2f).OnComplete(() => 
        {
            //淡出 背景
            m_ImgFade.DOFade(0, fadeOutSeconds).OnComplete(() => { CloseWindow(); });
        });
    }

    IEnumerator CorMinWaitSeconds(float minWaitSeconds, bool autoFadeOut)
    {
        m_IsMinWaitSecondsComplete = false;

        yield return new WaitForSeconds(minWaitSeconds);

        m_IsMinWaitSecondsComplete = true;
        
        //是否 自动淡出
        if(autoFadeOut)
        {
            FadeOut();
        }
        else
        {
            OnFadeOut();
        }
    }
}
