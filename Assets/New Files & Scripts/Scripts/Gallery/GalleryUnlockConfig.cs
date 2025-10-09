using System;
using UnityEngine;

[CreateAssetMenu(fileName ="new GalleryUnlockConfig", menuName = "GalleryUnlockConfig")]
public class GalleryUnlockConfig : ScriptableObject
{
    [SerializeField] private string _codeHash; // Store hash, not plain text
    [SerializeField] private string _salt = "YourGameSpecificSalt2024";
    [SerializeField] private int _reference;

    public string Salt => _salt;
    public string Hash => _codeHash;
    public int Length => _reference;

#if UNITY_EDITOR
    [Header("Editor Only - For Hash Generation")]
    [SerializeField] private string plainTextCode = "";

    [ContextMenu("Generate Hash from Plain Text")]
    public void GenerateHash()
    {
        if (!string.IsNullOrEmpty(plainTextCode))
        {
            _codeHash = GalleryHelper.ComputeHash(plainTextCode, _salt);
            _reference = plainTextCode.Length;
            plainTextCode = ""; // Clear after generating
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
