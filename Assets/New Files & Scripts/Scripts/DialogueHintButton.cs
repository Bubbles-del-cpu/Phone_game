using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueHintButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public string DisplayedText = "";
    public bool Interactable;

    MessagingResponsesPanel _panel;
    public void Setup(string text, MessagingResponsesPanel panel)
    {
        _panel = panel;
        DisplayedText = text;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Interactable)
            _panel.DisplayHint(DisplayedText, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Interactable)
            _panel.DisplayHint(DisplayedText, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (Interactable)
            _panel.DisplayHint("", false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Interactable)
            _panel.DisplayHint("", false);
    }
}
