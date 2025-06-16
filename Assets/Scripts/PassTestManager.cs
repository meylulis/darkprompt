using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

public class PassTestManager : MonoBehaviour
{
    public static PassTestManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject testPanel; // Панель, на которой отображается весь тест
    public TMP_Text testTitleText;
    public Transform questionContainer;
    public GameObject questionPrefab;
    public Button submitButton;
    public GameObject resultPanel;
    public TMP_Text resultText;
    public NavigationButton resultPanelBackButton;

    private List<PassQuestionUI> questionUIs = new List<PassQuestionUI>();
    private string currentTestId;
    private string currentTestDifficulty;
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
        submitButton.onClick.AddListener(SubmitTest);
        auth = FirebaseAuth.DefaultInstance;
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoadTest(string testId, string title, string difficulty, GameObject missionListPanel)
    {
        currentTestId = testId;
        currentTestDifficulty = difficulty;
        testTitleText.text = title;

        // Очистка старых вопросов
        foreach (Transform child in questionContainer)
            Destroy(child.gameObject);
        questionUIs.Clear();

        // Настраиваем кнопку "Назад" на панели результатов, чтобы она вела на список миссий
        if(resultPanelBackButton != null)
        {
            resultPanelBackButton.panelToShow = missionListPanel;
        }

        testPanel.SetActive(true); 

        // --- Улучшение: Скрываем контент до полной загрузки ---
        questionContainer.gameObject.SetActive(false);
        submitButton.gameObject.SetActive(false);
        // ----------------------------------------------------

        FirebaseDatabase.DefaultInstance.GetReference("tests").Child(testId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError($"[PassTestManager] Ошибка загрузки данных теста '{testId}': {task.Exception}");
                    return;
                }

                if (task.IsCompleted)
                {
                    var snapshot = task.Result;

                    if (!snapshot.Exists) {
                        Debug.LogError($"[PassTestManager] Тест с ID '{testId}' не найден в базе данных!");
                        return;
                    }

                    if (!snapshot.HasChild("questions")) {
                        Debug.LogError($"[PassTestManager] В данных теста '{testId}' отсутствует обязательное поле 'questions'!");
                        return;
                    }

                    Debug.Log($"[PassTestManager] Загрузка вопросов для теста '{testId}'...");
                    foreach (var qSnap in snapshot.Child("questions").Children)
                    {
                        string questionText = qSnap.Child("question").Value.ToString();
                        
                        List<string> answers = new List<string>();
                        var answersData = qSnap.Child("answers").Value as List<object>;
                        if (answersData != null)
                        {
                            foreach (var ansObj in answersData)
                            {
                                answers.Add(ansObj.ToString());
                            }
                        }

                        int correctIndex = int.Parse(qSnap.Child("correctAnswerIndex").Value.ToString());

                        GameObject go = Instantiate(questionPrefab, questionContainer);
                        var ui = go.GetComponent<PassQuestionUI>();
                        ui.Setup(questionText, answers, correctIndex);
                        questionUIs.Add(ui);
                    }

                    Debug.Log($"[PassTestManager] Успешно создано {questionUIs.Count} вопросов.");
                    // --- Улучшение: Показываем контент после загрузки ---
                    questionContainer.gameObject.SetActive(true);
                    submitButton.gameObject.SetActive(true);
                    // ------------------------------------------------------
                }
            });
    }

    void SubmitTest()
    {
        int correct = 0;
        foreach (var q in questionUIs)
        {
            if (q.IsCorrect()) correct++;
        }

        resultPanel.SetActive(true);
        resultText.text = $"Правильных ответов: {correct} из {questionUIs.Count}";
        
        // --- Логика начисления опыта и завершения миссии ---
        bool isPerfect = correct == questionUIs.Count && questionUIs.Count > 0;
        
        if (isPerfect)
        {
            resultText.text += "\n<color=green>ИСПЫТАНИЕ ПРОЙДЕНО!</color>";
        }

        // Начисляем опыт и помечаем миссию как пройденную
        AwardXP(isPerfect, currentTestId, currentTestDifficulty);
    }

    public void CompleteMission(string missionId, string difficulty)
    {
        // Этот метод можно вызывать из других типов миссий, которые не являются тестами
        Debug.Log($"[PassTestManager] Завершение миссии '{missionId}' со сложностью '{difficulty}'.");
        AwardXP(true, missionId, difficulty); // Считаем, что не-тестовые миссии всегда проходятся "идеально"
    }

    private void AwardXP(bool wasCompletedSuccessfully, string missionId, string difficulty)
    {
        // Проверяем, была ли миссия уже пройдена РАНЕЕ, чтобы не начислять XP дважды
        dbReference.Child("users").Child(auth.CurrentUser.UserId).Child("completedMissions").Child(missionId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                // Миссия уже была пройдена, опыт не начисляется
                Debug.Log($"[PassTestManager] Миссия {missionId} уже была пройдена ранее. Опыт не начисляется.");
            }
            else
            {
                // Миссия еще не пройдена. Если пройдена успешно, начисляем опыт и сохраняем статус.
                if (wasCompletedSuccessfully)
                {
                    if (XPManager.Instance != null)
                    {
                        int xpGained = 0;
                        switch (difficulty.ToLower())
                        {
                            case "лёгкая": xpGained = 10; break;
                            case "средняя": xpGained = 25; break;
                            case "тяжёлая": xpGained = 50; break;
                            default: 
                                xpGained = 10; // Стандартное значение для тестов или миссий без сложности
                                break; 
                        }

                        if (xpGained > 0)
                        {
                            XPManager.Instance.AddXP(xpGained);
                            resultText.text += $"\nПолучено опыта: {xpGained}";
                            Debug.Log($"[PassTestManager] Начислено {xpGained} опыта за миссию '{missionId}'.");
                        }
                    }
                    // Помечаем миссию как пройденную
                    MarkMissionAsCompleted(missionId);
                }
            }
        });
    }

    private void MarkMissionAsCompleted(string missionId)
    {
        if (auth.CurrentUser == null || string.IsNullOrEmpty(missionId)) return;

        Debug.Log($"[PassTestManager] Попытка сохранить миссию как пройденную. UserID: {auth.CurrentUser.UserId}, MissionID: {missionId}");

        dbReference.Child("users").Child(auth.CurrentUser.UserId).Child("completedMissions").Child(missionId).SetValueAsync(true).ContinueWithOnMainThread(task => {
            if (task.IsCompletedSuccessfully) {
                Debug.Log($"[PassTestManager] ✅ Миссия {missionId} успешно сохранена в Firebase как пройденная.");
                // Сообщаем всей игре, что миссия пройдена!
                GameEvents.MissionCompleted(missionId);
            } else {
                Debug.LogError($"[PassTestManager] ❌ Ошибка сохранения миссии {missionId} в Firebase: {task.Exception}");
            }
        });
    }
}
