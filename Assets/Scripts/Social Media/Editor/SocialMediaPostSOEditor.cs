using System;
using System.Collections.Generic;
using MeetAndTalk;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SocialMediaPostSO), true)]
public class SocialMediaPostSOEditor : Editor
{
    SerializedProperty _characterProp, _messageProp, _messageTextsProp, _typeProp, _imageProp, _videoProp, _videoThumbnailProp, _commentsProp, _displayProp, _backgroundProp, _targetPlatformProp;

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
        _targetPlatformProp = serializedObject.FindProperty("TargetPlatform");
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
        //EditorGUILayout.PropertyField(_messageProp);
        var rect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, 80);
        switch (_targetPlatformProp.enumValueIndex)
        {
            case (int)MediaTargetPlatform.SocialMediaPost:
                GUI.DrawTexture(rect, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Social Media/Sprites/daily_socials_logo.png"), ScaleMode.ScaleToFit, false, 0);
                EditorGUILayout.HelpBox("This post will appear on the standard social media platform.", MessageType.Info);
                break;
            case (int)MediaTargetPlatform.SpicySocialMediaPost:
                GUI.DrawTexture(rect, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Social Media/Sprites/spicy_social_logo.png"), ScaleMode.ScaleToFit, false, 0);
                EditorGUILayout.HelpBox("This post will appear on the spicy social media platform.", MessageType.Info);
                break;
        }

        EditorGUILayout.PropertyField(_targetPlatformProp);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_characterProp);
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