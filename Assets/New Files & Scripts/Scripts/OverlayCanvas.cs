using UnityEngine.UI;
using UnityEngine;
using System;

public class OverlayCanvas : UICanvas
{
    private static OverlayCanvas _instance;
    public static OverlayCanvas Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<OverlayCanvas>();

            return _instance;
        }
    }

    [SerializeField]
    private DialogWarningMessage _dialogPrefab;

    [Header("Transition Fade Settings")]
    [SerializeField] private float _timeToFade;
    [SerializeField] private float _fadeHoldTime;
    [SerializeField] private bool _fade;
    [SerializeField] private AnimationCurve _fadeInCurve;
    [SerializeField] private AnimationCurve _fadeOutCurve;
    [SerializeField] private Image _fadeOverlay;

    private float _fadeTimer;
    private float _fadeTime, _holdTime;
    private FadeState _fadeState;
    private Action actionOnFade;
    public enum FadeState
    {
        NONE,
        FADE_OUT,
        FADE_IN,
        HOLD
    }

    public override void Open()
    {
        if (_canvas != null)
        {
            var command = new PanelOpenCommand(this, openState: true);
            NavigationManager.Instance.InvokeCommand(command, allowUndo: false);
        }
    }

    public void ShowDialog(string message, System.Action eventToTrigger, string confirmButtonText, bool twoButtonSetup, string cancelButtonTest)
    {
        Open();

        var dialog = Instantiate(_dialogPrefab, transform.GetChild(0));
        dialog.Setup(message, eventToTrigger, confirmButtonText, twoButtonSetup, cancelButtonTest);
    }

    public void ShowDialog(GameObject newDialogue)
    {
        Open();
        newDialogue.transform.parent = transform.GetChild(0);
        newDialogue.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        newDialogue.GetComponent<RectTransform>().localPosition = Vector3.one;
        newDialogue.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (_fade)
        {
            var percentage = 0f;
            switch (_fadeState)
            {
                case FadeState.FADE_OUT:
                    _fadeTimer += Time.deltaTime;
                    percentage = _fadeOutCurve.Evaluate(_fadeTimer / _fadeTime);
                    if (_fadeTimer >= _fadeTime)
                    {
                        _fadeTimer = 0;
                        _fadeState = FadeState.HOLD;

                        //Reset stuff here
                        actionOnFade?.Invoke();
                    }
                    break;
                case FadeState.HOLD:
                    _fadeTimer += Time.deltaTime;
                    percentage = 1;
                    if (_fadeTimer >= _holdTime)
                    {
                        _fadeTimer = 0;
                        _fadeState = FadeState.FADE_IN;
                    }
                    break;
                case FadeState.FADE_IN:
                    _fadeTimer += Time.deltaTime;
                    percentage = _fadeInCurve.Evaluate(_fadeTimer / _fadeTime);
                    if (_fadeTimer >= _fadeTime)
                    {
                        _fadeTimer = 0;
                        _fadeState = FadeState.NONE;
                        _fade = false;
                    }
                    break;
            }

            _fadeOverlay.color = new Color(_fadeOverlay.color.r, _fadeOverlay.color.g, _fadeOverlay.color.b, percentage);
            _fadeOverlay.enabled = true;
        }
        else
        {
            _fadeOverlay.enabled = false;
        }
    }

    public void FadeToBlack(Action postEvent, float timeToFadeOverride = -1, float fadeHoldTimeOverride = -1)
    {
        //TODO: Disable inputs
        Open();
        _fadeState = FadeState.FADE_OUT;
        _fade = true;

        _fadeTime = timeToFadeOverride == -1 ?  _timeToFade : timeToFadeOverride;
        _holdTime = fadeHoldTimeOverride == -1 ? _fadeHoldTime : fadeHoldTimeOverride;

        actionOnFade = postEvent;
    }
}
