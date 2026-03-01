using MeetAndTalk;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TriggerNextChapterButton : MonoBehaviour
{
    Button _button;
    [SerializeField] private CanvasGroup _cg;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClick);
    }

    void Update()
    {
        _cg.alpha = GameManager.Instance.NextChapterReady ? 1 : 0;
        _cg.interactable = GameManager.Instance.NextChapterReady;
        _cg.blocksRaycasts = _cg.interactable;
    }

#if UNITY_EDITOR
    [ContextMenu("[DEBUG] Trigger Next Chapter")]
    /// <summary>
    /// Debug method to trigger the next chapter
    /// </summary>
    private void DEBUG_Trigger()
    {
        // Should only be used in editor to test next chapter trigger logic
        SaveAndLoadManager.Instance.CurrentSave.CompletedCurrentChapter();
        OnButtonClick();
    }
#endif
    void OnButtonClick()
    {
        OverlayCanvas.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.ResetGameState(startDialogue: false);
            var saveManager = SaveAndLoadManager.Instance;
            saveManager.CurrentSave.CompletedCurrentChapter();

            var wasReplay = saveManager.ReplayingCompletedChapter;
            var wasStandalone = saveManager.PlayingStandaloneChapter;

            GameManager.Instance.ResetGameState(false);
            //Open chapter selection
            if (wasStandalone)
            {
                DialogueChapterManager.Instance.OpenStandaloneChapterSelect();
            }
            else if (wasReplay)
            {
                DialogueChapterManager.Instance.OpenChapterSelect();
            }

            var chapterNumber = saveManager.CurrentSave.CurrentState.CompletedChapters.Count;
            saveManager.ClearChapterData(resetBackground: false);

            // Populate any seen characters before starting the dialogue
            DialogueManager.Instance.PopulateConversationButtons();
            DialogueChapterManager.Instance.TriggerStoryChapter(chapterNumber);

            // Returning to linear path mode so both flags should be false
            saveManager.ReplayingCompletedChapter = false;
            saveManager.PlayingStandaloneChapter = false;
        });
    }
}