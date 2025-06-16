using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PassQuestionUI : MonoBehaviour
{
    public TMP_Text questionText;
    public ToggleGroup toggleGroup;
    public List<Toggle> answerToggles;

    private int correctIndex;

    public void Setup(string question, List<string> answers, int correct)
    {
        questionText.text = question;
        correctIndex = correct;

        for (int i = 0; i < answerToggles.Count; i++)
        {
            Toggle toggle = answerToggles[i];
            if (toggle == null) continue;

            if (i < answers.Count)
            {
                toggle.gameObject.SetActive(true);
                
                Text answerText = toggle.GetComponentInChildren<Text>(true);
                if (answerText != null)
                {
                    answerText.text = answers[i];
                }
            }
            else
            {
                toggle.gameObject.SetActive(false);
            }
        }
    }

    public bool IsCorrect()
    {
        if (correctIndex >= 0 && correctIndex < answerToggles.Count && answerToggles[correctIndex] != null)
        {
            return answerToggles[correctIndex].isOn;
        }
        return false;
    }
}
