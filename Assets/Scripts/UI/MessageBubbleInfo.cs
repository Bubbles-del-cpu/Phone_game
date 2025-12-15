using MeetAndTalk;

public struct MessageBubbleInfo
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
                        timelapseBubble.transform.SetParent(panel.MessageContainers[0].transform, false);
                        timelapseBubble.Init(Hidden, nd.GetTimeLapse(), timelapse: true, NodeGUID, Source);
                    }

                    //Add element after timelapse
                    if (Text != string.Empty || nd.Image != null || nd.Video != null)
                    {
                        //var bubble = Instantiate(BubblePrefab, Container);
                        var bubble = DialogueUIManagerObjectPool.Instance.GetMessageBubble(containerSource);
                        bubble.transform.SetParent(panel.MessageContainers[0].transform, false);
                        bubble.Init(Hidden, Text, timelapse: false, NodeGUID, Source);
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
                        bubble.transform.SetParent(panel.MessageContainers[0].transform, false);
                        bubble.Init(Source != containerSource, Text, timelapse: false, NodeGUID, Source);
                    }
                }
                break;
        }
    }
}