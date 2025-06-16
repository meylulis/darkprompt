using UnityEngine;
using TMPro;

public class BlinkingCursor : MonoBehaviour
{
    public float blinkSpeed = 0.5f;
    private TextMeshProUGUI txt;
    private float timer;
    private bool visible = true;

    void Start()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= blinkSpeed)
        {
            visible = !visible;
            txt.text = visible ? "█" : "";
            timer = 0f;
        }
    }
}
