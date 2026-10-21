using UnityEngine;
using TMPro;

// FloatingEffect = Hiệu ứng bay rồi mờ dần.
// Dùng cho: hạt sáng khi quái nổ, chữ "+50" bay lên.
// Script này được Effects.cs tự gắn vào bằng code, không cần kéo thả.
public class FloatingEffect : MonoBehaviour
{
    public Vector3 velocity;        // Hướng và tốc độ bay (đơn vị/giây)
    public float lifetime = 0.6f;   // Sống bao lâu thì biến mất (giây)
    public float gravity = 0f;      // Kéo xuống (dùng cho hạt nổ để rơi nhẹ)

    private float age = 0f;
    private SpriteRenderer sprite;  // Nếu là hạt sáng
    private TMP_Text text;          // Nếu là chữ "+50"

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        age += Time.deltaTime;

        // Di chuyển theo vận tốc, có trọng lực kéo xuống
        velocity.y -= gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;

        // Mờ dần theo thời gian: alpha từ 1 về 0
        float alpha = 1f - age / lifetime;
        if (sprite != null) { Color c = sprite.color; c.a = alpha; sprite.color = c; }
        if (text != null)   { Color c = text.color;   c.a = alpha; text.color = c; }

        if (age >= lifetime) Destroy(gameObject);
    }
}
