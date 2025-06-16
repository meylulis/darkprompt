using UnityEngine;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Tooltip("Сюда добавь все панели, которые должны управляться этим менеджером (например, панель миссий, панель тестов, панель добавления теста и т.д.). Убедитесь, что все панели, которые вы хотите открывать и закрывать, находятся в этом списке.")]
    public List<GameObject> allPanels;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowOnly(GameObject panelToShow)
    {
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(panel == panelToShow);
        }
    }

    public void HideAll()
    {
        foreach (var panel in allPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }
}
