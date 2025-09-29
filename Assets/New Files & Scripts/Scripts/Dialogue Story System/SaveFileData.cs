using MeetAndTalk;
using MeetAndTalk.GlobalValue;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class SaveFileData
{
    public static string SAVE_FILE_VERSION = "0.10";
    public string Version;
    public int SaveFileSlot;
    public bool ForceUnlockAllChapters;
    public bool DisplayHints;
    public MediaData CustomBackgroundImage;
    public SystemLanguage LocalizationValue;
    [NonSerialized] public GameSaveState CurrentState;
    public int TotalChapters => CurrentState.Chapters.Count;
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

    [System.Serializable]
    public class MediaData
    {
        public string NodeGUID = string.Empty;
        public string FileName = string.Empty;
        public int ChapterIndex;
        public bool IsSocialMediaPost;
        public bool NotBackgroundCapable;
        public ChapterType ChapterType;
        public MediaLockState LockedState;
        [NonSerialized] public DialogueNodeData Node;
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
        public List<ChapterSaveData> Chapters;
        public List<int> CompletedChapters;
        public List<GlobalSaveVariable> SavedVariables;
        public List<LikedSocialMediaPosts> LikedPosts;
        public GameSaveState()
        {
            Chapters = new List<ChapterSaveData>();
            LikedPosts = new List<LikedSocialMediaPosts>();
            CompletedChapters = new List<int>();
            SavedVariables = new List<GlobalSaveVariable>();
        }

        public GameSaveState Clone()
        {
            var newClone = new GameSaveState();

            newClone.CompletedChapters = new List<int>(CompletedChapters);

            for (var index = 0; index < Chapters.Count; index++)
            {
                var item = Chapters[index];
                var newChapter = new ChapterSaveData()
                {
                    CurrentGUID = item.CurrentGUID,
                    Completed = item.Completed,
                    FileIndex = item.FileIndex,
                    FileName = item.FileName,
                    StartID = item.StartID
                };

                foreach (var pastConv in item.PastCoversations)
                {
                    newChapter.PastCoversations.Add(new ChapterSaveData.PastCoversationData()
                    {
                        GUID = pastConv.GUID,
                        IsChoice = pastConv.IsChoice,
                        SelectedChoice = pastConv.SelectedChoice,
                        Text = pastConv.Text
                    });
                }

                newClone.Chapters.Add(newChapter);
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
        saveFile.LocalizationValue = DialogueManager.Instance.localizationManager.selectedLang;
        saveFile.UnlockedMedia = new List<MediaData>();
        saveFile.ForceUnlockAllChapters = false;
        saveFile.DisplayHints = false;

        saveFile.SaveStates = new List<GameSaveState>();
        for (var index = 0; index < 4; index++)
        {
            saveFile.SaveStates.Add(new GameSaveState());
        }

        int count = 0;

        saveFile.AutoSaveState = new GameSaveState();
        saveFile.AutoSaveState.SavedVariables = SaveAndLoadManager.Instance.ValueManager.ConvertSaveFile();
        foreach (var item in DialogueChapterManager.Instance.StoryList)
        {
            saveFile.AutoSaveState.Chapters.Add(new ChapterSaveData()
            {
                CurrentGUID = "",
                Completed = false,
                FileIndex = count++,
                FileName = $"{item.Story.name}",
                StartID = item.StartID
            });
        }

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
            Version = newSaveFile.Version;
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

        //If there is a discrepancy in the total number of chapters then we need to update the collection
        if (newSaveFile.AutoSaveState.Chapters.Count > AutoSaveState.Chapters.Count)
        {
            //Loop forward from the last current chapter and add any missing to the list
            for (var index = AutoSaveState.Chapters.Count; index < newSaveFile.AutoSaveState.Chapters.Count; index++)
            {
                var item = newSaveFile.AutoSaveState.Chapters[index];
                AutoSaveState.Chapters.Add(new ChapterSaveData()
                {
                    CurrentGUID = "",
                    Completed = false,
                    FileIndex = index,
                    FileName = $"{item.FileName}",
                    StartID = item.StartID
                });

            }

            wasUpdated = true;
        }

        UpdateMediaData(generateThumbnails: true);

        if (wasUpdated)
        {
            //Save the file so that it is instantly updated with the new change and output a message to the debugger
            Debug.Log($"[SaveAndLoadManager] Save file detected differences between current and latest. Chapters and save variables has been updated for slot {SaveFileSlot}");
        }

        SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
    }

    public void UpdateMediaData(bool generateThumbnails)
    {
        var mediaCopy = new List<MediaData>(UnlockedMedia);
        UnlockedMedia.Clear();

        //Collect all new gallery content and update any existing if required
        CollectMediaFromChapters(mediaCopy, generateThumbnails);
        //CollectMediaFromChapters(DialogueChapterManager.Instance.StandaloneChapters, mediaCopy.Where(x => !x.ChapterType));
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
                if (item.FileName == string.Empty)
                {
                    var chapter = DialogueChapterManager.Instance.StoryList[item.ChapterIndex];
                    switch (item.ChapterType)
                    {
                        case ChapterType.Standalone:
                            chapter = DialogueChapterManager.Instance.StandaloneChapters[item.ChapterIndex];
                            break;

                    }

                    var node = DialogueNodeHelper.GetNodeByGuid(chapter.Story, item.NodeGUID);
                    if (node != null)
                    {
                        switch (item.LockedState)
                        {
                            case MediaLockState.Unknown:
                                UnlockMedia((DialogueNodeData)node, false);
                                break;
                            case MediaLockState.Unlocked:
                                UnlockMedia((DialogueNodeData)node, false);
                                break;
                        }
                    }
                }
                else
                {
                    switch (item.LockedState)
                    {
                        case MediaLockState.Unknown:
                        case MediaLockState.Unlocked:
                            UnlockMedia(item.FileName, false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Valied to collect media from chapter. {ex.Message}");
            }
        }
    }

    public ChapterSaveData CurrentChapterData
    {
        get
        {
            return CurrentState.Chapters[DialogueChapterManager.Instance.CurrentChapter.ChapterIndex];
        }
    }

    public void CompletedCurrentChapter()
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (!CurrentState.CompletedChapters.Contains(CurrentChapterData.FileIndex))
            CurrentState.CompletedChapters.Add(CurrentChapterData.FileIndex);

        CurrentChapterData.Completed = true;
        SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
    }

    public void ClearCurrentChapter()
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        CurrentState.CompletedChapters.Remove(CurrentChapterData.FileIndex);
        CurrentChapterData.CurrentGUID = "";
        CurrentChapterData.Completed = false;
    }

    public void RemoveNode(BaseNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (!CurrentChapterData.PastCoversations.Select(x => x.GUID).Contains(nodeData.NodeGuid))
            return;

        CurrentChapterData.PastCoversations.RemoveAll(x => x.GUID == nodeData.NodeGuid);
    }

    public void AddNode(BaseNodeData nodeData)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        if (CurrentChapterData.PastCoversations.Select(x => x.GUID).Contains(nodeData.NodeGuid))
        {
            return;
        }

        var newConversation = new ChapterSaveData.PastCoversationData()
        {
            GUID = nodeData.NodeGuid,
        };

        switch (nodeData)
        {
            case DialogueChoiceNodeData:
            case TimerChoiceNodeData:
                newConversation.IsChoice = true;
                break;
        }

        CurrentChapterData.CurrentGUID = nodeData.NodeGuid;
        CurrentChapterData.PastCoversations.Add(newConversation);
    }

    public bool AddMedia(DialogueChapterManager.ChapterData chapterData, DialogueNodeData nodeData)
    {
        var postData = UnlockedMedia.FirstOrDefault(x => x.FileName == nodeData.MediaFileName);
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
                NotBackgroundCapable = nodeData.NotBackgroundCapable,
                IsSocialMediaPost = false,
                Node = nodeData
            });

            GameManager.Instance.SetVideoFrame(nodeData.Video, nodeData.VideoThumbnail);
        }

        if (nodeData.Post != null)
        {
            var socialPostData = UnlockedMedia.FirstOrDefault(x => x.FileName == nodeData.Post.MediaFileName);
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
                    IsSocialMediaPost = true,
                    Node = nodeData
                });

                GameManager.Instance.SetVideoFrame(nodeData.Post.Video, nodeData.Post.VideoThumbnail);
            }
        }

        return true;
    }

    public void UnlockMedia(DialogueNodeData nodeData, bool save = true)
    {
        var item = UnlockedMedia.FirstOrDefault(x => x.FileName == nodeData.MediaFileName);
        if (item != null)
        {
            //We found the media, unlock it
            item.LockedState = MediaLockState.Unlocked;
            item.FileName = nodeData.MediaFileName;
        }

        if (nodeData.Post != null)
        {
            var socialItem = UnlockedMedia.FirstOrDefault(x => x.FileName == nodeData.Post.MediaFileName);
            if (socialItem != null)
            {
                socialItem.FileName = nodeData.Post.MediaFileName;
                socialItem.LockedState = MediaLockState.Unlocked;
            }
        }

        if (save)
            SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
    }

    private void UnlockMedia(string fileName, bool save = true)
    {
        var item = UnlockedMedia.FirstOrDefault(x => x.FileName == fileName);
        if (item != null)
        {
            item.LockedState = MediaLockState.Unlocked;
        }

        if (save)
            SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
    }

    public void UnlockAllMedia(bool save = true)
    {
        foreach (var item in UnlockedMedia)
        {
            item.LockedState = MediaLockState.Unlocked;
        }

        if (save)
            SaveAndLoadManager.SaveToJson(this, SaveFileSlot);
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

    public void MakeChoice(BaseNodeData nodeData, string choice)
    {
        if (!SaveAndLoadManager.Instance.ReplayingCompletedChapter)
        {
            var past = CurrentChapterData.PastCoversations.FirstOrDefault(x => x.GUID == nodeData.NodeGuid);
            if (past != null)
            {
                past.SelectedChoice = choice;
            }
        }


        //Update the runtime node data with the choice so that we can
        //check the selected choice against specific rules for the rollback action
        switch (nodeData)
        {
            case DialogueChoiceNodeData nd:
                nd.SelectedChoice = choice;
                break;
            case TimerChoiceNodeData nd:
                nd.SelectedChoice = choice;
                break;
        }
    }

    public void UpdateText(BaseNodeData nodeData, string text)
    {
        if (SaveAndLoadManager.Instance.ReplayingCompletedChapter)
            return;

        var past = CurrentChapterData.PastCoversations.FirstOrDefault(x => x.GUID == nodeData.NodeGuid);
        if (past != null)
        {
            past.Text = text;
        }
    }
}
