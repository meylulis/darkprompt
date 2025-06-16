using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    // === UI первой сцены ===
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;
    public Button loginButton;
    public Button registerButton;

    // === UI третьей сцены (GameScene) ===
    [Header("GameScene UI")]
    public TextMeshProUGUI EmailText;      // твой EmailText
    public TextMeshProUGUI TerminalText;   // здесь мы отобразим миссию или "Нет активной миссии"

    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    void Start()
    {
        if (loginButton != null) loginButton.interactable = false;
        if (registerButton != null) registerButton.interactable = false;
        statusText.text = "Подключение к серверам...";

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                statusText.text = "Система готова к работе!";
                if (loginButton != null) loginButton.interactable = true;
                if (registerButton != null) registerButton.interactable = true;
            }
            else
            {
                statusText.text = "Ошибка подключения к Firebase.";
                Debug.LogError("Не удалось разрешить зависимости Firebase: " + task.Result);
            }
        });

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            if (auth != null && auth.CurrentUser != null && EmailText != null)
            {
                EmailText.text = auth.CurrentUser.Email;
            }

            string activeMission = "";  

            if (!string.IsNullOrEmpty(activeMission))
            {
                TerminalText.text = activeMission;
            }
            else
            {
                TerminalText.text = "Нет активной миссии";
            }
        }
    }

    public void Register()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.Exception != null && task.Exception.GetBaseException() is FirebaseException firebaseEx)
                {
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                    string message = "Ошибка регистрации";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Введите адрес электронной почты.";
                            break;
                        case AuthError.MissingPassword:
                            message = "Введите пароль.";
                            break;
                        case AuthError.InvalidEmail:
                            message = "Некорректный формат email.";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Этот email уже зарегистрирован.";
                            break;
                        case AuthError.WeakPassword:
                            message = "Пароль слишком слабый (не менее 6 символов).";
                            break;
                        default:
                            message = $"Произошла ошибка: {errorCode}";
                            break;
                    }
                    statusText.text = message;
                }
                else
                {
                    statusText.text = "Произошла неизвестная ошибка. Проверьте подключение к интернету.";
                    Debug.LogError($"Ошибка регистрации: {task.Exception}");
                }
                return;
            }
            
            AuthResult result = task.Result;
            statusText.text = $"Зарегистрирован! Email: {result.User.Email}";

            dbReference.Child("users").Child(result.User.UserId).Child("role").SetValueAsync("user");
            dbReference.Child("users").Child(result.User.UserId).Child("xp").SetValueAsync(0);

            AdminManager.Instance?.CheckAdminStatus();

            SceneManager.LoadScene("MenuScene");
        });
    }

    public void Login()
    {
        if (auth == null)
        {
            statusText.text = "Ошибка: система аутентификации не готова. Подождите и попробуйте снова.";
            Debug.LogError("[AuthManager] Попытка входа при auth == null. Инициализация Firebase не завершена?");
            return;
        }

        string email = emailInput.text;
        string password = passwordInput.text.Trim();

        // Regex.Replace удаляет любые пробельные символы (\s) в начале (^) или конце ($) строки.
        email = Regex.Replace(email, @"^\s+|\s+$", "");

        Debug.Log($"[AuthManager] Попытка входа с email: [{email}]");

        if (string.IsNullOrEmpty(email))
        {
            statusText.text = "Введите адрес электронной почты.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                statusText.text = "Вход отменен.";
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                statusText.text = "Ошибка: " + task.Exception.InnerExceptions[0].Message;
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Пользователь {0} ({1}) вошел в систему успешно.", result.User.DisplayName, result.User.Email);
            statusText.text = $"Успешный вход: {result.User.Email}";
            
            AdminManager.Instance?.CheckAdminStatus(result.User.Email);

            SceneManager.LoadScene("GameScene");
        });
    }
}
