using UnityEngine;

public class SidePanelController : MonoBehaviour
{
    public RectTransform panel;
    public Transform arrowIcon;

    private bool isVisible = true;
    private float hiddenX = -600f;  // подставь свою ширину
    private float visibleX = 0f;

    public void TogglePanel()
    {
        Debug.Log("Кнопка работает — TogglePanel вызван!");
        isVisible = !isVisible;

        // смещаем панель
        panel.anchoredPosition = new Vector2(isVisible ? visibleX : hiddenX, 
            panel.anchoredPosition.y
        );

        // поворачиваем стрелку
        if (arrowIcon != null)
        {
            float z = isVisible ? 0 : 180; 
            arrowIcon.rotation = Quaternion.Euler(0, 0, z);
        }

        Debug.Log("TogglePanel: isVisible=" + isVisible);
    }
}
