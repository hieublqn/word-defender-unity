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

    // [MỚI - CHỈ ĐỊNH MỤC TIÊU] Con quái đang bị khóa. TypingManager sẽ gán biến này.
    public static Enemy CurrentTarget;

    [Header("Từ vựng của quái")]
    public string word = "apple";        // Từ tiếng Anh quái mang theo
    public string meaning = "quả táo";   // Nghĩa tiếng Việt (dùng cho màn Game Over sau này)

    [Header("Di chuyển")]
    public float speed = 1f;                              // Tốc độ đi (đơn vị Unity mỗi giây)
    public Vector2 targetPosition = new Vector2(0f, -4f); // Vị trí Tháp (giữa, phía dưới màn hình)

    private TMP_Text wordLabel; // Ô chữ hiển thị trên đầu quái
    private int typedCount = 0; // [MỚI] Số chữ cái người chơi đã gõ đúng

    // [MỚI - ĐỒ HỌA] Phần hình ảnh của quái
    private Transform body;          // GameObject con chứa hình con ma (để lắc lư mà không ảnh hưởng đường đi)
    private float baseScale = 1f;    // Kích thước gốc của hình
    private float bobOffset;         // Lệch pha lắc lư, để các con không lắc giống hệt nhau
    private float punch = 0f;        // Độ "phồng" tạm thời khi bị gõ trúng

    // [MỚI - CHỈ ĐỊNH MỤC TIÊU]
    private Transform reticle;       // Khung ngắm vàng bao quanh quái đang bị khóa
    private float wrongTimer = 0f;   // > 0 nghĩa là vừa gõ sai: chữ đỏ + quái rung
    private float labelAlpha = 1f;   // Độ đậm của chữ: 1 = rõ, 0.45 = mờ (khi đang nhắm con khác)

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

        SetupVisual(); // [MỚI - ĐỒ HỌA]
    }

    // [MỚI - ĐỒ HỌA] Thay ô vuông đỏ bằng hình con ma pixel art
    private void SetupVisual()
    {
        Sprite ghost = Effects.LoadSprite("Ghost");
        if (ghost == null) return; // Không có ảnh thì giữ nguyên ô vuông cũ

        // Tắt ô vuông đỏ cũ trên GameObject chính
        SpriteRenderer oldSquare = GetComponent<SpriteRenderer>();
        if (oldSquare != null) oldSquare.enabled = false;

        // Tạo GameObject con "Body" chứa hình con ma
        GameObject bodyObj = new GameObject("Body");
        body = bodyObj.transform;
        body.SetParent(transform, false);
        baseScale = Effects.ScaleFor(ghost);
        body.localScale = Vector3.one * baseScale;

        SpriteRenderer sr = bodyObj.AddComponent<SpriteRenderer>();
        sr.sprite = ghost;
        sr.sortingOrder = 5;

        bobOffset = Random.Range(0f, 10f);

        // [MỚI - CHỈ ĐỊNH MỤC TIÊU] Tạo khung ngắm (ẩn sẵn, chỉ hiện khi quái bị khóa)
        Sprite reticleSprite = Effects.LoadSprite("Reticle");
        if (reticleSprite != null)
        {
            GameObject r = new GameObject("Reticle");
            reticle = r.transform;
            reticle.SetParent(transform, false);
            reticle.localScale = Vector3.one * Effects.ScaleFor(reticleSprite);
            SpriteRenderer rsr = r.AddComponent<SpriteRenderer>();
            rsr.sprite = reticleSprite;
            rsr.sortingOrder = 8;
            r.SetActive(false);
        }

        // Viền đen quanh chữ cho dễ đọc trên nền trời đêm, và vẽ chữ nằm TRÊN hình con ma
        if (wordLabel != null)
        {
            wordLabel.outlineWidth = 0.25f;
            wordLabel.outlineColor = new Color32(0, 0, 0, 255);
            wordLabel.GetComponent<Renderer>().sortingOrder = 10;
        }
    }

    void Start()
    {
        UpdateLabel();

        // [MỚI] Đích đến = ĐỈNH Tháp (chỗ quyển sách), thay vì chân tháp.
        // Nhờ vậy quái nổ sớm hơn, không còn chui vào thân tháp và chồng chữ lên nhau.
        if (Tower.Instance != null)
        {
            Bounds towerBox = Tower.Instance.GetComponent<SpriteRenderer>().bounds; // Khung bao quanh hình tháp
            targetPosition = new Vector2(towerBox.center.x, towerBox.max.y - 0.3f); // max.y = mép trên của tháp
        }
    }

    void Update()
    {
        // Di chuyển quái tiến dần tới Tháp
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, speed * Time.deltaTime);

        // [MỚI - ĐỒ HỌA] Lắc lư lên xuống + phồng lên khi bị gõ trúng rồi co lại dần
        if (body != null)
        {
            body.localPosition = new Vector3(0f, Mathf.Sin((Time.time + bobOffset) * 4f) * 0.08f, 0f);
            punch = Mathf.MoveTowards(punch, 0f, Time.deltaTime * 1.5f);
            body.localScale = Vector3.one * baseScale * (1f + punch);

            // [MỚI] Vừa gõ sai -> rung ngang mạnh rồi yếu dần
            if (wrongTimer > 0f)
            {
                float shake = Mathf.Sin(Time.time * 60f) * 0.12f * (wrongTimer / 0.35f);
                body.localPosition += new Vector3(shake, 0f, 0f);
            }
        }

        UpdateTargetVisual(); // [MỚI - CHỈ ĐỊNH MỤC TIÊU]

        // Nếu quái đã chạm Tháp
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            // [MỚI] Báo cho Tháp biết để trừ tim và ghi lại từ bị lọt
            if (Tower.Instance != null)
            {
                Tower.Instance.TakeDamage(word, meaning);
            }
            Effects.SpawnExplosion(transform.position, new Color(1f, 0.3f, 0.3f)); // [MỚI] Nổ đỏ khi chạm Tháp
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
        punch = 0.25f; // [MỚI - ĐỒ HỌA] Phồng lên 25% để báo "trúng rồi!"
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
    // [SỬA] Nhận thêm số điểm để hiện chữ "+điểm" bay lên
    public void Die(int points = 0)
    {
        Debug.Log("Tiêu diệt quái '" + word + "' (" + meaning + ")");

        // [MỚI - ĐỒ HỌA] Nổ hạt sáng vàng + chữ điểm bay lên
        Effects.SpawnExplosion(transform.position, new Color(1f, 0.9f, 0.5f));
        if (points > 0)
        {
            Effects.SpawnFloatingText(transform.position + Vector3.up * 0.5f, "+" + points, new Color(1f, 0.84f, 0f));
        }

        Destroy(gameObject);
    }

    // [MỚI - CHỈ ĐỊNH MỤC TIÊU] Gọi khi người chơi gõ sai chữ của con quái đang khóa
    public void FlashWrong()
    {
        wrongTimer = 0.35f;
        UpdateLabel();
    }

    // [MỚI - CHỈ ĐỊNH MỤC TIÊU] Cập nhật hình ảnh mỗi khung hình:
    //   - Quái đang khóa: hiện khung ngắm (xoay + nhịp đập), chữ to hơn
    //   - Quái khác: chữ mờ đi khi đang có mục tiêu, để người chơi tập trung
    private void UpdateTargetVisual()
    {
        bool isTarget = (CurrentTarget == this);
        bool someoneTargeted = (CurrentTarget != null);

        if (reticle != null)
        {
            reticle.gameObject.SetActive(isTarget);
            if (isTarget)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.08f;
                reticle.localScale = Vector3.one * Effects.ScaleFor(reticle.GetComponent<SpriteRenderer>().sprite) * pulse;
                reticle.Rotate(0f, 0f, 60f * Time.deltaTime); // Xoay chậm
            }
        }

        if (wordLabel != null)
        {
            wordLabel.transform.localScale = Vector3.one * (isTarget ? 1.2f : 1f);
            float newAlpha = (!someoneTargeted || isTarget) ? 1f : 0.45f;
            if (newAlpha != labelAlpha)
            {
                labelAlpha = newAlpha;
                UpdateLabel(); // Vẽ lại chữ với độ mờ mới
            }
        }

        // Hết thời gian báo sai -> vẽ lại chữ về màu bình thường
        if (wrongTimer > 0f)
        {
            wrongTimer -= Time.deltaTime;
            if (wrongTimer <= 0f) UpdateLabel();
        }
    }

    // Vẽ lại chữ: phần đã gõ màu vàng, phần chưa gõ màu trắng (hoặc ĐỎ nếu vừa gõ sai).
    // <color=#FFD700>...</color> là "rich text" của TextMeshPro để tô màu một đoạn chữ.
    private void UpdateLabel()
    {
        if (wordLabel == null) return;
        string typed = word.Substring(0, typedCount);
        string rest = word.Substring(typedCount);
        // [MỚI] Mã màu 8 ký tự #RRGGBBAA: 2 ký tự cuối là độ đậm (alpha), 00 = trong suốt, FF = rõ hẳn
        string a = Mathf.RoundToInt(labelAlpha * 255).ToString("X2");
        string restColor = wrongTimer > 0f ? "#FF4444" : "#FFFFFF"; // Gõ sai -> phần còn lại chuyển đỏ
        wordLabel.text = "<color=#FFD700" + a + ">" + typed + "</color><color=" + restColor + a + ">" + rest + "</color>";
    }
}
