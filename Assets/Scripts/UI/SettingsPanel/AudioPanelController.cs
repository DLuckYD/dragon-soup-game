using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioPanelController : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;

    private void Start()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnGameVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
    }
    public void OnGameVolumeChanged(float value)
    {
        SettingsManager.Instance.CurrentSettings.masterVolume = value;
        SettingsManager.Instance.SaveSettings();
        Debug.Log("Game volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsManager.Instance.CurrentSettings.musicVolume = value;
        SettingsManager.Instance.SaveSettings();
        Debug.Log("Music volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnEffectsVolumeChanged(float value)
    {
        SettingsManager.Instance.CurrentSettings.effectsVolume = value;
        SettingsManager.Instance.SaveSettings();
        Debug.Log("Effects volume: " + Mathf.RoundToInt(value * 100).ToString());
    }
}