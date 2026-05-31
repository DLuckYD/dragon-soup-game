using System;
using UnityEngine;

public class AudioSlider : MonoBehaviour
{
    public UnityEngine.UI.Slider slider;
    public enum AudioType
    {
        Music,
        SFX,
        Main
    }
    public AudioType type;
    public void OnSliderChange()
    {
        switch (type)
        {
            case AudioType.Music:
                WwiseAudioManager.Instance.SetRTPCValue("Music_Volume", slider.value);
                break;
            case AudioType.SFX:
                WwiseAudioManager.Instance.SetRTPCValue("SFX_Volume", slider.value);
                break;
            case AudioType.Main:
                WwiseAudioManager.Instance.SetRTPCValue("Master_Volume", slider.value);
                break;

        }
    }
}
