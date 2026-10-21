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
