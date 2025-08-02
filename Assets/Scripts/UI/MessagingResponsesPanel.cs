using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections.Generic;

public class MessagingResponsesPanel : UIPanel
{
    [SerializeField] RectTransform responseButtonsContainer;
    [SerializeField] Slider timerSlider;
    [SerializeField] UIPanel subPanel;
    [SerializeField] TMP_Text _hintText;
    [SerializeField] GameObject _hintContainer;

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

    public RectTransform ResponseButtonsContainer
    {
        get { return responseButtonsContainer; }
    }

    public void DisplayHint(string text, bool value)
    {
        _hintText.text = text;
        _hintContainer.gameObject.SetActive(value);
    }
}
