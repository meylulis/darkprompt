using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class RoleInitializer : MonoBehaviour
{
    public static bool isExpert = false;
    public static event Action OnRoleLoaded; // событие

    void Start()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(user.UserId);
        dbRef.Child("role").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string role = task.Result.Value.ToString();
                isExpert = (role == "expert");

                Debug.Log("Роль загружена: " + role);
                OnRoleLoaded?.Invoke(); // уведомляем, что роль готова
            }
        });
    }
}
