using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public static class LevelLoader
{
    public static LevelData CurrentLevel { get; private set; }

    public static async void LoadLevel(LevelData level)
    {
        AudioManager.Instance.FadeOutBGM(0.5f);
        CurrentLevel = level;
        LoadingUI.Instance.Show();

        AsyncOperation op = SceneManager.LoadSceneAsync(level.sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            LoadingUI.Instance.SetProgress(op.progress);
            await UniTask.Yield();
        }

        await UniTask.Delay(500);
        LoadingUI.Instance.SetProgress(1f);
        await UniTask.Delay(300);

        SceneManager.sceneLoaded += OnSceneLoaded;
        op.allowSceneActivation = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadingUI.Instance.FadeOutAndHide();
        SceneManager.sceneLoaded -= OnSceneLoaded; // 记得解绑，防止重复调用
    }
}