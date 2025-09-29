using UnityEditor;
using UnityEditor.Rendering;

[CustomEditor(typeof(SocialMediaPostSO))]
public class SocialMediaPostSOEditor : UnityEditor.Editor
{
    SerializedProperty _characterProp, _messageProp, _typeProp, _imageProp, _videoProp, _videoThumbnailProp, _commentsProp, _displayProp, _backgroundProp;

    void OnEnable()
    {
        _characterProp = serializedObject.FindProperty("Character");
        _messageProp = serializedObject.FindProperty("Message");
        _typeProp = serializedObject.FindProperty("MediaType");
        _commentsProp = serializedObject.FindProperty("Comments");
        _imageProp = serializedObject.FindProperty("Image");
        _videoProp = serializedObject.FindProperty("Video");
        _videoThumbnailProp = serializedObject.FindProperty("VideoThumbnail");
        _displayProp = serializedObject.FindProperty("GalleryVisibility");
        _backgroundProp = serializedObject.FindProperty("NotBackgroundCapable");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_characterProp);
        EditorGUILayout.PropertyField(_messageProp);
        EditorGUILayout.PropertyField(_typeProp);

        switch ((MediaType)_typeProp.enumValueIndex)
        {
            case MediaType.Sprite:
                EditorGUILayout.PropertyField(_imageProp);
                EditorGUILayout.PropertyField(_backgroundProp);
                break;
            case MediaType.Video:
                EditorGUILayout.PropertyField(_videoProp);
                EditorGUILayout.PropertyField(_videoThumbnailProp);
                break;
        }
        EditorGUILayout.PropertyField(_displayProp);
        EditorGUILayout.PropertyField(_commentsProp);
        serializedObject.ApplyModifiedProperties();
    }
}