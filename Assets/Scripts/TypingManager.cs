using UnityEngine;
using UnityEngine.InputSystem; // Hệ thống nhập liệu mới của Unity (đọc bàn phím)

// TypingManager = Bộ xử lý gõ phím. Gắn vào 1 GameObject rỗng tên "GameManager".
// Nhiệm vụ: đọc phím người chơi gõ -> khóa mục tiêu -> so sánh chữ -> tiêu diệt quái.
public class TypingManager : MonoBehaviour
{
    public int score = 0;     // Điểm
    public int combo = 0;     // Số từ gõ đúng liên tiếp

    private Enemy target;     // Con quái đang bị khóa mục tiêu (null = chưa khóa con nào)

    // OnEnable: bắt đầu "lắng nghe" bàn phím.
    // Mỗi khi người chơi gõ 1 ký tự, Unity sẽ tự gọi hàm OnTextInput.
    void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    // OnDisable: ngừng lắng nghe (tránh lỗi khi tắt game)
    void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    void Update()
    {
        // Nhấn Backspace -> bỏ mục tiêu hiện tại để chọn con quái khác
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (target != null) target.ResetTyping();
            target = null;
        }

        // [MỚI - CHỈ ĐỊNH MỤC TIÊU] Báo cho các quái biết con nào đang bị khóa (để hiện khung ngắm)
        Enemy.CurrentTarget = target;
    }

    // Được gọi mỗi khi người chơi gõ 1 ký tự
    private void OnTextInput(char c)
    {
        if (!char.IsLetter(c)) return; // Bỏ qua phím không phải chữ cái (số, dấu cách...)
        c = char.ToLower(c);

        // Trường hợp 1: chưa khóa mục tiêu -> tìm quái phù hợp
        if (target == null)
        {
            target = FindTarget(c);
            if (target == null)
            {
                WrongKey(c); // Không con nào bắt đầu bằng chữ này
                return;
            }
        }
        // Trường hợp 2: đã khóa mục tiêu nhưng gõ sai chữ tiếp theo
        else if (target.NextLetter() != c)
        {
            WrongKey(c);
            return;
        }

        // Gõ đúng -> tô vàng thêm 1 chữ
        target.TypeLetter();

        // Gõ xong cả từ -> tiêu diệt quái, cộng điểm
        if (target.IsComplete())
        {
            combo++;
            int points = target.word.Length * 10;  // Điểm = số chữ cái x 10
            if (combo >= 5) points *= 2;           // Combo 5 từ trở lên -> nhân đôi
            score += points;
            Debug.Log("+" + points + " điểm | Tổng: " + score + " | Combo: " + combo);

            target.Die(points); // [SỬA] truyền điểm để hiện chữ "+điểm" bay lên
            target = null;
        }
    }

    // Tìm con quái GẦN THÁP NHẤT có từ bắt đầu bằng chữ c
    private Enemy FindTarget(char c)
    {
        Enemy best = null;
        float bestDistance = float.MaxValue;

        foreach (Enemy e in Enemy.All)
        {
            if (e.NextLetter() != c) continue; // Chữ đầu không khớp -> bỏ qua

            float d = Vector2.Distance(e.transform.position, e.targetPosition);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = e;
            }
        }
        return best;
    }

    // [MỚI] Thời điểm gõ sai gần nhất (GameUI dùng để nháy đỏ ô Combo)
    [HideInInspector] public float lastWrongTime = -10f;

    // [SỬA] Gõ sai: mất combo (không bị trừ tim) + báo hiệu rõ ràng cho người chơi
    private void WrongKey(char c)
    {
        int lostCombo = combo;
        combo = 0;
        lastWrongTime = Time.time;
        Debug.Log("Gõ sai chữ '" + c + "' - mất combo!");

        Color red = new Color(1f, 0.3f, 0.3f);

        if (target != null)
        {
            // Đang khóa 1 con: con đó rung + chữ đỏ, thông báo hiện ngay trên đầu nó
            target.FlashWrong();
            string msg = lostCombo >= 2 ? "Mất combo x" + lostCombo + "!" : "Sai chữ!";
            Effects.SpawnFloatingText(target.transform.position + Vector3.up * 1.6f, msg, red);
        }
        else
        {
            // Chưa khóa con nào và không có từ nào bắt đầu bằng chữ này: báo phía trên Tháp
            Vector3 pos = Tower.Instance != null ? Tower.Instance.transform.position + Vector3.up * 2.5f : Vector3.zero;
            string msg = "Không có từ bắt đầu bằng '" + char.ToUpper(c) + "'";
            if (lostCombo >= 2) msg += "\nMất combo x" + lostCombo + "!";
            Effects.SpawnFloatingText(pos, msg, red);
        }
    }
}
