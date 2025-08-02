using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class ContactsCanvas : UICanvas
{
    [Space(10f)]
    [Header("Contacts")]
    [SerializeField] ContactButton contactsButtonPrefab;
    [SerializeField] RectTransform contactsButtonsContainer;
    [SerializeField] UIPanel contactPanel;
    [SerializeField] ProfileIcon contactProfileIcon;
    [SerializeField] TMP_Text contactNameLabel;
    [SerializeField] Button contactMessagingButton;
    [SerializeField] Button contactGalleryButton;

    DialogueCharacterSO currentCharacter;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        foreach (DialogueCharacterSO _character in GameManager.Instance.Characters)
        {
            ContactButton _button = Instantiate(contactsButtonPrefab, contactsButtonsContainer);
            _button.Character = _character;
            _button.GetComponent<Button>().onClick.AddListener(() => OpenContact(_character));
        }

        contactMessagingButton.onClick.AddListener(() => GameManager.Instance.MessagingCanvas.Open(currentCharacter));
        contactGalleryButton.onClick.AddListener(() => GameManager.Instance.GalleryCanvas.Open());
    }

    public override void Close()
    {
        if (contactPanel.IsOpen)
            contactPanel.Close();
        else
            base.Close();
    }

    void OpenContact(DialogueCharacterSO _character)
    {
        contactProfileIcon.Character = _character;
        contactNameLabel.text = _character.name;
        contactPanel.Open();
        currentCharacter = _character;
    }
}
