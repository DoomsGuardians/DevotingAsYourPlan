using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Cysharp.Threading.Tasks;

public class OptionsMenu : MonoSingleton<OptionsMenu>
{
    [Header("UI Elements")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown displayModeDropdown;
    [SerializeField] Slider volumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] GameObject panel;
    [SerializeField] Button restartButton;
    [SerializeField] private Button backMainButton;

    private Resolution[] resolutions;

    private void Start()
    {
        SetupUI();
        panel.SetActive(false); // 初始隐藏
    }
    

    private void SetupUI()
    {
        // 初始化分辨率选项
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        int currentIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resolutionDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(res.ToString()));
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                currentIndex = i;
        }
        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "窗口模式",
            "全屏模式",
            "无边框窗口"
        });
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentIndex);
        displayModeDropdown.value = PlayerPrefs.GetInt("DisplayMode", 1);
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        resolutionDropdown.RefreshShownValue();
        displayModeDropdown.RefreshShownValue();

        // 游戏中才显示“重开一局”按钮
        if (restartButton != null)
        {
            restartButton.transform.parent.gameObject.SetActive(GameManager.IsExisted);
        }

        if (backMainButton != null)
        {
            backMainButton.transform.parent.gameObject.SetActive(GameManager.IsExisted);
        }
    }

    public void OnResolutionChanged(int index)
    {
        SettingsManager.SaveResolution(index);
    }

    public void OnDisplayModeChanged(int index)
    {
        SettingsManager.SaveDisplayMode(index);
    }

    public void OnVolumeChanged(float value)
    {
        SettingsManager.SaveVolume(value);
    }

    public async void OnRestartGamePressed()
    {
        if (GameManager.Instance != null)
        {
            panel.SetActive(false);
            await GameManager.Instance.RestartGameSession();
        }
        else
        {
            Debug.LogWarning("GameManager 不存在，无法重新开始游戏。");
        }
    }

    public void OnBGMVolumeChanged(float value)
    {
        SettingsManager.SaveBGMVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        SettingsManager.SaveSFXVolume(value);
    }
    
    public async void OnReturnToMainMenuPressed()
    {
        LoadingUI.Instance.ShowWithText("返回主菜单中...");

        if (GameManager.IsExisted)
            Destroy(GameManager.Instance);

        if (OptionsMenu.IsExisted)
            OptionsMenu.Instance.panel.SetActive(false);

        if (UIPanelManager.IsExisted)
            UIPanelManager.Instance.Clear();

        var op = SceneManager.LoadSceneAsync("MainMenu");
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            LoadingUI.Instance.SetProgress(op.progress);
            await UniTask.Yield();
        }

        LoadingUI.Instance.SetProgress(1f);

        bool sceneLoaded = false;
        SceneManager.sceneLoaded += (_, _) => sceneLoaded = true;

        op.allowSceneActivation = true;

        // 等待 Unity 真正完成场景切换
        await UniTask.WaitUntil(() => sceneLoaded);
        await UniTask.NextFrame(); // 多等一帧，确保画面已渲染

        await LoadingUI.Instance.FadeOutAndHide();
    }
    
    
    public void Show()
    {
        SetupUI(); // 可选：每次打开前刷新
        panel.SetActive(true);
        UIPanelManager.Instance.Show(panel);
    }
    
    public override bool IsNotDestroyOnLoad() => true;
}
