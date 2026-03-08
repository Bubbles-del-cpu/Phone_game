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
    public bool BaseSocialMediaProfileItem;
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
        DialogueChapterManager.ChapterData chapter = null;
        switch (ChapterType)
        {
            case ChapterType.Standalone:
                chapter = DialogueChapterManager.Instance.StandaloneChapters[ChapterIndex];
                break;
            case ChapterType.Story:
                chapter = DialogueChapterManager.Instance.StoryList[ChapterIndex];
                break;

        }

        if (chapter == null)
            return null;

        var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, NodeGUID);
        return node;
    }
}