using UnityEngine;
using UnityEngine.UI;

// Этот скрипт можно повесить на любую кнопку для навигации между панелями.
public class NavigationButton : MonoBehaviour
{
    [Header("Панель для открытия")]
    [Tooltip("Какую панель нужно показать при нажатии на эту кнопку.")]
    public GameObject panelToShow;

    [Header("Панель для закрытия (необязательно)")]
    [Tooltip("Какую панель нужно скрыть. Если не указано, будет скрыт родительский объект кнопки.")]
    public GameObject panelToHide;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Navigate);
        }
        else
        {
            Debug.LogError("[NavigationButton] Компонент Button не найден на этом объекте!", gameObject);
        }
    }

    private void Navigate()
    {
        if (panelToHide != null)
        {
            panelToHide.SetActive(false);
        }
        else
        {
            // Если панель для скрытия не указана, пытаемся скрыть родителя
            if (transform.parent != null && transform.parent.gameObject != null)
            {
                transform.parent.gameObject.SetActive(false);
            }
        }

        if (panelToShow != null)
        {
            if (PanelManager.Instance != null)
            {
                // Используем PanelManager, чтобы корректно обработать другие панели
                PanelManager.Instance.ShowOnly(panelToShow);
            }
            else
            {
                // Фоллбэк, если PanelManager не используется
                panelToShow.SetActive(true);
            }
        }
        else
        {
             // Если панель для показа не указана, значит, это просто кнопка "Закрыть всё"
             if (PanelManager.Instance != null)
             {
                PanelManager.Instance.HideAll();
             }
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(Navigate);
        }
    }
} 