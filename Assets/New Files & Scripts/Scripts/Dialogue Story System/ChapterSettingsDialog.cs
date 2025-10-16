using UnityEngine;
using UnityEngine.UI;
using static DialogueChapterManager.ChapterData;

public class ChapterSettingsDialog : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _textLabel;
    [SerializeField]  private Button _option1;
    [SerializeField] private Button _option2;
    [SerializeField] private TMPro.TMP_Text _option1Label;
    [SerializeField] private TMPro.TMP_Text _option2Label;

    public void Setup(ChapterReplaySetting setting, System.Action action)
    {
        _textLabel.text = setting.DialogueTitleString.GetLocalizedString();
        _option1Label.text = setting.Option1TextString.GetLocalizedString();
        _option2Label.text = setting.Option2TextString.GetLocalizedString();
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

    private void OnClick()
    {
        Destroy(gameObject);
    }
}
