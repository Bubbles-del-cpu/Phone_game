public class GalleryUnlockData
{
    public string Salt, Hash;
    public int Length;
    public bool UnlockTriggered;
    public string UsedPass;

    public GalleryUnlockData()
    {
        Salt = GameManager.Instance.GalleryConfig.Salt;
        Hash = GameManager.Instance.GalleryConfig.Hash;
        Length = GameManager.Instance.GalleryConfig.Length;
    }
}