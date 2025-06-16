using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestionUI : MonoBehaviour
{
    public TMP_InputField questionInput;
    public List<TMP_InputField> answerInputs;
    public List<Toggle> answerToggles;
    public Button deleteButton;

    private void Start()
    {
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelf);
        }
    }

    public QuestionData GetQuestionData()
    {
        string questionText = questionInput.text;
        List<string> answers = new List<string>();
        int correctIndex = -1;

        for (int i = 0; i < answerInputs.Count; i++)
        {
            answers.Add(answerInputs[i].text);
            if (answerToggles[i].isOn)
                correctIndex = i;
        }

        return new QuestionData
        {
            question = questionText,
            answers = answers,
            correctAnswerIndex = correctIndex
        };
    }

    private void DeleteSelf()
    {
        Destroy(gameObject);
    }
}
