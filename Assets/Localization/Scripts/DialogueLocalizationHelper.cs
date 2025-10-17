using System.Collections.Generic;

namespace MeetAndTalk
{
    public static class DialogueLocalizationHelper
    {
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