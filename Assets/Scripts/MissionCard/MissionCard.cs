using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText, _difficultyText;
    [SerializeField] private Button _button;
    public TextMeshProUGUI TitleText => _titleText;
    public TextMeshProUGUI DifficultyText => _difficultyText;
    public Button Button => _button;

    // Добавляем поля для хранения данных миссии
    public string MissionId { get; set; }
    public string MissionTitle { get; set; }
    public string MissionType { get; set; }
    public string MissionDifficulty { get; set; }
}
