using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class GameVolumeControl : MonoBehaviour
{
    [SerializeField]
    private Slider _slider;

    [SerializeField]
    private TMPro.TMP_Text _valueDisplay;

    private void Start()
    {
        _slider = GetComponent<Slider>();
        _slider.value = GameManager.Instance.audioSource.volume * _slider.maxValue;

        _valueDisplay.text = $"{_slider.value}";
    }

    public void AdjustGameVolume(float value)
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.audioSource.volume = value / _slider.maxValue;
            _valueDisplay.text = $"{_slider.value}";
        }
    }
}
