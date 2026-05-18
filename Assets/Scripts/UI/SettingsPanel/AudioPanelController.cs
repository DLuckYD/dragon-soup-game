using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioPanelController : MonoBehaviour
{
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider effectsVolumeSlider;

    private void Awake()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnGameVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
    }

    private void OnEnable()
    {
        LoadValuesFromSettings();
    }

    private void LoadValuesFromSettings()
    {
        SettingsData settings = SettingsSaveLoadManager.Instance.CurrentSettings;

        masterVolumeSlider.SetValueWithoutNotify(settings.masterVolume);
        musicVolumeSlider.SetValueWithoutNotify(settings.musicVolume);
        effectsVolumeSlider.SetValueWithoutNotify(settings.effectsVolume);
    }

    public void OnGameVolumeChanged(float value)
    {
        SettingsSaveLoadManager.Instance.CurrentSettings.masterVolume = value;
        Debug.Log("Game volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnMusicVolumeChanged(float value)
    {
        SettingsSaveLoadManager.Instance.CurrentSettings.musicVolume = value;
        Debug.Log("Music volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnEffectsVolumeChanged(float value)
    {
        SettingsSaveLoadManager.Instance.CurrentSettings.effectsVolume = value;
        Debug.Log("Effects volume: " + Mathf.RoundToInt(value * 100).ToString());
    }
}