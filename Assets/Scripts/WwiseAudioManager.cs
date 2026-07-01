using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class WwiseAudioManager : MonoBehaviour
{
private static WwiseAudioManager _instance;
public static WwiseAudioManager Instance { get { return _instance; } }
 void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        PostEvent("Play_Music", gameObject);
    }

    public void PostEvent(string eventName, GameObject gameObject)
    {
        AkUnitySoundEngine.PostEvent(eventName, gameObject);
    }

    public void SetSwitchValue(string switchGroup, string switchState, GameObject gameObject)
    {
        AkUnitySoundEngine.SetSwitch(switchGroup, switchState, gameObject);
    }

    public void SetRTPCValue(string rtpcName, float value)
    {
        AkUnitySoundEngine.SetRTPCValue(rtpcName, value);
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                AkUnitySoundEngine.SetState("Game_State", "In_Menu");
                break;
            case "SampleScene":
                AkUnitySoundEngine.SetState("Game_State", "In_Game");
                break;
            default:
                break;
        }
    }
}