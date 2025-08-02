using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class ContactButton : MonoBehaviour
{
    [SerializeField] ProfileIcon icon;
    [SerializeField] TMP_Text nameLabel;

    public DialogueCharacterSO Character
    {
        set
        {
            icon.Character = value;
            nameLabel.text = value.name;
        }
    }
}
