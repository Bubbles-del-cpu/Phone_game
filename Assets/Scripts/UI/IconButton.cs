using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconButton : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Button button;

    public bool Interactable
    {
        get { return button.interactable; }
        set
        {
            icon.color = value ? Color.white : new Color(1f, 1f, 1f, 0.33f);
            button.interactable = value;
        }
    }

    public Button Button { get { return button; } }
}
