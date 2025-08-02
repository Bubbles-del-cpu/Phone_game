using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections;

public class MessagingConversationPanel : UIPanel
{
    [SerializeField] RectTransform[] messageBubbleContainers;
    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    DialogueCharacterSO character;

    public override void Open()
    {
        base.Open();
        GameManager.Instance.SetNewMessage(character, false);
        ScrollToBottom();
    }

    private bool ShouldAutoScroll()
    {
        // Only auto-scroll if user is already near the bottom
        return _scrollView.verticalNormalizedPosition <= 0.1f;
    }

    public void AddElement(BaseNodeData nodeData, MessagingBubble prefab, string text, int containerNumber, bool hide = false)
    {
        var wasNearBottom = ShouldAutoScroll();

        switch (nodeData)
        {
            case DialogueNodeData nd when nodeData is DialogueNodeData:
                {
                    if (nd.Timelapse.Length > 0)
                    {
                        MessagingBubble _timelapseBubble = Instantiate(prefab, MessageBubbleContainers[containerNumber]);
                        _timelapseBubble.Init(hide, nd.Timelapse);
                        _timelapseBubble.IsTimelapse = true;
                    }

                    //Add element after timelapse
                    var _bubble = Instantiate(prefab, MessageBubbleContainers[containerNumber]);
                    switch (nd.PostMediaType)
                    {
                        case MediaType.Sprite:
                            _bubble.Init(hide, text, nd.Image);
                            break;
                        case MediaType.Video:
                            _bubble.Init(hide, text, nd.Video);
                            break;
                    }
                }
                break;
            case DialogueChoiceNodeData nd when nodeData is DialogueChoiceNodeData:
                {
                    //Add element
                    var _bubble = Instantiate(prefab, MessageBubbleContainers[containerNumber]);
                    _bubble.Init(hide, text);
                    if (!nd.RequireCharacterInput)
                    {
                        _bubble.gameObject.SetActive(false);
                    }
                }
                break;
        }

        if (wasNearBottom)
        {
            Canvas.ForceUpdateCanvases();
            StartCoroutine(CoAutoScrollToBottom());
        }
    }

    IEnumerator CoAutoScrollToBottom(float delay = 0)
    {
        // Wait for layout to rebuild
        yield return null;
        yield return null;

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        while (_scrollView.verticalNormalizedPosition > 0)
        {
            yield return new WaitForEndOfFrame();
            _scrollView.verticalNormalizedPosition -= DialogueUIManager.Instance.MessagePanelAutoScrollSpeed * Time.deltaTime;
        }
    }

    public void ScrollToBottom(float delay = 0.1f)
    {
        Canvas.ForceUpdateCanvases();
        _scrollView.verticalNormalizedPosition = 0;

        //StartCoroutine(CoAutoScrollToBottom(delay));
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.MessagingCanvas.ConversationClosed();
    }

    public void Clear()
    {
        foreach(var item in messageBubbleContainers)
        {
            for(var index = 0; index < item.childCount; index++)
            {
                item.GetChild(index).gameObject.SetActive(false);
                Destroy(item.GetChild(index).gameObject, 1);
            }
        }
    }

    private void Update()
    {
        _contentContainer.sizeDelta = new Vector2(_contentContainer.sizeDelta.x, messageBubbleContainers[0].sizeDelta.y);
    }

    public RectTransform[] MessageBubbleContainers { get { return messageBubbleContainers; } }

    public MessagingResponsesPanel ResponsesPanel { get { return responsesPanel; } }

    public DialogueCharacterSO Character { get { return character; } set { character = value; } }
}
