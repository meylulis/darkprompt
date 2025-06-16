using UnityEngine;

public class GameSceneUIController : MonoBehaviour
{
    public GameObject xpbar;
    public GameObject activemission;
    public GameObject missionname;
    public GameObject cursortext;
    public GameObject terminaltext;

    void Start()
    {
        RoleInitializer.OnRoleLoaded += CheckAndHideUI;
        CheckAndHideUI(); // на случай, если роль уже загружена
    }

    void CheckAndHideUI()
    {
        if (!RoleInitializer.isExpert) return;

        Debug.Log("Скрываю элементы для эксперта");

        if (xpbar != null) xpbar.SetActive(false);
        if (activemission != null) activemission.SetActive(false);
        if (missionname != null) missionname.SetActive(false);
        if (cursortext != null) cursortext.SetActive(false);
        if (terminaltext != null) terminaltext.SetActive(false);
    }

    void OnDestroy()
    {
        RoleInitializer.OnRoleLoaded -= CheckAndHideUI; // отписываемся
    }
}
