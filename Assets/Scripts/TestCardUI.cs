using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestCardUI : MonoBehaviour
{
    public TMP_Text titleText;          // Название теста
    public Button startButton;          // Кнопка "Пройти"

    private string testId;              // ID теста из Firebase

    public GameObject passTestPanel;           // Панель с вопросами
    public PassTestManager passTestManager;    // Скрипт, управляющий прохождением теста
    private GameObject listPanel;              // Панель, на которую нужно вернуться

    public void Setup(string id, string title, GameObject pnl, PassTestManager manager, GameObject parentPanel)
    {
        Debug.Log("📌 Setup вызван для: " + id);
        testId = id;
        titleText.text = title;

        passTestPanel = pnl;
        passTestManager = manager;
        listPanel = parentPanel; // Сохраняем родительскую панель

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartTest);
    }

    void StartTest()
    {
        Debug.Log("Начать тест: " + testId);
        Debug.Log("▶️ StartTest вызван, testId = " + testId + ", title = " + titleText.text);

        // Так как у тестов нет сложности, передаем заглушку. 
        // Логика начисления XP в PassTestManager обработает это.
        if (passTestManager != null)
        {
            passTestManager.LoadTest(testId, titleText.text, "не указана", listPanel); 
        }
        else
        {
            Debug.LogError("[TestCardUI] passTestManager не назначен!");
            return;
        }
        
        // Добавляем кнопку закрытия на панель теста, указывая, куда вернуться
        if (CloseButtonManager.Instance != null)
        {
            CloseButtonManager.Instance.AttachCloseButton(passTestPanel, listPanel);
        }

        // Открываем панель через менеджер
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.ShowOnly(passTestPanel);
        }
        else
        {
            // Фоллбэк, если менеджера нет
            passTestPanel.SetActive(true);
        }
    }
}
