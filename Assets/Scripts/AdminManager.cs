using System.Collections.Generic;
using UnityEngine;
using System; // Нужно для Action
using Firebase.Auth;

public class AdminManager : MonoBehaviour
{
    public static AdminManager Instance { get; private set; }

    [Header("Настройки доступа")]
    [Tooltip("Email администратора, который будет иметь доступ к специальным панелям.")]
    public string adminEmail = "admin@example.com"; // Укажите здесь реальный email админа

    public bool IsAdmin { get; private set; } = false;

    // Событие, которое будет вызываться при изменении статуса админа
    public static event Action<bool> OnAdminStatusChanged;

    private readonly HashSet<string> _adminEmails = new HashSet<string>
    {
        "admin@example.com",
        "another.admin@example.com"
        // Добавьте другие email администраторов сюда
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            IsAdmin = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Подписываемся на изменение состояния аутентификации
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChanged;
    }

    void OnDestroy()
    {
        // Отписываемся, чтобы избежать утечек
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChanged;
    }

    private void HandleAuthStateChanged(object sender, System.EventArgs e)
    {
        CheckAdminStatus();
    }

    public void CheckAdminStatus()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            string email = user.Email.ToLower().Trim();
            IsAdmin = _adminEmails.Contains(email);
            Debug.Log($"[AdminManager] Проверка статуса администратора. Пользователь: {user.Email}, Email после обработки: {email}, IsAdmin: {IsAdmin}");
            Debug.Log($"[AdminManager] Список админ-email: {string.Join(", ", _adminEmails)}");
        }
        else
        {
            IsAdmin = false;
            Debug.Log("[AdminManager] Пользователь не аутентифицирован. Доступ администратора не предоставлен.");
        }

        // Вызываем событие и передаем ему новый статус
        OnAdminStatusChanged?.Invoke(IsAdmin);
    }

    public void CheckAdminStatus(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            IsAdmin = false;
            Debug.Log("[AdminManager] Email пустой, IsAdmin: false");
        }
        else
        {
            string processedEmail = email.ToLower().Trim();
            IsAdmin = _adminEmails.Contains(processedEmail);
            Debug.Log($"[AdminManager] Проверка статуса для email: {email}, после обработки: {processedEmail}. IsAdmin: {IsAdmin}");
            Debug.Log($"[AdminManager] Список админ-email: {string.Join(", ", _adminEmails)}");
        }
        
        // Вызываем событие и передаем ему новый статус
        OnAdminStatusChanged?.Invoke(IsAdmin);
    }
} 