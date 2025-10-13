using System.Collections;
using TMPro;
using UnityEngine;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvas;

    [Header("Settings")]
    [SerializeField] private float _homeScreenFadeSpeed;
    [SerializeField] private float _delayToStartGame = .5f;
    [SerializeField] private bool _useVideo;

    [Header("Components")]
    [SerializeField] private HomeScreenUnlockSlider _slider;
    [SerializeField] private TMP_Text _buildText;
    [SerializeField] private GameObject _backgroundCover;
    [SerializeField] private CanvasGroup _navigationControls;
    private void Awake()
    {
        _navigationControls.alpha = 0;
    }
    public void OnHomeScreenUnlocked()
    {
        StartCoroutine(FadeHomeScreenOut());
    }

    public void OnHomeScreenLocked()
    {
        StartCoroutine(FadeHomeScreenIn());
    }

    private void Update()
    {
        _backgroundCover.SetActive(!_useVideo);
        _buildText.text = $"{Application.version}";
    }

    private IEnumerator FadeHomeScreenIn()
    {
        _slider.Reset();
        _navigationControls.alpha = 0;
        while (_canvas.alpha < 1)
        {
            _canvas.alpha += _homeScreenFadeSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _canvas.blocksRaycasts = true;
    }

    private IEnumerator FadeHomeScreenOut()
    {
        while (_canvas.alpha > 0)
        {
            _canvas.alpha -= _homeScreenFadeSpeed * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _slider.Reset();
        _navigationControls.alpha = 1;
        _canvas.blocksRaycasts = false;

        yield return new WaitForSeconds(_delayToStartGame);
        SaveAndLoadManager.Instance.StartGame();
    }
}
