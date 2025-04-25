using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    public List<AudioClipInfo> audioClips;

    public AudioClip GetClip(string key)
    {
        return audioClips.Find(c => c.key == key)?.clip;
    }
}

[System.Serializable]
public class AudioClipInfo
{
    public string key;
    public AudioClip clip;
}