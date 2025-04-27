using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Audio/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    public List<AudioClipInfo> audioClips;

    public AudioClip GetClip(string key)
    {
        return audioClips.Find(c => c.key == key)?.clip;
    }
    
    public List<string> GetKeysByPrefix(string prefix)
    {
        return audioClips
            .Where(c => c.key.StartsWith(prefix))
            .Select(c => c.key)
            .ToList();
    }
}

[System.Serializable]
public class AudioClipInfo
{
    public string key;
    public AudioClip clip;
}