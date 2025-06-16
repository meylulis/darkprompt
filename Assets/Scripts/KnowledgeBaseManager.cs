using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class KnowledgeBaseManager : MonoBehaviour
{
    public GameObject addArticleForm;
    public GameObject articleScrollView; // 👈 Добавь это в инспекторе (ArticleScrollView)
    public TMP_InputField titleInput;
    public TMP_InputField contentInput;
    public Button submitButton;
    
    [Tooltip("Ссылка на компонент ArticleLoader для обновления списка статей")]
    public ArticleLoader articleLoader; // Добавляем ссылку на ArticleLoader

    private DatabaseReference dbRef;

    [System.Serializable]
    public class Article
    {
        public string title;
        public string content;
        public string author;

        public Article(string title, string content, string author)
        {
            this.title = title;
            this.content = content;
            this.author = author;
        }
    }

    void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // Скрыть оба окна при старте
        addArticleForm.SetActive(false);
        articleScrollView.SetActive(false);

        submitButton.onClick.AddListener(OnSubmitArticle);
        
        // Если ссылка на ArticleLoader не назначена, попробуем найти компонент
        if (articleLoader == null)
        {
            articleLoader = FindFirstObjectByType<ArticleLoader>();
            if (articleLoader == null)
            {
                Debug.LogWarning("[KnowledgeBaseManager] ArticleLoader не найден. Обновление списка статей после добавления не будет работать.");
            }
        }
    }

    public void ShowAddArticleForm()
    {
        Debug.Log("Нажата кнопка 'Добавить статью' — вызывается ShowAddArticleForm()");
        addArticleForm.SetActive(true);
        articleScrollView.SetActive(false);
        addArticleForm.transform.SetAsLastSibling(); // Чтобы не перекрывалось
    }

    public void ShowKnowledgeBasePanel()
    {
        Debug.Log("Нажата кнопка 'База знаний' — вызывается ShowKnowledgeBasePanel()");
        articleScrollView.SetActive(true);
        addArticleForm.SetActive(false);
        articleScrollView.transform.SetAsLastSibling(); // Чтобы не перекрывалось
    }

    private void OnSubmitArticle()
    {
        Debug.Log("📝 Кнопка 'Отправить статью' нажата");

        string title = titleInput.text.Trim();
        string content = contentInput.text.Trim();
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("⚠️ Пожалуйста, заполните все поля.");
            return;
        }

        if (user == null)
        {
            Debug.LogError("❌ Пользователь не авторизован.");
            return;
        }

        string articleId = dbRef.Child("articles").Push().Key;

        Article articleData = new Article(title, content, user.Email);
        string json = JsonUtility.ToJson(articleData);

        Debug.Log($"📦 Добавление статьи: ID={articleId}, JSON={json}");

        dbRef.Child("articles").Child(articleId).SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("✅ Статья успешно добавлена в Firebase!");

                    // Очистить поля и закрыть форму
                    titleInput.text = "";
                    contentInput.text = "";
                    addArticleForm.SetActive(false);

                    // 👉 Переключиться обратно к списку статей
                    articleScrollView.SetActive(true);
                    
                    // Обновляем список статей
                    if (articleLoader != null)
                    {
                        Debug.Log("🔄 Обновляем список статей после добавления новой статьи");
                        articleLoader.RefreshArticles();
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ ArticleLoader не найден. Список статей не будет обновлен автоматически.");
                    }
                }
                else
                {
                    Debug.LogError("❌ Ошибка при добавлении статьи: " + task.Exception);
                }
            });
    }
}
