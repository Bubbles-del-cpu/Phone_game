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

    [ContextMenu("DEBUG_RegenerateAll")]
    void RegenerateAll()
    {
        StartCoroutine(CoRegenerate());
    }

    IEnumerator CoRegenerate()
    {
        bool complete = false;
        foreach (var item in GameManager.Instance.Thumbnails.Keys.ToList())
        {
            complete = false;
            Debug.Log($"Starting thumbnail generation for {item}"); 
            GenerateThumbnail(item, (texture) =>
            {
                complete = true;
                Debug.Log($"Thumbnail generation completed for {item}");
                GameManager.Instance.Thumbnails[item] = texture;
            });

            while (!complete)
            {
                yield return new WaitForEndOfFrame();
            }
        }
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
        
        // Render the camera
        cam.Render();
        
        // Capture the result
        Texture2D thumbnail = RenderTextureToTexture2D(renderTexture);
        _positionOffset -= 100;
        // Cleanup
        videoPlayer.Stop();
        DestroyImmediate(cameraGO);
        DestroyImmediate(quadGO);
        renderTexture.Release();
        
        onComplete?.Invoke(thumbnail);
    }

    // private IEnumerator GenerateThumbnailCoroutine(VideoClip video, System.Action<Texture2D> onComplete)
    // {
    //     RenderTexture renderTexture = new RenderTexture((int)video.width, (int)video.height, 0);

    //     // Configure VideoPlayer
    //     videoPlayer.playOnAwake = false;
    //     videoPlayer.clip = video;
    //     videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    //     videoPlayer.targetTexture = renderTexture;
    //     videoPlayer.skipOnDrop = true;

    //     // Subscribe to frame ready event
    //     bool frameReady = false;
    //     videoPlayer.frameReady += (VideoPlayer vp, long frameIdx) =>
    //     {
    //         frameReady = true;
    //     };

    //     // Prepare the video
    //     videoPlayer.Prepare();

    //     // Wait for video to be prepared
    //     while (!videoPlayer.isPrepared)
    //     {
    //         yield return null;
    //     }

    //     // Seek to desired time (optional)
    //     videoPlayer.time = _playTime; // 1 second in

    //     // Play the video
    //     videoPlayer.Play();

    //     // Wait for the first frame to be ready
    //     while (!frameReady)
    //     {
    //         yield return null;
    //     }

    //     // Wait one more frame to ensure rendering is complete
    //     yield return new WaitForEndOfFrame();

    //     // Capture the frame
    //     Texture2D thumbnail = RenderTextureToTexture2D(renderTexture);

    //     // Cleanup
    //     videoPlayer.Stop();
    //     renderTexture.Release();

    //     onComplete?.Invoke(thumbnail);
    // }

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