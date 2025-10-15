using System.Collections.Generic;
using MeetAndTalk;
using UnityEditor;
using UnityEngine;

public static class CustomEditorHelper
{
    public static void DrawDialogueChoiceNodes<T>(List<T> nodeDatas, string foldoutTitle, string description, string Icon, ref bool fold) where T : DialogueChoiceNodeData
    {
        MAT_Editor.FoldoutGroup(foldoutTitle, description, MAT_Editor.GetTinyIcon(Icon), ref fold);
        if (!fold)
            return;

        EditorGUILayout.BeginVertical("HelpBox");
        // List
        for (int i = 0; i < nodeDatas.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            // Display Node
            MAT_Editor.BeginBoxGroup(nodeDatas[i].NodeGuid, i + 1);

            nodeDatas[i].Position = EditorGUILayout.Vector2Field("Position", nodeDatas[i].Position);
            nodeDatas[i].Character = (DialogueCharacterSO)EditorGUILayout.ObjectField("Character", nodeDatas[i].Character, typeof(DialogueCharacterSO), false);
            nodeDatas[i].AvatarPos = (AvatarPosition)EditorGUILayout.EnumPopup("Avatar Display", nodeDatas[i].AvatarPos);
            nodeDatas[i].AvatarType = (AvatarType)EditorGUILayout.EnumPopup("Avatar Emotion", nodeDatas[i].AvatarType);
            nodeDatas[i].Duration = EditorGUILayout.FloatField("Display Time", nodeDatas[i].Duration);
            nodeDatas[i].RequireCharacterInput = EditorGUILayout.Toggle("Require Input", nodeDatas[i].RequireCharacterInput);

            if (nodeDatas[i].GetType() == typeof(TimerChoiceNodeData))
            {
                var cast = nodeDatas[i] as TimerChoiceNodeData;
                cast.time = EditorGUILayout.FloatField("Time to make decision", cast.time);
            }

            for (int j = 0; j < GameManager.LOCALIZATION_MANAGER.lang.Count + 1; j++)
            {
                MAT_Editor.BeginBoxGroup($"{nodeDatas[i].TextType[j].languageEnum}", 00);
                nodeDatas[i].AudioClips[j].LanguageGenericType = (AudioClip)EditorGUILayout.ObjectField("Audio Clips", nodeDatas[i].AudioClips[j].LanguageGenericType, typeof(AudioClip), false);
                if(nodeDatas[i].RequireCharacterInput)
                    nodeDatas[i].TextType[j].LanguageGenericType = EditorGUILayout.TextField("Displayed String", nodeDatas[i].TextType[j].LanguageGenericType);
                var portIndex = 1;
                foreach (var port in nodeDatas[i].DialogueNodePorts)
                {
                    MAT_Editor.BeginBoxGroup($"Choice {j + 1}", portIndex);
                    port.TextLanguage[j].LanguageGenericType = EditorGUILayout.TextField("Choice", port.TextLanguage[j].LanguageGenericType);
                    port.HintLanguage[j].LanguageGenericType = EditorGUILayout.TextField("Choice Hint", port.HintLanguage[j].LanguageGenericType);
                    EditorGUILayout.EndVertical();
                    portIndex++;
                }

                //EditorGUILayout.LabelField("Options: ", EditorStyles.boldLabel);

                // EditorGUI.indentLevel++;

                // EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
            // Display Node
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}