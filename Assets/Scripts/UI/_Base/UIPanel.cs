using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    [SerializeField] bool isOpenOnStart = true;
    [Range(0f, 1f)]
    [SerializeField] float openAlpha = 1f;
    [SerializeField] Button closeButton;

    CanvasGroup canvas;
    RectTransform rect;

    public virtual void Awake()
    {
        canvas = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        if (closeButton != null)
            closeButton.onClick.AddListener(() => Close());
    }

    public virtual void Start()
    {
        if (IsOpen != isOpenOnStart) Toggle();
    }

    public virtual void Open()
    {
        canvas.alpha = openAlpha;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    public virtual void Close()
    {
        canvas.alpha = 0f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public virtual void Toggle()
    {
        if (!IsOpen) Open(); else Close();
    }

    public bool IsOpen { get { return canvas.alpha >= openAlpha; } }

    public RectTransform Rect { get { return rect; } }
}
