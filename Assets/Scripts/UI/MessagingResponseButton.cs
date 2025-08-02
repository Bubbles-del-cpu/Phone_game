using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent(typeof(Button))]
public class MessagingResponseButton : MonoBehaviour
{
    [SerializeField, FormerlySerializedAs("label")] TMP_Text _label;
    [SerializeField] DialogueHintButton _hintButton;

    private string _fullText;
    private string _hint;
    MessagingResponsesPanel _panel;

    private string AdjustedText
    {
        get
        {
            if (_fullText.Length > GameManager.Instance.MaximumResponseLength)
            {
                return $"{_fullText.Substring(0, GameManager.Instance.MaximumResponseLength)}...";
            }

            return _fullText;
        }
    }

    Button _button;
    UnityAction _action;
    BaseNodeData _assignedNode;

    public void Init(BaseNodeData node, string text, string hint, MessagingResponsesPanel panel, UnityAction preAction, UnityAction action)
    {
        _assignedNode = node;
        _fullText = text;
        _hint = hint;

        _button = GetComponent<Button>();

        _hintButton.Setup(hint, panel);
        _hintButton.Interactable = _hint != DialogueNodePort.BLANK_HINT && _hint != string.Empty;

        GetComponentInChildren<TMP_Text>().text = AdjustedText;
        gameObject.SetActive(true);

        _button.onClick.AddListener(() =>
        {
            preAction.Invoke();
            OnClicked();
        });

        _button.onClick.AddListener(() => new Button.ButtonClickedEvent());
        this._action = action;
    }

    private void Update()
    {
        if (DialogueUIManager.Instance.DisplayHints && _hint != DialogueNodePort.BLANK_HINT && _hint != string.Empty)
        {
            //Adjust the right side to accomodate the hint button
            _label.GetComponent<RectTransform>().sizeDelta = new(-20, 0);
            _hintButton.gameObject.SetActive(true);
        }
        else
        {
            _hintButton.gameObject.SetActive(false);
            _label.GetComponent<RectTransform>().sizeDelta = new(0, 0);
        }
    }


    public void OnClicked()
    {
        GameManager.Instance.PlaySendTextFX();
        StartCoroutine(CoOnClicked());
    }

    IEnumerator CoOnClicked()
    {
        GetComponentInParent<UIPanel>().Close();

        DialogueManager.Instance.dialogueUIManager.SetFullText(_fullText, _assignedNode, DialogueUIManager.MessageSource.Player);

        yield return new WaitForSeconds(DialogueManager.Instance.PostChoiceDelay);
        _action.Invoke();
    }
}
