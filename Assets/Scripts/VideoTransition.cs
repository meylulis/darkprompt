using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoTransition : MonoBehaviour
{
    public Button playButton;
    public GameObject menuUI;
    public GameObject videoPanel; // панель с видеоплеером
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = videoPanel.GetComponentInChildren<VideoPlayer>();
        videoPanel.SetActive(false); // скрыть видео до запуска
        videoPlayer.loopPointReached += OnVideoFinished; // событие окончания видео
    }

    public void OnPlayPressed()
    {
        Debug.Log("▶ Видео запускается");

        menuUI.SetActive(false); // скрыть меню
        videoPanel.SetActive(true); // показать видео

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("✅ Видео закончилось. Загружаем сцену...");
        SceneManager.LoadScene("GameScene");
    }
}
