using System;
using UnityEngine;

[Serializable]
public class QuestDialogueEntry
{
    public string id;
    public string text;
}

[Serializable]
public class QuestDialogueBankJson
{
    public QuestDialogueEntry[] intros;
    public QuestDialogueEntry[] outros;
    public QuestDialogueEntry[] hints;
}