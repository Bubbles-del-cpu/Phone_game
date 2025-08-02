using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MeetAndTalk;
using System.Collections;

public class UICanvas : MonoBehaviour
{
    [SerializeField] bool isOpenOnStart = true;
    [SerializeField] Button closeButton;

    Canvas canvas;

    protected virtual void Awake()
    {
        canvas = GetComponent<Canvas>();

        if (closeButton != null)
            closeButton.onClick.AddListener(() => Close());
    }

    protected virtual void Start()
    {
        if (IsOpen != isOpenOnStart) Toggle();
    }

    public virtual void Open()
    {
        if (canvas != null)
            canvas.enabled = true;
    }

    public virtual void Close()
    {
        if (canvas != null)
            canvas.enabled = false;
    }

    private IEnumerator CoSetOpenState(float delay, bool state)
    {
        yield return new WaitForSeconds(delay);
        if (canvas != null)
            canvas.enabled = state;
    }

    public virtual void Open(float delay = 0)
    {
        StartCoroutine(CoSetOpenState(delay, true));
    }

    public virtual void Close(float delay = 0)
    {
        if (canvas != null)
        {
            StartCoroutine(CoSetOpenState(delay, false));
        }
    }

    public virtual void Toggle()
    {
        if (!IsOpen) Open(); else Close();
    }

    public bool IsOpen { get { return canvas.enabled; } }

    public Canvas Canvas { get { return canvas; } }
}
