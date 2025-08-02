using UnityEngine;

public class SaveStatesDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _stateButtons;
    [SerializeField] private GameObject _warning;
    // Update is called once per frame
    void Update()
    {
        //The save state buttons are disabled for chapter replay
        _warning.SetActive(SaveAndLoadManager.Instance.ReplayingCompletedChapter);
        _stateButtons.SetActive(!SaveAndLoadManager.Instance.ReplayingCompletedChapter);
    }
}
