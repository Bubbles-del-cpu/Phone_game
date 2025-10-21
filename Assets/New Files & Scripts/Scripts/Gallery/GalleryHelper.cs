using System;
using System.Security.Cryptography;
using System.Text;

public class GalleryHelper
{
    public static string USED_PASS;
    private string _salt;
    private string _hash;
    private int _refLength;
    public GalleryHelper(GalleryCanvas.GalleryUnlockData data, string pass)
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

    private bool IsValidCode(string inputCode)
    {
#if UNITY_EDITOR
        if (inputCode == "DEV UNLOCK")
            return true;
#endif
        string inputHash = ComputeHash(inputCode, _salt);
        return string.Equals(_hash, inputHash, StringComparison.Ordinal);
    }

    public void Unlock()
    {
        //Unlock the gallery buttons
        foreach (var item in SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia)
        {
            GameManager.Instance.GalleryCanvas.UnlockMedia(item.FileName, reloadedGallery: false);
        }

        SaveAndLoadManager.Instance.CurrentSave.UnlockAllMedia();
        SaveAndLoadManager.Instance.AutoSave();
        GameManager.Instance.GalleryCanvas.RefreshGalleryPage();
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