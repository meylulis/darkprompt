using UnityEngine;

public class ArticleManager : MonoBehaviour
{
    [Header("Дочерние панели внутри KnowledgeBasePanel")]
    public GameObject articleScrollView;  // ArticleScrollView GameObject
    public GameObject addArticleForm;     // AddArticleForm GameObject
    public GameObject knowledgeBaseChoicePanel;
    void Start()
    {
        // Прячем оба окна при старте
        articleScrollView.SetActive(false);
        addArticleForm.SetActive(false);
        knowledgeBaseChoicePanel.SetActive(false);
    }

    // Нажали "База знаний"
    public void OnClickShowKnowledgeBase()
    {
        knowledgeBaseChoicePanel.SetActive(true);
        articleScrollView.SetActive(false);
        addArticleForm.SetActive(false);
        
    }

    // Нажали "Добавить статью"
    public void OnClickShowAddForm()
    {
        addArticleForm.SetActive(true);
        articleScrollView.SetActive(false);
        addArticleForm.transform.SetAsLastSibling();
    }
    
    public void OnClickOpenArticles()
    {
        knowledgeBaseChoicePanel.SetActive(false);
        articleScrollView.SetActive(true);
    }

    // Кнопка "Тесты"
    public void OnClickOpenTests()
    {
        Debug.Log("Раздел с тестами пока не реализован");
        // Можно показать заглушку или сообщение
    }
}
