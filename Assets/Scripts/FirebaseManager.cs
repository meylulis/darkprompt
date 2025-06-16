using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;
using System;

public static class FirebaseManager
{
    private static DatabaseReference dbReference = FirebaseDatabase.DefaultInstance.RootReference;

    public static void SaveTestToFirebase(string testTitle, List<QuestionData> questions, Action onComplete = null)
    {
        string testId = dbReference.Child("tests").Push().Key;

        Dictionary<string, object> testData = new Dictionary<string, object>();
        testData["title"] = testTitle;

        Dictionary<string, object> questionDict = new Dictionary<string, object>();
        for (int i = 0; i < questions.Count; i++)
        {
            QuestionData q = questions[i];
            Dictionary<string, object> qData = new Dictionary<string, object>();
            qData["question"] = q.question;
            qData["answers"] = q.answers;
            qData["correctAnswerIndex"] = q.correctAnswerIndex;
            questionDict[$"question_{i}"] = qData;
        }

        testData["questions"] = questionDict;

        dbReference.Child("tests").Child(testId).SetValueAsync(testData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Test saved to Firebase!");
                onComplete?.Invoke();
            }
            else
                Debug.LogError("Error saving test: " + task.Exception);
        });
    }
}
