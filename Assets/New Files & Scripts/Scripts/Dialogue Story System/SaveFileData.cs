using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;

[Serializable]
public class SaveFileData
{
    public static string SAVE_FILE_VERSION = "0.15.beta";
    public string Version;
    public int SaveFileSlot;
    public bool ForceUnlockAllChapters;
    public bool DisplayHints;
    public MediaData CustomBackgroundImage;
    public SystemLanguage CurrentLanguage = SystemLanguage.English;
    [NonSerialized] public GameSaveState CurrentState;
    public GameSaveState AutoSaveState;
    public List<GameSaveState> SaveStates;
    public List<MediaData> UnlockedMedia;

    [System.Serializable]
    public class GlobalSaveVariable
    {
        public int ID;
        public string Value;
        public List<string> PreviousValues;
        public GlobalValueType Type;
        public string Name;
    }

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

    [System.Serializable]
    public class LikedSocialMediaPosts
    {
        public string NodeGUID;
    }

    [System.Serializable]
    public class GameSaveState
    {
        public bool IsSaved;
        public string Name;
        public ChapterSaveData LastChapter;
        public List<int> CompletedChapters;
        public List<GlobalSaveVariable> SavedVariables;
        public List<LikedSocialMediaPosts> LikedPosts;
        public List<SeenCharacterSaveData> SeenCharacterIDs;
        public List<string> LastVisibleSocialMediaPosts;
        public List<string> LastVisibleSpicySocialMediaPosts;

        public GameSaveState()
        {
            LastChapter = new ChapterSaveData();
            LikedPosts = new List<LikedSocialMediaPosts>();
            CompletedChapters = new List<int>();
            SavedVariables = new List<GlobalSaveVariable>();
            SeenCharacterIDs = new List<SeenCharacterSaveData>();
            LastVisibleSocialMediaPosts = new List<string>();
            LastVisibleSpicySocialMediaPosts = new List<string>();
        }

        public void SetupForNewChapter(DialogueChapterManager.ChapterData chapterData, int chapterIndex)
        {
            LastChapter.FileIndex = chapterIndex;
            LastChapter.FileName = chapterData.Story.name;
            LastChapter.StartID = "";
        }

        public bool IsChapterCompleted(int chapterIndex)
        {
            return CompletedChapters.Contains(chapterIndex);
        }

        public GameSaveState Clone()
        {
            var newClone = new GameSaveState();

            newClone.CompletedChapters = new List<int>(CompletedChapters);

            newClone.LastChapter = new ChapterSaveData()
            {
                CurrentGUID = LastChapter.CurrentGUID,
                Completed = LastChapter.Completed,
                FileIndex = LastChapter.FileIndex,
                FileName = LastChapter.FileName,
                StartID = LastChapter.StartID
            };

            foreach (var character in SeenCharacterIDs)
            {
                newClone.SeenCharacterIDs.Add(new SeenCharacterSaveData()
                {
                    CharacterID = character.CharacterID,
                    TimesSeen = character.TimesSeen
                });
            }

            foreach (var post in LastVisibleSocialMediaPosts)
            {
                newClone.LastVisibleSocialMediaPosts.Add(post);
            }

            foreach (var post in LastVisibleSpicySocialMediaPosts)
            {
                newClone.LastVisibleSpicySocialMediaPosts.Add(post);
            }

            foreach (var pastConv in LastChapter.PastCoversations)
            {
                newClone.LastChapter.PastCoversations.Add(new ChapterSaveData.PastCoversationData()
                {
                    GUID = pastConv.GUID,
                    IsChoice = pastConv.IsChoice,
                    SelectedChoice = pastConv.SelectedChoice, //Soon to be obsolete
                    Text = pastConv.Text, //Soon to be obsolete
                    Texts = pastConv.Texts,
                    SelectedChoiceTexts = pastConv.SelectedChoiceTexts
                });
            }

            foreach (var varible in SavedVariables)
            {
                newClone.SavedVariables.Add(new GlobalSaveVariable()
                {
                    ID = varible.ID,
                    Name = varible.Name,
                    Type = varible.Type,
                    Value = varible.Value,
                    PreviousValues = varible.PreviousValues
                });
            }

            foreach (var likedPost in LikedPosts)
            {
                newClone.LikedPosts.Add(new LikedSocialMediaPosts()
                {
                    NodeGUID = likedPost.NodeGUID
                });
            }

            return newClone;
        }
    }

    public static SaveFileData CreateBaseSave(int slot)
    {
        SaveFileData saveFile = new SaveFileData();

        saveFile.Version = SAVE_FILE_VERSION;
        saveFile.SaveFileSlot = slot;
        saveFile.CurrentLanguage = DialogueManager.Instance.localizationManager.selectedLang;
        saveFile.UnlockedMedia = new List<MediaData>();
        saveFile.ForceUnlockAllChapters = false;
        saveFile.DisplayHints = false;

        saveFile.SaveStates = new List<GameSaveState>();
        saveFile.AutoSaveState = new GameSaveState();
        saveFile.AutoSaveState.SavedVariables = SaveAndLoadManager.Instance.ValueManager.ConvertSaveFile();
        saveFile.AutoSaveState.LastChapter = new ChapterSaveData();

        saveFile.AutoSaveState.LastVisibleSocialMediaPosts = new List<string>();
        saveFile.AutoSaveState.LastVisibleSpicySocialMediaPosts = new List<string>();
        saveFile.AutoSaveState.SeenCharacterIDs = new List<SeenCharacterSaveData>();

        saveFile.UpdateMediaData(generateThumbnails: false);

        return saveFile;
    }


    /// <summary>
    /// Compares the save file against another and added any missing data
    /// Useful for if the story content has been updated but you'd like to continue using the same save file
    /// </summary>
    /// <param name="newSaveFile">Save file to compare against</param>
    public void ComparedAgainstLastest(SaveFileData newSaveFile)
    {
        var wasUpdated = false;
        var oldVersion = Version;
        if (Version != newSaveFile.Version)
        {
            wasUpdated = true;
        }

        //Check the saved variables to make sure we have all the ones we need
        foreach (var item in newSaveFile.AutoSaveState.SavedVariables)
        {
            if (!AutoSaveState.SavedVariables.Select(x => x.Name).Contains(item.Name))
            {
                AutoSaveState.SavedVariables.Add(item);
                wasUpdated = true;
            }
        }

        UpdateMediaData(generateThumbnails: true);
        if (AutoSaveState.CompletedChapters.Contains(AutoSaveState.LastChapter.FileIndex))
        {
            // This should'nt ever occur, this was old functionality that has been altered.
            // If the chapter is "complete" but the last chapter contains the same file index then just remove it from the completed chapters list
            AutoSaveState.CompletedChapters.Remove(AutoSaveState.LastChapter.FileIndex);
        }

        // Check the background image data to make sure we account for the new target platform
        if (wasUpdated && newSaveFile.Version == "0.15.beta")
        {
            // This version added the MediaTargetPlatform enum change as well as the SpicySocialMediaPost option
            if (CustomBackgroundImage != null)
            {
                Debug.Log($"[SaveAndLoadManager] Updating custom background image target platform for save slot {SaveFileSlot} from version {oldVersion} to {newSaveFile.Version}");
                if (CustomBackgroundImage.IsSocialMediaPost && CustomBackgroundImage.TargetPlatform == MediaTargetPlatform.Gallery)
                {
                    // Update to SocialMediaPost target platform
                    // If the image is a social media post but the target platform is still Gallery, update it. It cannot be SpicySocialMediaPost at this specific point
                    // As this social media app wasn't present in earlier versions
                    Debug.Log($"[SaveAndLoadManager] Updating custom background image target platform to SocialMediaPost for save slot {SaveFileSlot}");
                    CustomBackgroundImage.TargetPlatform = MediaTargetPlatform.SocialMediaPost;
                }
                else
                {
                    // Ensure Gallery target platform for non-social media posts
                    Debug.Log($"[SaveAndLoadManager] Setting custom background image target platform to Gallery for save slot {SaveFileSlot}");
                    CustomBackgroundImage.TargetPlatform = MediaTargetPlatform.Gallery;
                }
            }
        }

        if (wasUpdated)
        {
            //Save the file so that it is instantly updated with the new change and output a message to the debugger
            Debug.Log($"[SaveAndLoadManager] Save file detected differences between current and latest. Chapters and save variables has been updated for slot {SaveFileSlot}");
        }

        // Update the version
        Version = newSaveFile.Version;
    }

    public void UpdateMediaData(bool generateThumbnails)
    {
        var mediaCopy = new List<MediaData>(UnlockedMedia);
        UnlockedMedia.Clear();

        //Collect all new gallery content and update any existing if required
        CollectMediaFromChapters(mediaCopy, generateThumbnails);
    }

    private void CollectMediaFromChapters(IEnumerable<MediaData> saveFileData, bool generateThumbnails)
    {
        //Add all the gallery content, initially everything will start out as locked
        foreach (var chapter in DialogueChapterManager.Instance.StoryList)
        {
            foreach (var dialogueNode in chapter.Story.DialogueNodeDatas)
                AddMedia(chapter, dialogueNode);
        }

        foreach (var chapter in DialogueChapterManager.Instance.StandaloneChapters)
        {
            foreach (var dialogueNode in chapter.Story.DialogueNodeDatas)
                AddMedia(chapter, dialogueNode);
        }

        if (generateThumbnails)
            GameManager.Instance.GenerateThumbnails();

        //Take the current saved media and buttons and unlock them based on our save file data
        foreach (var item in saveFileData)
        {
            try
            {
                // If the file name is empty, we need to get the node and unlock based on that
                var node = item.GetNode();
                if (node != null)
                {
                    switch (item.LockedState)
                    {
                        case MediaLockState.Unlocked:
                            var dialogueNode = (DialogueNodeData)node;
                            UnlockMedia(dialogueNode, item.IsLinearPathUnlock);
                            UnlockMedia(dialogueNode, item.IsLinearPathUnlock);

                            /* Check if the media is a social media post and make sure the profile button exists on the social media canvas
                            * Because the social media app now contains profile buttons that should exist across chapters, its possible that we need to
                            * Display a profile button for a character even if the character hasn't performed a social media post this chapter
                            * This is only relevant for save file load as the chapter repopulation will handle it chapter posts and thus profile button creation
                            */
                            if (dialogueNode.Post != null && item.IsLinearPathUnlock)
                            {
                                switch (dialogueNode.Post.TargetPlatform)
                                {
                                    case MediaTargetPlatform.SocialMediaPost:
                                        GameManager.Instance.SocialMediaCanvas.AddProfileButton(dialogueNode.Post);
                                        break;
                                    case MediaTargetPlatform.SpicySocialMediaPost:
                                        GameManager.Instance.SpicySocialMediaCanvas.AddProfileButton(dialogueNode.Post);
                                        break;
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to collect media from chapter. {ex.Message}");
            }
        }
    }

    public ChapterSaveData CurrentChapterData => CurrentState.LastChapter;

    public void CompletedCurrentChapter()
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (!CurrentState.CompletedChapters.Contains(CurrentChapterData.FileIndex))
            CurrentState.CompletedChapters.Add(CurrentChapterData.FileIndex);

        CurrentChapterData.Completed = true;
    }

    public void RemoveNode(BaseNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (!CurrentChapterData.PastCoversations.Select(x => x.GUID).Contains(nodeData.NodeGuid))
            return;

        CurrentChapterData.PastCoversations.RemoveAll(x => x.GUID == nodeData.NodeGuid);
        if (nodeData.AssignedCharacter != null)
        {
            // Decrement seen character count or remove if zero
            if (CurrentState.SeenCharacterIDs.Select(x => x.CharacterID).Contains(nodeData.AssignedCharacter.ID))
            {
                var seenCharData = CurrentState.SeenCharacterIDs.First(x => x.CharacterID == nodeData.AssignedCharacter.ID);
                seenCharData.TimesSeen -= 1;
                if (seenCharData.TimesSeen <= 0)
                {
                    CurrentState.SeenCharacterIDs.RemoveAll(x => x.CharacterID == nodeData.AssignedCharacter.ID);
                }
            }
        }
    }

    public void AddNode(BaseNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter || nodeData == null)
            return;

        if (CurrentChapterData.PastCoversations.Select(x => x.GUID).Contains(nodeData.NodeGuid))
        {
            return;
        }

        var newConversation = new ChapterSaveData.PastCoversationData()
        {
            GUID = nodeData.NodeGuid,
        };

        // Record seen character, create a new one or increment existing
        if (nodeData.AssignedCharacter != null)
        {
            // Record seen character, create a new one or increment existing
            if (CurrentState.SeenCharacterIDs.Select(x => x.CharacterID).Contains(nodeData.AssignedCharacter.ID))
            {
                var seenCharData = CurrentState.SeenCharacterIDs.First(x => x.CharacterID == nodeData.AssignedCharacter.ID);
                seenCharData.TimesSeen += 1;
            }
            else
            {
                // Add new seen character entry
                CurrentState.SeenCharacterIDs.Add(new SeenCharacterSaveData()
                {
                    CharacterID = nodeData.AssignedCharacter.ID,
                    TimesSeen = 1
                });
            }
        }

        CurrentChapterData.CurrentGUID = nodeData.NodeGuid;
        CurrentChapterData.PastCoversations.Add(newConversation);
    }

    /// <summary>
    /// Adds a social media post to the list of visible posts in the save file
    /// </summary>
    /// <param name="nodeData">The dialogue node data containing the social media post to add.</param>
    public void AddSocialMediaPost(DialogueNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (nodeData.Post == null)
            return;

        switch (nodeData.Post.TargetPlatform)
        {
            case MediaTargetPlatform.SocialMediaPost:
                if (CurrentState.LastVisibleSocialMediaPosts.Count >= DialogueManager.Instance.MaximumNumberOfSocialPosts)
                {
                    // Remove the oldest post
                    CurrentState.LastVisibleSocialMediaPosts.RemoveAt(0);
                }

                CurrentState.LastVisibleSocialMediaPosts.Add(nodeData.NodeGuid);
                break;
            case MediaTargetPlatform.SpicySocialMediaPost:
                if (CurrentState.LastVisibleSpicySocialMediaPosts.Count >= DialogueManager.Instance.MaximumNumberOfSocialPosts)
                {
                    // Remove the oldest post
                    CurrentState.LastVisibleSpicySocialMediaPosts.RemoveAt(0);
                }

                CurrentState.LastVisibleSpicySocialMediaPosts.Add(nodeData.NodeGuid);
                break;
        }
    }

    /// <summary>
    /// Removes a social media post from the list of visible posts in the save file
    /// </summary>
    /// <param name="nodeData">The dialogue node data containing the social media post to remove.</param>
    public void RemoveSocialMediaPost(DialogueNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (nodeData.Post == null)
            return;

        switch (nodeData.Post.TargetPlatform)
        {
            case MediaTargetPlatform.SocialMediaPost:
                if (CurrentState.LastVisibleSocialMediaPosts.Contains(nodeData.NodeGuid))
                {
                    CurrentState.LastVisibleSocialMediaPosts.RemoveAll(x => x == nodeData.NodeGuid);
                }
                break;
            case MediaTargetPlatform.SpicySocialMediaPost:
                if (CurrentState.LastVisibleSpicySocialMediaPosts.Contains(nodeData.NodeGuid))
                {
                    CurrentState.LastVisibleSpicySocialMediaPosts.RemoveAll(x => x == nodeData.NodeGuid);
                }
                break;
        }
    }

    public bool AddMedia(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData)
    {
        var postData = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.MediaFileName);
        if (postData == null && nodeData.MediaFileName != string.Empty)
        {
            //Add Post
            UnlockedMedia.Add(new MediaData()
            {
                NodeGUID = nodeData.NodeGuid,
                FileName = nodeData.MediaFileName,
                ChapterIndex = chapterData.ChapterIndex,
                ChapterType = chapterData.IsStoryChapter ? ChapterType.Story : ChapterType.Standalone,
                LockedState = MediaLockState.Locked,
                IsLinearPathUnlock = false,
                NotBackgroundCapable = nodeData.NotBackgroundCapable,
                TargetPlatform = MediaTargetPlatform.Gallery,
                IsSocialMediaPost = false,
                Node = nodeData
            });

            GameManager.Instance.SetVideoFrame(nodeData.Video, nodeData.VideoThumbnail);
        }

        if (nodeData.Post != null)
        {
            var socialPostData = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.Post.MediaFileName);
            if (socialPostData == null && nodeData.Post.MediaFileName != string.Empty)
            {
                //Add social post
                UnlockedMedia.Add(new MediaData()
                {
                    NodeGUID = nodeData.NodeGuid,
                    FileName = nodeData.Post.MediaFileName,
                    ChapterIndex = chapterData.ChapterIndex,
                    ChapterType = chapterData.IsStoryChapter ? ChapterType.Story : ChapterType.Standalone,
                    LockedState = MediaLockState.Locked,
                    NotBackgroundCapable = nodeData.Post.NotBackgroundCapable,
                    IsLinearPathUnlock = false,
                    TargetPlatform = nodeData.Post.TargetPlatform,
                    IsSocialMediaPost = true,
                    Node = nodeData
                });

                GameManager.Instance.SetVideoFrame(nodeData.Post.Video, nodeData.Post.VideoThumbnail);
            }
        }

        return true;
    }

    /// <summary>
    /// Unlocks media in the save file based on the provided node data
    /// </summary>
    /// <param name="nodeData">The node data containing media information</param>
    /// <param name="linearPath">Indicates if the unlock is part of a linear path (non-replay unlock)</param>
    public void UnlockMedia(DialogueNodeData nodeData, bool linearPath)
    {
        var item = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.MediaFileName);
        if (item != null)
        {
            //We found the media, unlock it
            SetLockState(nodeData.MediaFileName, item, MediaLockState.Unlocked, linearPath);
        }

        if (nodeData.Post != null)
        {
            var socialItem = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.Post.MediaFileName);
            if (socialItem != null)
            {
                SetLockState(nodeData.Post.MediaFileName, socialItem, MediaLockState.Unlocked, linearPath);
            }
        }
    }

    /// <summary>
    /// Rolls back the unlock of media in the save file based on the provided node data
    /// </summary>
    /// <param name="nodeData">The node data containing media information</param>
    public void RollbackUnlockMedia(DialogueNodeData nodeData)
    {
        var item = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.MediaFileName);
        if (item != null)
        {
            //We found the media, unlock it
            SetLockState(nodeData.MediaFileName, item, MediaLockState.Locked, false);
        }

        if (nodeData.Post != null)
        {
            var socialItem = UnlockedMedia.FirstOrDefault(x => x.NodeGUID == nodeData.NodeGuid && x.FileName == nodeData.Post.MediaFileName);
            if (socialItem != null)
            {
                SetLockState(nodeData.Post.MediaFileName, socialItem, MediaLockState.Locked, false);
            }
        }
    }

    private void SetLockState(string fileName, MediaData item, MediaLockState state, bool linearPath)
    {
        item.FileName = fileName;
        item.LockedState = state;
        item.IsLinearPathUnlock = linearPath;
    }

    public void UnlockAllMedia(bool save = true)
    {
        foreach (var item in UnlockedMedia)
        {
            item.LockedState = MediaLockState.Unlocked;
        }

        //if (save)
        //SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
    }

    public void LikePost(BaseNodeData nodeData, bool state)
    {
        if (state)
        {
            //Add the liked post to the save file
            if (!CurrentState.LikedPosts.Select(x => x.NodeGUID).Contains(nodeData.NodeGuid))
            {
                var newLikedPost = new LikedSocialMediaPosts()
                {
                    NodeGUID = nodeData.NodeGuid,
                };

                CurrentState.LikedPosts.Add(newLikedPost);
            }
        }
        else
        {
            //Remove the liked post from the save file
            CurrentState.LikedPosts.RemoveAll(x => x.NodeGUID == nodeData.NodeGuid);
        }
    }

    public void MakeChoice(BaseNodeData nodeData, List<LanguageGeneric<string>> choices)
    {
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            var past = CurrentChapterData.PastCoversations.FirstOrDefault(x => x.GUID == nodeData.NodeGuid);
            if (past != null)
            {
                //Don't set SelectedChoice anymore as its obsolete
                //past.SelectedChoice = choices.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;
                past.SelectedChoiceTexts = choices;
            }
        }


        //Update the runtime node data with the choice so that we can
        //check the selected choice against specific rules for the rollback action
        var choiceText = choices.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;
        switch (nodeData)
        {
            case TimerChoiceNodeData nd:
                nd.SelectedChoice = choices;
                break;
            case DialogueChoiceNodeData nd:
                nd.SelectedChoice = choices;
                break;
        }
    }

    public void UpdateText(BaseNodeData nodeData, List<LanguageGeneric<string>> texts)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        var past = CurrentChapterData.PastCoversations.FirstOrDefault(x => x.GUID == nodeData.NodeGuid);
        if (past != null)
        {
            //Don't set Text anymore as its obsolete
            //past.Text = texts.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;
            past.Texts = texts;
        }
    }
}
