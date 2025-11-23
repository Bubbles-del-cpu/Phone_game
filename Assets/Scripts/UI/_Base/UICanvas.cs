using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class UICanvas : MonoBehaviour
{
    //[SerializeField] bool isOpenOnStart = true;
    //[SerializeField] Button closeButton;

    protected Canvas _canvas;

    protected virtual void Awake()
    {
        _canvas = GetComponent<Canvas>();

        // if (closeButton != null)
        //     closeButton.onClick.AddListener(() => Close());
    }

    protected virtual void Start()
    {
        //if (IsOpen != isOpenOnStart) Toggle();
    }

    public virtual void Open()
    {
        if (_canvas != null)
        {
            var command = new PanelOpenCommand(this, openState: true);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: true);
        }
    }

    public virtual void Close()
    {
        if (_canvas != null)
        {
            var command = new PanelOpenCommand(this, openState: false);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: false);
        }
    }

    private IEnumerator CoSetOpenState(float delay, bool state)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (_canvas != null)
        {
            var command = new PanelOpenCommand(this, state);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: state);
        }
    }

    public virtual void Open(float delay = 0)
    {
        StartCoroutine(CoSetOpenState(delay, true));
    }

    public virtual void Close(float delay = 0)
    {
        if (_canvas != null)
        {
            StartCoroutine(CoSetOpenState(delay, false));
        }
    }

    // public virtual void Toggle()
    // {
    //     if (!IsOpen) Open(); else Close();
    // }

    public bool IsOpen { get { return _canvas.enabled; } }

    public Canvas Canvas { get { return _canvas; } }
}
