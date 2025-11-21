using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[ExecuteInEditMode]
public class UITextureBlur : MonoBehaviour
{
    [Header("Blur Settings")]
    [SerializeField] private Material _blurMaterial;
    [SerializeField] private Texture _sourceTexture;
    [SerializeField] private Image _sourceImage;
    private Texture _targetTexture => _sourceImage != null && _sourceImage.sprite != null ? _sourceImage.sprite.texture : _sourceTexture;
    [SerializeField] [Range(0f, 20f)] private float _blurSize = 2f;
    [SerializeField][Range(1, 20)] private int _iterations = 2;
    [SerializeField] private int _fixedTextureSize = 512;
    [SerializeField] private bool _useDynamicTextureSize = false;

    [Header("Optional: Auto-update")]
    [SerializeField] private bool _updateEveryFrame = false;

    private RawImage _targetImage;
    private RenderTexture _tempRT1;
    private RenderTexture _tempRT2;
    private RenderTexture _resultRT;
    private int _lastWidth;
    private int _lastHeight;

    private void Start()
    {
        if (_blurMaterial == null)
        {
            Debug.LogWarning("[UITextureBlur] Blur material not assigned!");
            enabled = false;
            return;
        }

        if (_targetTexture == null)
        {
            Debug.LogWarning("[UITextureBlur] Source texture not assigned!");
            enabled = false;
            return;
        }

        // Apply blur once on start
        ApplyBlur();
    }

    private void Update()
    {
        if (_updateEveryFrame)
        {
            ApplyBlur();
        }
    }

    public void ApplyBlur()
    {
        if (_blurMaterial == null || _targetTexture == null)
            return;

        if (_targetImage == null)
            _targetImage = GetComponent<RawImage>();

        // int width = _blurTextureSize;
        // int height = _blurTextureSize;

        // Recreate render textures if size changed
        if (_tempRT1 == null || (_useDynamicTextureSize && (_lastWidth != _targetTexture.width || _lastHeight != _targetTexture.height)))
        {
            //Debug.Log("[UITextureBlur] Recreating render textures due to size change.");
            ReleaseRenderTextures();
            CreateRenderTextures(_fixedTextureSize, _fixedTextureSize);
            // _lastWidth = width;
            // _lastHeight = height;
        }

        // Set blur size
        _blurMaterial.SetFloat("_BlurSize", _blurSize);

        // Start with source texture
        Graphics.Blit(_targetTexture, _tempRT1);

        var source = _tempRT1;
        var dest = _tempRT2;

        // Perform blur iterations
        for (var index = 0; index < _iterations; index++)
        {
            // Horizontal pass
            Graphics.Blit(source, dest, _blurMaterial, 0);

            // Vertical pass
            Graphics.Blit(dest, source, _blurMaterial, 1);
        }

        // Copy final result to persistent render texture
        if (_resultRT == null || (_useDynamicTextureSize && (_lastWidth != _targetTexture.width || _lastHeight != _targetTexture.height)))
        {
            if (_resultRT != null)
            {
                _resultRT.Release();
                DestroyRT(_resultRT);
            }
            _resultRT = new RenderTexture(_fixedTextureSize, _fixedTextureSize, 0, RenderTextureFormat.ARGB32);
            _resultRT.filterMode = FilterMode.Bilinear;
        }

        Graphics.Blit(source, _resultRT);

        // Set the final result to the UI
        _targetImage.texture = _resultRT;
    }

    private void DestroyRT(RenderTexture texture)
    {
        if (Application.isPlaying)
            Destroy(texture);
        else
            DestroyImmediate(texture);
    }

    private void CreateRenderTextures(int width, int height)
    {
        _tempRT1 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        _tempRT1.filterMode = FilterMode.Bilinear;
        _tempRT1.wrapMode = TextureWrapMode.Clamp;

        _tempRT2 = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        _tempRT2.filterMode = FilterMode.Bilinear;
        _tempRT2.wrapMode = TextureWrapMode.Clamp;
    }

    private void ReleaseRenderTextures()
    {
        if (_tempRT1 != null)
        {
            _tempRT1.Release();
            DestroyRT(_tempRT1);

            _tempRT1 = null;
        }

        if (_tempRT2 != null)
        {
            _tempRT2.Release();
            DestroyRT(_tempRT2);

            _tempRT2 = null;
        }
    }

    private void OnDisable()
    {
        ReleaseRenderTextures();
    }

    private void OnDestroy()
    {
        ReleaseRenderTextures();

        if (_resultRT != null)
        {
            _resultRT.Release();
            DestroyRT(_resultRT);
        }
    }

    public void SetSourceTarget(Image image)
    {
        if (image && image.sprite != null)
        {
            _sourceTexture = image.sprite.texture;
            ApplyBlur();
        }
    }
}