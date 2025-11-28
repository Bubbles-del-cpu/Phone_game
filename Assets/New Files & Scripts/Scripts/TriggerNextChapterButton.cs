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

    void OnButtonClick()
    {
        OverlayCanvas.Instance.FadeToBlack(() =>
        {
            GameManager.Instance.ResetGameState(startDialogue: false);
            var saveManager = SaveAndLoadManager.Instance;

            var wasReplay = saveManager.ReplayingCompletedChapter;
            var wasStandalone = saveManager.PlayingStandaloneChapter;

            //Open chapter selection
            if (wasStandalone)
            {
                DialogueChapterManager.Instance.OpenStandaloneChapterSelect();
            }
            else if (wasReplay)
            {
                DialogueChapterManager.Instance.OpenChapterSelect();
            }
            else
            {
                var chapterNumber = saveManager.CurrentSave.CurrentState.CompletedChapters.Count;
                saveManager.ClearChapterData(resetBackground: false);
                DialogueChapterManager.Instance.TriggerStoryChapter(chapterNumber);
            }
        });
    }
}