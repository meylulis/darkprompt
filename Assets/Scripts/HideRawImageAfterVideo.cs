using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HideRawImageAfterVideo : MonoBehaviour
{
    public RawImage rawImage; // ссылка на RawImage
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // событие по окончанию видео
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        rawImage.gameObject.SetActive(false); // скрываем RawImage
    }
}
