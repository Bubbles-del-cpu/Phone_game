using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResponseSpeedButton : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    [SerializeField]
    private TMP_Text _label;

    private void Awake()
    {
        _button.onClick.AddListener(() =>
        {
            var value = (int)MeetAndTalk.DialogueManager.Instance.DisplaySpeedMultipler;
            MeetAndTalk.DialogueManager.Instance.DisplaySpeedMultipler = (MeetAndTalk.DialogueManager.ResponseSpeed)(value << 1);

            if (MeetAndTalk.DialogueManager.Instance.DisplaySpeedMultipler > MeetAndTalk.DialogueManager.ResponseSpeed.X4)
                MeetAndTalk.DialogueManager.Instance.DisplaySpeedMultipler = MeetAndTalk.DialogueManager.ResponseSpeed.X1;
        });
    }
    private void Update()
    {
        if (_label)
            _label.text = $"x{(int)MeetAndTalk.DialogueManager.Instance.DisplaySpeedMultipler}";
    }
}
