using System.Collections.Generic;
using System.Text.RegularExpressions;
using MeetAndTalk.GlobalValue;
using UnityEngine;

namespace MeetAndTalk
{
    public static class DialogueLocalizationHelper
    {
        public static Regex REGEX = new Regex(@"\{(.*?)\}");
        private static string ChangeReplaceableText(string text)
        {
            GlobalValueManager manager = SaveAndLoadManager.Instance.ValueManager;

            string TextToReplace = "[Error Value]";
            /* Global Value */
            for (int i = 0; i < manager.IntValues.Count; i++) { if (text == manager.IntValues[i].ValueName) TextToReplace = manager.IntValues[i].Value.ToString(); }
            for (int i = 0; i < manager.FloatValues.Count; i++) { if (text == manager.FloatValues[i].ValueName) TextToReplace = manager.FloatValues[i].Value.ToString(); }
            for (int i = 0; i < manager.BoolValues.Count; i++) { if (text == manager.BoolValues[i].ValueName) TextToReplace = manager.BoolValues[i].Value.ToString(); }
            for (int i = 0; i < manager.StringValues.Count; i++) { if (text == manager.StringValues[i].ValueName) TextToReplace = manager.StringValues[i].Value; }

            //
            if(text.Contains(","))
            {
                string[] tmp = text.Split(',');
                for (int i = 0; i < manager.IntValues.Count; i++) { if (tmp[0] == manager.IntValues[i].ValueName) TextToReplace = Mathf.Abs(manager.IntValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
                for (int i = 0; i < manager.FloatValues.Count; i++) { if (tmp[0] == manager.FloatValues[i].ValueName) TextToReplace = Mathf.Abs(manager.FloatValues[i].Value - (int)System.Convert.ChangeType(tmp[1], typeof(int))).ToString(); }
            }

            return TextToReplace;
        }

        public static string GetConvertedText(string text)
        {
            var newText = GameManager.ToUTF32(text);
            MatchEvaluator matchEvaluator = new MatchEvaluator(match =>
            {
                string OldText = match.Groups[1].Value;
                return ChangeReplaceableText(OldText);
            });

            return REGEX.Replace(newText, matchEvaluator);
        }

        public static string GetText(List<LanguageGeneric<string>> texts)
        {
            if (texts.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()) == null)
                return "";

            return texts.Find(x => x.languageEnum == GameManager.LOCALIZATION_MANAGER.SelectedLang()).LanguageGenericType;
        }

        public static string GetCharacterName(DialogueCharacterSO character)
        {
            if (character == null)
                return "";

            return GetText(character.characterName);
        }
    }
}