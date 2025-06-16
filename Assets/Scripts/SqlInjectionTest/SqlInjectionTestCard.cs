using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SQLInjectionTestCard : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private TextMeshProUGUI _tmpQuery;
    [SerializeField] private Image _background;

    public bool Checked => _toggle.isOn;
    public string Query => _tmpQuery.text;

    public void Init(string query)
    {
        _tmpQuery.text = query;
    }

    public void SetColor(bool correct)
    {
        _background.color = correct ? Color.green : Color.red;
    }
}
