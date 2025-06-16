using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingTestCard : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private TextMeshProUGUI _tmpLink;
    [SerializeField] private Image _background;
    public bool Checked => _toggle.isOn;
    public string Link => _tmpLink.text;
    public void Init(string link)
    {
        _tmpLink.text = link;
    }
    public void SetColor(bool correct)
    {
        _background.color = correct ? Color.green : Color.red;
    }
}
