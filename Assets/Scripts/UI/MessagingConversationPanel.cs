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
    [SerializeField] RectTransform[] messageBubbleContainers;
    [SerializeField] MessagingResponsesPanel responsesPanel;
    [SerializeField] ScrollRect _scrollView;
    [SerializeField] RectTransform _contentContainer;
    [SerializeField] private ProfileIcon _characterIcon;
    [SerializeField] private TMP_Text _characterName;
    DialogueCharacterSO character;

    public int ChildCount => messageBubbleContainers[0].transform.childCount;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Open()
    {
        base.Open();

        transform.SetAsLastSibling();

        _characterIcon.Character = character;
        _characterName.text = character.name;

        GameManager.Instance.SetNewMessage(character, false);

        ScrollToBottom();
    }

    public void RemoveElements(int count)
    {
        var objectList = new List<GameObject>();
        for (var cIndex = 0; cIndex < MessageBubbleContainers.Length; cIndex++)
        {
            var index = 1;
            var container = MessageBubbleContainers[cIndex];
            while (index <= count)
            {
                try
                {
                    var item = container.transform.GetChild(container.transform.childCount - index);
                    item.gameObject.SetActive(false);
                    objectList.Add(item.gameObject);
                    index++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to clear conversation panel for {character.name}. Error: {ex.Message}");
                    break;
                }
            }
        }

        foreach (var obj in objectList)
            Destroy(obj);
    }

    public void AddElement(BaseNodeData nodeData, MessagingBubble prefab, string text, DialogueUIManager.MessageSource source, bool notification)
    {
        // Re-added the loop to create hidden bubbles for layout
        for (var index = 0; index < MessageBubbleContainers.Length; index++)
        {
            var container = MessageBubbleContainers[index];
            var containerSource = (DialogueUIManager.MessageSource)index;

            var hidden = source != containerSource;

            switch (nodeData)
            {
                case DialogueNodeData nd when nodeData is DialogueNodeData:
                    {
                        if (nd.GetTimeLapse().Length > 0)
                        {
                            MessagingBubble _timelapseBubble = Instantiate(prefab);
                            _timelapseBubble.transform.SetParent(container, false);
                            _timelapseBubble.Init(hidden, nd.GetTimeLapse());
                            _timelapseBubble.IsTimelapse = true;
                        }

                        //Add element after timelapse
                        if (text != string.Empty || nd.Image != null || nd.Video != null)
                        {
                            var bubble = Instantiate(prefab);
                            bubble.transform.SetParent(container, false);
                            bubble.Init(hidden, text);
                            bubble.SetupMediaViewer(nd); // This line is correct
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
                        if (text != string.Empty && text[0] != '*')
                        {
                            //Frist character is the special action character so don't send the message
                            var bubble = Instantiate(prefab);
                            bubble.transform.SetParent(container, false);
                            bubble.Init(source != containerSource, text);
                            
                            // --- ERROR LINE REMOVED ---
                            // bubble.SetupMediaViewer(nd); // <-- This was the line causing the error
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
            yield return new WaitForSecondsRealtime(delay); 

        while (_scrollView.verticalNormalizedPosition > 0)
        {
            yield return new WaitForEndOfFrame();
            _scrollView.verticalNormalizedPosition -= DialogueUIManager.Instance.MessagePanelAutoScrollSpeed * Time.unscaledDeltaTime;
        }
    }

    public void ScrollToBottom()
    {
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
        // Reverted to original logic that works with the hidden bubble system
        _contentContainer.sizeDelta = new Vector2(_contentContainer.sizeDelta.x, messageBubbleContainers[0].sizeDelta.y);
    }

    public RectTransform[] MessageBubbleContainers { get { return messageBubbleContainers; } }

    public MessagingResponsesPanel ResponsesPanel { get { return responsesPanel; } }

    public DialogueCharacterSO Character { get { return character; } set { character = value; } }
}