using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HomeScreenUnlockSlider : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] private Slider _unlockSlider;

    private bool _performReset = false;
    [SerializeField] private float _resetSpeed;

    public UnityEvent SliderUnlocked;

    private void Awake()
    {
        _unlockSlider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (_performReset)
        {
            _unlockSlider.value -= Time.deltaTime * _resetSpeed;
            if (_unlockSlider.value <= 0)
                Reset();
        }
    }

    public void Reset()
    {
        _performReset = false;
        _unlockSlider.value = 0;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _unlockSlider.value = _unlockSlider.value;
        if (_unlockSlider.value >= 1)
        {
            SliderUnlocked?.Invoke();
        }
        else
        {
            _performReset = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _performReset = false;
    }
}
