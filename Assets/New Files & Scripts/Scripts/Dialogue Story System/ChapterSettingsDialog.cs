using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using static DialogueChapterManager.ChapterData;

public class ChapterSettingsDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text _textLabel;
    [SerializeField] private Button _option1;
    [SerializeField] private Button _option2;
    [SerializeField] private TMP_Text _option1Label;
    [SerializeField] private TMP_Text _option2Label;

    public void Setup(ChapterReplaySetting setting, System.Action action)
    {
        // Keep meaningful labels visible while the localized strings resolve.
        _option1Label.text = FallbackLabel(setting.Option1TextString, _option1Label.text);
        _option2Label.text = FallbackLabel(setting.Option2TextString, _option2Label.text);
        StartCoroutine(ApplyLocalizedLabels(setting));

        _option1.onClick.AddListener(() =>
        {
            SaveAndLoadManager.Instance.ValueManager.Set(setting.Value.ValueName, setting.OptionSetting1);
            action?.Invoke();
            OnClick();
        });

        _option2.onClick.AddListener(() =>
        {
            SaveAndLoadManager.Instance.ValueManager.Set(setting.Value.ValueName, setting.OptionSetting2);
            action?.Invoke();
            OnClick();
        });
    }

    private IEnumerator ApplyLocalizedLabels(ChapterReplaySetting setting)
    {
        var initialization = LocalizationSettings.InitializationOperation;
        while (!initialization.IsDone)
            yield return null;

        SetLocalizedTextIfAvailable(_textLabel, setting.DialogueTitleString);
        SetLocalizedTextIfAvailable(_option1Label, setting.Option1TextString);
        SetLocalizedTextIfAvailable(_option2Label, setting.Option2TextString);
    }

    private static void SetLocalizedTextIfAvailable(TMP_Text label, LocalizedString localized)
    {
        if (localized == null)
            return;

        var text = localized.GetLocalizedString();
        if (!string.IsNullOrWhiteSpace(text))
            label.text = text;
    }

    private static string FallbackLabel(LocalizedString localized, string existingLabel)
    {
        if (localized == null)
            return existingLabel;

        switch (localized.TableEntryReference.KeyId)
        {
            case 52484340674560: return "DOM";
            case 52539290251264: return "SUB";
            case 52649986322432: return "YES";
            case 52698598305792: return "NO";
            case 9340416000: return "YES";
            case 235388235776: return "NO";
            case 10148971538341888: return "NTS";
            case 10149008259473408: return "NTR";
            default: return existingLabel;
        }
    }

    private void OnClick()
    {
        Destroy(gameObject);
    }
}
