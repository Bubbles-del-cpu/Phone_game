using System.Collections.Generic;
using MeetAndTalk;
using UnityEngine;

/// <summary>
/// Manages the notification canvas for displaying notifications.
/// </summary>
public class NotificationCanvas : UICanvas
{
    private static NotificationCanvas _instance;
    public static NotificationCanvas Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<NotificationCanvas>();

            return _instance;
        }
    }

    [Header("Notification Settings")]
    [SerializeField, Tooltip("Batches notifications from a single character into a single notification.")]
    private bool _batchNotifications = false;

    [SerializeField, Tooltip("Batched notifications will have their text updated to show the latest message but a new notification is not created.")]
    private bool _batchReplaceText = false;

    [SerializeField, Tooltip("Limits the number of notifications that can be displayed on the screen to 1")]
    private bool _singleNotificationOnly = false;

    private Dictionary<DialogueCharacterSO, Notification> _notificationDictionary = new();
    public Dictionary<DialogueCharacterSO, Notification> NotificationDictionary => _notificationDictionary;

    private DialogueCharacterSO _lastNotificationCharacter;
    private float _lastNotificationTime;

    [Header("Components")]
    [SerializeField] RectTransform notificationsContainer;

    public void SpawnNotification(Notification.NotificationType type, DialogueCharacterSO character, string label)
    {
        switch (type)
        {
            default:
                GameManager.Instance.PlayReceiveTextFX();
                if (GameManager.Instance.MessagingCanvas.GetConversationPanel(character).IsOpen)
                    return;
                break;
            case Notification.NotificationType.SocialMedia:
                GameManager.Instance.PlayNotificationFX();
                if (GameManager.Instance.SocialMediaCanvas.IsOpen)
                    return;
                break;
        }

        //Remove the last notifcation if it is still there
        if (_singleNotificationOnly)
        {
            var lastNotifcation = FindFirstObjectByType<Notification>();
            if (lastNotifcation)
            {
                _notificationDictionary.Remove(lastNotifcation.Character);
                DialogueUIManagerObjectPool.Instance.ReturnNotification(lastNotifcation);
            }
        }

        if (_batchNotifications)
        {
            if (_notificationDictionary.ContainsKey(character))
            {
                var characterNotification = _notificationDictionary[character];
                if (characterNotification != null && characterNotification.transform.parent == notificationsContainer)
                {
                    if (_batchReplaceText)
                    {
                        characterNotification.Setup(type, character, label);
                        return;
                    }
                }
                else
                {
                    _notificationDictionary.Remove(character);
                }
            }
        }

        var notification = DialogueUIManagerObjectPool.Instance.GetNotification();
        if (notification != null)
        {
            notification.transform.SetParent(notificationsContainer, false);
            notification.Setup(type, character, label);

            if (!_notificationDictionary.ContainsKey(character))
                _notificationDictionary.Add(character, null);

            _notificationDictionary[character] = notification;
        }
    }
}