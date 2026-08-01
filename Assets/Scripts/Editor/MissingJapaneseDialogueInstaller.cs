#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MeetAndTalk;
using MeetAndTalk.Localization;
using UnityEditor;
using UnityEngine;

public static class MissingJapaneseDialogueInstaller
{
    private const string TranslationPath = "Assets/Localization/Missing Japanese Dialogue.tsv";

    [MenuItem("Tools/NTS/Apply Missing Japanese Dialogue Translations")]
    public static void ApplyTranslations()
    {
        var translations = LoadTranslations();
        var changedAssets = 0;
        var changedFields = 0;

        foreach (var story in StoryAssets())
        {
            var storyChanges = ApplyToStory(story, translations);
            if (storyChanges == 0)
                continue;

            changedFields += storyChanges;
            changedAssets++;
            EditorUtility.SetDirty(story);
        }

        AssetDatabase.SaveAssets();
        VerifyTranslations();
        Debug.Log($"[Japanese Dialogue] Filled {changedFields} missing fields across {changedAssets} story assets without replacing existing Japanese text.");
    }

    [MenuItem("Tools/NTS/Verify Japanese Dialogue Coverage")]
    public static void VerifyTranslations()
    {
        var missing = new List<string>();
        var localizedFields = 0;

        foreach (var story in StoryAssets())
        {
            var storyMissing = 0;
            var storyTotal = 0;
            AuditStory(story, ref storyTotal, ref storyMissing);
            localizedFields += storyTotal;
            if (storyMissing > 0)
                missing.Add($"{story.name}: {storyMissing}/{storyTotal}");
        }

        if (missing.Count > 0)
            throw new InvalidOperationException("Japanese dialogue fields are still missing:\n" + string.Join("\n", missing));

        Debug.Log($"[Japanese Dialogue] Coverage passed: {localizedFields} English story fields all have Japanese text.");
    }

    private static List<DialogueContainerSO> StoryAssets()
    {
        var manager = Resources.FindObjectsOfTypeAll<DialogueChapterManager>()
            .FirstOrDefault(item => item.gameObject.scene.IsValid());
        if (manager == null)
            throw new InvalidOperationException("No active DialogueChapterManager was found. Open the main mobile scene first.");

        return manager.StoryList
            .Where(chapter => chapter.Story != null)
            .Select(chapter => chapter.Story)
            .Distinct()
            .ToList();
    }

    private static Dictionary<string, string> LoadTranslations()
    {
        if (!File.Exists(TranslationPath))
            throw new FileNotFoundException("Japanese dialogue translation map was not found.", TranslationPath);

        var translations = new Dictionary<string, string>();
        var lineNumber = 0;
        foreach (var rawLine in File.ReadLines(TranslationPath))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#", StringComparison.Ordinal))
                continue;

            var columns = rawLine.Split('\t');
            if (columns.Length != 2)
                throw new InvalidDataException($"Malformed Japanese translation row {lineNumber}.");

            string english;
            string japanese;
            try
            {
                english = Encoding.UTF8.GetString(Convert.FromBase64String(columns[0]));
                japanese = Encoding.UTF8.GetString(Convert.FromBase64String(columns[1]));
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"Invalid base64 in Japanese translation row {lineNumber}.", exception);
            }

            if (string.IsNullOrWhiteSpace(japanese))
                throw new InvalidDataException($"Empty Japanese translation on row {lineNumber}.");
            if (!translations.TryAdd(english, japanese))
                throw new InvalidDataException($"Duplicate English source text on row {lineNumber}: {english}");
        }

        return translations;
    }

    private static int ApplyToStory(DialogueContainerSO story, IReadOnlyDictionary<string, string> translations)
    {
        var changed = 0;
        foreach (var node in story.DialogueNodeDatas)
        {
            changed += Fill(node.Texts, translations);
            changed += Fill(node.Timelapses, translations);
        }

        foreach (var node in story.DialogueChoiceNodeDatas)
            changed += FillChoice(node, translations);
        foreach (var node in story.TimerChoiceNodeDatas)
            changed += FillChoice(node, translations);
        foreach (var node in story.RandomNodeDatas)
            foreach (var port in node.DialogueNodePorts)
                changed += FillPort(port, translations);

        return changed;
    }

    private static int FillChoice(DialogueChoiceNodeData node, IReadOnlyDictionary<string, string> translations)
    {
        var changed = Fill(node.TextType, translations) + Fill(node.SelectedChoice, translations);
        foreach (var port in node.DialogueNodePorts)
            changed += FillPort(port, translations);
        return changed;
    }

    private static int FillPort(DialogueNodePort port, IReadOnlyDictionary<string, string> translations) =>
        Fill(port.TextLanguage, translations) + Fill(port.HintLanguage, translations);

    private static int Fill(List<LanguageGeneric<string>> languages, IReadOnlyDictionary<string, string> translations)
    {
        if (languages == null)
            return 0;

        var englishItem = languages.FirstOrDefault(item => item.languageEnum == LocalizationEnum.English);
        var english = englishItem?.LanguageGenericType;
        if (string.IsNullOrWhiteSpace(english))
            return 0;

        var japaneseItem = languages.FirstOrDefault(item => item.languageEnum == LocalizationEnum.Japanese);
        if (japaneseItem != null && !string.IsNullOrWhiteSpace(japaneseItem.LanguageGenericType))
            return 0;

        if (!translations.TryGetValue(english, out var japanese))
            throw new KeyNotFoundException($"No Japanese translation exists for: {english}");

        if (japaneseItem == null)
        {
            languages.Add(new LanguageGeneric<string>
            {
                languageEnum = LocalizationEnum.Japanese,
                LanguageGenericType = japanese
            });
        }
        else
        {
            japaneseItem.LanguageGenericType = japanese;
        }

        return 1;
    }

    private static void AuditStory(DialogueContainerSO story, ref int total, ref int missing)
    {
        foreach (var node in story.DialogueNodeDatas)
        {
            Audit(node.Texts, ref total, ref missing);
            Audit(node.Timelapses, ref total, ref missing);
        }

        foreach (var node in story.DialogueChoiceNodeDatas)
            AuditChoice(node, ref total, ref missing);
        foreach (var node in story.TimerChoiceNodeDatas)
            AuditChoice(node, ref total, ref missing);
        foreach (var node in story.RandomNodeDatas)
            foreach (var port in node.DialogueNodePorts)
                AuditPort(port, ref total, ref missing);
    }

    private static void AuditChoice(DialogueChoiceNodeData node, ref int total, ref int missing)
    {
        Audit(node.TextType, ref total, ref missing);
        Audit(node.SelectedChoice, ref total, ref missing);
        foreach (var port in node.DialogueNodePorts)
            AuditPort(port, ref total, ref missing);
    }

    private static void AuditPort(DialogueNodePort port, ref int total, ref int missing)
    {
        Audit(port.TextLanguage, ref total, ref missing);
        Audit(port.HintLanguage, ref total, ref missing);
    }

    private static void Audit(List<LanguageGeneric<string>> languages, ref int total, ref int missing)
    {
        if (languages == null)
            return;
        var english = languages.FirstOrDefault(item => item.languageEnum == LocalizationEnum.English)?.LanguageGenericType;
        if (string.IsNullOrWhiteSpace(english))
            return;

        total++;
        var japanese = languages.FirstOrDefault(item => item.languageEnum == LocalizationEnum.Japanese)?.LanguageGenericType;
        if (string.IsNullOrWhiteSpace(japanese))
            missing++;
    }
}
#endif
