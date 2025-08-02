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
        var mananger = FindFirstObjectByType<GameManager>();

        _slider = GetComponent<Slider>();
        _slider.value = mananger.audioSource.volume * _slider.maxValue;

        _valueDisplay.text = $"{_slider.value}";
    }

    public void AdjustGameVolume(float value)
    {
        var mananger = FindFirstObjectByType<GameManager>();
        if (mananger)
        {
            mananger.audioSource.volume = value / _slider.maxValue;

            _valueDisplay.text = $"{_slider.value}";
        }
    }
}
