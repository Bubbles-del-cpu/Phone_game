using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class MessagingConversationPanel : UIPanel
{
    private struct MessageBubbleInfo
    {
        public DialogueUIManager.MessageSource Source;
        public string NodeGUID;
        public string Text;
        public bool Hidden;

        public void SendToPanel(MessagingConversationPanel panel)
        {
            var containerSource = Source;
            var node = DialogueManager.Instance.GetNodeByGuid(NodeGUID);
            switch (node)
            {
                case DialogueNodeData nd when node is DialogueNodeData:
                    {
                        if (nd.GetTimeLapse().Length > 0)
                        {
                            //MessagingBubble _timelapseBubble = Instantiate(BubblePrefab, Container);
                            var timelapseBubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            timelapseBubble.transform.SetParent(panel._messageContainers[0].transform, false);
                            timelapseBubble.Init(Hidden, nd.GetTimeLapse(), timelapse: true, Source);
                        }

                        //Add element after timelapse
                        if (Text != string.Empty || nd.Image != null || nd.Video != null)
                        {
                            //var bubble = Instantiate(BubblePrefab, Container);
                            var bubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            bubble.transform.SetParent(panel._messageContainers[0].transform, false);
                            bubble.Init(Hidden, Text, timelapse: false, Source);
                            bubble.SetupMediaViewer(nd);
                        }
                    }
                    break;
                case DialogueChoiceNodeData nd when node is DialogueChoiceNodeData:
                    {
                        //Add element
                        if (Text != string.Empty && Text[0] != '*')
                        {
                            //Frist character is the special action character so don't send the message
                            //var bubble = Instantiate(BubblePrefab, Container);
                            var bubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                            bubble.transform.SetParent(panel._messageContainers[0].transform, false);
                            bubble.Init(Source != containerSource, Text, timelapse: false, Source);
                        }
                    }
                    break;
            }
        }
    }

    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    [SerializeField] private ProfileIcon _characterIcon;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private MatchChildScaleAutomatic[] _messageContainers;
    private DialogueCharacterSO _character;

    public MessagingResponsesPanel ResponsesPanel { get { return responsesPanel; } }
    public DialogueCharacterSO Character { get { return _character; } set { _character = value; } }
    private List<MessageBubbleInfo> _messageBubbleInfosLeft = new List<MessageBubbleInfo>();
    public int ChildCount => _messageContainers[0].transform.childCount;

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
                leftInfo.SendToPanel(this);

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

        _characterIcon.Character = _character;
        _characterName.text = _character.name;
        GameManager.Instance.SetNewMessage(_character, false);

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
        var container = _messageContainers[0];
        var index = 0;
        while (index < count)
        {
            try
            {
                var item = container.transform.GetChild(container.transform.childCount - 1);
                var bubble = item.GetComponent<MessagingBubble>();
                var messageInfo = _messageBubbleInfosLeft[_messageBubbleInfosLeft.Count - 1];
                DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(bubble, messageInfo.Source);
                _messageBubbleInfosLeft.RemoveAt(_messageBubbleInfosLeft.Count - 1);

                index++;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to clear conversation panel for {_character.name}. Error: {ex.Message}");
                break;
            }
        }

        UpdateChildContainers();
    }

    public void AddElement(BaseNodeData nodeData, string text, DialogueUIManager.MessageSource source)
    {
        var newBubbleData = new MessageBubbleInfo()
        {
            Source = source,
            NodeGUID = nodeData.NodeGuid,
            Text = text,
            Hidden = false
        };

        _messageBubbleInfosLeft.Add(newBubbleData);

        if (IsOpen)
        {
            newBubbleData.SendToPanel(this);
            Canvas.ForceUpdateCanvases();
            ScrollToBottom();
            UpdateChildContainers();
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
            }
        }
    }

    private void ClearMessageBubbles()
    {
        var item = _messageContainers[0].transform;
        var index = 0;
        while(item.childCount > 0)
        {
            var child = item.GetChild(0);
            var messageInfo = _messageBubbleInfosLeft[index];
            DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(child.GetComponent<MessagingBubble>(), messageInfo.Source);

            index++;
        }
    }

    private void UpdateChildContainers()
    {
        foreach (var item in _messageContainers)
        {
            item.UpdateSize();
        }
    }
}
