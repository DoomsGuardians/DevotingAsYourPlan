using UnityEngine;

public class SFXPlayer
{
    private AudioSource source;
    private AudioConfig config;

    public SFXPlayer(AudioSource source, AudioConfig config)
    {
        this.source = source;
        this.config = config;
    }

    public void Play(string key)
    {
        var clip = config.GetClip(key);
        if (clip != null)
        {
            source.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX Key not found: {key}");
        }
    }
}