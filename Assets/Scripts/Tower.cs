using System.Collections.Generic;
using UnityEngine;

// Tower = Tháp Tri Thức. Gắn vào GameObject "Tower" ở giữa, phía dưới màn hình.
// Nhiệm vụ: giữ số tim, trừ tim khi quái chạm vào, ghi lại các từ bị lọt.
public class Tower : MonoBehaviour
{
    // "Instance" giúp các script khác gọi Tháp nhanh: Tower.Instance.TakeDamage(...)
    // (Kỹ thuật này gọi là Singleton: cả game chỉ có đúng 1 Tháp)
    public static Tower Instance;

    public int maxHearts = 3;  // Số tim ban đầu
    public int hearts;         // Số tim hiện tại

    // Danh sách từ bị lọt, dạng "apple - quả táo" (dùng cho màn Game Over)
    public List<string> missedWords = new List<string>();

    void Awake()
    {
        Instance = this;
        hearts = maxHearts;
        SetupVisual(); // [MỚI - ĐỒ HỌA]
    }

    // [MỚI - ĐỒ HỌA] Thay hình chữ nhật xanh bằng hình Tháp Tri Thức pixel art
    private void SetupVisual()
    {
        Sprite towerSprite = Effects.LoadSprite("Tower");
        if (towerSprite == null) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = towerSprite;
        sr.color = Color.white;   // Bỏ màu xanh cũ, giữ đúng màu gốc của ảnh
        sr.sortingOrder = 1;

        transform.localScale = Vector3.one * Effects.ScaleFor(towerSprite);

        // Đặt chân tháp sát mép dưới màn hình (camera nhìn thấy từ y = -5 đến y = 5)
        float height = sr.bounds.size.y;
        transform.position = new Vector3(0f, -5f + height / 2f, 0f);
    }

    // Quái gọi hàm này khi chạm vào Tháp
    public void TakeDamage(string word, string meaning)
    {
        hearts--;
        missedWords.Add(word + " - " + meaning);
        Debug.Log("Tháp bị tấn công bởi '" + word + "'! Còn " + hearts + " tim.");

        if (hearts <= 0)
        {
            Debug.Log("GAME OVER! Các từ cần ôn lại: " + string.Join(", ", missedWords));
            Time.timeScale = 0f; // Dừng toàn bộ chuyển động trong game
        }
    }
}
