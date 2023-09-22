using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Audio
{
    public string name;

    public AudioClip clip;
    public AudioMixerGroup group;
    public bool loop;

    [Range(0f,1f)] public float volume;
    [Range(.1f, 3f)] public float pitch;

    [HideInInspector] public AudioSource source;
}
