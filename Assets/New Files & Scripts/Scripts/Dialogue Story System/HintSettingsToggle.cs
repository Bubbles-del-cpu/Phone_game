using MeetAndTalk;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HintSettingsToggle : MonoBehaviour
{
    Toggle _toggle;
    bool _updated;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        if (_toggle)
        {
            _toggle.onValueChanged.AddListener((value) =>
            {
                DialogueUIManager.Instance.DisplayHints = value;
            });
        }
    }

    private void Start()
    {
        gameObject.SetActive(_toggle);
    }

    private void Update()
    {
        //First update frame needs to read the DialogueUIManager and set the toggle
        //Saves us having some public reference on a script - Maybe slightly hacky
        if (_toggle && !_updated)
        {
            _updated = true;
            _toggle.SetIsOnWithoutNotify(DialogueUIManager.Instance.DisplayHints);
        }
    }
}
