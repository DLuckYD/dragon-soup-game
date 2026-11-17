using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[Serializable]
public class VideoTutorialPage
{
    public VideoClip video;

    public int tutorialNumber;
    public string tutorialName;

    [TextArea(3, 8)]
    public string tutorialDescription;

    public bool loopVideo = true;
}



[CreateAssetMenu(fileName = "VideoTutorialData", menuName = "Scriptable Objects/VideoTutorialData")]
public class VideoTutorialData : ScriptableObject
{
    [SerializeField] private List<VideoTutorialPage> pages = new();

    public IReadOnlyList<VideoTutorialPage> Pages => pages;
}


