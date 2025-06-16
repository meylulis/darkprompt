using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroll : MonoBehaviour
{
    public float speed = 0.1f;
    private RawImage image;

    void Start()
    {
        image = GetComponent<RawImage>();

        // Убедись, что тайлинг больше 1, чтобы был эффект повторения
        image.uvRect = new Rect(0, 0, 1, 1);
    }

    void Update()
    {
        Rect uv = image.uvRect;
        uv.y += speed * Time.deltaTime;

        // Обрезаем значение, чтобы оно не стало слишком большим
        if (uv.y > 1f) uv.y -= 1f;
        image.uvRect = uv;
    }
}
