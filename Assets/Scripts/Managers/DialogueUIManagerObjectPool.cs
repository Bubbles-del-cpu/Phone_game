using UnityEngine;
using static MeetAndTalk.DialogueUIManager;

public class DialogueUIManagerObjectPool : MonoBehaviour
{
    private static DialogueUIManagerObjectPool _instance;
    public static DialogueUIManagerObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueUIManagerObjectPool>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("DialogueUIManagerObjectPool");
                    _instance = obj.AddComponent<DialogueUIManagerObjectPool>();
                }
            }

            return _instance;
        }
    }
    [Header("Message Bubble Pool")]
    [SerializeField] private int _messageBubblePoolSize = 50;
    [SerializeField] private int _messageBubblePoolIncreaseSize = 10;
    [SerializeField] private GameObject _leftMessageBubblePrefab;
    [SerializeField] private GameObject _rightMessageBubblePrefab;
    [SerializeField] private Transform _leftMessageBubblePoolContainer;
    [SerializeField] private Transform _rightMessageBubblePoolContainer;

    [Header("Notifcation Pool")]
    [SerializeField] private int _notificationPoolSize = 20;
    [SerializeField] private GameObject _notificationPrefab;
    [SerializeField] private Transform _notificationPoolContainer;

    // Pools
    private ObjectPool<MessagingBubble> _leftMessageBubblePool;
    private ObjectPool<MessagingBubble> _rightMessageBubblePool;
    private ObjectPool<Notification> _notificationPool;

    private void Awake()
    {
        // Create the object pools
        _leftMessageBubblePool = new ObjectPool<MessagingBubble>(
            poolMax: _messageBubblePoolSize,
            increaseSize: _messageBubblePoolIncreaseSize,
            prefab: _leftMessageBubblePrefab,
            poolContainer: _leftMessageBubblePoolContainer
        );

        _rightMessageBubblePool = new ObjectPool<MessagingBubble>(
            poolMax: _messageBubblePoolSize,
            increaseSize: _messageBubblePoolIncreaseSize,
            prefab: _rightMessageBubblePrefab,
            poolContainer: _rightMessageBubblePoolContainer
        );

        _notificationPool = new ObjectPool<Notification>(
            poolMax: _notificationPoolSize,
            increaseSize: 1,
            prefab: _notificationPrefab,
            poolContainer: _notificationPoolContainer
        );
    }

    /// <summary>
    /// Get a message bubble from the pool based on the side (0 = left, 1 = right)
    /// </summary>
    /// <param name="side"></param>
    /// <returns></returns>
    public MessagingBubble GetMessageBubble(MessageSource side)
    {
        if (side == MessageSource.Character)
            return _leftMessageBubblePool.GetObject();
        else
            return _rightMessageBubblePool.GetObject();
    }

    /// <summary>
    /// Return a message bubble to the pool based on the side (0 = left, 1 = right)
    /// </summary>
    /// <param name="bubble"></param>
    /// <param name="side"></param>
    public void ReturnMessageBubble(MessagingBubble bubble, MessageSource side)
    {
        bubble.Clear();
        if (side == MessageSource.Character)
            _leftMessageBubblePool.ReturnObject(bubble);
        else
            _rightMessageBubblePool.ReturnObject(bubble);
    }

    /// <summary>
    /// Get a notification from the pool
    /// </summary>
    /// <returns></returns>
    public Notification GetNotification()
    {
        return _notificationPool.GetObject();
    }

    /// <summary>
    /// Return a notification to the pool
    /// </summary>
    /// <param name="notification"></param>
    public void ReturnNotification(Notification notification)
    {
        _notificationPool.ReturnObject(notification);
    }
}