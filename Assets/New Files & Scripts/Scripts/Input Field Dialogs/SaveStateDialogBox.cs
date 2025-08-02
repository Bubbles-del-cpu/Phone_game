public class SaveStateDialogBox : InputFieldDialogBox
{
    private int _slotNumber;

    public void Setup(int slotNumber)
    {
        _slotNumber = slotNumber;
    }

    public override void Submit()
    {
        SaveAndLoadManager.Instance.CreateSaveState(_slotNumber, _inputField.text);
        base.Submit();
    }
}