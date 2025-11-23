using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;

[System.Serializable]
public class ChapterSaveData
{
    public string FileName;
    public int FileIndex;
    public string StartID;
    public bool Completed;
    public string CurrentGUID;
    public List<PastCoversationData> PastCoversations;

    public ChapterSaveData()
    {
        FileName = "";
        StartID = "";
        CurrentGUID = "";
        Completed = false;
        PastCoversations = new List<PastCoversationData>();
    }

    [System.Serializable]
    public class PastCoversationData
    {
        public string GUID;
        public string Text;
        public List<LanguageGeneric<string>> Texts;

        public bool IsChoice;
        public string SelectedChoice;
        public List<LanguageGeneric<string>> SelectedChoiceTexts;
    }
}
