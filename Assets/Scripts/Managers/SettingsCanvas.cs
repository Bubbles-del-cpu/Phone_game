using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsCanvas : UICanvas
{
    private static SettingsCanvas _instance;
    public static SettingsCanvas Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<SettingsCanvas>();

            return _instance;
        }
    }

    public enum CurrentView
    {
        Settings,
        SaveStates
    }

    public CurrentView View;

    [Header("Components")]
    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private CanvasGroup[] _views;
    [SerializeField] private RectTransform _saveButtonContainer;
    [SerializeField] private Button _addNewSaveButton;
    [SerializeField] private GameObject _saveStateWarning;
    [SerializeField] private GameObject _noSaveStatesMessage;
    [SerializeField] private GameObject _earlyReplayExitButton;

    [Header("Prefabs")]
    [SerializeField] private SaveStateButton _saveStateEntryPrefab;

    private List<SaveStateButton> _saveStateButtons = new List<SaveStateButton>();

    protected override void Awake()
    {
        SetView((int)CurrentView.Settings);

        // Setup button listeners
        _addNewSaveButton.onClick.AddListener(() =>
        {
            AddNewSaveState(loadedFromSave: false);
        });

        for (var index = 0; index < _tabButtons.Length; index++)
        {
            var closureIndex = index;
            _tabButtons[index].onClick.AddListener(() =>
            {
                SetView(closureIndex);
            });
        }

        base.Awake();
    }

    public override void Close()
    {
        SetView((int)CurrentView.Settings);
        base.Close();
    }

    private void Update()
    {
        var canAccessSaves = !SaveAndLoadManager.Instance.ReplayingCompletedChapter;
        _saveStateWarning.SetActive(SaveAndLoadManager.Instance.CurrentSave.SaveStates.Count >= SaveAndLoadManager.Instance.SuggestMaxSaveStates);
        _noSaveStatesMessage.SetActive(SaveAndLoadManager.Instance.CurrentSave.SaveStates.Count == 0);

        // We are replaying so disable access to the save states
        _tabButtons[(int)CurrentView.SaveStates].interactable = canAccessSaves;
        if (!canAccessSaves)
        {
            SetView((int)CurrentView.Settings);
        }

        if (_earlyReplayExitButton)
            _earlyReplayExitButton.SetActive(SaveAndLoadManager.Instance.ReplayingCompletedChapter);
    }

    /// <summary>
    /// Sets the current view of the settings canvas
    /// </summary>
    /// <param name="view">View to display (CurrentView enum)</param>
    public void SetView(int view)
    {
        var currentView = (CurrentView)view;
        for (var index = 0; index < _views.Length; index++)
        {
            if (index == view)
            {
                _tabButtons[index].image.color = Color.green;
                _views[index].alpha = 1;
                _views[index].blocksRaycasts = true;
                _views[index].interactable = true;
            }
            else
            {
                _tabButtons[index].image.color = Color.white;
                _views[index].alpha = 0;
                _views[index].blocksRaycasts = false;
                _views[index].interactable = false;
            }
        }
    }

    /// <summary>
    /// Populates the save states in the save states view
    /// </summary>
    /// <param name="saveStates">List of save states to populate</param>
    public void PopulateSaveStates(List<GameSaveState> saveStates)
    {
        for (var index = 0; index < _saveStateButtons.Count; index++)
        {
            Destroy(_saveStateButtons[index].gameObject);
        }

        _saveStateButtons.Clear();
        foreach (var save in saveStates)
        {
            AddNewSaveState(loadedFromSave: true);
        }
    }

    /// <summary>
    /// Adds a new save state button to the save states view
    /// </summary>
    private void AddNewSaveState(bool loadedFromSave)
    {
        if (!loadedFromSave)
        {
            SaveAndLoadManager.Instance.DisplaySaveStateDialog(_saveStateButtons.Count, () =>
            {
                var saveButton = Instantiate(_saveStateEntryPrefab, _saveButtonContainer);
                saveButton.SlotNumber = _saveStateButtons.Count;
                _saveStateButtons.Add(saveButton);
            });
        }
        else
        {
            var saveButton = Instantiate(_saveStateEntryPrefab, _saveButtonContainer);
            saveButton.SlotNumber = _saveStateButtons.Count;
            _saveStateButtons.Add(saveButton);
        }
    }
}