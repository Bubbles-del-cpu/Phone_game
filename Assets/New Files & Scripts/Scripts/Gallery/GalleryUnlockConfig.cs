using System;
using UnityEngine;

[CreateAssetMenu(fileName ="new GalleryUnlockConfig", menuName = "GalleryUnlockConfig")]
public class GalleryUnlockConfig : ScriptableObject
{
    [SerializeField] private string _codeHash; // Store hash, not plain text
    [SerializeField] private string _salt = "YourGameSpecificSalt2024";

    public bool IsValidCode(string inputCode)
    {
#if UNITY_EDITOR
        if (inputCode == "DEV UNLOCK")
            return true;
#endif
        string inputHash = ComputeHashWithSalt(inputCode, _salt);
        return string.Equals(_codeHash, inputHash, StringComparison.Ordinal);
    }

    public void Unlock()
    {
        //Unlock the gallery buttons
        foreach (var item in SaveAndLoadManager.Instance.CurrentSave.UnlockedMedia)
        {
            GameManager.Instance.GalleryCanvas.UnlockMedia(item.FileName);
        }

        SaveAndLoadManager.Instance.CurrentSave.UnlockAllMedia();
        SaveAndLoadManager.Instance.AutoSave();
    }

    private string ComputeHashWithSalt(string input, string salt)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            string combined = input + salt;
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return System.Convert.ToBase64String(bytes);
        }
    }

#if UNITY_EDITOR
    [Header("Editor Only - For Hash Generation")]
    [SerializeField] private string plainTextCode = "";

    [ContextMenu("Generate Hash from Plain Text")]
    public void GenerateHash()
    {
        if (!string.IsNullOrEmpty(plainTextCode))
        {
            _codeHash = ComputeHashWithSalt(plainTextCode, _salt);
            plainTextCode = ""; // Clear after generating
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
