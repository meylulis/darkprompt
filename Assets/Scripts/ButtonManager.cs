using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("ButtonManager работает!");
    }

    public void OnPlayButtonClicked()
    {
        Debug.Log("Нажата кнопка Играть");
        SceneManager.LoadScene("GameScene");
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("Нажата кнопка Выйти");
        SceneManager.LoadScene("MainMenu");
    }
}
