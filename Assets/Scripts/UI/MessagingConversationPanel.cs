using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using TMPro;
using Unity.VisualScripting;
using System;

public class MessagingConversationPanel : UIPanel
{
    private struct MessageBubbleInfo
    {
        public Transform Container;
        public int Index;
        public DialogueUIManager.MessageSource Source;
        public string Text;
        public BaseNodeData NodeData;
        public bool Hidden;
        public bool IsDisplayed;

        public void SendToPanel()
        {
            var containerSource = (DialogueUIManager.MessageSource)Index;
            switch (NodeData)
            {
                case DialogueNodeData nd when NodeData is DialogueNodeData:
                    {
                        if (nd.GetTimeLapse().Length > 0)
                        {
                            //MessagingBubble _timelapseBubble = Instantiate(BubblePrefab, Container);
                            var timelapseBubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            timelapseBubble.transform.SetParent(Container, false);
                            timelapseBubble.Init(Hidden, nd.GetTimeLapse(), timelapse: true, Source);
                        }

                        //Add element after timelapse
                        if (Text != string.Empty || nd.Image != null || nd.Video != null)
                        {
                            //var bubble = Instantiate(BubblePrefab, Container);
                            var bubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            bubble.transform.SetParent(Container, false);
                            bubble.Init(Hidden, Text, timelapse: false, Source);
                            bubble.SetupMediaViewer(nd);
                        }
                    }
                    break;
                case DialogueChoiceNodeData nd when NodeData is DialogueChoiceNodeData:
                    {
                        //Add element
                        if (Text != string.Empty && Text[0] != '*')
                        {
                            //Frist character is the special action character so don't send the message
                            //var bubble = Instantiate(BubblePrefab, Container);
                            var bubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            bubble.transform.SetParent(Container, false);
                            bubble.Init(Source != containerSource, Text, timelapse: false, Source);
                        }
                    }
                    break;
            }
        }
    }

    [SerializeField] RectTransform[] messageBubbleContainers;
    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    [SerializeField] private ProfileIcon _characterIcon;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private MatchChildScaleAutomatic[] _messageContainers;
    DialogueCharacterSO character;

    private List<MessageBubbleInfo> _messageBubbleInfosLeft = new List<MessageBubbleInfo>();
    private List<MessageBubbleInfo> _messageBubbleInfosRight = new List<MessageBubbleInfo>();

    public int ChildCount => messageBubbleContainers[0].transform.childCount;

    public override void Awake()
    {
        base.Awake();
    }

    private float _delay = .05f;

    private IEnumerator OpenDelay()
    {
        yield return new WaitForSeconds(_delay);
        if (!IsOpen)
        {
            var count = 0;
            var maxLoops = 30;
            for(var index = 0; index < _messageBubbleInfosLeft.Count; index++)
            {
                var leftInfo = _messageBubbleInfosLeft[index];
                leftInfo.SendToPanel();

                var rightInfo = _messageBubbleInfosRight[index];
                rightInfo.SendToPanel();

                count++;
                if (count >= maxLoops)
                {
                    count = 0;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(_delay);
            Canvas.ForceUpdateCanvases();
            ScrollToBottom();
            UpdateChildContainers();
        }

        _characterIcon.Character = character;
        _characterName.text = character.name;
        GameManager.Instance.SetNewMessage(character, false);

        yield return new WaitForSeconds(_delay);
        base.Open();
    }

    public override void Open()
    {
        transform.SetAsLastSibling();
        StartCoroutine(OpenDelay());
    }

    public void RemoveElements(int count)
    {
        for (var cIndex = 0; cIndex < MessageBubbleContainers.Length; cIndex++)
        {
            var container = MessageBubbleContainers[cIndex];
            var containerSource = (DialogueUIManager.MessageSource)cIndex;
            var index = 0;
            while (index < count)
            {
                try
                {
                    var item = container.transform.GetChild(container.transform.childCount - 1);
                    var bubble = item.GetComponent<MessagingBubble>();
                    DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(bubble, containerSource);
                    switch(containerSource)
                    {
                        case DialogueUIManager.MessageSource.Character:
                            _messageBubbleInfosLeft.RemoveAt(_messageBubbleInfosLeft.Count - 1);
                            break;
                        case DialogueUIManager.MessageSource.Player:
                            _messageBubbleInfosRight.RemoveAt(_messageBubbleInfosRight.Count - 1);
                            break;
                    }

                    index++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to clear conversation panel for {character.name}. Error: {ex.Message}");
                    break;
                }
            }
        }

        UpdateChildContainers();
    }

    public void AddElement(BaseNodeData nodeData, string text, DialogueUIManager.MessageSource source, bool notification)
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
                        if (nd.Post != null && containerSource == DialogueUIManager.MessageSource.Character)
                        {
                            GameManager.Instance.SocialMediaCanvas.PostToSocialMedia(nd.Post, nd, notification);
                        }
                    }
                    break;
            }

            var newBubbleData = new MessageBubbleInfo()
            {
                Container = container,
                Index = index,
                Source = source,
                Text = text,
                NodeData = nodeData,
                Hidden = hidden
            };

            if (containerSource == DialogueUIManager.MessageSource.Character)
                _messageBubbleInfosLeft.Add(newBubbleData);
            else
                _messageBubbleInfosRight.Add(newBubbleData);

            if (IsOpen)
            {
                newBubbleData.SendToPanel();
                Canvas.ForceUpdateCanvases();
                ScrollToBottom();
                UpdateChildContainers();
            }
        }
    }

    IEnumerator CoAutoScrollToBottom(float delay = 0, float speed = 1)
    {
        // Wait for layout to rebuild
        yield return null;
        yield return null;

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        while (_scrollView.verticalNormalizedPosition > 0)
        {
            yield return new WaitForEndOfFrame();
            _scrollView.verticalNormalizedPosition -= DialogueUIManager.Instance.MessagePanelAutoScrollSpeed * (Time.deltaTime * speed);
        }
    }

    public void ScrollToBottom()
    {
        if (_scrollView.verticalNormalizedPosition == 0)
            return;

        try
        {
            Canvas.ForceUpdateCanvases();
            _scrollView.verticalNormalizedPosition = 0;
        }
        catch (System.Exception){}
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.MessagingCanvas.ConversationClosed();
        Clear();
    }

    public void Clear()
    {
        ClearMessageBubbles();
        UpdateChildContainers();
    }

    private void Update()
    {
        if (IsOpen)
        {
            Debug.Log($"{_contentContainer.rect.height} vs {_scrollView.viewport.rect.height}");
            if (_contentContainer.rect.height > _scrollView.viewport.rect.height)
            {
                //Alter the anchors to keep the scroll at the bottom if content is larger than the viewport
                _contentContainer.anchorMin = new Vector2(0, 0);
                _contentContainer.anchorMax = new Vector2(1, 0);
                _contentContainer.pivot = new Vector2(0.5f, 0);
                foreach(var item in _messageContainers)
                {
                    var rect = item.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                }

                Debug.Log("Anchors set to bottom");
            }
        }
    }

    private void ClearMessageBubbles()
    {
        for(var index = 0; index < messageBubbleContainers.Length; index++)
        {
            var item = messageBubbleContainers[index];
            DialogueUIManager.MessageSource containerSource = (DialogueUIManager.MessageSource)index;
            while(item.childCount > 0)
            {
                var child = item.GetChild(0);
                DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(child.GetComponent<MessagingBubble>(), containerSource);
            }
        }
    }

    private void UpdateChildContainers()
    {
        foreach (var item in _messageContainers)
        {
            item.UpdateSize();
        }
    }

    public RectTransform[] MessageBubbleContainers { get { return messageBubbleContainers; } }

    public MessagingResponsesPanel ResponsesPanel { get { return responsesPanel; } }

    public DialogueCharacterSO Character { get { return character; } set { character = value; } }
}
