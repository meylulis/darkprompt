using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;

public class ArticleLoader : MonoBehaviour
{
    public Transform contentPanel; // Контейнер для статей
    public GameObject articleItemPrefab; // Префаб одной статьи

    [Header("Admin UI")]
    [Tooltip("Кнопка 'Добавить статью', которая видна только админу.")]
    public Button addArticleButton; // Ссылка на кнопку "Добавить статью"
    [Tooltip("Панель для добавления новой статьи.")]
    public GameObject addArticlePanel; // Новая ссылка на панель
    
    [Header("Full Article UI")]
    public GameObject fullArticlePanel; // Панель полной статьи
    public TMP_Text fullArticleTitleText;
    public TMP_Text fullArticleContentText;
    public Button closeButton;

    [Header("Main Scroll View")]
    public GameObject articleScrollView; // Объект со ScrollView, чтобы скрывать при чтении статьи

    private DatabaseReference dbRef;
    private bool wasArticleScrollViewActive = false; // для отслеживания изменений активности

    void Start()
    {
        // Проверяем наличие AdminManager и создаем его, если нужно
        if (AdminManager.Instance == null)
        {
            Debug.Log("[ArticleLoader] AdminManager.Instance равен null. Создаю новый экземпляр.");
            GameObject adminManagerObject = new GameObject("AdminManager");
            adminManagerObject.AddComponent<AdminManager>();
            DontDestroyOnLoad(adminManagerObject);
        }
        
        dbRef = FirebaseDatabase.DefaultInstance.GetReference("articles");
        LoadArticles();
        fullArticlePanel.SetActive(false);
        closeButton.onClick.AddListener(() =>
        {
            fullArticlePanel.SetActive(false);
            articleScrollView.SetActive(true);
        });

        // Подписываемся на событие изменения статуса админа
        AdminManager.OnAdminStatusChanged += HandleAdminStatusChange;
        
        // Первичная проверка UI на случай, если пользователь уже вошел
        UpdateAdminUI();

        // --- Логика для кнопки добавления статьи ---
        if (addArticleButton != null)
        {
            addArticleButton.onClick.AddListener(ShowAddArticlePanel);
        }
        // -----------------------------------------
    }

    void OnDestroy()
    {
        // Обязательно отписываемся, чтобы избежать ошибок
        AdminManager.OnAdminStatusChanged -= HandleAdminStatusChange;
    }

    private void HandleAdminStatusChange(bool isAdmin)
    {
        Debug.Log($"[ArticleLoader] Получено событие OnAdminStatusChanged. IsAdmin: {isAdmin}. Обновляем UI...");
        UpdateAdminUI();
    }

    void OnEnable()
    {
        // Обновляем админский UI каждый раз при открытии панели
        UpdateAdminUI();
    }

    void OnDisable()
    {
        // Когда панель со статьями выключается, всегда прячем админскую кнопку
        if (addArticleButton != null)
        {
            addArticleButton.gameObject.SetActive(false);
        }
    }

    private void UpdateAdminUI()
    {
        if (addArticleButton == null)
        {
            // Эта ошибка поможет вам найти проблему, если ссылка в инспекторе потерялась
            Debug.LogError("[ArticleLoader] Кнопка 'addArticleButton' не назначена в инспекторе! Пожалуйста, перетащите ее в соответствующее поле.");
            return;
        }

        if (AdminManager.Instance == null)
        {
            Debug.LogWarning("[ArticleLoader] AdminManager.Instance равен null. Кнопка добавления статьи будет скрыта.");
            addArticleButton.gameObject.SetActive(false);
            return;
        }

        bool isAdmin = AdminManager.Instance.IsAdmin;
        // Кнопка должна быть видна ТОЛЬКО если пользователь - админ И панель со статьями активна.
        bool shouldBeVisible = isAdmin && articleScrollView.activeInHierarchy;
        addArticleButton.gameObject.SetActive(shouldBeVisible);
        
        // Проверяем, что кнопка действительно активна/неактивна
        Debug.Log($"[ArticleLoader] Обновление UI для админа. IsAdmin: {isAdmin}, IsPanelActive: {articleScrollView.activeInHierarchy}, ShouldButtonBeVisible: {shouldBeVisible}, Кнопка активна: {addArticleButton.gameObject.activeSelf}");
        
        // Проверяем, что у кнопки есть изображение и оно видимо
        Image buttonImage = addArticleButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            Debug.Log($"[ArticleLoader] Изображение кнопки: Color={buttonImage.color}, Enabled={buttonImage.enabled}, Material={buttonImage.material}");
        }
        else
        {
            Debug.LogWarning("[ArticleLoader] У кнопки нет компонента Image!");
        }
    }

    // Проверяем изменения видимости панели в каждом кадре
    void Update()
    {
        // Отслеживаем изменения состояния активности articleScrollView
        bool isArticleScrollViewActive = articleScrollView != null && articleScrollView.activeInHierarchy;
        
        // Если состояние изменилось с неактивного на активное, обновляем UI
        if (isArticleScrollViewActive && !wasArticleScrollViewActive)
        {
            Debug.Log("[ArticleLoader] Обнаружена активация панели статей. Обновляем UI админа...");
            UpdateAdminUI();
        }
        
        // Запоминаем текущее состояние для следующего кадра
        wasArticleScrollViewActive = isArticleScrollViewActive;
    }

    public void LoadArticles()
    {
        Debug.Log("Начинаем загрузку статей...");

        // Удаляем старые статьи, если есть
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());

        // Загружаем статьи из Firebase
        dbRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                Debug.Log($"Получено {snapshot.ChildrenCount} статей из Firebase");

                foreach (DataSnapshot articleSnapshot in snapshot.Children)
                {
                    string title = articleSnapshot.Child("title").Value?.ToString() ?? "Без названия";
                    string content = articleSnapshot.Child("content").Value?.ToString() ?? "Без содержимого";
                    string author = articleSnapshot.Child("author").Value?.ToString() ?? "Автор неизвестен";

                    Debug.Log($"Загружено: {title} от {author}");

                    // Создаем объект статьи
                    GameObject articleItem = Instantiate(articleItemPrefab, contentPanel);

                    articleItem.transform.Find("TitleText").GetComponent<TMP_Text>().text = title;

                    // Обрезаем контент до 70 символов
                    string preview = content.Length > 70 ? content.Substring(0, 70) + "..." : content;
                    articleItem.transform.Find("ContentText").GetComponent<TMP_Text>().text = preview;

                    // Кнопка "Читать"
                    Button readButton = articleItem.transform.Find("ReadButton").GetComponent<Button>();
                    readButton.onClick.AddListener(() =>
                    {
                        fullArticleTitleText.text = title;
                        fullArticleContentText.text = content;

                        fullArticlePanel.SetActive(true);
                        articleScrollView.SetActive(false); // Скрываем список статей
                    });
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogError("Ошибка при загрузке статей: " + task.Exception);
            }
        });
    }

    // Вызывается при нажатии на кнопку "База знаний"
    public void OpenKnowledgeBase()
    {
        Debug.Log("Открыта база знаний, загружаем статьи...");
        fullArticlePanel.SetActive(false);
        articleScrollView.SetActive(true);
        LoadArticles(); // Повторно загружаем статьи из базы
        
        // Принудительно обновляем UI для админа, так как панель стала видимой
        UpdateAdminUI();
    }

    // Метод для открытия панели добавления статьи (можно вызывать из UI)
    public void ShowAddArticlePanel()
    {
        if (addArticlePanel != null)
        {
            Debug.Log("[ArticleLoader] Открываем панель добавления статьи");
            PanelManager.Instance.ShowOnly(addArticlePanel);
        }
        else
        {
            Debug.LogError("[ArticleLoader] Панель 'addArticlePanel' не назначена в инспекторе!");
        }
    }
    
    // Публичный метод для обновления списка статей (вызывается из KnowledgeBaseManager)
    public void RefreshArticles()
    {
        Debug.Log("[ArticleLoader] Принудительное обновление списка статей");
        LoadArticles();
    }
}
