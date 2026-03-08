using System.Collections.Generic;
using static SaveFileData;

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