using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class MessagingConversationPanel : UIPanel
{
    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    [SerializeField] private ProfileIcon _characterIcon;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private MatchChildScaleAutomatic[] _messageContainers;
    private DialogueCharacterSO _character;

    public MessagingResponsesPanel ResponsesPanel { get { return responsesPanel; } }
    public MatchChildScaleAutomatic[] MessageContainers { get { return _messageContainers; } }
    public DialogueCharacterSO Character { get { return _character; } set { _character = value; } }
    private List<MessageBubbleInfo> _messageBubbleInfosLeft = new List<MessageBubbleInfo>();
    public int ChildCount => _messageContainers[0].transform.childCount;
    private float _delay = .05f;

    public void OpenWithAction(Action onComplete)
    {
        transform.SetAsLastSibling();
        StartCoroutine(OpenDelay(onComplete));
    }

    public override void Open()
    {
        transform.SetAsLastSibling();
        StartCoroutine(OpenDelay(null));
    }

    public override void Close()
    {
        base.Close();

        GameManager.Instance.MessagingCanvas.ConversationClosed();
        Clear();
    }

    public void CloseAndWipeHistory()
    {
        base.Close();
        Clear(wipeHistory: true);
    }

    public void Clear(bool wipeHistory = false)
    {
        var item = _messageContainers[0].transform;
        var index = 0;
        while (item.childCount > 0)
        {
            var child = item.GetChild(0);
            var bubble = child.GetComponent<MessagingBubble>();
            DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(bubble, bubble.Source);

            index++;
        }

        if (wipeHistory)
        {
            _messageBubbleInfosLeft.Clear();
        }

        UpdateChildContainers();
    }

    private IEnumerator OpenDelay(Action onComplete)
    {
        yield return new WaitForSecondsRealtime(_delay);
        if (!IsOpen)
        {
            var count = 0;
            var maxLoops = 30;
            for (var index = 0; index < _messageBubbleInfosLeft.Count; index++)
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

            yield return new WaitForSecondsRealtime(_delay);
            Canvas.ForceUpdateCanvases();
            ScrollToBottom();
            UpdateChildContainers();
        }

        _characterIcon.Character = _character;
        _characterName.text = _character.name;
        GameManager.Instance.SetNewMessage(_character, false);
        MainMenuCanvas.Instance.SetMessagingAppNotification(_character, messageSeen: true);
        yield return new WaitForSecondsRealtime(_delay);
        base.Open();
        onComplete?.Invoke();
    }

    public void RemoveElements(int count)
    {
        var container = _messageContainers[0];
        var index = 0;
        while (index < count)
        {
            try
            {
                if (container.transform.childCount == 0)
                    break;

                var item = container.transform.GetChild(container.transform.childCount - 1);
                var bubble = item.GetComponent<MessagingBubble>();
                DialogueUIManagerObjectPool.Instance.ReturnMessageBubble(bubble, bubble.Source);

                // Its possible for more than 1 bubble to share the same GUID (timelapse + main message), so remove all that match
                _messageBubbleInfosLeft.RemoveAll(info => info.NodeGUID == bubble.NodeGUID);

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

    private void UpdateChildContainers()
    {
        foreach (var item in _messageContainers)
        {
            item.UpdateSize();
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
        catch (System.Exception) { }
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
                foreach (var item in _messageContainers)
                {
                    var rect = item.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(0.5f, 0);
                }
            }
        }
    }
}
