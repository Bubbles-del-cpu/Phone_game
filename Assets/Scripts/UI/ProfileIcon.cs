using UnityEngine;
using UnityEngine.UI;
using MeetAndTalk;

public class ProfileIcon : MonoBehaviour
{
    [SerializeField] Image icon;

    public DialogueCharacterSO Character
    {
        set
        {
            if (value != null)
            {
                icon.sprite = value.GetAvatar(AvatarPosition.Left, AvatarType.Normal);
            }
        }
    }
}
