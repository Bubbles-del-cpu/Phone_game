using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class SystemClockTimerText : MonoBehaviour
{
    private TMP_Text _tmpText;

    void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        var currentTime = System.DateTime.Now;
        _tmpText.text = $"{currentTime.Hour}:{currentTime.Minute}";
    }
}