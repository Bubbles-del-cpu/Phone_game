using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;

public class MessagingConversationPanel : UIPanel
{
    [SerializeField] RectTransform[] messageBubbleContainers;
    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    DialogueCharacterSO character;

    public int ChildCount => messageBubbleContainers[0].transform.childCount;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Open()
    {
        base.Open();

        GameManager.Instance.SetNewMessage(character, false);
        ScrollToBottom();
    }

    public void RemoveElements(int count)
    {
        var objectList = new List<GameObject>();
        for (var index = 1; index <= count; index++)
        {
            for (var cIndex = 0; cIndex < MessageBubbleContainers.Length; cIndex++)
            {
                var container = MessageBubbleContainers[cIndex];

                if (container.transform.childCount > 0)
                {
                    var item = container.transform.GetChild(container.transform.childCount - index);
                    item.gameObject.SetActive(false);
                    objectList.Add(item.gameObject);
                }
                else
                {
                    break;
                }
            }
        }

        foreach (var obj in objectList)
            Destroy(obj);
    }

    public void AddElement(BaseNodeData nodeData, MessagingBubble prefab, string text, DialogueUIManager.MessageSource source, bool notification)
    {
        //var wasNearBottom = ShouldAutoScroll();
        for (var index = 0; index < MessageBubbleContainers.Length; index++)
        {
            var container = MessageBubbleContainers[index];
            var containerSource = (DialogueUIManager.MessageSource)index;

            //Both panels recieve the same message - this allows both "sides" to scroll to the same locations without issue
            //Depending on the source one side will be hidden and one will be visible.
            var hidden = source != containerSource;

            switch (nodeData)
            {
                case DialogueNodeData nd when nodeData is DialogueNodeData:
                    {
                        if (nd.Timelapse.Length > 0)
                        {
                            MessagingBubble _timelapseBubble = Instantiate(prefab, container);
                            _timelapseBubble.Init(hidden, nd.Timelapse);
                            _timelapseBubble.IsTimelapse = true;
                        }

                        //Add element after timelapse
                        var bubble = Instantiate(prefab, container);
                        switch (nd.PostMediaType)
                        {
                            case MediaType.Sprite:
                                bubble.Init(hidden, text, nd.Image);
                                break;
                            case MediaType.Video:
                                bubble.Init(hidden, text, nd.Video);
                                break;
                        }

                        if (nd.Post != null && containerSource == DialogueUIManager.MessageSource.Character)
                        {
                            GameManager.Instance.SocialMediaCanvas.PostToSocialMedia(nd.Post, nd, notification);
                        }
                    }
                    break;
                case DialogueChoiceNodeData nd when nodeData is DialogueChoiceNodeData:
                    {
                        //Add element
                        if (text != string.Empty)
                        {
                            var bubble = Instantiate(prefab, container);
                            bubble.Init(source != containerSource, text);
                        }
                    }
                    break;
            }
        }

        Canvas.ForceUpdateCanvases();
        StartCoroutine(CoAutoScrollToBottom());
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

    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        _scrollView.verticalNormalizedPosition = 0;
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
