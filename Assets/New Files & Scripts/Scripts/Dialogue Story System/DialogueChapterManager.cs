using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChapterManager : UICanvas
{
    [System.Serializable]
    public class ChapterData
    {
        public DialogueContainerSO Story;
        public string StartID;
        [HideInInspector] public int ChapterIndex;
        [HideInInspector] public bool IsStoryChapter;
        public List<ChapterReplaySetting> ReplaySettings;

        [System.Serializable]
        public class ChapterReplaySetting
        {
            public string DialogTitle;
            public GlobalValueSave Value;
            public string Option1Text;
            public string OptionSetting1;
            public string Option2Text;
            public string OptionSetting2;
        }

    }

    [SerializeField] private RectTransform _content;
    [SerializeField] private GameObject _unlockAllButton;
    public RectTransform MainContentContainer => _content;

    private static DialogueChapterManager _instance;
    public static DialogueChapterManager Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<DialogueChapterManager>();

            return _instance;
        }
    }

    public List<ChapterData> StoryList;
    public List<ChapterData> StandaloneChapters;
    public DialogueCharacterSO CurrentStory;
    [HideInInspector] public ChapterData CurrentChapter;
    public ChapterSettingsDialog ReplayDialog;

    [SerializeField]
    private StoryChapterButton _chapterButtonPrefab;

    [SerializeField]
    private RectTransform _chapterButtonContainer;
    private List<StoryChapterButton> _chapterButtons;
    private const float OPEN_DELAY = .05f;

    protected override void Awake()
    {
        base.Awake();
        _chapterButtons = new List<StoryChapterButton>();

        //Set the chapter index so that the position is the storylist when we don't have direct access to the StoryList container
        for (var index = 0; index < StoryList.Count; index++)
        {
            StoryList[index].ChapterIndex = index;
            StoryList[index].IsStoryChapter = true;
        }

        //Do the same for the standalone chapters
        for (var index = 0; index < StandaloneChapters.Count; index++)
        {
            StandaloneChapters[index].ChapterIndex = index;
            StandaloneChapters[index].IsStoryChapter = false;
        }
    }

    public void ChapterReplayButtonClicked()
    {
        DisplayChapters(true);
    }

    public void StandaloneChapterButtonClicked()
    {
        DisplayChapters(false);
    }

    private void DisplayChapters(bool isStoryReplay)
    {
        if (SaveAndLoadManager.Instance.PlayingStandaloneChapter)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.STANDALONE_CHAPTER_SELECT_WARNING, eventToTrigger: null, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
        }
        else if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.REPLAY_CHAPTER_SELECT_WARNING, eventToTrigger: null, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
        }
        else
        {
            if (isStoryReplay)
                OpenChapterSelect();
            else
                OpenStandaloneChapterSelect();
        }
    }

    private void ClearChapterButtons()
    {
        while (_chapterButtons.Count > 0)
        {
            if (_chapterButtons[0])
                DestroyImmediate(_chapterButtons[0].gameObject);

            _chapterButtons.RemoveAt(0);
        }
    }

    public void OpenChapterSelect()
    {
        ClearChapterButtons();

        var currentChapter = SaveAndLoadManager.Instance.CurrentSave.CurrentChapterData;
        for (var index = 0; index < StoryList.Count; index++)
        {
            var chapterData = StoryList[index];
            var isCompleted = SaveAndLoadManager.Instance.CurrentSave.ForceUnlockAllChapters || SaveAndLoadManager.Instance.CurrentSave.AutoSaveState.Chapters[chapterData.ChapterIndex].Completed;
            var newButton = Instantiate(_chapterButtonPrefab, _chapterButtonContainer);
            newButton.Setup(chapterData, isCompleted, currentChapter.FileIndex == index, false);

            _chapterButtons.Add(newButton);
        }

        _unlockAllButton.SetActive(true);
        Open(OPEN_DELAY);
    }

    public void OpenStandaloneChapterSelect()
    {
        ClearChapterButtons();

        for (var index = 0; index < StandaloneChapters.Count; index++)
        {
            var newButton = Instantiate(_chapterButtonPrefab, _chapterButtonContainer);
            newButton.Setup(StandaloneChapters[index], true, false, true);

            _chapterButtons.Add(newButton);
        }

        _unlockAllButton.SetActive(false);

        //Note, we aren't triggering show because we don't want to run the standard chapter checks for the story mode.
        //Standalone chapters are always unlocked
        Open(OPEN_DELAY);
    }

    public void ReturnToChapterSelection()
    {
        SaveAndLoadManager.Instance.AutoSave();
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            SaveAndLoadManager.Instance.LoadSave(SaveAndLoadManager.Instance.CurrentSaveSlot);
        }

        GameManager.Instance.ResetGameState();
        Open(OPEN_DELAY);
    }

    public void Hide()
    {
        Close();
    }

    public void CompleteCurrentChapter()
    {
        SaveAndLoadManager.Instance.CurrentSave.CompletedCurrentChapter();
        StartCoroutine(CoShowDialog());
    }

    public void CompleteChapterReplayEarly()
    {
        OverlayCanvas.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.ResetGameState();
            TriggerStoryChapter(SaveAndLoadManager.Instance.CurrentSave.CurrentState.CompletedChapters.Count);

            //Open chapter selection
            OpenChapterSelect();
        });
    }

    private IEnumerator CoShowDialog()
    {
        yield return new WaitForSeconds(.5f);

        var wasReplaying = SaveAndLoadManager.Instance.ReplayingCompletedChapter;

        if (SaveAndLoadManager.Instance.CurrentSave.CurrentState.CompletedChapters.Count >= SaveAndLoadManager.Instance.CurrentSave.TotalChapters)
        {
            //We have completed the last chapter.
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.ALL_CHAPTERS_COMPLETE, eventToTrigger: null, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
        }
        else
        {
            var message = wasReplaying ? GameConstants.DialogTextKeys.CHAPTER_REPLAY_COMPLETE : GameConstants.DialogTextKeys.CHAPTER_COMPLETE;
            GameManager.Instance.DisplayDialog(message, eventToTrigger: null, GameConstants.UIElementKeys.CONTINUE, args: null, twoButtonSetup: false);
            GameManager.Instance.NextChapterReady = true;
        }
    }

    /// <summary>
    /// Triggers a chapter
    /// </summary>
    /// <param name="chapter">Chapter data to load</param>
    /// <param name="isChapterReplay">Flag to set if the chapter will be a replay(no data will be saved other than gallery unlocks in a replay)</param>
    private void TriggerChapter(ChapterData chapter, bool isChapterReplay = false, bool isStandaloneChapter = false)
    {
        CurrentChapter = chapter;
        SaveAndLoadManager.Instance.ReplayingCompletedChapter = isChapterReplay;
        SaveAndLoadManager.Instance.PlayingStandaloneChapter = isStandaloneChapter;

        GameManager.Instance.TriggerDialogueChapter(chapter.Story);

        if (IsOpen)
            Hide();
    }

    /// <summary>
    /// Triggers a standalone chapter (one that is not tied to the story)
    /// </summary>
    /// <param name="chapter">Chapter to trigger</param>
    public void TriggerStandaloneChapter(ChapterData chapter)
    {
        TriggerChapter(chapter, true, true);
    }

    /// <summary>
    /// Triggers a story chapter
    /// </summary>
    /// <param name="chapter">Chapter to trigger</param>
    /// <param name="isChapterReplay">Flag to set if the chapter will be a replay (no data will be saved other than gallery unlocks in a replay)</param>
    public void TriggerStoryChapter(ChapterData chapter, bool isChapterReplay = false)
    {
        TriggerChapter(chapter, isChapterReplay, false);
    }

    /// <summary>
    /// Triggers a story chapter based on the index/position of the chapter in the StoryList container
    /// </summary>
    /// <param name="chapterNumber">Position of the chapter in the StoryList</param>
    /// <param name="isChapterReplay">Flag to set if the chapter will be a replay (no data will be saved other than gallery unlocks in a replay)</param>
    public void TriggerStoryChapter(int chapterNumber, bool isChapterReplay = false)
    {
         if (chapterNumber >= StoryList.Count)
        {
            //We have completed all chapters. Maybe do something here but for now just load the last chapter
            chapterNumber = StoryList.Count - 1;
        }

        TriggerStoryChapter(StoryList[chapterNumber]);
    }

    public void UnlockAllChapters()
    {
        GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.CHAPTER_UNLOCK_WARNING, () =>
        {
            SaveAndLoadManager.Instance.CurrentSave.ForceUnlockAllChapters = true;
            SaveAndLoadManager.Instance.AutoSave();
            OpenChapterSelect();
        });
    }
}
