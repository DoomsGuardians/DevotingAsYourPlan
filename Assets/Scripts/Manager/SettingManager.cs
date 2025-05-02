using UnityEngine;

public static class SettingsManager
{
    private const string KeyRes = "ResolutionIndex";
    private const string KeyDisplay = "DisplayMode";
    private const string KeyVolume = "Volume";
    private const string KeyBGMVolume = "BGMVolume";
    private const string KeySFXVolume = "SFXVolume";

    public static void ApplyAllSettings()
    {
        ApplyResolution();
        ApplyDisplayMode();
        ApplyVolume();
        ApplyBGMVolume();
        ApplySFXVolume();
    }

    // 分辨率
    public static void SaveResolution(int index)
    {
        PlayerPrefs.SetInt(KeyRes, index);
        ApplyResolution();
    }

    public static void ApplyResolution()
    {
        int index = PlayerPrefs.GetInt(KeyRes, Screen.resolutions.Length - 1);
        var res = Screen.resolutions[Mathf.Clamp(index, 0, Screen.resolutions.Length - 1)];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
    }

    // 显示模式：0 窗口、1 全屏、2 无边框
    public static void SaveDisplayMode(int mode)
    {
        PlayerPrefs.SetInt(KeyDisplay, mode);
        ApplyDisplayMode();
    }

    public static void ApplyDisplayMode()
    {
        int mode = PlayerPrefs.GetInt(KeyDisplay, 0);
        FullScreenMode selected = mode switch
        {
            0 => FullScreenMode.Windowed,
            1 => FullScreenMode.ExclusiveFullScreen,
            2 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };
        Screen.fullScreenMode = selected;
    }

    // 总音量
    public static void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat(KeyVolume, volume);
        ApplyVolume();
    }

    public static void ApplyVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(KeyVolume, 1f);
    }

    // BGM 音量
    public static void SaveBGMVolume(float value)
    {
        PlayerPrefs.SetFloat(KeyBGMVolume, value);
        ApplyBGMVolume();
    }

    public static void ApplyBGMVolume()
    {
        float volume = PlayerPrefs.GetFloat(KeyBGMVolume, 1f);
        if (AudioManager.IsExisted)
            AudioManager.Instance.SetBGMVolume(volume);
    }

    // SFX 音量
    public static void SaveSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(KeySFXVolume, value);
        ApplySFXVolume();
    }

    public static void ApplySFXVolume()
    {
        float volume = PlayerPrefs.GetFloat(KeySFXVolume, 1f);
        if (AudioManager.IsExisted)
            AudioManager.Instance.SetSFXVolume(volume);
    }
}
