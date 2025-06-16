using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordCrackPanel : MonoBehaviour
{
    [SerializeField] private PasswordHashCard _cardPrefab;
    [SerializeField] private Transform _answersPanel;
    [SerializeField] private Button _checkButton;
    [SerializeField] private TextMeshProUGUI _resultTmp;

    private List<PasswordHashCard> _spawnedCards;
    private Dictionary<string, string> _hashToPasswordMap = new();

    // Поля для интеграции с системой миссий
    private string currentMissionId;
    private string currentMissionDifficulty;
    private bool missionCompleted = false;

    public void Setup(string missionId, string difficulty)
    {
        currentMissionId = missionId;
        currentMissionDifficulty = difficulty;
        missionCompleted = false; // Сбрасываем флаг при новой настройке
    }

    private void OnEnable()
    {
        _resultTmp.gameObject.SetActive(false);
        
        if (_spawnedCards != null)
            foreach (var card in _spawnedCards)
                Destroy(card.gameObject);

        _spawnedCards = new();

        var passwords = new List<string> { "qwerty", "123456", "admin", "letmein", "welcome" };
        _hashToPasswordMap.Clear();

        foreach (var pwd in passwords)
        {
            var hash = CalculateMD5(pwd);
            _hashToPasswordMap[hash] = pwd;
        }

        foreach (var pair in _hashToPasswordMap)
        {
            var card = Instantiate(_cardPrefab, _answersPanel);
            card.Init(pair.Key, passwords); // Передаём хэш и варианты
            _spawnedCards.Add(card);
        }
    }

    public void CheckAnswers()
    {
        _resultTmp.gameObject.SetActive(true);
        bool allCorrect = true;

        foreach (var card in _spawnedCards)
        {
            var correctPassword = _hashToPasswordMap[card.Hash];
            if (card.SelectedPassword != correctPassword)
            {
                allCorrect = false;
                break; // Если нашли ошибку, дальше можно не проверять
            }
        }

        if (allCorrect)
        {
            _resultTmp.text = "✅ Всё верно!";
            if (!missionCompleted && !string.IsNullOrEmpty(currentMissionId))
            {
                PassTestManager.Instance?.CompleteMission(currentMissionId, currentMissionDifficulty);
                missionCompleted = true;
            }
        }
        else
        {
            _resultTmp.text = "❌ Есть ошибки.";
        }
    }

    private string CalculateMD5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
