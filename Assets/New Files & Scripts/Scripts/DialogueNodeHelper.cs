using MeetAndTalk;
public static class DialogueNodeHelper
{
    public static BaseNodeData GetNodeByGuid(DialogueContainerSO container, string targetNodeGuid)
    {
        return container.AllNodes.Find(node => node.NodeGuid == targetNodeGuid);
    }

    public static BaseNodeData GetNextNode(DialogueContainerSO container, BaseNodeData baseNodeData)
    {
        NodeLinkData nodeLinkData = container.NodeLinkDatas.Find(edge => edge.BaseNodeGuid == baseNodeData.NodeGuid);
        if (nodeLinkData != null)
        {
            return GetNodeByGuid(container, nodeLinkData.TargetNodeGuid);
        }

        return null;
    }
}
