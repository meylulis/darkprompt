using UnityEngine;
using UnityEngine.UI;

public class CloseButtonAdder : MonoBehaviour
{
    public GameObject closeButtonPrefab; // ← Префаб крестика

    public void AddCloseButtonTo(GameObject panel)
    {
        // Если уже есть — не добавляем
        if (panel.transform.Find("CloseButton") != null)
            return;

        GameObject closeBtn = Instantiate(closeButtonPrefab, panel.transform);
        closeBtn.name = "CloseButton";

        RectTransform rt = closeBtn.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-10, -10); // чуть внутрь

        // Добавление OnClick программно
        Button btn = closeBtn.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => panel.SetActive(false));
        }
    }
}
