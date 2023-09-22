using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Audio[] audios;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("AudioManager");

        if (objs.Length > 1) Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);

        foreach(Audio a in audios)
        {
            a.source = gameObject.AddComponent<AudioSource>();
            a.source.clip = a.clip;
            a.source.outputAudioMixerGroup = a.group;

            a.source.volume = a.volume;
            a.source.pitch = a.pitch;
            a.source.loop = a.loop;
        }
    }

    public void Play(string name)
    {
        Audio a = Array.Find(audios, audio => audio.name == name);
        if (a == null)
        {
            Debug.LogWarning("Audio name '" + name + "' not found idiot");
        }
        a.source.Play();
    }

    public void Stop(string name)
    {
        Audio a = Array.Find(audios, audio => audio.name == name);
        if (a == null)
        {
            Debug.LogWarning("Audio name '" + name + "' not found idiot");
        }
        a.source.Stop();
    }

    public void Pause(string name)
    {
        Audio a = Array.Find(audios, audio => audio.name == name);
        if (a == null)
        {
            Debug.LogWarning("Audio name '" + name + "' not found idiot");
        }
        a.source.Pause();
    }

    public void Unpause(string name)
    {
        Audio a = Array.Find(audios, audio => audio.name == name);
        if (a == null)
        {
            Debug.LogWarning("Audio name '" + name + "' not found idiot");
        }
        a.source.UnPause();
    }

    public void StopAll()
    {
        foreach (Audio a in audios)
        {
            if (a == null)
            {
                Debug.LogWarning("Audio name '" + name + "' not found idiot");
            }
            a.source.Stop();
        }
    }
}
