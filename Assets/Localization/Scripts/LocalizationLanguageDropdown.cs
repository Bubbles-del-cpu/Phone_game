using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;


/// <summary>
/// A dropdown for selecting the localization language.
/// </summary>
[AddComponentMenu("Localization Language Dropdown")]
public class LocalizationLanguageDropdown : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMPro.TMP_Dropdown _dropdown;
    [SerializeField] private bool _reloadGameOnChange;

    private int _previousLanguageIndex = -1;

    void Awake()
    {
        _dropdown.ClearOptions();
        _previousLanguageIndex = 0;
        _dropdown.onValueChanged.AddListener((index) =>
        {
            _dropdown.SetValueWithoutNotify(_previousLanguageIndex);

            var langauges = LocalizationSettings.AvailableLocales.Locales;
            var newLanguage = langauges[index];
            if (_reloadGameOnChange)
            {
                GameManager.Instance.DisplayDialog(GameConstants.DialogTextKeys.WARNING_LANGUAGE_CHANGE, () =>
                {
                    OverlayCanvas.Instance.FadeToBlack(() =>
                    {
                        GameManager.Instance.ChangeLanguage(newLanguage);
                        GameManager.Instance.ResetGameState();

                        _previousLanguageIndex = index;
                        _dropdown.SetValueWithoutNotify(index);
                    });

                }, GameConstants.UIElementKeys.YES, new object[] { langauges[index].LocaleName });
            }
            else
            {
                GameManager.Instance.ChangeLanguage(newLanguage);
                _previousLanguageIndex = index;
                _dropdown.SetValueWithoutNotify(index);
            }
        });

        foreach (var item in LocalizationSettings.AvailableLocales.Locales)
        {
            _dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(item.LocaleName.Split(" ")[0].ToString()));
        }
    }

    void Start()
    {
        //Delay the start to allow time for the save file to be loaded and the current language to be read
        StartCoroutine(CoStart());
        ShowDropDown(GameManager.Instance.EnableLanaguageSwitching);
    }

    private IEnumerator CoStart()
    {
        yield return new WaitForSeconds(0.1f);
        var index = LocalizationSettings.AvailableLocales.Locales.FindIndex(x => x.LocaleName.StartsWith(SaveAndLoadManager.Instance.CurrentSave.CurrentLanguage.ToString()));
        if (index != -1)
            _dropdown.SetValueWithoutNotify(index);
    }

    private void ShowDropDown(bool show)
    {
        _canvasGroup.alpha = show ? 1 : 0;
        _canvasGroup.interactable = show;
        _canvasGroup.blocksRaycasts = show;
    }
}
