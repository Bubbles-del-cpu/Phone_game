using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Linq;

public class VideoThumbnailGenerator : MonoBehaviour
{
    // [SerializeField] private Camera _thumbnailCamera;
    // [SerializeField] private GameObject _quad;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float _playTime;
    [SerializeField] private float _cameraLensSize = .5f;
    [SerializeField] private Vector3 _quadPosition = new(0, 0, 1);
    [SerializeField] private int _thumbnailSize = 512;

    [SerializeField] float _positionOffset;

    void Awake()
    {
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        //videoPlayer.frameReady += OnFrameReady;
        videoPlayer.playOnAwake = false;
    }

    [SerializeField] private float _thumbnailRegenTimer = 2;
    [SerializeField] private bool _performThumbnailRegeneration;
    private float _timer = 0;
    private bool _canRegen = false;

    private void Update()
    {
        if (_canRegen && _performThumbnailRegeneration)
        {
            _timer += Time.deltaTime;
            if (_timer >= _thumbnailRegenTimer)
            {
                _canRegen = false;
                _timer = 0;
                StartCoroutine(CoRegenerate());
            }
        }
    }

    [ContextMenu("DEBUG_RegenerateAll")]
    public void RegenerateAll()
    {
        StartCoroutine(CoRegenerate());
    }

    IEnumerator CoRegenerate()
    {
        bool complete = false;
        foreach (var item in GameManager.Instance.Thumbnails.Keys.ToList())
        {
            complete = false;
            //Debug.Log($"Starting thumbnail generation for {item}");
            GenerateThumbnail(item, (texture) =>
            {
                complete = true;
                Debug.Log($"Thumbnail generation completed for {item}");

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                GameManager.Instance.Thumbnails[item] = (texture, sprite, GameManager.Instance.Thumbnails[item].Item3);
            });

            while (!complete)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        _canRegen = true;
    }

    public void GenerateThumbnail(VideoClip video, System.Action<Texture2D> onComplete)
    {
        _positionOffset += 100;
        StartCoroutine(GenerateThumbnailCoroutine(video, onComplete));
    }

    private IEnumerator GenerateThumbnailCoroutine(VideoClip video, System.Action<Texture2D> onComplete)
    {
        // Create a temporary camera setup
        GameObject cameraGO = new GameObject("ThumbnailCamera");
        cameraGO.transform.position = new(_positionOffset, 0, 0);

        Camera cam = cameraGO.AddComponent<Camera>();
        // IMPORTANT: Disable the camera from rendering to screen
        cam.enabled = false;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 10f;

        // Create a quad to display the video
        GameObject quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadGO.transform.parent = cameraGO.transform;
        quadGO.transform.localPosition = _quadPosition;

        // Create unlit material for true color reproduction
        Material unlitMaterial = new Material(Shader.Find("Unlit/Texture"));
        quadGO.GetComponent<Renderer>().material = unlitMaterial;

        // Setup VideoPlayer
        VideoPlayer videoPlayer = quadGO.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.clip = video;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        videoPlayer.targetMaterialRenderer = quadGO.GetComponent<Renderer>();
        videoPlayer.targetMaterialProperty = "_MainTex";
        videoPlayer.skipOnDrop = false;

        // Prepare video to get dimensions
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Get video dimensions
        int videoWidth = (int)videoPlayer.width;
        int videoHeight = (int)videoPlayer.height;
        float videoAspect = (float)videoWidth / videoHeight;

        // Set desired thumbnail size (you can adjust this)
        int thumbnailHeight = _thumbnailSize;
        int thumbnailWidth = Mathf.RoundToInt(thumbnailHeight * videoAspect);

        // Create render texture with correct aspect ratio
        RenderTexture renderTexture = new RenderTexture(thumbnailWidth, thumbnailHeight, 24);
        renderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D32_SFloat;
        cam.targetTexture = renderTexture;

        // Adjust camera to match video aspect ratio
        cam.aspect = videoAspect;
        cam.orthographicSize = _cameraLensSize;

        // Scale the quad to match video aspect ratio
        quadGO.transform.localScale = new Vector3(videoAspect, 1, 1);

        // Play and wait for content
        videoPlayer.Play();

        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }

        // Wait for several frames
        yield return new WaitForSeconds(_playTime);

        // Capture the result
        // Cleanup
        videoPlayer.Pause();

        //videoPlayer.frame = 0;

        videoPlayer.Prepare();

        // Render the camera
        cam.Render();

        Texture2D thumbnail = RenderTextureToTexture2D(renderTexture);
        yield return new WaitForSeconds(0.1f);

        DestroyImmediate(cameraGO);
        DestroyImmediate(quadGO);
        renderTexture.Release();

        _positionOffset -= 100;

        yield return new WaitForSeconds(0.1f);
        onComplete?.Invoke(thumbnail);
    }

    private bool IsValidThumbnail(Texture2D texture)
    {
        if (texture == null) return false;

        Color centerPixel = texture.GetPixel(texture.width / 2, texture.height / 2);
        return centerPixel.r + centerPixel.g + centerPixel.b > 0.05f;
    }

    private Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = currentActiveRT;
        return texture;
    }
}