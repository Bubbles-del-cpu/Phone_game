using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(AspectRatioFitter))]
public class AspectRatioScaler : MonoBehaviour
{
    [SerializeField]
    private AspectRatioFitter _fitter;

    [SerializeField]
    private bool _forcePotraitMode;
    
    [SerializeField]
    private bool _disableDynamicScaling;

    private void Awake()
    {
        _fitter = GetComponent<AspectRatioFitter>();
    }

    private void Update()
    {
        if (_disableDynamicScaling)
            return;
            
        if (_forcePotraitMode)
        {
            _fitter.aspectRatio = (float)Screen.height / (float)Screen.width;
        }
        else
        {
            _fitter.aspectRatio = (float)Screen.width / (float)Screen.height;
        }
    }
}
