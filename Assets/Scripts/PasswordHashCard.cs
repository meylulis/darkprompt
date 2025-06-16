using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PasswordHashCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hashText;
    [SerializeField] private TMP_Dropdown _dropdown;

    public string Hash => _hashText.text;
    public string SelectedPassword => _dropdown.options[_dropdown.value].text;

    public void Init(string hash, List<string> passwordOptions)
    {
        _hashText.text = hash;
        _dropdown.ClearOptions();
        _dropdown.AddOptions(passwordOptions);
    }
}
