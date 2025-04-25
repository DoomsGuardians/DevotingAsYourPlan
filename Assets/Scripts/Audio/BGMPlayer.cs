using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class BGMPlayer
{
    private AudioSource source;
    private AudioConfig config;
    private string[] currentPlaylist;
    private int currentTrackIndex = 0;
    private bool isRandom = false;
    private bool isLooping = false;

    // 构造函数
    public BGMPlayer(AudioSource source, AudioConfig config)
    {
        this.source = source;
        this.config = config;
        source.loop = false;  // 默认不循环
    }

    // 设置播放模式
    public void SetPlayMode(bool random, bool loop)
    {
        isRandom = random;
        isLooping = loop;
    }

    // 播放顺序列表
    public void PlaySequence(params string[] keys)
    {
        currentPlaylist = keys;
        currentTrackIndex = 0;
        PlayCurrentTrack();
    }

    // 播放随机列表
    public void PlayRandom(params string[] keys)
    {
        currentPlaylist = keys;
        currentTrackIndex = UnityEngine.Random.Range(0, keys.Length);
        PlayCurrentTrack();
    }

    // 播放当前曲目
    private void PlayCurrentTrack()
    {
        string currentTrack = currentPlaylist[currentTrackIndex];
        source.clip = config.GetClip(currentTrack);
        source.Play();
        if (source.clip != null)
        {
            source.Play();
            source.loop = isLooping; // 如果是循环模式，设为 true
            Debug.Log($"Playing: {currentTrack}");

            // 注册播放结束事件
            if (!isLooping)
            {
                UniTask.Delay(TimeSpan.FromSeconds(source.clip.length)).ContinueWith(() => OnTrackFinished());
            }
        }
    }

    // 播放完一首后自动切换
    private void OnTrackFinished()
    {
        if (isRandom)
        {
            // 随机播放下一首
            currentTrackIndex = UnityEngine.Random.Range(0, currentPlaylist.Length);
        }
        else
        {
            // 顺序播放下一首
            currentTrackIndex++;
            if (currentTrackIndex >= currentPlaylist.Length)
            {
                currentTrackIndex = 0;  // 循环播放列表
            }
        }

        // 播放下一首
        PlayCurrentTrack();
    }

    // 淡出音效，持续一段时间
    public async UniTask FadeOut(float duration)
    {
        float time = 0f;
        float startVolume = source.volume;

        while (time < duration)
        {
            source.volume = Mathf.Lerp(startVolume, 0, time / duration);
            time += Time.deltaTime;
            await UniTask.Yield();
        }

        source.Stop();
        source.volume = startVolume;
    }

    // 淡入音效
    public async UniTask FadeIn(string key, float duration)
    {
        var clip = config.GetClip(key);
        if (clip == null) return;

        source.clip = clip;
        source.volume = 0;
        source.Play();

        float time = 0f;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(0, 1f, time / duration);
            time += Time.deltaTime;
            await UniTask.Yield();
        }

        source.volume = 1f;
    }
}
