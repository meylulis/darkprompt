using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI greetingText;

    void Start()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            greetingText.text = $"Привет, {user.Email}!";
        }
        else
        {
            greetingText.text = "Пользователь не найден.";
        }
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("GameScene"); // Название сцены с игрой
    }

    public void OnLogoutPressed()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("MainMenu"); // Название первой сцены
    }
}
