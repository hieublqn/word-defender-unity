using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

// GameUI = Giao diện trong game. Gắn vào GameObject "GameManager".
// Script này TỰ TẠO toàn bộ giao diện bằng code khi game bắt đầu:
//   - Góc trên trái: số tim
//   - Góc trên phải: điểm và combo
//   - Giữa màn hình: thông báo "WAVE 2", "WAVE 3"...
//   - Màn hình Game Over: điểm, đợt, danh sách từ bị lọt kèm nghĩa, nhấn Enter để chơi lại
public class GameUI : MonoBehaviour
{
    // Các script khác mà giao diện cần đọc dữ liệu
    private Tower tower;
    private TypingManager typing;
    private EnemySpawner spawner;

    // Các ô chữ trên màn hình
    private TMP_Text heartsText;
    private TMP_Text scoreText;
    private TMP_Text waveBanner;
    private GameObject gameOverPanel;
    private TMP_Text gameOverText;

    private int lastWave = 0;          // Đợt đã hiển thị gần nhất (để biết khi nào sang đợt mới)
    private float bannerTimer = 0f;    // Thời gian còn lại để hiện chữ "WAVE ..."
    private bool isGameOver = false;

    void Start()
    {
        // Tìm các script cần thiết trong scene
        tower = FindFirstObjectByType<Tower>();
        typing = FindFirstObjectByType<TypingManager>();
        spawner = FindFirstObjectByType<EnemySpawner>();

        BuildUI();
    }

    void Update()
    {
        // ===== 1. Cập nhật tim, điểm, combo mỗi khung hình =====
        heartsText.text = "Tim: " + tower.hearts + " / " + tower.maxHearts;

        string comboInfo = typing.combo >= 5 ? "  (x2 điểm!)" : "";
        scoreText.text = "Điểm: " + typing.score + "\nCombo: " + typing.combo + comboInfo;

        // ===== 2. Hiện chữ "WAVE n" khi sang đợt mới =====
        if (spawner.currentWave != lastWave)
        {
            lastWave = spawner.currentWave;
            waveBanner.text = "WAVE " + lastWave;
            bannerTimer = 2f; // Hiện trong 2 giây
        }
        if (bannerTimer > 0f)
        {
            // unscaledDeltaTime: vẫn đếm giờ kể cả khi game bị dừng (Time.timeScale = 0)
            bannerTimer -= Time.unscaledDeltaTime;
            waveBanner.gameObject.SetActive(bannerTimer > 0f);
        }

        // ===== 3. Game Over =====
        if (!isGameOver && tower.hearts <= 0)
        {
            ShowGameOver();
        }

        // Đang ở màn Game Over -> nhấn Enter để chơi lại
        if (isGameOver && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f; // Cho thời gian chạy lại bình thường
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Tải lại màn chơi từ đầu
        }
    }

    private void ShowGameOver()
    {
        isGameOver = true;

        string text = "<size=90><color=#FF5555>GAME OVER</color></size>\n\n";
        text += "Điểm: " + typing.score + "     Đạt tới: Wave " + spawner.currentWave + "\n\n";
        text += "<color=#FFD700>Những từ bạn cần ôn lại:</color>\n";
        foreach (string w in tower.missedWords)
        {
            text += "• " + w + "\n";
        }
        text += "\n<size=36>Nhấn ENTER để chơi lại</size>";

        gameOverText.text = text;
        gameOverPanel.SetActive(true);
    }

    // ================== TẠO GIAO DIỆN BẰNG CODE ==================

    private void BuildUI()
    {
        // Canvas = "tấm kính" nằm trên màn hình, mọi chữ/nút của UI được vẽ lên đó
        GameObject canvasObj = new GameObject("UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Luôn nằm trên cùng màn hình

        // CanvasScaler: tự co giãn UI theo kích thước màn hình (thiết kế theo chuẩn 1920x1080)
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Góc trên trái: tim
        heartsText = CreateText(canvasObj.transform, "Hearts", new Vector2(0, 1), new Vector2(40, -30),
                                new Vector2(500, 80), 48, TextAlignmentOptions.TopLeft);
        heartsText.color = new Color(1f, 0.4f, 0.4f);

        // Góc trên phải: điểm + combo
        scoreText = CreateText(canvasObj.transform, "Score", new Vector2(1, 1), new Vector2(-40, -30),
                               new Vector2(600, 140), 44, TextAlignmentOptions.TopRight);

        // Giữa màn hình: thông báo đợt
        waveBanner = CreateText(canvasObj.transform, "WaveBanner", new Vector2(0.5f, 0.5f), new Vector2(0, 100),
                                new Vector2(1000, 200), 120, TextAlignmentOptions.Center);
        waveBanner.color = new Color(1f, 0.84f, 0f); // Vàng
        waveBanner.gameObject.SetActive(false);

        // Màn hình Game Over: 1 tấm nền tối phủ kín màn hình + 1 ô chữ
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasObj.transform, false);
        Image bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.85f); // Đen, hơi trong suốt
        RectTransform bgRect = gameOverPanel.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;   // Kéo giãn phủ kín
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        gameOverText = CreateText(gameOverPanel.transform, "GameOverText", new Vector2(0.5f, 0.5f), Vector2.zero,
                                  new Vector2(1400, 900), 44, TextAlignmentOptions.Center);
        gameOverPanel.SetActive(false);
    }

    // Hàm phụ: tạo 1 ô chữ TextMeshPro trên UI
    // anchor = điểm neo (0,1 = góc trên trái; 1,1 = góc trên phải; 0.5,0.5 = giữa)
    private TMP_Text CreateText(Transform parent, string name, Vector2 anchor, Vector2 position,
                                Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }
}
