using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;

public class MessagingResponsesPanel : UIPanel
{
    [SerializeField] CanvasGroup responseButtonsContainer;
    [SerializeField] Slider timerSlider;
    [SerializeField] UIPanel subPanel;
    [SerializeField] TMP_Text _hintText;
    [SerializeField] GameObject _hintContainer;
    [SerializeField] ConversationRollbackButton _rollbackButton;

    public override void Open()
    {
        base.Open();
        subPanel.Open();
    }

    public override void Close()
    {
        base.Close();
        subPanel.Close();
    }

    private void Update()
    {
        if (_rollbackButton)
        {
            _rollbackButton.Interactable = responseButtonsContainer.alpha > 0 && responseButtonsContainer.transform.childCount > 0;
        }
    }

    public RectTransform ResponseButtonsContainer
    {
        get { return responseButtonsContainer.GetComponent<RectTransform>(); }
    }

    public void DisplayHint(string text, bool value)
    {
        _hintText.text = text;
        _hintContainer.gameObject.SetActive(value);
    }
}
