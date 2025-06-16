using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions; // Обязательно подключи этот неймспейс
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class TestListLoader : MonoBehaviour
{
    public GameObject testCardPrefab;    // Префаб карточки теста
    public Transform cardContainer;       // Контейнер для карточек (Content в ScrollView)
    public GameObject panelWithTestList;
    public GameObject passTestPanel; // Панель прохождения теста
    // Панель со списком тестов

    [Header("Admin UI")]
    [Tooltip("Кнопка для добавления нового теста. Появится только у админа.")]
    public Button addTestButton;
    [Tooltip("Панель (форма) для создания нового теста.")]
    public GameObject addTestPanel;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    private Dictionary<string, bool> completedTests = new Dictionary<string, bool>();
    private PassTestManager passTestManager;

    void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        passTestManager = PassTestManager.Instance; // Принудительно находим менеджер
        
        // Подписка на события
        GameEvents.OnMissionCompleted += HandleTestCompleted;
        AdminManager.OnAdminStatusChanged += HandleAdminStatusChange;

        // Настройка кнопки "Добавить тест"
        if (addTestButton != null)
        {
            addTestButton.onClick.AddListener(() => {
                if (addTestPanel != null)
                {
                    PanelManager.Instance.ShowOnly(addTestPanel);
                }
                else
                {
                    Debug.LogError("[TestListLoader] Панель 'addTestPanel' не назначена в инспекторе!");
                }
            });
        }
    }

    void OnDestroy()
    {
        GameEvents.OnMissionCompleted -= HandleTestCompleted;
        AdminManager.OnAdminStatusChanged -= HandleAdminStatusChange; // Отписка
    }

    void OnEnable()
    {
        //UpdateAdminUI(); // Проверка при включении
        LoadUserCompletionData();
    }

    private void HandleAdminStatusChange(bool isAdmin)
    {
        Debug.Log($"[TestListLoader] Получено событие OnAdminStatusChanged. IsAdmin: {isAdmin}. Обновляем UI...");
        UpdateAdminUI();
    }

    private void UpdateAdminUI()
    {
        if (addTestButton == null)
        {
            Debug.LogError("[TestListLoader] Кнопка 'addTestButton' не назначена в инспекторе!");
            return;
        }
        bool isAdmin = AdminManager.Instance != null && AdminManager.Instance.IsAdmin;
        addTestButton.gameObject.SetActive(isAdmin);
    }

    // Этот метод теперь вызывается извне, например, кнопкой из KnowledgeBaseUIManager
    public void ShowTests()
    {
        if (PanelManager.Instance != null)
        {
            // `this.gameObject` - это и есть panelWithTestList, т.к. скрипт теперь на ней
            PanelManager.Instance.ShowOnly(this.gameObject);
        }
        else
        {
            Debug.LogError("[TestListLoader] PanelManager не найден!");
        }
    }

    void LoadUserCompletionData()
    {
        if (auth.CurrentUser == null)
        {
            LoadTestsFromFirebase();
            return;
        }

        dbRef.Child("users").Child(auth.CurrentUser.UserId).Child("completedMissions").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            completedTests.Clear();
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                foreach (var child in task.Result.Children)
                {
                    completedTests[child.Key] = true;
                }
            }
            LoadTestsFromFirebase();
        });
    }

    void LoadTestsFromFirebase()
    {
        // Удаляем старые карточки перед загрузкой новых
        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        FirebaseDatabase.DefaultInstance
            .GetReference("tests")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    foreach (var test in snapshot.Children)
                    {
                        string testId = test.Key;
                        string title = test.Child("title").Value?.ToString();

                        Debug.Log("🧩 Создание карточки для: " + title);

                        GameObject card = Instantiate(testCardPrefab, cardContainer);
                        var ui = card.GetComponent<TestCardUI>();
                        var buttonText = ui.startButton.GetComponentInChildren<TMP_Text>();

                        ui.passTestPanel = passTestPanel;

                        if (completedTests.ContainsKey(testId))
                        {
                            // Тест пройден - блокируем
                            ui.startButton.interactable = false;
                            if(buttonText != null) buttonText.text = "Пройдено";
                        }
                        else
                        {
                             if(buttonText != null) buttonText.text = "Начать";
                        }

                        // ❗️ПРОВЕРКИ
                        if (ui.passTestPanel == null)
                            Debug.LogError("❌ passTestPanel не задан у карточки: " + title);

                        // Вызов Setup
                        ui.Setup(testId, title, passTestPanel, passTestManager, panelWithTestList);

                    }
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError("Ошибка загрузки тестов: " + task.Exception);
                }
            });
    }

    private void HandleTestCompleted(string testId)
    {
        Debug.Log($"[TestListLoader] Получено событие о завершении теста: {testId}. Добавляю в локальный список.");
        if (!completedTests.ContainsKey(testId))
        {
            completedTests.Add(testId, true);
        }
    }
}
