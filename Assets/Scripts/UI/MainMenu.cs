using System;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private void Start()
    {
        // 随机播放背景音乐
        AudioManager.Instance.PlayRandomBGM(AudioManager.Instance.audioConfig.GetKeysByPrefix("bgm_").ToArray());
    }

    [SerializeField] private GameObject levelSelectPanel;

    public void OnStartGamePressed()
    {
        UIPanelManager.Instance.Show(levelSelectPanel);
    }

    public void OnOptionsPressed()
    {
        OptionsMenu.Instance.Show();
    }

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
}