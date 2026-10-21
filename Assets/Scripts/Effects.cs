using UnityEngine;
using TMPro;

// Effects = Bộ công cụ hiệu ứng hình ảnh dùng chung (static, không gắn vào GameObject).
// Gồm: tải sprite từ thư mục Resources, tạo nền, tạo vụ nổ hạt sáng, tạo chữ bay.
public static class Effects
{
    // Kích thước chuẩn: 16 pixel ảnh = 1 đơn vị Unity (giữ nét pixel art đồng đều)
    private const float PixelsPerUnit = 16f;

    // Tải 1 sprite trong thư mục Assets/Resources/Sprites theo tên file (không cần đuôi .png).
    // Đặt chế độ lọc Point để ảnh pixel art phóng to vẫn sắc nét, không bị nhòe.
    public static Sprite LoadSprite(string name)
    {
        Sprite s = Resources.Load<Sprite>("Sprites/" + name);
        if (s == null)
        {
            Debug.LogWarning("Không tìm thấy sprite: Resources/Sprites/" + name);
            return null;
        }
        s.texture.filterMode = FilterMode.Point;
        return s;
    }

    // Hệ số phóng to để sprite có đúng kích thước "16 pixel = 1 đơn vị"
    // (phòng trường hợp Unity nhập ảnh với thông số Pixels Per Unit khác)
    public static float ScaleFor(Sprite s)
    {
        return s.pixelsPerUnit / PixelsPerUnit;
    }

    // Tạo ảnh nền trời đêm phủ kín camera, nằm sau mọi thứ
    public static void CreateBackground()
    {
        Sprite bg = LoadSprite("Background");
        if (bg == null) return;

        GameObject obj = new GameObject("Background");
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = bg;
        sr.sortingOrder = -100; // Số càng nhỏ càng nằm phía sau

        // Co giãn ảnh nền cho vừa khít khung nhìn của camera
        Camera cam = Camera.main;
        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        Vector2 size = bg.bounds.size;
        float scale = Mathf.Max(viewWidth / size.x, viewHeight / size.y);
        obj.transform.localScale = new Vector3(scale, scale, 1f);
        obj.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }

    // Vụ nổ: bắn ra "count" hạt sáng theo vòng tròn
    public static void SpawnExplosion(Vector3 position, Color color, int count = 12)
    {
        Sprite spark = LoadSprite("Spark");
        if (spark == null) return;

        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("Spark");
            p.transform.position = position;
            p.transform.localScale = Vector3.one * ScaleFor(spark);

            SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = spark;
            sr.color = color;
            sr.sortingOrder = 20;

            // Góc bay chia đều quanh vòng tròn, tốc độ hơi ngẫu nhiên cho tự nhiên
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            float speed = Random.Range(2f, 4f);

            FloatingEffect fx = p.AddComponent<FloatingEffect>();
            fx.velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * speed;
            fx.gravity = 4f;
            fx.lifetime = 0.6f;
        }
    }

    // Chữ bay lên rồi mờ dần (ví dụ "+50")
    public static void SpawnFloatingText(Vector3 position, string message, Color color)
    {
        GameObject obj = new GameObject("FloatingText");
        obj.transform.position = position;

        TextMeshPro tmp = obj.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 5;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.rectTransform.sizeDelta = new Vector2(12f, 2f); // [SỬA] Rộng hơn để chứa câu thông báo dài
        tmp.outlineWidth = 0.25f;                            // [MỚI] Viền đen cho dễ đọc
        tmp.outlineColor = new Color32(0, 0, 0, 255);
        tmp.GetComponent<Renderer>().sortingOrder = 30;

        FloatingEffect fx = obj.AddComponent<FloatingEffect>();
        fx.velocity = new Vector3(0f, 1.5f, 0f); // Bay thẳng lên
        fx.lifetime = 0.9f;
    }
}
