using MeetAndTalk.GlobalValue;
using UnityEngine;

public class GameAppButtonChecker : MonoBehaviour
{
    public GameAppButton GameAppButton;
    public GlobalBoolValueCheck UnlockVariable;

    private void Update()
    {
        GameAppButton.Interactable = SaveAndLoadManager.Instance.ValueManager.IfTrue(UnlockVariable.ValueName, GlobalValueIFOperations.Equal, UnlockVariable.TargetValue.ToString());
    }
}