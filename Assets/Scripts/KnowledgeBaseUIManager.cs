using UnityEngine;

public class KnowledgeBaseUIManager : MonoBehaviour
{
    public GameObject knowledgeBaseChoicePanel;
    public GameObject articleScrollView;
    public GameObject addArticleForm;
    public GameObject fullArticlePanel;

    public GameObject testListScrollView;
    public GameObject addTestPanel;

    private void ShowPanel(GameObject panelToShow)
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.ShowOnly(panelToShow);
        }
        else
        {
            Debug.LogError("[KnowledgeBaseUIManager] PanelManager не найден на сцене!");
            // Фоллбэк, если PanelManager отсутствует
            CloseAll();
            panelToShow.SetActive(true);
        }
    }

    public void ShowArticleList()
    {
        ShowPanel(articleScrollView);
    }

    public void ShowTestList()
    {
        ShowPanel(testListScrollView);
    }

    public void ShowAddArticleForm()
    {
        ShowPanel(addArticleForm);
    }

    public void ShowAddTestPanel()
    {
        ShowPanel(addTestPanel);
    }

    public void ShowChoicePanel()
    {
        ShowPanel(knowledgeBaseChoicePanel);
    }

    public void ShowFullArticle()
    {
        ShowPanel(fullArticlePanel);
    }

    // Этот метод теперь используется только как запасной вариант, если PanelManager не найден.
    private void CloseAll()
    {
        articleScrollView.SetActive(false);
        knowledgeBaseChoicePanel.SetActive(false);
        addArticleForm.SetActive(false);
        fullArticlePanel.SetActive(false);
        testListScrollView.SetActive(false);
        addTestPanel.SetActive(false);
    }
}
