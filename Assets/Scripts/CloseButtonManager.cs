using UnityEngine;

public class CloseButtonManager : MonoBehaviour
{
    public static CloseButtonManager Instance { get; private set; }

    public GameObject closeButtonPrefab; // Префаб крестика

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Менеджер не исчезает при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AttachCloseButton(GameObject panel, GameObject panelToReturnTo = null)
    {
        // Не добавляем повторно, если кнопка уже есть
        // Проверяем по имени, чтобы избежать дублирования
        string buttonName = closeButtonPrefab.name + "(Clone)";
        if (panel.transform.Find(buttonName) != null)
        {
            return;
        }

        GameObject btnGo = Instantiate(closeButtonPrefab, panel.transform);
        btnGo.name = buttonName; // Даем клону предсказуемое имя
        
        // Добавляем обработчик нажатия
        var buttonComponent = btnGo.GetComponent<UnityEngine.UI.Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => {
                if (PanelManager.Instance != null)
                {
                    if (panelToReturnTo != null)
                    {
                        // Если указана панель для возврата - показываем ее
                        PanelManager.Instance.ShowOnly(panelToReturnTo);
                    }
                    else
                    {
                        // Иначе - просто скрываем все
                        PanelManager.Instance.HideAll();
                    }
                }
            });
        }

        // --- Позиционирование кнопки в правом верхнем углу ---
        RectTransform rt = btnGo.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(1, 1); // Правый
            rt.anchorMax = new Vector2(1, 1); // Верхний
            rt.pivot = new Vector2(1, 1);     // Угол
            rt.anchoredPosition = new Vector2(-20, -20); // Небольшой отступ
        }
    }
}
