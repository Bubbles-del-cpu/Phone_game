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
        //currentTime = new System.DateTime(2022, 02, 02, 01, 09, 44); Test timer

        _tmpText.text = $"{(currentTime.Hour < 10 ? "0" : "")}{currentTime.Hour}:{(currentTime.Minute < 10 ? "0" : "")}{currentTime.Minute}";
    }
}