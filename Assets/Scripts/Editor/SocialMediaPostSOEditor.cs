using System;
using System.Collections.Generic;
using MeetAndTalk;
using UnityEditor;
using UnityEditor.Rendering;

[CustomEditor(typeof(SocialMediaPostSO))]
public class SocialMediaPostSOEditor : UnityEditor.Editor
{
    SerializedProperty _characterProp, _messageProp, _messageTextsProp, _typeProp, _imageProp, _videoProp, _videoThumbnailProp, _commentsProp, _displayProp, _backgroundProp;

    void OnEnable()
    {
        _characterProp = serializedObject.FindProperty("Character");
        //_messageProp = serializedObject.FindProperty("Message");
        _messageTextsProp = serializedObject.FindProperty("MessageTexts");
        _typeProp = serializedObject.FindProperty("MediaType");
        _commentsProp = serializedObject.FindProperty("Comments");
        _imageProp = serializedObject.FindProperty("Image");
        _videoProp = serializedObject.FindProperty("Video");
        _videoThumbnailProp = serializedObject.FindProperty("VideoThumbnail");
        _displayProp = serializedObject.FindProperty("GalleryVisibility");
        _backgroundProp = serializedObject.FindProperty("NotBackgroundCapable");
    }

    private void CorrectForLocalization()
    {
        var post = (SocialMediaPostSO)target;
        if (post.Message != "")
        {
            post.MessageTexts = new List<LanguageGeneric<string>>();

            foreach (MeetAndTalk.Localization.LocalizationEnum item in Enum.GetValues(typeof(MeetAndTalk.Localization.LocalizationEnum)))
            {
                post.MessageTexts.Add(new LanguageGeneric<string>()
                {
                    languageEnum = item,
                    LanguageGenericType = post.Message
                });

                post.Message = "";
            }
        }

        EditorUtility.SetDirty(target);
    }
    public override void OnInspectorGUI()
    {
        CorrectForLocalization();
        serializedObject.Update();
        EditorGUILayout.PropertyField(_characterProp);
        //EditorGUILayout.PropertyField(_messageProp);
        EditorGUILayout.PropertyField(_messageTextsProp);
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