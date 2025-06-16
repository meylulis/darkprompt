using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("UI Elements")]
    public Slider xpSlider;
    public TMP_Text xpText;
    public GameObject xpBarContainer; // Весь объект XP-бара для скрытия

    [Header("Settings")]
    public int xpPerCorrectAnswer = 10;
    // public int[] xpLevels = { 100, 250, 500, 1000 }; // Опыт для следующих уровней (пока не используется)

    private int currentUserXP = 0;
    // private int currentUserLevel = 1; // (пока не используется)
    private DatabaseReference dbReference;
    private FirebaseAuth auth;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        // Подписываемся на изменение состояния аутентификации
        auth.StateChanged += AuthStateChanged;
        // Проверяем текущее состояние
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != null)
        {
            CheckUserRoleAndLoadXP(auth.CurrentUser);
        }
        else
        {
            // Пользователь не вошел в систему, скрываем XP-бар
            if (xpBarContainer != null)
            {
                xpBarContainer.SetActive(false);
            }
        }
    }

    void CheckUserRoleAndLoadXP(FirebaseUser user)
    {
        dbReference.Child("users").Child(user.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Ошибка получения данных пользователя: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string role = snapshot.Child("role").Value?.ToString() ?? "user";

                if (role == "expert")
                {
                    // Это эксперт/админ, скрываем XP-бар
                    if (xpBarContainer != null) xpBarContainer.SetActive(false);
                }
                else
                {
                    // Это обычный пользователь, показываем XP-бар и загружаем опыт
                    if (xpBarContainer != null) xpBarContainer.SetActive(true);
                    LoadUserXP(user.UserId);
                }
            }
        });
    }

    void LoadUserXP(string userId)
    {
        dbReference.Child("users").Child(userId).Child("xp").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Ошибка загрузки XP: " + task.Exception);
                return;
            }

            if (task.IsCompleted && task.Result.Exists)
            {
                currentUserXP = int.Parse(task.Result.Value.ToString());
            }
            else
            {
                currentUserXP = 0; // Если XP нет, начинаем с 0
            }
            UpdateXPUI();
        });
    }

    public void AddXP(int amount)
    {
        if (auth.CurrentUser == null) return; // Не добавлять XP, если пользователь не залогинен

        currentUserXP += amount;
        dbReference.Child("users").Child(auth.CurrentUser.UserId).Child("xp").SetValueAsync(currentUserXP);
        UpdateXPUI();
    }

    void UpdateXPUI()
    {
        if (xpSlider == null || xpText == null) return;
        
        // Простое отображение общего количества XP
        int maxXP = 100; // Устанавливаем максимальное значение для уровня

        // Ограничиваем значение для слайдера, чтобы оно не выходило за пределы
        float sliderValue = Mathf.Clamp(currentUserXP, 0, maxXP);

        xpSlider.value = sliderValue;
        xpSlider.maxValue = maxXP; 
        xpText.text = $"XP: {(int)sliderValue} / {maxXP}";
    }

    void OnDestroy()
    {
        if(auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
        }
    }
} 