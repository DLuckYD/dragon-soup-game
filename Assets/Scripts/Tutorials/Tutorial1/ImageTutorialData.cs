using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ImageTutorialPage
{
    public Sprite image;
    
    public int tutorialNumber;

    [FormerlySerializedAs("description")] 
    public string tutorialName;
    
    [TextArea(3, 8)]
    public string tutorialDescription;
    
}

[CreateAssetMenu(fileName = "ImageTutorialData", menuName = "Scriptable Objects/ImageTutorialData")]
public class ImageTutorialData : ScriptableObject
{
    [SerializeField] private List<ImageTutorialPage> pages = new();

    public IReadOnlyList<ImageTutorialPage> Pages => pages;
}
