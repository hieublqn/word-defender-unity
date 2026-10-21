using System.Collections.Generic;
using UnityEngine;
using TMPro; // Thư viện TextMeshPro: dùng để hiển thị chữ đẹp, rõ nét trong Unity

// Enemy = Quái. Script này gắn vào GameObject "Enemy".
// Nhiệm vụ: hiển thị 1 từ tiếng Anh trên đầu quái, đi về phía Tháp,
// và đổi màu các chữ cái đã được người chơi gõ đúng.
public class Enemy : MonoBehaviour
{
    // [MỚI] Danh sách TẤT CẢ quái đang có trên màn hình.
    // "static" = dùng chung cho mọi con quái, để TypingManager tìm quái nhanh.
    public static List<Enemy> All = new List<Enemy>();

    [Header("Từ vựng của quái")]
    public string word = "apple";        // Từ tiếng Anh quái mang theo
    public string meaning = "quả táo";   // Nghĩa tiếng Việt (dùng cho màn Game Over sau này)

    [Header("Di chuyển")]
    public float speed = 1f;                              // Tốc độ đi (đơn vị Unity mỗi giây)
    public Vector2 targetPosition = new Vector2(0f, -4f); // Vị trí Tháp (giữa, phía dưới màn hình)

    private TMP_Text wordLabel; // Ô chữ hiển thị trên đầu quái
    private int typedCount = 0; // [MỚI] Số chữ cái người chơi đã gõ đúng

    // [MỚI] Khi quái xuất hiện -> thêm vào danh sách; khi biến mất -> xóa khỏi danh sách
    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Awake()
    {
        // Tìm component chữ (TextMeshPro) nằm trong các GameObject con của quái
        wordLabel = GetComponentInChildren<TMP_Text>();
        word = word.ToLower(); // Đưa về chữ thường để so sánh cho dễ

        // [MỚI] Căn chữ nằm GIỮA, ngay sát phía trên con quái
        if (wordLabel != null)
        {
            wordLabel.alignment = TextAlignmentOptions.Center;                 // Căn giữa ngang + dọc
            wordLabel.rectTransform.localPosition = new Vector3(0f, 0.8f, 0f); // Cao hơn tâm quái 0.8 đơn vị
        }
    }

    void Start()
    {
        UpdateLabel();
    }

    void Update()
    {
        // Di chuyển quái tiến dần tới Tháp
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, speed * Time.deltaTime);

        // Nếu quái đã chạm Tháp
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            // [MỚI] Báo cho Tháp biết để trừ tim và ghi lại từ bị lọt
            if (Tower.Instance != null)
            {
                Tower.Instance.TakeDamage(word, meaning);
            }
            Destroy(gameObject);
        }
    }

    // ===== [MỚI] Các hàm để TypingManager gọi =====

    // Chữ cái tiếp theo cần gõ (ví dụ "apple" đã gõ "ap" -> trả về 'p')
    public char NextLetter()
    {
        return word[typedCount];
    }

    // Người chơi gõ đúng thêm 1 chữ
    public void TypeLetter()
    {
        typedCount++;
        UpdateLabel();
    }

    // Đã gõ xong cả từ chưa?
    public bool IsComplete()
    {
        return typedCount >= word.Length;
    }

    // Bỏ khóa mục tiêu: xóa tiến độ gõ, chữ trở lại màu trắng
    public void ResetTyping()
    {
        typedCount = 0;
        UpdateLabel();
    }

    // Quái bị tiêu diệt
    public void Die()
    {
        Debug.Log("Tiêu diệt quái '" + word + "' (" + meaning + ")");
        Destroy(gameObject);
    }

    // Vẽ lại chữ: phần đã gõ màu vàng, phần chưa gõ màu trắng.
    // <color=#FFD700>...</color> là "rich text" của TextMeshPro để tô màu một đoạn chữ.
    private void UpdateLabel()
    {
        if (wordLabel == null) return;
        string typed = word.Substring(0, typedCount);
        string rest = word.Substring(typedCount);
        wordLabel.text = "<color=#FFD700>" + typed + "</color>" + rest;
    }
}
