using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

public class HintSettingsToggle : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Sprite _enableSprite;
    [SerializeField] private Sprite _disableSprite;

    private void Awake()
    {
        if (_button)
        {
            _button.onClick.AddListener(() =>
            {
                DialogueUIManager.Instance.DisplayHints = !DialogueUIManager.Instance.DisplayHints;
                _button.image.sprite = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
            });
        }
    }

    private void Start()
    {
        _button.image.sprite = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
    }

    private void Update()
    {
        _button.image.sprite = DialogueUIManager.Instance.DisplayHints ? _enableSprite : _disableSprite;
    }
}
