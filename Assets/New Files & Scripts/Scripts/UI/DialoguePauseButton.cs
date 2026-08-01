using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A button that toggles the paused state of the dialogue manager when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class DialoguePauseButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Sprite _pauseSprite;
    [SerializeField] private Sprite _playSprite;
    [SerializeField] private Image _buttonImage;


    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_buttonImage == null)
            _buttonImage = _button.GetComponentInChildren<Image>();
    }

    private void Update()
    {
        if (!_buttonImage)
            return;

        if (DialogueManager.Instance.Paused)
        {
            _buttonImage.sprite = _playSprite;
        }
        else
        {
            _buttonImage.sprite = _pauseSprite;
        }
    }

    private void OnEnable()
    {
        // Ensure we don't add multiple listeners if the button is enabled multiple times.
        _button.onClick.RemoveListener(OnButtonClicked);
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        DialogueManager.Instance.Paused = !DialogueManager.Instance.Paused;
    }
}