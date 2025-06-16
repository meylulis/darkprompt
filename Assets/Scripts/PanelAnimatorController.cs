using UnityEngine;

public class PanelAnimatorController : MonoBehaviour
{
    public Animator panelAnimator;   // Animator у панели
    public Transform arrowIcon;      // Transform стрелки

    private bool isOpen = true;

    public void TogglePanel()
    {
        isOpen = !isOpen;
        panelAnimator.SetBool("isOpen", isOpen);
        Debug.Log("TogglePanel called, isOpen=" + isOpen);

        // Поворот стрелки, если нужно
        if (arrowIcon != null)
            arrowIcon.rotation = Quaternion.Euler(0, 0, isOpen ? 180 : 0);
    }
}
