using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    public void Close()
    {
        if (PanelManager.Instance != null)
            PanelManager.Instance.HideAll();
        else
            Debug.LogError("[ClosePanel] PanelManager.Instance не найден!");
    }
}
