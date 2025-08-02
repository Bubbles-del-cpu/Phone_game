using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GalleryUnlockConfig))]
class GalleryUnlockConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Hash"))
        {
            GalleryUnlockConfig script = (GalleryUnlockConfig)target;
            script.GenerateHash();
        }
    }
}