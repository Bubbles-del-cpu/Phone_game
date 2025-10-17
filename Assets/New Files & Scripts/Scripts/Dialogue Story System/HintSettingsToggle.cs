using MeetAndTalk;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class HintSettingsToggle : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private LocalizeSpriteEvent _localizedSpriteEvent;
    [SerializeField] private LocalizedSprite _enableSprite;
    [SerializeField] private LocalizedSprite _disableSprite;

    private void Awake()
    {
        if (_button)
        {
            _button.onClick.AddListener(() =>
            {
                DialogueUIManager.Instance.DisplayHints = !DialogueUIManager.Instance.DisplayHints;
                _localizedSpriteEvent.AssetReference = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
            });
        }
    }

    private void Start()
    {
        _localizedSpriteEvent.AssetReference = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
    }

    private void Update()
    {
        _localizedSpriteEvent.AssetReference = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
    }
}
