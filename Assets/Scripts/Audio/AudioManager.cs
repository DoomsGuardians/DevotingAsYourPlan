using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioConfig audioConfig;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private BGMPlayer bgmPlayer;
    private SFXPlayer sfxPlayer;

    protected override void Awake()
    {
        base.Awake();
        bgmPlayer = new BGMPlayer(bgmSource, audioConfig);
        sfxPlayer = new SFXPlayer(sfxSource, audioConfig);
    }

    // 播放 SFX 音效
    public void PlaySFX(string key)
    {
        sfxPlayer.Play(key);
    }

    // 设置 BGM 播放模式：顺序播放/随机播放
    public void SetBGMPlayMode(bool random, bool loop)
    {
        bgmPlayer.SetPlayMode(random, loop);
    }

    // 顺序播放 BGM
    public void PlayBGM(params string[] bgmKeys)
    {
        bgmPlayer.PlaySequence(bgmKeys);
    }

    // 随机播放 BGM
    public void PlayRandomBGM(params string[] bgmKeys)
    {
        bgmPlayer.PlayRandom(bgmKeys);
    }

    // 淡出 BGM
    public async UniTask FadeOutBGM(float duration)
    {
        await bgmPlayer.FadeOut(duration);
    }

    // 淡入指定 BGM
    public async UniTask FadeInBGM(string key, float duration)
    {
        await bgmPlayer.FadeIn(key, duration);
    }

    // 播放指定的 BGM 曲目
    public void PlaySpecificBGM(string bgmKey)
    {
        bgmPlayer.PlaySequence(bgmKey);
    }
}