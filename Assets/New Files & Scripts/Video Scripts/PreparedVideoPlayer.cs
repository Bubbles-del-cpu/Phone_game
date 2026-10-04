using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class PreparedVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer _player;

    public Texture Texture => _player.texture;
    public bool IsPlaying => _player.isPlaying;

    private void Awake()
    {
        _player.sendFrameReadyEvents = true;
        _player.renderMode = VideoRenderMode.APIOnly;
        _player.playOnAwake = false;
    }

    private void Start()
    {
        _player.audioOutputMode = VideoAudioOutputMode.AudioSource;
        _player.SetTargetAudioSource(0, GameManager.Instance.audioSource);
    }

    public void PlayVideo(VideoClip clip)
    {
        Stop();
        if (clip == null || !isActiveAndEnabled) return;
        _player.source = VideoSource.VideoClip;
        _player.clip = clip;
        _player.playOnAwake = false;
        _player.waitForFirstFrame = true;

        // Unity 6 Mac workaround - ensure player is completely reset
        _player.clip = null;

        _player.prepareCompleted += OnPrepared;
        _player.errorReceived += OnError;
        StartCoroutine(PlayAfterFrame(clip));
    }

    private IEnumerator PlayAfterFrame(VideoClip clip)
    {
        yield return null;

        _player.clip = clip;
        _player.isLooping = true; // Set your desired settings
        _player.playOnAwake = false;
        _player.waitForFirstFrame = true;

        _player.Prepare();

    }

    private void OnPrepared(VideoPlayer player)
    {
        player.prepareCompleted -= OnPrepared;
        player.Play();
    }

    public void Stop()
    {
        StopAllCoroutines();
        if (_player == null)
            return;

        _player.prepareCompleted -= OnPrepared;
        _player.errorReceived -= OnError;
        _player.Stop();
    }

    private void OnDisable()
    {
        Stop();
    }

    void OnError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[PreparedVideoPlayer] VIDEO ERROR: {message}");
        Stop();
    }
}