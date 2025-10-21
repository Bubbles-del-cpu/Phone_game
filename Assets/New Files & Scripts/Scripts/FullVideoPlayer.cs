// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.Video;

// public class FullVideoPlayer : UIPanel
// {
//     [SerializeField] private Button _playButton;
//     [SerializeField] private RawImage _videoImage;

//     private VideoClip _clip;

//     public void Setup(VideoClip clip)
//     {
//         _clip = clip;
//         GameManager.Instance.MainVideoPlayer.clip = clip;
//         GameManager.Instance.MainVideoPlayer.frame = 5;
//         GameManager.Instance.MainVideoPlayer.Play();
//         GameManager.Instance.MainVideoPlayer.Pause();

//         _videoImage.texture = GameManager.Instance.GetVideoFrame(clip).Item1;
//     }
//     public void OnClick()
//     {
//         _videoImage.texture = GameManager.Instance.MainVideoPlayer.texture;
//         GameManager.Instance.MainVideoPlayer.frame = 0;
//         GameManager.Instance.MainVideoPlayer.Play();
//     }

//     private void Update()
//     {
//         _playButton.gameObject.SetActive(!GameManager.Instance.MainVideoPlayer.isPlaying);
//     }
// }