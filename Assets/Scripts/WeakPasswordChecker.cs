using TMPro;
using UnityEngine;

public class WeakPasswordChecker : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public TMP_Text tooShortText, noNumbersText, noLettersText, noSpecialsText, popularPasswordText, resultText;
    
    // Поля для интеграции с системой миссий
    private string currentMissionId;
    private string currentMissionDifficulty;
    private bool missionCompleted = false; // Флаг, чтобы избежать повторного вызова

    private string[] popularPasswords = { "123456", "password", "qwerty", "12345678" };

    public void SetupMission(string missionId, string difficulty)
    {
        currentMissionId = missionId;
        currentMissionDifficulty = difficulty;
        missionCompleted = false; // Сбрасываем флаг при новой настройке

        // Сбрасываем UI при новой настройке
        passwordInput.text = "";
        tooShortText.gameObject.SetActive(false);
        noNumbersText.gameObject.SetActive(false);
        noLettersText.gameObject.SetActive(false);
        noSpecialsText.gameObject.SetActive(false);
        popularPasswordText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
    }

    public void CheckPassword()
    {
        string password = passwordInput.text;
        bool isStrong = true;

        // Сброс всех подсказок
        tooShortText.gameObject.SetActive(false);
        noNumbersText.gameObject.SetActive(false);
        noLettersText.gameObject.SetActive(false);
        noSpecialsText.gameObject.SetActive(false);
        popularPasswordText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);

        // Проверка длины
        if (password.Length < 8)
        {
            tooShortText.gameObject.SetActive(true);
            isStrong = false;
        }

        // Проверка на буквы
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[a-zA-Z]"))
        {
            noLettersText.gameObject.SetActive(true);
            isStrong = false;
        }

        // Проверка на цифры
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
        {
            noNumbersText.gameObject.SetActive(true);
            isStrong = false;
        }

        // Проверка на спецсимволы
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            noSpecialsText.gameObject.SetActive(true);
            isStrong = false;
        }

        // Проверка на популярный пароль
        foreach (var pop in popularPasswords)
        {
            if (password.ToLower() == pop)
            {
                popularPasswordText.gameObject.SetActive(true);
                isStrong = false;
                break;
            }
        }

        // Показываем финальный результат
        resultText.gameObject.SetActive(true);
        resultText.text = isStrong ? "✅ Надёжный пароль!" : "❌ Слабый пароль!";
        resultText.color = isStrong ? Color.green : Color.red;

        // Если пароль надежный и миссия еще не была завершена
        if (isStrong && !missionCompleted && !string.IsNullOrEmpty(currentMissionId))
        {
            if (PassTestManager.Instance != null)
            {
                Debug.Log($"[WeakPasswordChecker] Пароль надежный. Завершаю миссию {currentMissionId}.");
                PassTestManager.Instance.CompleteMission(currentMissionId, currentMissionDifficulty);
                missionCompleted = true; // Помечаем, чтобы не вызывать снова
            }
            else
            {
                Debug.LogError("[WeakPasswordChecker] PassTestManager.Instance не найден!");
            }
        }
    }
}
