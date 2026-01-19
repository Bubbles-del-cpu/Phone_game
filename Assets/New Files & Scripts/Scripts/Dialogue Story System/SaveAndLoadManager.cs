using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public int SuggestMaxSaveStates;
    [HideInInspector] public int CurrentSaveSlot;
    public bool ReplayingCompletedChapter;
    public bool PlayingStandaloneChapter;
    public SaveFileData CurrentSave;
    public GlobalValueManager ValueManager;

    [Header("Prefabs")]
    [SerializeField] private SaveStateDialogBox _saveDialogPrefab;

    private void Awake()
    {
        GameManager.Instance.ChangeLanguage(CurrentSave.CurrentLanguage);
        ValueManager.LoadFile();
    }

    private void Start()
    {
        LoadSave(0);
        DialogueUIManager.Instance.DisplayHints = CurrentSave.DisplayHints;
    }

    private void OnApplicationQuit()
    {
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            CurrentSave.CurrentState.SavedVariables = ValueManager.ConvertSaveFile();
            CurrentSave.AutoSaveState = CurrentSave.CurrentState.Clone();
        }

        SaveToJson(CurrentSave, CurrentSaveSlot);
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

        // Load the language from the save file
        GameManager.Instance.ChangeLanguage(CurrentSave.CurrentLanguage);

        // Populate the save states in the settings canvas
        SettingsCanvas.Instance.PopulateSaveStates(CurrentSave.SaveStates);
    }

    public void StartGame()
    {
        GameManager.Instance.GalleryCanvas.Load();
        DialogueChapterManager.Instance.TriggerStoryChapter(CurrentSave.CurrentState.CompletedChapters.Count);
    }

    [ContextMenu("Save to Json")]
    public static void SaveToJson(SaveFileData saveData, int saveSlot)
    {
        string data = JsonUtility.ToJson(saveData, true);
        Debug.Log($"[SaveAndLoadManager] Saving to slot {saveSlot}. File Location: {GetPath(saveSlot)}");
        System.IO.File.WriteAllText(GetPath(saveSlot), data);
    }

    public static void Save()
    {
        Instance.CurrentSave.AutoSaveState = Instance.CurrentSave.CurrentState.Clone();
        SaveToJson(Instance.CurrentSave, Instance.CurrentSaveSlot);
    }

    [ContextMenu("Load from Json")]
    public static SaveFileData LoadFromJson(int saveSlot)
    {
        var path = GetPath(saveSlot);
        if (System.IO.File.Exists(path))
        {
            if (SaveMigrationSystem.SaveMigration.NeedMigration(path))
            {
                Debug.Log($"[SaveAndLoadManager] Migrating save file for slot {saveSlot}. File Location: {path}");
                SaveMigrationSystem.SaveMigration.MigrateLargeSave(path);
            }

            Debug.Log($"[SaveAndLoadManager] Loading save slot {saveSlot}. File Location: {path}");
            var data = System.IO.File.ReadAllText(GetPath(saveSlot));
            var saveFileData = JsonUtility.FromJson<SaveFileData>(data);

            //When the save file is loaded, check it against the latest base save file
            //And update if there are any variables or chapters missing
            saveFileData.ComparedAgainstLastest(SaveFileData.CreateBaseSave(saveSlot));

            if (saveFileData.CustomBackgroundImage.NodeGUID != string.Empty)
            {
                try
                {
                    var chapter = saveFileData.CustomBackgroundImage.ChapterType == ChapterType.Story ?
                        DialogueChapterManager.Instance.StoryList[saveFileData.CustomBackgroundImage.ChapterIndex] :
                        DialogueChapterManager.Instance.StandaloneChapters[saveFileData.CustomBackgroundImage.ChapterIndex];

                    var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, saveFileData.CustomBackgroundImage.NodeGUID);
                    var dialogueNode = node as DialogueNodeData;
                    GameManager.Instance.SetBackgroundImage(dialogueNode, saveFileData.CustomBackgroundImage.IsSocialMediaPost && dialogueNode.Post != null);
                }
                catch (Exception)
                {
                    Debug.LogError($"[SaveAndLoadManager] Failed to load custom background image from save file for slot {saveSlot}. Reverting to default background.");
                    GameManager.Instance.SetBackgroundImage(GameManager.Instance.DefaultBackgroundSprite);
                }
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
            if (CurrentSave.SaveStates.Count <= saveSlot)
                return false;

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
            if (SaveStateExists(slot))
            {
                CurrentSave.SaveStates[slot] = CurrentSave.CurrentState.Clone();
            }
            else
            {
                CurrentSave.SaveStates.Add(CurrentSave.CurrentState.Clone());
            }

            CurrentSave.SaveStates[slot].Name = name == string.Empty ? $"Save slot {slot + 1}" : name;
            CurrentSave.SaveStates[slot].IsSaved = true;
            SaveToJson(CurrentSave, CurrentSaveSlot);
        }
        catch (Exception) { }
    }

    public void ClearChapterData(bool resetBackground)
    {
        try
        {
            //Reset the chapter
            CurrentSave.CurrentState.LastChapter.PastCoversations = new List<ChapterSaveData.PastCoversationData>();
            CurrentSave.CurrentState.LastChapter.CurrentGUID = "";
            CurrentSave.CurrentState.LastChapter.Completed = false;
            CurrentSave.CurrentState.LastChapter.FileName = string.Empty;
            CurrentSave.CurrentState.LastChapter.FileIndex = -1;

            if (resetBackground)
                GameManager.Instance.ResetBackgroundImage();

            SaveAndLoadManager.Save();
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

                GameManager.Instance.ResetGameState(startDialogue: false);
                GameManager.Instance.GalleryCanvas.Load();
                DialogueChapterManager.Instance.TriggerStoryChapter(CurrentSave.CurrentState.CompletedChapters.Count);
            });
        }
        catch (Exception) { }
    }

    public void ClearSaveStateSlot(int saveSlot)
    {
        try
        {
            CurrentSave.SaveStates.RemoveAt(saveSlot);
            SaveToJson(CurrentSave, CurrentSaveSlot);
        }
        catch (Exception) { }
    }

    /// <summary>s
    /// Displays the save state dialog for the specified slot
    /// </summary>
    /// <param name="slotNumber">Save slot number</param>
    /// <param name="actionOnSubmit">Action to perform on submit</param>
    public void DisplaySaveStateDialog(int slotNumber, Action actionOnSubmit = null)
    {
        var newDialog = Instantiate(_saveDialogPrefab);
        newDialog.Setup(slotNumber);
        newDialog.OnSubmit.AddListener(() =>
        {
            actionOnSubmit?.Invoke();
        });

        OverlayCanvas.Instance.ShowDialog(newDialog.gameObject);
    }

    public void StartNewSave(bool startDialogue = true)
    {
        System.IO.File.Delete(GetPath(0));

        //Reset and clear the global value manager so that it can be loaded in fresh for the new save
        ValueManager.Reset();

        LoadSave(0);

        GameManager.Instance.ResetBackgroundImage();
        GameManager.Instance.ResetGameState(startDialogue: false);

        if (startDialogue)
            StartGame();
    }
}