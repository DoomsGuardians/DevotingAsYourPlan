using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string sceneName;
    [TextArea] public string description;
    public Sprite previewImage;
}