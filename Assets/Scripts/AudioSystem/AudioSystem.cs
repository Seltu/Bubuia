using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

public class AudioSystem : Singleton<AudioSystem>
{
    [SerializeField] private SoundtrackSO _soundtrackSO;
    [SerializeField] private AudioMixer _audioMixer;

    private readonly string _masterVolumeParam = "MasterVolume";
    private readonly string _bgmVolumeParam = "BGMVolume";
    private readonly string _sfxVolumeParam = "SFXVolume";

    private AudioSource _bgmAudioSource;
    private List<AudioSource> _sfxAudioSources = new List<AudioSource>(); // Pool of AudioSources for SFX

    private int _maxSfxSources = 10; // Limit the number of simultaneous SFX

    private void Start()
    {
        if (_bgmAudioSource == null)
            _bgmAudioSource = gameObject.AddComponent<AudioSource>();

        // Initialize the SFX pool
        for (int i = 0; i < _maxSfxSources; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            _sfxAudioSources.Add(source);
        }

        // Load volume settings
        float masterVolume = PlayerPrefs.GetFloat("MasterVolumeKey", 1f);
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolumeKey", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolumeKey", 1f);

        SetMasterVolume(masterVolume);
        SetBackgroundMusicVolume(bgmVolume);
        SetSoundEffectsVolume(sfxVolume);
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene currentScene)
    {
        StopAllLoopingSFX();
    }

    // Plays a looping background music track
    public void PlayBGM(string soundName)
    {
        if (_soundtrackSO == null) return;
        var clip = _soundtrackSO.GetClipByName(soundName);
        if (clip == null) return;
        if (_bgmAudioSource.clip == clip && _bgmAudioSource.isPlaying)
        {
            return;
        }
        _bgmAudioSource.clip = clip;
        _bgmAudioSource.loop = true;
        _bgmAudioSource.Play();
        _bgmAudioSource.outputAudioMixerGroup = _soundtrackSO.GetClipMixerGroup(clip);
    }

    // Stops the background music
    public void StopBackgroundMusic()
    {
        _bgmAudioSource.Stop();
    }

    // Plays a one-shot sound effect
    public void PlaySFX(string soundName)
    {
        var clip = _soundtrackSO?.GetClipByName(soundName);
        if (clip == null) return;

        // Find an available AudioSource from the pool
        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.loop = false;
            source.clip = clip;
            source.Play();
            source.outputAudioMixerGroup = _soundtrackSO.GetClipMixerGroup(clip);
        }
    }

    // Plays a looping sound effect
    public void PlayLoopingSFX(string soundName)
    {
        if (IsLoopingSFXPlaying(soundName)) return;

        var clip = _soundtrackSO?.GetClipByName(soundName);
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source != null)
        {
            source.loop = true;
            source.clip = clip;
            source.Play();
            source.outputAudioMixerGroup = _soundtrackSO.GetClipMixerGroup(clip);
        }
    }



    // Stops a specific looping sound effect
    public void StopLoopingSFX(string soundName)
    {
        foreach (var source in _sfxAudioSources)
        {
            if (source.isPlaying && source.clip != null && source.clip == _soundtrackSO.GetClipByName(soundName))
            {
                source.Stop();
                source.loop = false;
            }
        }
    }

    // Sets the master volume
    public void SetMasterVolume(float volume)
    {
        SetVolume(_masterVolumeParam, volume);
        PlayerPrefs.SetFloat("MasterVolumeKey", volume);
    }

    // Sets the BGM volume
    public void SetBackgroundMusicVolume(float volume)
    {
        SetVolume(_bgmVolumeParam, volume);
        PlayerPrefs.SetFloat("BGMVolumeKey", volume);
    }

    // Sets the SFX volume
    public void SetSoundEffectsVolume(float volume)
    {
        SetVolume(_sfxVolumeParam, volume);
        PlayerPrefs.SetFloat("SFXVolumeKey", volume);
    }

    // Sets volume for a specific mixer parameter
    private void SetVolume(string parameterName, float volume)
    {
        if (_audioMixer == null) return;

        float decibelVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat(parameterName, decibelVolume);
    }

    // Finds an available AudioSource in the pool
    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in _sfxAudioSources)
        {
            if (!source.isPlaying)
                return source;
        }

        Debug.LogWarning("All SFX AudioSources are in use!");
        return null;
    }

    public void StopAllLoopingSFX()
    {
        foreach (var source in _sfxAudioSources)
        {
            if (source.isPlaying && source.loop)
            {
                source.Stop();
                source.loop = false;
            }
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        PlayerPrefs.Save();
    }

    public bool IsLoopingSFXPlaying(string soundName)
    {
        var clip = _soundtrackSO?.GetClipByName(soundName);
        if (clip == null) return false;

        foreach (var source in _sfxAudioSources)
        {
            if (source.isPlaying && source.loop && source.clip == clip)
            {
                return true;
            }
        }

        return false;
    }
}
