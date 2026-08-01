using System;
using System.Security.Cryptography;
using System.Text;

public class GalleryHelper
{
    public static string USED_PASS;
    private string _salt;
    private string _hash;
    private int _refLength;
    public GalleryHelper(GalleryUnlockData data, string pass)
    {
        _salt = data.Salt;
        _hash = data.Hash;
        _refLength = data.Length;
        USED_PASS = pass;
    }

    public static string ComputeHash(string input, string _salt)
    {
        using (var sha256 = SHA256.Create())
        {
            string combined = input + _salt;
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(bytes);
        }
    }

    public static bool MatchesCode(string inputCode, string salt, string expectedHash)
    {
        if (string.IsNullOrEmpty(inputCode) || string.IsNullOrEmpty(expectedHash))
            return false;

        return string.Equals(expectedHash, ComputeHash(inputCode, salt), StringComparison.Ordinal);
    }

    private bool IsValidCode(string inputCode)
    {
#if UNITY_EDITOR
        if (inputCode == "DEV UNLOCK")
            return true;
#endif
        return MatchesCode(inputCode, _salt, _hash);
    }

    public void Unlock()
    {
        //Unlock the gallery buttons
        foreach (var item in SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia)
        {
            GameManager.Instance.GalleryCanvas.UnlockMedia(item.NodeGUID, item.FileName, reloadedGallery: false);
        }

        SaveAndLoadManager.Instance.CurrentSave.UnlockAllMedia();
        GameManager.Instance.GalleryCanvas.RefreshGalleryPage();
        GameManager.Instance.GalleryCanvas.UnlockData.UnlockTriggered = false;
    }


    public bool CheckLength()
    {
#if UNITY_EDITOR
        if (USED_PASS == "DEV UNLOCK")
            return true;
#endif
        return USED_PASS != string.Empty && USED_PASS.Length == _refLength;
    }

    public bool CheckContent(string p)
    {
#if UNITY_EDITOR
        if (USED_PASS == "DEV UNLOCK")
            return true;
#endif
        return IsValidCode(p);
    }

    public bool CheckHash()
    {
#if UNITY_EDITOR
        if (USED_PASS == "DEV UNLOCK")
            return true;
#endif
        return ComputeHash(USED_PASS, _salt).Length == _hash.Length;
    }

}
