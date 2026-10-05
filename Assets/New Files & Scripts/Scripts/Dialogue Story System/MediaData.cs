using System;
using MeetAndTalk;

[Serializable]
public class MediaData
{
    public string NodeGUID = string.Empty;
    public string FileName = string.Empty;
    public int ChapterIndex;
    public MediaTargetPlatform TargetPlatform;
    public bool IsSocialMediaPost;
    public bool IsLinearPathUnlock;
    public bool NotBackgroundCapable;
    public ChapterType ChapterType;
    public MediaLockState LockedState;
    [NonSerialized] public DialogueNodeData Node;

    /// <summary>
    /// Gets the node associated with this media data
    /// </summary>
    /// <returns>The node data associated with this media data</returns>
    public BaseNodeData GetNode()
    {
        // If the node has already been set, return it
        if (Node != null)
            return Node;

        // Otherwise, find the node based on the chapter and GUID
        var chapters = ChapterType == ChapterType.Standalone
            ? DialogueChapterManager.Instance.StandaloneChapters
            : DialogueChapterManager.Instance.StoryList;

        if (ChapterIndex >= 0 && ChapterIndex < chapters.Count && chapters[ChapterIndex] != null)
        {
            var node = DialogueNodeHelper.GetNodeByGuid(chapters[ChapterIndex].Story, NodeGUID);
            if (node != null)
                return node;
        }

        // Chapter positions can differ between versions (0.21 listed Prologue 5 before the Christmas special),
        // so fall back to searching every chapter of this type and remember where the node was found
        for (var index = 0; index < chapters.Count; index++)
        {
            if (index == ChapterIndex || chapters[index] == null)
                continue;

            var node = DialogueNodeHelper.GetNodeByGuid(chapters[index].Story, NodeGUID);
            if (node != null)
            {
                ChapterIndex = index;
                return node;
            }
        }

        return null;
    }
}