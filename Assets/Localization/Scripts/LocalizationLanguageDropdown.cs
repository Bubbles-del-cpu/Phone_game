using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

/// <summary>
/// A dropdown for selecting the localization language.
/// </summary>
[AddComponentMenu("Localization Language Dropdown")]
public class LocalizationLanguageDropdown : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private bool _reloadGameOnChange;

    private List<Locale> _locales;
    private int _previousLanguageIndex = -1;

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private IEnumerator Start()
    {
        _dropdown.interactable = false;
        ShowDropDown(GameManager.Instance.EnableLanaguageSwitching);

        // Locale assets may still be loading when this component becomes active.
        yield return LocalizationSettings.InitializationOperation;
        yield return null; // Let SaveAndLoadManager.Start load the saved preference.
        while (LocalizationSettings.AvailableLocales.Locales.Count == 0)
            yield return null;

        _locales = new List<Locale>(LocalizationSettings.AvailableLocales.Locales);
        _dropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>();
        foreach (var locale in _locales)
            options.Add(new TMP_Dropdown.OptionData(locale.LocaleName.Split(' ')[0]));
        _dropdown.AddOptions(options);

        OnLocaleChanged(LocalizationSettings.SelectedLocale);
        if (_previousLanguageIndex < 0 && _locales.Count > 0)
        {
            _previousLanguageIndex = 0;
            _dropdown.SetValueWithoutNotify(0);
        }

        _dropdown.onValueChanged.AddListener(OnLanguageSelected);
        _dropdown.interactable = _locales.Count > 0;
    }

    private void OnDestroy()
    {
        if (_dropdown != null)
            _dropdown.onValueChanged.RemoveListener(OnLanguageSelected);
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (_locales == null || locale == null)
            return;

        var index = _locales.IndexOf(locale);
        if (index < 0)
            return;

        _previousLanguageIndex = index;
        _dropdown.SetValueWithoutNotify(index);
    }

    private void OnLanguageSelected(int index)
    {
        if (_locales == null || index < 0 || index >= _locales.Count)
            return;

        // Keep the saved choice displayed while the restart confirmation is open.
        _dropdown.SetValueWithoutNotify(_previousLanguageIndex);
        if (index == _previousLanguageIndex)
            return;

        var newLanguage = _locales[index];
        if (_reloadGameOnChange)
        {
            GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.WARNING_LANGUAGE_CHANGE, () =>
            {
                OverlayCanvas.Instance.FadeToBlack(() => ApplyLanguage(newLanguage, index, true));
            }, GameConstants.UIElementKeys.YES, new object[] { newLanguage.LocaleName });
        }
        else
        {
            ApplyLanguage(newLanguage, index, false);
        }
    }

    private void ApplyLanguage(Locale locale, int index, bool resetGame)
    {
        GameManager.Instance.ChangeLanguage(locale);
        SaveAndLoadManager.SaveToJson(SaveAndLoadManager.Instance.CurrentSave, SaveAndLoadManager.Instance.CurrentSaveSlot);
        if (resetGame)
            GameManager.Instance.HardResetGameState();

        _previousLanguageIndex = index;
        _dropdown.SetValueWithoutNotify(index);
    }

    private void ShowDropDown(bool show)
    {
        _canvasGroup.alpha = show ? 1 : 0;
        _canvasGroup.interactable = show;
        _canvasGroup.blocksRaycasts = show;
    }
}
