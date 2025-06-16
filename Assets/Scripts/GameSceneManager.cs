using UnityEngine;
using TMPro;
using Firebase.Auth;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI EmailText;    // перетащите сюда ваш EmailText из GameScene

    void Start()
    {
        // Получаем пользователя прямо из FirebaseAuth
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            EmailText.text = user.Email;
        }
        else
        {
            EmailText.text = "Email не найден";
            Debug.LogWarning("[GameSceneManager] FirebaseAuth.CurrentUser == null");
        }
    }
}
