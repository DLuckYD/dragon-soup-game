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
        Debug.Log("Game volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnMusicVolumeChanged(float value)
    {
        Debug.Log("Music volume: " + Mathf.RoundToInt(value * 100).ToString());
    }

    public void OnEffectsVolumeChanged(float value)
    {
        Debug.Log("Effects volume: " + Mathf.RoundToInt(value * 100).ToString());
    }
}