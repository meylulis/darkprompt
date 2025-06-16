using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions; // ВАЖНО: для ContinueWithOnMainThread

public class TestPanelManager : MonoBehaviour
{
    public Button addTestButton;
    public GameObject addTestPanel;

    void Start()
    {
        addTestButton.gameObject.SetActive(false); // скрываем до проверки роли
        CheckUserRole();
    }

    void CheckUserRole()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        FirebaseDatabase.DefaultInstance
            .GetReference("users")
            .Child(user.UserId)
            .Child("role")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogWarning("Ошибка при получении роли из БД");
                    return;
                }

                DataSnapshot snapshot = task.Result;
                string role = snapshot.Value?.ToString();

                if (role == "expert")
                {
                    addTestButton.gameObject.SetActive(true);
                    addTestButton.onClick.AddListener(ShowAddTestPanel);
                }
            });
    }

    void ShowAddTestPanel()
    {
        if (PanelManager.Instance != null)
            PanelManager.Instance.ShowOnly(addTestPanel);
        else
            Debug.LogError("[TestPanelManager] PanelManager.Instance не найден!");
    }
}
