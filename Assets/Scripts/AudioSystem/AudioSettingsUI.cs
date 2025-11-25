using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        // Initialize sliders with saved values
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolumeKey", 1f);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolumeKey", 1f);
            bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolumeKey", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    private void SetMasterVolume(float value)
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.SetMasterVolume(value);
        }
    }

    private void SetBGMVolume(float value)
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.SetBackgroundMusicVolume(value);
        }
    }

    private void SetSFXVolume(float value)
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.SetSoundEffectsVolume(value);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.RemoveListener(SetBGMVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        }
    }
}
