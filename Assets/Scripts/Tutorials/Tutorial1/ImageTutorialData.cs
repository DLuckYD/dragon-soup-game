using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ImageTutorialPage
{
    public Sprite image;

    [TextArea(3, 8)]
    public string description;
}

[CreateAssetMenu(fileName = "ImageTutorialData", menuName = "Scriptable Objects/ImageTutorialData")]
public class ImageTutorialData : ScriptableObject
{
    [SerializeField] private List<ImageTutorialPage> pages = new();

    public IReadOnlyList<ImageTutorialPage> Pages => pages;
}
