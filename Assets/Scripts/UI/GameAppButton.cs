using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameAppButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Button _button;
    [SerializeField] private Image _notificationIcon;
    [SerializeField] private Image _responseNotificationIcon;

    private Dictionary<DialogueCharacterSO, int> _notificationDictionary = new Dictionary<DialogueCharacterSO, int>();
    private Dictionary<DialogueCharacterSO, int> _responseNotificationDictionary = new Dictionary<DialogueCharacterSO, int>();

    public Button.ButtonClickedEvent OnClick => _button.onClick;
    public bool Interactable
    {
        get => _button.interactable;
        set
        {
            var before = _button.interactable;
            _button.interactable = value;

            // Update animator state if interactability changed
            if (before != value && _animator)
            {
                if (!_button.interactable)
                {
                    _animator.SetTrigger("Disabled");
                    _animator.ResetTrigger("Normal");
                    _animator.ResetTrigger("Highlighted");

                    HasNotification = false;
                    HasResponseNotification = false;
                    _notificationCount = 0;
                }
                else
                {
                    _animator.ResetTrigger("Disabled");
                    _animator.SetTrigger("Normal");
                }
            }
        }
    }

    public void AddNotification(DialogueCharacterSO character)
    {
        if (_notificationDictionary.ContainsKey(character))
            _notificationDictionary[character]++;
        else
            _notificationDictionary[character] = 1;

        HasNotification = true;
        _notificationCount++;
    }

    public void AddResponseNotification(DialogueCharacterSO character)
    {
        if (_responseNotificationDictionary.ContainsKey(character))
            _responseNotificationDictionary[character]++;
        else
            _responseNotificationDictionary[character] = 1;

        HasResponseNotification = true;
        _responseNotificationCount++;
    }

    public void RemoveNotifications(DialogueCharacterSO character)
    {
        // NOTE: This is temporary logic until we have a better notification system in place for the social app
        //       This will only apply to the social media app notifications. Messaging app notifications will be handled as normal
        if (character == null)
        {
            // Remove all notifications
            _notificationCount = 0;
            _notificationDictionary.Clear();
            HasNotification = false;
            return;
        }

        // When a removing notifications for a character, remove both standard and response notifications. Remove them all at once.
        // This is because multiple messages and therefore multiple notifications could have piled up while the user wasn't checking their messages.
        if (_notificationDictionary.ContainsKey(character))
        {
            _notificationCount = Mathf.Clamp(_notificationCount - _notificationDictionary[character], 0, int.MaxValue);
            _notificationDictionary[character] = 0;
        }
        // Update response notification state
        // With will naturally prioritize newer messages with a notification over older response notifications
        if (_notificationCount == 0 && _responseNotificationCount == 0)
        {
            HasResponseNotification = false;
            HasNotification = false;
        }

        HasNotification = _notificationCount > 0;
    }

    public void RemoveResponseNotification(DialogueCharacterSO character)
    {
        if (_responseNotificationDictionary.ContainsKey(character))
        {
            _responseNotificationCount = Mathf.Clamp(_responseNotificationCount - _responseNotificationDictionary[character], 0, int.MaxValue);
            _responseNotificationDictionary[character] = 0;
        }

        // Update response notification state
        HasResponseNotification = _responseNotificationCount > 0;
    }

    public void ClearAllNotifications()
    {
        _notificationDictionary.Clear();
        _responseNotificationDictionary.Clear();
        _notificationCount = 0;
        _responseNotificationCount = 0;
        HasNotification = false;
        HasResponseNotification = false;
    }

    /// <summary>
    /// Indicates whether there is a notification for this app
    /// </summary>
    private bool HasNotification
    {
        get => _notificationIcon.gameObject.activeInHierarchy;
        set => _notificationIcon.gameObject.SetActive(value);
    }

    /// <summary>
    /// Indicates whether there is a response notification for this app
    /// </summary>
    private bool HasResponseNotification
    {
        get => _responseNotificationIcon.gameObject.activeInHierarchy;
        set => _responseNotificationIcon.gameObject.SetActive(value);
    }

    [SerializeField] private int _notificationCount;
    [SerializeField] private int _responseNotificationCount;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable)
            return;

        if (_animator)
        {
            _animator.SetTrigger("Highlighted");
            _animator.ResetTrigger("Normal");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable)
            return;

        if (_animator)
        {
            _animator.SetTrigger("Normal");
            _animator.ResetTrigger("Highlighted");
        }
    }
}
