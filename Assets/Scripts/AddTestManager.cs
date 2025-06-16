using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Firebase.Database; // если сделаешь через пространство имён  
// или просто: using Firebase.Database;


public class AddTestManager : MonoBehaviour
{
    [Header("Основные элементы")]
    public TMP_InputField testTitleInput;
    public GameObject questionPrefab; // Префаб вопроса
    public Transform questionsContainer;
    public Button addQuestionButton;
    public Button saveTestButton;
    private List<QuestionUI> questions = new List<QuestionUI>();

    void Start()
    {
        addQuestionButton.onClick.AddListener(AddQuestion);
        saveTestButton.onClick.AddListener(SaveTest);
    }

    void AddQuestion()
    {
        GameObject newQuestionGO = Instantiate(questionPrefab, questionsContainer);
        QuestionUI q = newQuestionGO.GetComponent<QuestionUI>();
        questions.Add(q);
    }

  void SaveTest()
    {
        string testTitle = testTitleInput.text;
        List<QuestionData> questionDataList = new List<QuestionData>();

        foreach (var q in questions)
        {
            questionDataList.Add(q.GetQuestionData());
        }
        
        FirebaseManager.SaveTestToFirebase(testTitle, questionDataList, () => {
            // Просто закрываем панель добавления теста
            this.gameObject.SetActive(false);
            
            // Возвращаемся к предыдущей панели (если есть KnowledgeBaseUIManager)
            KnowledgeBaseUIManager knowledgeManager = FindFirstObjectByType<KnowledgeBaseUIManager>();
            if (knowledgeManager != null)
            {
                knowledgeManager.ShowTestList();
                Debug.Log("Возвращаемся к списку тестов после сохранения");
            }
        });
    }
}
