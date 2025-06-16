using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using Firebase.Auth;

public class MissionsLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cyberChallengesPanel;
    public Transform cardsContainer;
    public Button cyberChallengesButton;

    [Header("Mission Panels")]
    [Tooltip("Панель для миссии с проверкой надежности пароля.")]
    public GameObject weakPasswordPanel;
    [Tooltip("Панель для миссии с фишинг-тестом.")]
    public GameObject fishingTestPanel;
    [Tooltip("Панель для миссии со взломом пароля.")]
    public GameObject passwordCrackPanel;
    [Tooltip("Панель для миссии с SQL-инъекцией.")]
    public GameObject sqlInjectionPanel;

    [Header("Dependencies")]
    [Tooltip("Префаб карточки для отображения миссии в списке.")]
    public GameObject cardPrefab;
    [Tooltip("Ссылка на PassTestManager для загрузки тестов.")]
    [SerializeField] private PassTestManager _passTestManager;

    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private Dictionary<string, bool> completedMissions = new Dictionary<string, bool>();
    private Dictionary<string, MissionCard> missionCards = new Dictionary<string, MissionCard>();
    private HashSet<string> _completedMissions = new HashSet<string>();
    
    private string currentMissionId;
    private string currentMissionType;
    private string currentMissionDifficulty;

    private void Awake()
    {
        Debug.Log("[MissionsLoader] Инициализация...");
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        // Инициализируем менеджер тестов, если он не назначен
        if (_passTestManager == null)
        {
            _passTestManager = FindFirstObjectByType<PassTestManager>();
            if (_passTestManager == null)
            {
                Debug.LogError("[MissionsLoader] PassTestManager не найден на сцене!");
            }
        }
        // Подписываемся на события
        GameEvents.OnMissionCompleted += HandleMissionCompleted;
        AdminManager.OnAdminStatusChanged += HandleAdminStatusChange;
    }

    void OnDestroy()
    {
        // Отписываемся от всех событий
        GameEvents.OnMissionCompleted -= HandleMissionCompleted;
        AdminManager.OnAdminStatusChanged -= HandleAdminStatusChange;
    }

    void OnEnable()
    {
        // Этот метод вызывается КАЖДЫЙ раз, когда панель становится активной.
        Debug.Log("[MissionsLoader] Панель миссий открыта. Обновление списка...");
        UpdateAdminUI(); // Обновляем админские кнопки
        LoadUserCompletionData();
    }

    private void UpdateAdminUI()
    {
        Debug.Log("[MissionsLoader] Проверка админ-статуса (в данный момент нет админ-элементов).");
    }

    public void ShowCyberChallengesPanel()
    {
        // Этот метод будет вызываться кнопкой "Кибер-испытания"
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.ShowOnly(this.gameObject); 
        }
        else
        {
            Debug.LogError("[MissionsLoader] PanelManager.Instance не найден!");
        }
    }

    // Сначала загружаем данные о прохождении
    void LoadUserCompletionData()
    {
        if (auth.CurrentUser == null)
        {
            Debug.LogWarning("[MissionsLoader] Пользователь не аутентифицирован. Загрузка миссий без данных о прохождении.");
            completedMissions.Clear();
            LoadMissions();
            return;
        }

        Debug.Log($"[MissionsLoader] Запрос данных о пройденных миссиях для пользователя {auth.CurrentUser.UserId}...");
        dbReference.Child("users").Child(auth.CurrentUser.UserId).Child("completedMissions").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            completedMissions.Clear();
            if (task.IsCompleted && task.Result.Exists)
            {
                Debug.Log("[MissionsLoader] Данные о прохождении получены. Обработка...");
                foreach (var child in task.Result.Children)
                {
                    completedMissions[child.Key] = true;
                    Debug.Log($"[MissionsLoader] -> Обнаружена пройденная миссия: {child.Key}");
                }
            }
            else
            {
                Debug.LogWarning("[MissionsLoader] Для пользователя не найдено пройденных миссий или произошла ошибка.");
            }
            // После загрузки данных о прохождении, загружаем сами миссии
            LoadMissions();
        });
    }

    void LoadMissions()
    {
        Debug.Log("[MissionsLoader] Начало загрузки миссий из Firebase");
        
        if (cardPrefab == null)
        {
            Debug.LogError("[MissionsLoader] Префаб карточки не назначен!");
            return;
        }

        if (cardsContainer == null)
        {
            Debug.LogError("[MissionsLoader] Контейнер карточек не назначен!");
            return;
        }

        FirebaseDatabase.DefaultInstance.GetReference("missions")
            .GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[MissionsLoader] Ошибка загрузки миссий: {task.Exception}");
                    return;
                }

                if (!task.IsCompleted)
                {
                    Debug.LogWarning("[MissionsLoader] Задача не завершена");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                
                if (!snapshot.Exists)
                {
                    Debug.LogWarning("[MissionsLoader] В базе данных нет миссий");
                    return;
                }

                Debug.Log($"[MissionsLoader] Получено миссий: {snapshot.ChildrenCount}");

                // Очистка старых карточек
                int childCount = cardsContainer.childCount;
                Debug.Log($"[MissionsLoader] Удаление старых карточек (найдено {childCount})");
                foreach (Transform child in cardsContainer)
                {
                    Destroy(child.gameObject);
                }
                missionCards.Clear();

                // Создание новых карточек
                int createdCards = 0;
                foreach (DataSnapshot missionSnapshot in snapshot.Children)
                {
                    if (!missionSnapshot.HasChildren)
                    {
                        Debug.LogWarning($"[MissionsLoader] Пустая миссия (ключ: {missionSnapshot.Key})");
                        continue;
                    }

                    string missionId = missionSnapshot.Key;
                    string title = missionSnapshot.Child("title").Value?.ToString() ?? "Без названия";
                    string difficulty = missionSnapshot.Child("difficulty").Value?.ToString() ?? "Не указана";
                    string type = missionSnapshot.Child("type").Value?.ToString() ?? "";
                    
                    // --- Улучшенная логика для определения testId ---
                    string testId = null; 
                    if (missionSnapshot.HasChild("testId") && !string.IsNullOrEmpty(missionSnapshot.Child("testId").Value?.ToString()))
                    {
                        testId = missionSnapshot.Child("testId").Value.ToString();
                    }
                    else if (missionSnapshot.HasChild("useMissionIdAsTestId") && (bool)missionSnapshot.Child("useMissionIdAsTestId").Value)
                    {
                        testId = missionId;
                    }
                    // ---------------------------------------------

                    Debug.Log($"[MissionsLoader] Обработка миссии: {title} (id: {missionId}, testId: {testId})");

                    GameObject card = Instantiate(cardPrefab, cardsContainer);
                    createdCards++;

                    var cardItem = card.GetComponent<MissionCard>();
                    if (cardItem == null)
                    {
                        Debug.LogError("[MissionsLoader] Компонент MissionCard не найден на префабе");
                        continue;
                    }

                    missionCards[missionId] = cardItem;

                    // Сохраняем данные в карточку
                    cardItem.MissionId = missionId;
                    cardItem.MissionTitle = title;
                    cardItem.MissionType = type;
                    cardItem.MissionDifficulty = difficulty;
                    
                    // Установка текста
                    if (cardItem.TitleText != null)
                    {
                        cardItem.TitleText.text = title;
                    }
                    else
                    {
                        Debug.LogWarning("[MissionsLoader] TitleText не найден");
                    }

                    if (cardItem.DifficultyText != null)
                    {
                        cardItem.DifficultyText.text = difficulty;
                        // Цвет сложности
                        switch (difficulty.ToLower())
                        {
                            case "лёгкая": cardItem.DifficultyText.color = Color.green; break;
                            case "средняя": cardItem.DifficultyText.color = Color.yellow; break;
                            case "тяжёлая": cardItem.DifficultyText.color = Color.red; break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[MissionsLoader] DifficultyText не найден");
                    }

                    // Настройка кнопки и состояния "Пройдено"
                    UpdateButtonState(cardItem, missionId, testId, title, type, difficulty);
                }

                Debug.Log($"[MissionsLoader] Создано и настроено карточек: {missionCards.Count}/{snapshot.ChildrenCount}");
            });
    }

    public void OpenMissionPanel(string missionId, string testId, string title, string difficulty, string type)
    {
        Debug.Log($"[MissionsLoader] Выбрана миссия: {title} (тип: {type}). ID миссии: {missionId}, ID теста: {testId}");
        
        switch (type.ToLower())
        {
            case "test":
                if (string.IsNullOrEmpty(testId))
                {
                    Debug.LogError($"[MissionsLoader] ID теста для миссии '{title}' (тип: 'test') пустой! Не могу загрузить тест.");
                    return;
                }
                _passTestManager.LoadTest(testId, title, difficulty, this.gameObject);
                break;

            case "weakpassword":
                if (weakPasswordPanel != null)
                {
                    PanelManager.Instance.ShowOnly(weakPasswordPanel);
                    weakPasswordPanel.GetComponent<WeakPasswordChecker>()?.SetupMission(missionId, difficulty);
                }
                else Debug.LogError("[MissionsLoader] Панель для проверки слабого пароля не назначена!");
                break;
            
            case "passwordcrack":
                if (passwordCrackPanel != null)
                {
                    PanelManager.Instance.ShowOnly(passwordCrackPanel);
                    passwordCrackPanel.GetComponent<PasswordCrackPanel>()?.Setup(missionId, difficulty);
                }
                else Debug.LogError("[MissionsLoader] Панель для взлома пароля не назначена!");
                break;

            case "fishingtest":
                if (fishingTestPanel != null)
                {
                    PanelManager.Instance.ShowOnly(fishingTestPanel);
                    var panelScript = fishingTestPanel.GetComponentInChildren<FishingTestPanel>();
                    if (panelScript != null)
                    {
                        panelScript.Setup(missionId, difficulty);
                    }
                    else
                    {
                        Debug.LogError($"[MissionsLoader] Не удалось найти компонент FishingTestPanel на объекте {fishingTestPanel.name} или его дочерних элементах.");
                    }
                }
                else Debug.LogError("[MissionsLoader] Панель для фишинг-теста не назначена!");
                break;

            case "sqltest":
                if (sqlInjectionPanel != null)
                {
                    PanelManager.Instance.ShowOnly(sqlInjectionPanel);
                    sqlInjectionPanel.GetComponent<SQLInjectionTestPanel>()?.Setup(missionId, difficulty);
                }
                else Debug.LogError("[MissionsLoader] Панель для SQL-инъекций не назначена в инспекторе!");
                break;

            default:
                Debug.LogError($"[MissionsLoader] Неизвестный тип миссии: '{type}' для миссии '{title}'.");
                break;
        }
    }

    private void UpdateButtonState(MissionCard cardItem, string missionId, string testId, string title, string type, string difficulty)
    {
        // Проверяем, была ли миссия пройдена
        bool isCompleted = completedMissions.ContainsKey(missionId);
        
        // Настраиваем кнопку
        var button = cardItem.Button; // Используем правильное имя свойства 'Button'
        button.onClick.RemoveAllListeners();

        // Проверяем, можно ли запустить эту миссию
        bool canStart = true;
        if (type == "test" && string.IsNullOrEmpty(testId))
        {
            canStart = false;
            Debug.LogWarning($"[MissionsLoader] Миссия '{title}' (id: {missionId}) имеет тип 'test', но для нее не указан 'testId' или 'useMissionIdAsTestId'. Кнопка будет отключена.");
        }

        if (canStart)
        {
            button.interactable = !isCompleted; // Кнопка неактивна, если миссия пройдена
            button.onClick.AddListener(() => OpenMissionPanel(missionId, testId, title, difficulty, type));
        }
        else
        {
            button.interactable = false;
        }

        // Обновляем текст на кнопке
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = isCompleted ? "Пройдено" : "Начать";
        }
    }
    
    private void HandleMissionCompleted(string missionId)
    {
        Debug.Log($"[MissionsLoader] Получено событие о завершении миссии: {missionId}. Обновление карточки...");
        
        completedMissions[missionId] = true;

        // Тут логика обновления карточки уже не нужна, так как мы перезагружаем их в OnEnable.
        // Но оставим на всякий случай, если решим поменять поведение.
        if (missionCards.TryGetValue(missionId, out MissionCard card))
        {
            // Здесь мы не можем просто вызвать UpdateButtonState, т.к. у нас нет всех данных (testId и т.д.)
            // Самый простой способ - просто обновить текст и интерактивность.
             var buttonText = card.Button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) buttonText.text = "Пройдено";
            card.Button.interactable = false;
        }
    }

    private void HandleAdminStatusChange(bool isAdmin)
    {
        Debug.Log($"[MissionsLoader] Получено событие OnAdminStatusChanged. IsAdmin: {isAdmin}.");
        UpdateAdminUI(); // Оставим вызов на случай будущих админ-функций здесь
    }
}