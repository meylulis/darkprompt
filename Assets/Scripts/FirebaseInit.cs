using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("✅ Firebase успешно инициализирован!");
            }
            else
            {
                Debug.LogError("❌ Firebase не может быть инициализирован: " + dependencyStatus);
            }
        });
    }
}
