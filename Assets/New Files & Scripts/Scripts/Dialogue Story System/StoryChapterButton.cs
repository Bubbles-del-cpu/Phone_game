using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Button))]
public class StoryChapterButton : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _label;

    public DialogueChapterManager.ChapterData AssignedChapter { get; private set; }
    public bool IsStandaloneChapterButton{ get; private set; }
    public Color BaseColor;
    public Color CompletedColor;

    public bool Interactable
    {
        get
        {
            return _button.interactable;
        }
        set
        {
            _button.interactable = value;
        }
    }

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.REPLAY_START_WARNING, () =>
            {
                StartCoroutine(CoShowOptions());
            });
        });
    }

    private IEnumerator CoShowOptions()
    {
        yield return new WaitForSeconds(.1f);

        var dialogueComplete = false;
        if (AssignedChapter.ReplaySettings.Count == 0)
        {
            //We have to replay settings to just trigger the chapter instantly
            OverlayCanvas.Instance.FadeToBlack(() =>
            {
                GameManager.Instance.ResetGameState(startDialogue: false);
                if (IsStandaloneChapterButton)
                {
                    DialogueChapterManager.Instance.TriggerStandaloneChapter(AssignedChapter);
                }
                else
                {
                    DialogueChapterManager.Instance.TriggerStoryChapter(AssignedChapter, true);
                }
            });
        }
        else
        {
            for (var index = 0; index < AssignedChapter.ReplaySettings.Count; index++)
            {
                dialogueComplete = false;
                var replayDialog = Instantiate(DialogueChapterManager.Instance.ReplayDialog);
                GameManager.Instance.DisplayPopup(replayDialog.gameObject);
                if (index == AssignedChapter.ReplaySettings.Count - 1)
                {
                    dialogueComplete = true;
                    replayDialog.Setup(AssignedChapter.ReplaySettings[index], () =>
                    {
                        OverlayCanvas.Instance.FadeToBlack(() =>
                        {
                            GameManager.Instance.ResetGameState(startDialogue: false);
                            if (IsStandaloneChapterButton)
                            {
                                DialogueChapterManager.Instance.TriggerStandaloneChapter(AssignedChapter);
                            }
                            else
                            {
                                DialogueChapterManager.Instance.TriggerStoryChapter(AssignedChapter, true);
                            }
                        });
                    });
                }
                else
                {
                    replayDialog.Setup(AssignedChapter.ReplaySettings[index], () => dialogueComplete = true);
                }

                while (!dialogueComplete)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    public void Setup(DialogueChapterManager.ChapterData data, bool interactable, bool currentChapter, bool standaloneChapter)
    {
        AssignedChapter = data;
        IsStandaloneChapterButton = standaloneChapter;
        _label.text = $"{data.Story.name}";

        //var completed = SaveAndLoadManager.Instance.CurrentSave.CurrentState.CompletedChapters.Contains(_chapterNumber);
        _button.image.color = currentChapter ? CompletedColor : BaseColor;
        _button.interactable = interactable;
    }
}