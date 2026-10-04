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

    private bool? _displayedHints;
    private bool _listenerRegistered;

    private void OnEnable()
    {
        EnsureListener();
    }

    private void OnDisable()
    {
        if (_listenerRegistered && _button != null)
            _button.onClick.RemoveListener(ToggleHints);
        _listenerRegistered = false;
    }

    private void EnsureListener()
    {
        if (_listenerRegistered || _button == null)
            return;

        _button.onClick.AddListener(ToggleHints);
        _listenerRegistered = true;
    }

    private void ToggleHints()
    {
        var saveManager = SaveAndLoadManager.Instance;
        if (saveManager == null || saveManager.CurrentSave == null || DialogueUIManager.Instance == null)
            return;

        DialogueUIManager.Instance.DisplayHints = !DialogueUIManager.Instance.DisplayHints;
        RefreshVisual();
        SaveAndLoadManager.SaveToJson(saveManager.CurrentSave, saveManager.CurrentSaveSlot);
    }

    private void Update()
    {
        EnsureListener();
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (_localizedSpriteEvent == null || DialogueUIManager.Instance == null)
            return;

        var showHints = DialogueUIManager.Instance.DisplayHints;
        if (_displayedHints == showHints)
            return;

        _displayedHints = showHints;
        _localizedSpriteEvent.AssetReference = showHints ? _enableSprite : _disableSprite;
    }
}
