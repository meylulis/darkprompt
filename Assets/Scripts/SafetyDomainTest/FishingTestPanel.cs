using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingTestPanel : MonoBehaviour
{
    [SerializeField] private FishingTestCard _cardPrefab;
    [SerializeField] private SafetyDomainTester _domainTester;
    [SerializeField] private Button _checkURLsButton;
    [SerializeField] private TextMeshProUGUI _resultTmp;
    private List<FishingTestCard> _spawnedCards;

    // Поля для интеграции с системой миссий
    private string currentMissionId;
    private string currentMissionDifficulty;
    private bool missionCompleted = false; // Флаг, чтобы избежать повторного вызова

    public void Setup(string missionId, string difficulty)
    {
        currentMissionId = missionId;
        currentMissionDifficulty = difficulty;
        missionCompleted = false; // Сбрасываем флаг при новой настройке
        Debug.Log($"[FishingTestPanel] Настроена миссия: ID={missionId}, сложность={difficulty}");
    }

    private void OnEnable()
    {
        // Сбрасываем текст результата при каждом открытии
        if (_resultTmp != null)
        {
            _resultTmp.gameObject.SetActive(false);
        }

        if(_spawnedCards != null)
        {
            foreach(var card in _spawnedCards)
            {
                Destroy(card.gameObject);
            }
        }
        _spawnedCards = new List<FishingTestCard>();
        var urls = _domainTester.GetRandomUrls(4);
        for (int i = 0; i < 4; i++)
        {
            var card = Instantiate(_cardPrefab, transform);
            card.Init(urls[i]);
            _spawnedCards.Add(card);
        }
    }
    
    public void CheckAllCards()
    {
        if (_resultTmp != null)
        {
            _resultTmp.gameObject.SetActive(true);
        }

        int totalPhishingLinks = _spawnedCards.Count(c => _domainTester.IsPhishingLink(c.Link));
        int foundPhishingLinks = _spawnedCards.Count(c => _domainTester.IsPhishingLink(c.Link) && c.Checked);
        int incorrectlyMarked = _spawnedCards.Count(c => !_domainTester.IsPhishingLink(c.Link) && c.Checked);
        
        Debug.Log($"[FishingTestPanel] Всего фишинговых: {totalPhishingLinks}, найдено: {foundPhishingLinks}, ошибочно отмечено: {incorrectlyMarked}");

        bool allCorrect = (foundPhishingLinks == totalPhishingLinks) && (incorrectlyMarked == 0);

        foreach (var card in _spawnedCards)
        {
            bool isPhishing = _domainTester.IsPhishingLink(card.Link);
            bool isCorrectlyMarked = (isPhishing && card.Checked) || (!isPhishing && !card.Checked);
            card.SetColor(isCorrectlyMarked);
        }
        
        if (allCorrect)
        {
            _resultTmp.text = "Всё верно!";

            if (!missionCompleted && !string.IsNullOrEmpty(currentMissionId))
            {
                Debug.Log($"[FishingTestPanel] Миссия {currentMissionId} успешно пройдена!");
                
                PassTestManager passTestManager = FindFirstObjectByType<PassTestManager>();
                if (passTestManager != null)
                {
                    passTestManager.CompleteMission(currentMissionId, currentMissionDifficulty);
                    missionCompleted = true;
                }
                else
                {
                    Debug.LogError("[FishingTestPanel] PassTestManager не найден на сцене!");
                }
            }
        }
        else
        {
            string resultString = $"❌ Некорректный выбор.\n";
            resultString += $"Найдено фишинговых ссылок: {foundPhishingLinks} из {totalPhishingLinks}.\n";
            if (incorrectlyMarked > 0)
            {
                resultString += $"Ошибочно отмечено безопасных ссылок: {incorrectlyMarked}.";
            }
            _resultTmp.text = resultString;
        }
    }
}
