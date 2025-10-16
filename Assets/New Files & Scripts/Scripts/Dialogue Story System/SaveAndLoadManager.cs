using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SaveAndLoadManager : MonoBehaviour
{
    private static SaveAndLoadManager _instance;
    public static SaveAndLoadManager Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<SaveAndLoadManager>();

            return _instance;
        }
    }

    public static string GetPath(int saveSlot)
    {
        return $"{Application.persistentDataPath}/slot{saveSlot}_SaveData.json";
    }

    [HideInInspector] public int CurrentSaveSlot;
    public bool ReplayingCompletedChapter;
    public bool PlayingStandaloneChapter;
    public SaveFileData CurrentSave;
    public GlobalValueManager ValueManager;

    private void Awake()
    {
        LoadSave(0);
        GameManager.Instance.ChangeLanguage(CurrentSave.CurrentLanguage);
    }

    private void Start()
    {
        DialogueUIManager.Instance.DisplayHints = CurrentSave.DisplayHints;
        GameManager.Instance.GalleryCanvas.Load();
    }

    public void LoadSave(int slot = 0)
    {
        CurrentSaveSlot = slot;
        CurrentSave = LoadFromJson(slot);

        if (CurrentSave.Version == string.Empty)
        {
            //Old save, we must delete
            ClearSaveStateSlot(0);
            LoadSave(0);
            return;
        }

        CurrentSave.CurrentState = CurrentSave.AutoSaveState.Clone();
        ValueManager.LoadSaveFile(CurrentSave.CurrentState.SavedVariables);
        ValueManager.SaveFile();

        CurrentSave.CurrentState.SavedVariables = ValueManager.ConvertSaveFile();

        //DialogueChapterManager.Instance.Show()
    }

    public void StartGame()
    {
        DialogueChapterManager.Instance.TriggerStoryChapter(CurrentSave.CurrentState.CompletedChapters.Count);
    }

    public void AutoSave()
    {
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            CurrentSave.CurrentState.SavedVariables = ValueManager.ConvertSaveFile();
            CurrentSave.AutoSaveState = CurrentSave.CurrentState.Clone();
        }

        SaveToJson(CurrentSave, CurrentSaveSlot);
    }

    [ContextMenu("Save to Json")]
    public static void SaveToJson(SaveFileData saveData, int saveSlot)
    {
        string data = JsonUtility.ToJson(saveData, true);
        System.IO.File.WriteAllText(GetPath(saveSlot), data);
    }

    [ContextMenu("Load from Json")]
    public static SaveFileData LoadFromJson(int saveSlot)
    {
        var path = GetPath(saveSlot);
        if (System.IO.File.Exists(path))
        {
            Debug.Log($"[SaveAndLoadManager] Loading save slot {saveSlot}. File Location: {path}");
            var data = System.IO.File.ReadAllText(GetPath(saveSlot));
            var saveFileData = JsonUtility.FromJson<SaveFileData>(data);

            //When the save file is loaded, check it against the latest base save file
            //And update if there are any variables or chapters missing
            saveFileData.ComparedAgainstLastest(SaveFileData.CreateBaseSave(saveSlot));

            if (saveFileData.CustomBackgroundImage.NodeGUID != string.Empty)
            {
                var chapter = saveFileData.CustomBackgroundImage.ChapterType == ChapterType.Story ?
                    DialogueChapterManager.Instance.StoryList[saveFileData.CustomBackgroundImage.ChapterIndex] :
                    DialogueChapterManager.Instance.StandaloneChapters[saveFileData.CustomBackgroundImage.ChapterIndex];

                var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, saveFileData.CustomBackgroundImage.NodeGUID);
                GameManager.Instance.SetBackgroundImage((DialogueNodeData)node, saveFileData.CustomBackgroundImage.IsSocialMediaPost);
            }
            else
            {
                GameManager.Instance.SetBackgroundImage(GameManager.Instance.DefaultBackgroundSprite);
            }

            return saveFileData;
        }
        else
        {
            Debug.Log($"[SaveAndLoadManager] New save file created for {saveSlot}. File Location: {path}");
            var save = SaveFileData.CreateBaseSave(saveSlot);
            SaveToJson(save, saveSlot);
            return save;
        }
    }

    public bool SaveStateExists(int saveSlot)
    {
        try
        {
            return CurrentSave.SaveStates[saveSlot].IsSaved;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void CreateSaveState(int slot, string name = "")
    {
        try
        {
            CurrentSave.SaveStates[slot] = CurrentSave.CurrentState.Clone();
            CurrentSave.SaveStates[slot].IsSaved = true;
            CurrentSave.SaveStates[slot].Name = name == string.Empty ? $"Save slot {slot + 1}" : name;

            AutoSave();
        }
        catch (Exception) { }
    }

    public void ClearChapterData(int chapterIndex)
    {
        try
        {
            //Reset the chapter
            CurrentSave.CurrentState.Chapters[chapterIndex] = new ChapterSaveData();
            GameManager.Instance.ResetBackgroundImage();
            AutoSave();
        }
        catch (Exception) { }
    }

    public void LoadSaveSlot(int slot)
    {
        try
        {
            OverlayCanvas.Instance.FadeToBlack(() =>
            {
                var stateToLoad = CurrentSave.SaveStates[slot];
                CurrentSave.CurrentState = stateToLoad.Clone();

                ValueManager.LoadSaveFile(CurrentSave.CurrentState.SavedVariables);
                ValueManager.SaveFile();

                CurrentSave.CurrentState.SavedVariables = ValueManager.ConvertSaveFile();

                GameManager.Instance.ResetGameState();
                DialogueChapterManager.Instance.TriggerStoryChapter(CurrentSave.CurrentState.CompletedChapters.Count);
                GameManager.Instance.GalleryCanvas.Load();
            });
        }
        catch (Exception) { }
    }

    public void ClearSaveStateSlot(int saveSlot)
    {
        try
        {
            CurrentSave.SaveStates[saveSlot].IsSaved = false;
            CurrentSave.SaveStates[saveSlot] = new SaveFileData.GameSaveState();

            AutoSave();
        }
        catch (Exception) { }
    }

    public void StartNewSave()
    {
        System.IO.File.Delete(GetPath(0));

        //Reset and clear the global value manager so that it can be loaded in fresh for the new save
        ValueManager.Reset();

        LoadSave(0);

        GameManager.Instance.ResetGameState();
        GameManager.Instance.ResetBackgroundImage();
    }
}