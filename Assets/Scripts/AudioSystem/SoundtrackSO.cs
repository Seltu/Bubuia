using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;


[CreateAssetMenu(fileName = "Soundtrack", menuName = "AudioSystem/Soundtrack List")]
public class SoundtrackSO : ScriptableObject
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private List<SoundEntry> _sounds;

    public AudioClip GetClipByName(string name)
    {
        foreach (var sound in _sounds)
        {
            if (sound.name == name)
            {
                return sound.clip;
            }
        }
        return null;
    }

    public AudioMixerGroup GetClipMixerGroup(AudioClip clip)
    {
        foreach (var sound in _sounds)
        {
            if (sound.clip == clip)
            {
                return _mixer.FindMatchingGroups(sound.mixerGroup)[0];
            }
        }
        return null;
    }
}

[System.Serializable]
public class SoundEntry
{
    public string name;
    public AudioClip clip;
    public string mixerGroup;
}