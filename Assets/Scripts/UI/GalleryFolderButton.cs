using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;

public class GalleryFolderButton : MonoBehaviour
{
    [SerializeField] TMP_Text nameLabel;

    public DialogueCharacterSO Character
    {
        set
        {
            nameLabel.text = value.name;
        }
    }
}
