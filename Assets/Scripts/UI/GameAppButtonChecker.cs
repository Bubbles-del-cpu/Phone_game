using MeetAndTalk.GlobalValue;
using UnityEngine;

public class GameAppButtonChecker : MonoBehaviour
{
    public GameObject GameAppButton;
    public GlobalBoolValueCheck UnlockVariable;

    private void Update()
    {
        GameAppButton.SetActive(SaveAndLoadManager.Instance.ValueManager.IfTrue(UnlockVariable.ValueName, GlobalValueIFOperations.Equal, UnlockVariable.TargetValue.ToString()));
    }
}