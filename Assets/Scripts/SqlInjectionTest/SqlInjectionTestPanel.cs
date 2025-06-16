using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SQLInjectionTestPanel : MonoBehaviour
{
    [SerializeField] private SQLInjectionTestCard _cardPrefab;
    [SerializeField] private Transform _cardsContainer;
    [SerializeField] private SQLInjectionTester _queryTester;
    [SerializeField] private Button _checkButton;
    [SerializeField] private TextMeshProUGUI _resultTmp;

    private List<SQLInjectionTestCard> _spawnedCards;

    // Поля для интеграции с системой миссий
    private string currentMissionId;
    private string currentMissionDifficulty;
    private bool missionCompleted = false;
    private void Awake()
    {
        // Базовые проверки на null
        if (_cardPrefab == null)
            Debug.LogError("[SQLInjectionTestPanel] Префаб карточки не назначен в инспекторе!");

        if (_cardsContainer == null)
            _cardsContainer = transform;

        if (_queryTester == null)
            Debug.LogError("[SQLInjectionTestPanel] Компонент SQLInjectionTester не назначен в инспекторе!");

        if (_checkButton == null)
            Debug.LogError("[SQLInjectionTestPanel] Кнопка проверки не назначена в инспекторе!");

        if (_resultTmp == null)
            Debug.LogWarning("[SQLInjectionTestPanel] Текстовое поле для результата не назначено в инспекторе!");
    }

    // Метод для настройки миссии - вызывается из MissionsLoader
    public void Setup(string missionId, string difficulty)
    {
        currentMissionId = missionId;
        currentMissionDifficulty = difficulty;
        missionCompleted = false; // Сбрасываем флаг при новой настройке
        Debug.Log($"[SQLInjectionTestPanel] Настроена миссия: ID={missionId}, сложность={difficulty}");
    }

    private void OnEnable()
    {
        Debug.Log("[SQLInjectionTestPanel] Панель активирована, создаем карточки");

        // Сбрасываем текст результата при каждом открытии
        if (_resultTmp != null)
        {
            _resultTmp.gameObject.SetActive(false);
        }

        // Очистка старых карточек
        if (_spawnedCards != null)
        {
            foreach (var card in _spawnedCards)
                if (card != null) Destroy(card.gameObject);
        }

        _spawnedCards = new List<SQLInjectionTestCard>();

        // Получаем случайные запросы (3 запроса)
        var queries = _queryTester.GetRandomQueries(3);
        Debug.Log($"[SQLInjectionTestPanel] Получено {queries.Count} запросов для теста");

        // Создаем карточки
        for (int i = 0; i < queries.Count; i++)
        {
            var card = Instantiate(_cardPrefab, _cardsContainer);
            card.Init(queries[i]);
            _spawnedCards.Add(card);

            // Позиционирование карточки (если нет Layout Group)
            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Вычисляем позицию карточки (каждая следующая ниже предыдущей)
                float cardHeight = rt.rect.height;
                rt.anchoredPosition = new Vector2(0, -i * (cardHeight + 10));
            }

            Debug.Log($"[SQLInjectionTestPanel] Создана карточка {i + 1}: {queries[i]}");
        }
    }

    public void CheckAllCards()
    {
        if (_spawnedCards == null || _spawnedCards.Count == 0)
        {
            Debug.LogError("[SQLInjectionTestPanel] Нет карточек для проверки!");
            return;
        }

        // Показываем результат
        if (_resultTmp != null)
            _resultTmp.gameObject.SetActive(true);

        // 1. Считаем общее количество уязвимых запросов
        int totalUnsafeQueries = _spawnedCards.Count(c => _queryTester.IsQueryUnsafe(c.Query));

        // 2. Считаем количество правильно выбранных уязвимых запросов
        int foundUnsafeQueries = _spawnedCards.Count(c => _queryTester.IsQueryUnsafe(c.Query) && c.Checked);

        // 3. Считаем количество неправильно выбранных безопасных запросов
        int incorrectlyMarked = _spawnedCards.Count(c => !_queryTester.IsQueryUnsafe(c.Query) && c.Checked);

        Debug.Log($"[SQLInjectionTestPanel] Всего уязвимых: {totalUnsafeQueries}, найдено: {foundUnsafeQueries}, ошибочно отмечено: {incorrectlyMarked}");

        // Условие победы: найдены ВСЕ уязвимые запросы И не отмечено ни одного безопасного
        bool allCorrect = (foundUnsafeQueries == totalUnsafeQueries) && (incorrectlyMarked == 0);

        // Показываем цветовую индикацию результатов
        foreach (var card in _spawnedCards)
        {
            bool isUnsafe = _queryTester.IsQueryUnsafe(card.Query);
            bool isCorrectlyMarked = (isUnsafe && card.Checked) || (!isUnsafe && !card.Checked);
            card.SetColor(isCorrectlyMarked);
        }

        // Формируем сообщение с результатом
        if (allCorrect)
        {
            _resultTmp.text = "✅ Всё верно! Вы правильно определили все уязвимые запросы.";

            // Если тест пройден правильно и миссия еще не была завершена
            if (!missionCompleted && !string.IsNullOrEmpty(currentMissionId))
            {
                Debug.Log($"[SQLInjectionTestPanel] Миссия {currentMissionId} успешно пройдена!");

                // Вызываем завершение миссии и начисление опыта
                PassTestManager passTestManager = FindFirstObjectByType<PassTestManager>();
                if (passTestManager != null)
                {
                    passTestManager.CompleteMission(currentMissionId, currentMissionDifficulty);
                    missionCompleted = true; // Отмечаем, чтобы не отправлять повторно
                }
                else
                {
                    Debug.LogError("[SQLInjectionTestPanel] PassTestManager не найден на сцене!");
                }
            }
        }
        else
        {
            // Формируем подробное сообщение об ошибке
            string resultString = $"❌ Некорректный выбор.\n";
            resultString += $"Найдено уязвимых запросов: {foundUnsafeQueries} из {totalUnsafeQueries}.\n";
            if (incorrectlyMarked > 0)
            {
                resultString += $"Ошибочно отмечено безопасных запросов: {incorrectlyMarked}.";
            }
            _resultTmp.text = resultString;
        }
    }
}
