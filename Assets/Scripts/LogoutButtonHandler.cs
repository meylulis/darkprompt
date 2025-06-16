using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutButtonHandler : MonoBehaviour
{
    public string sceneToLoad = "MainMenu"; // Название первой сцены

    public void Logout()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
