using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DG.Tweening;

public enum AudioEnum
{
    /// <summary>
    /// 背景音乐，同时只有一个，循环
    /// </summary>
    Music,

    /// <summary>
    /// 短音效，同时只有两个，不循环
    /// </summary>
    Sound,

    /// <summary>
    /// 短音效，同时可多个，循环
    /// </summary>
    LoopSound,

    /// <summary>
    /// 短音效，同时只有两个，不循环
    /// </summary>
    SoundAdd,

    /// <summary>
    /// 语音，同时只有一个，不循环
    /// </summary>
    Speak,
}

/// <summary>
/// 通用音效
/// </summary>
public static class UIAudioEnum
{
    /// <summary>
    /// 如果你忘了设置，那就是这个音乐
    /// </summary>
    public static string Defalut = "UI-General-Touch";
    /// <summary>
    /// 通用确认，ok，对号等
    /// </summary>
    public static string DefalutOK = "UI-General-Enter";
    /// <summary>
    /// 关闭按钮，点击任意位置关闭等，适合小界面的关闭
    /// </summary>
    public static string DefalutClose = "UI-General-Close";
    /// <summary>
    /// 也是关闭，感觉适合大界面的关闭
    /// </summary>
    public static string DefalutBack = "UI-General-Return";
    /// <summary>
    /// 切换
    /// </summary>
    public static string DefalutSwitch = "UI-General-Tab";
    /// <summary>
    /// 功能未开放
    /// </summary>
    public static string DefalutShut = "UI-General-NegativeClick";
}

public class AudioModel : MonoBehaviourSingleton<AudioModel>
{
    [SerializeField] private AudioGroupComponent[] audioGroupComponents = default;

    private Dictionary<AudioEnum, AudioGroupComponent> audioDic;
    Dictionary<AudioEnum, bool> isPlayingDict = new Dictionary<AudioEnum, bool>();

    Coroutine fadeInCoroutine;
    Coroutine fadeOutCoroutine;

    public Dictionary<AudioEnum, float> audioMaxVolumes = new Dictionary<AudioEnum, float>();

    private void Awake()
    {
        InitAudioGroup();
    }

    /// <summary>
    /// 播放 背景音乐
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="volume"></param>
    /// <param name="callBack"></param>
    public void PlayMusic(string assetName, float volume = 1.0f, Action<AudioClip> callBack = null)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(AudioEnum.Music);
        if (audioGroupInfo.audioSource.clip != null && audioGroupInfo.asset == assetName)
        {
            return;
        }

        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
        }

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        Play(AudioEnum.Music, assetName, true, volume, callBack);
    }

    /// <summary>
    /// 播放 上一个 背景音乐
    /// </summary>
    /// <param name="volume"></param>
    public void PlayLastMusic(float volume = 1.0f)
    {
        AudioModel.Instance.Stop(AudioEnum.Music);
    }

    /// <summary>
    /// 播放 音效
    /// </summary>
    /// <param name="assetName"></param>
    public void PlaySound(string assetName)
    {
        Play(AudioEnum.Sound, assetName, true);
    }

    public void PlaySoundAdd(string assetName)
    {
        Play(AudioEnum.SoundAdd, assetName, true);
    }

    /// <summary>
    /// 播放 音效 循环
    /// </summary>
    /// <param name="assetName"></param>
    public void PlayLoopSound(string assetName)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(AudioEnum.LoopSound);
        if (audioGroupInfo.audioSource.clip != null && audioGroupInfo.asset == assetName)
        {
            return;
        }
        Play(AudioEnum.LoopSound, assetName, true);
    }

    /// <summary>
    /// 播放 语音
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callBack"></param>
    public void PlaySpeak(string assetName, Action<AudioClip> callBack = null)
    {
        Play(AudioEnum.Speak, assetName, true, 1f, callBack);
    }

    private void InitAudioGroup()
    {
        audioDic = new Dictionary<AudioEnum, AudioGroupComponent>();
        if (audioGroupComponents == null)
        {
            return;
        }
        for (int i = 0; i < audioGroupComponents.Length; i++)
        {
            AudioGroupComponent audioGroupComponent = audioGroupComponents[i];
            audioDic.Add(audioGroupComponent.audioEnum, audioGroupComponent);
            isPlayingDict.Add(audioGroupComponent.audioEnum, false);
            audioMaxVolumes.Add(audioGroupComponent.audioEnum, GetMaxVolume(audioGroupComponent.audioEnum));
            GameObject gobj = new GameObject(audioGroupComponent.name);
            gobj.transform.parent = audioGroupComponent.transform;
            audioGroupComponent.audioSource = gobj.AddComponent<AudioSource>();
            audioGroupComponent.audioSource.loop = audioGroupComponent.isLoop;
        }
    }

    private float GetMaxVolume(AudioEnum audioEnum)
    {
        float defaultV = 1;
        switch (audioEnum)
        {
            case AudioEnum.Music:
                defaultV = 0.3f;
                break;
        }

        return PlayerPrefsUtil.GetFloat("AudioEnum:" + audioEnum.ToString(), defaultV, false);
    }

    private void SetMaxVolume(AudioEnum audioEnum,float value)
    {
        PlayerPrefsUtil.SetFloat("AudioEnum:" + audioEnum.ToString(), value, false);
        audioMaxVolumes[audioEnum] = value;
        GetAudioSource(audioEnum).volume = audioMaxVolumes[audioEnum];
    }

    private AudioGroupComponent GetAudioGroup(AudioEnum audioEnum)
    {
        if (audioDic == null)
        {
            return null;
        }
        AudioGroupComponent audioGroupComponent;
        if (audioDic.TryGetValue(audioEnum, out audioGroupComponent))
        {
            return audioGroupComponent;
        }
        return null;
    }

    private void Play(AudioEnum audioEnum, string assetName, bool stopPre = false, float volume = 1.0f, Action<AudioClip> callBack = null)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            //Debug.Log("audio " + assetName + " is null");
            return;
        }
        //else
        //{
        //    Debug.LogError("audio " + assetName);
        //}

        AudioGroupComponent audioGroupInfo = GetAudioGroup(audioEnum);

        if (audioGroupInfo == null)
        {
            return;
        }
        string audioPath = Enum.GetName(typeof(AudioEnum), audioEnum);
        if (audioEnum == AudioEnum.SoundAdd || audioEnum == AudioEnum.LoopSound)
        {
            audioPath = Enum.GetName(typeof(AudioEnum), AudioEnum.Sound);
        }
        string assetPath = AssetAddressUtil.GetAudioAddress(audioPath, assetName);
        if (stopPre)
        {
            AudioSource audioSource = audioGroupInfo.audioSource;

            if (audioSource.clip != null)
            {
                if (audioGroupInfo.asset == assetName)
                {
                    isPlayingDict[audioEnum] = true;
                    audioSource.SetScheduledStartTime(0);
                    audioSource.volume = audioMaxVolumes[audioEnum] * volume;
                    audioSource.Play();
                    callBack?.Invoke(audioSource.clip);
                    return;
                }
                else
                {
                    OnStop(audioEnum, audioGroupInfo);
                }
            }
        }

        isPlayingDict[audioEnum] = true;
        AssetAddressSystem.Instance.LoadAsset<AudioClip>(assetPath, (Result) =>
        {
            AudioClip audioClip = (AudioClip)Result;
            audioGroupInfo.asset = assetName;
            audioGroupInfo.audioSource.clip = audioClip;
            audioGroupInfo.audioSource.volume = audioMaxVolumes[audioEnum] * volume;
            if (audioGroupInfo.audioSource.clip != null)
            {
                if(isPlayingDict[audioEnum] == true)
                {
                    audioGroupInfo.audioSource.Play();
                }
                callBack?.Invoke(audioClip);
            }
            else
            {
                Debug.Log("audio " + assetPath + " is null");
            }
        });
    }

    public bool IsPlaying(AudioEnum audioEnum)
    {
        return isPlayingDict[audioEnum];
    }

    private string GetAudioSourceAssetName(AudioEnum audioEnum)
    {
        return GetAudioGroup(audioEnum).asset;
    }

    private AudioSource GetAudioSource(AudioEnum audioEnum)
    {
        return GetAudioGroup(audioEnum).audioSource;
    }

    public float GetVolume(AudioEnum audioEnum)
    {
        return GetAudioGroup(audioEnum).audioSource.volume / audioMaxVolumes[audioEnum];
    }

    public void SetVolume(AudioEnum audioEnum,float volume)
    {
        GetAudioGroup(audioEnum).audioSource.volume = audioMaxVolumes[audioEnum] * volume;
    }

    public void Pause(AudioEnum audioEnum)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(audioEnum);
        audioGroupInfo.audioSource.Pause();
    }

    public void Resume(AudioEnum audioEnum)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(audioEnum);
        audioGroupInfo.audioSource.UnPause();
    }

    /// <summary>
    /// 停止音频
    /// </summary>
    /// <param name="audioEnum"></param>
    public void Stop(AudioEnum audioEnum)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(audioEnum);
        OnStop(audioEnum, audioGroupInfo);
    }

    private void OnStop(AudioEnum audioEnum, AudioGroupComponent audioGroupInfo)
    {
        isPlayingDict[audioEnum] = false;
        if (audioGroupInfo.audioSource.clip != null)
        {
            string assetName = audioGroupInfo.asset;

            if (audioEnum == AudioEnum.SoundAdd || audioEnum == AudioEnum.LoopSound)
            {
                audioEnum = AudioEnum.Sound;
            }

            string audioPath = Enum.GetName(typeof(AudioEnum), audioEnum);

            audioGroupInfo.audioSource.Stop();

            //todo UI音效暂不卸载，同时加载卸载address有不明bug
            //if (audioEnum != AudioEnum.Sound)
            {
                string assetPath = AssetAddressUtil.GetAudioAddress(audioPath, assetName);

                audioGroupInfo.audioSource.clip.UnloadAudioData();
                audioGroupInfo.audioSource.clip = null;

                AssetAddressSystem.Instance.UnloadAsset(assetPath, 0);
            }
        }
    }

    public void FadeInBGM(float fadeTime, float startVolume = 0f, float targetVolume = 1f, Action fadeCallback = null)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(AudioEnum.Music);
        if(fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
            fadeInCoroutine = null;
        }

        audioGroupInfo.audioSource.volume = audioMaxVolumes[audioGroupInfo.audioEnum] * startVolume;
        fadeInCoroutine = StartCoroutine(StartFade(audioGroupInfo.audioSource, fadeTime, audioMaxVolumes[audioGroupInfo.audioEnum] * targetVolume, () =>
        {
            if (fadeCallback != null)
            {
                fadeCallback.Invoke();
            }

            fadeInCoroutine = null;
        }));
    }

    public void FadeOutBGM(float fadeTime, float startVolume = 1f, float targetVolume = 0f, Action fadeCallback = null)
    {
        AudioGroupComponent audioGroupInfo = GetAudioGroup(AudioEnum.Music);
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        audioGroupInfo.audioSource.volume = audioMaxVolumes[audioGroupInfo.audioEnum] * startVolume;
        fadeOutCoroutine = StartCoroutine(StartFade(audioGroupInfo.audioSource, fadeTime, audioMaxVolumes[audioGroupInfo.audioEnum] * targetVolume, () =>
        {
            if (fadeCallback != null)
            {
                fadeCallback.Invoke();
            }

            fadeOutCoroutine = null;
        }));
    }

    IEnumerator StartFade(AudioSource audioSouce, float duration, float targetVolume, Action fadeCallback = null)
    {
        float currentTime = 0f;
        float startVolume = audioSouce.volume;

        while(currentTime < duration)
        {
            currentTime += Time.unscaledDeltaTime;
            audioSouce.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        if(fadeCallback != null)
        {
            fadeCallback.Invoke();
        }

        yield break;
    }
}
