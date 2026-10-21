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
    private GameObject menuPanel;      // [MỚI] Màn hình menu chọn chủ đề
    private TMP_Text topicText;        // [MỚI] Dòng hiện chủ đề đang chơi

    private int lastWave = 0;          // Đợt đã hiển thị gần nhất (để biết khi nào sang đợt mới)
    private float bannerTimer = 0f;    // Thời gian còn lại để hiện chữ "WAVE ..."
    private bool isGameOver = false;
    private bool inMenu = false;       // [MỚI] Đang ở menu hay không

    // [MỚI] "static" nên giữ được giá trị khi tải lại màn chơi.
    // true = lần tải tiếp theo bỏ qua menu, vào chơi luôn với chủ đề cũ.
    private static bool skipMenu = false;

    void Start()
    {
        // Tìm các script cần thiết trong scene
        tower = FindFirstObjectByType<Tower>();
        typing = FindFirstObjectByType<TypingManager>();
        spawner = FindFirstObjectByType<EnemySpawner>();

        BuildUI();
        Effects.CreateBackground(); // [MỚI - ĐỒ HỌA] Nền trời đêm

        // [MỚI] Chơi lại cùng chủ đề -> vào game luôn; ngược lại -> hiện menu
        if (skipMenu)
        {
            skipMenu = false;
            BeginGame();
        }
        else
        {
            inMenu = true;
            menuPanel.SetActive(true);
        }
    }

    // [MỚI] Bắt đầu chơi: ẩn menu, ghi tên chủ đề, cho Spawner sinh quái
    private void BeginGame()
    {
        inMenu = false;
        menuPanel.SetActive(false);
        topicText.text = "Chủ đề: " + WordBank.TopicName(WordBank.currentTopic)
                       + "   |   Kỷ lục: " + GetHighScore(WordBank.currentTopic); // [MỚI] hiện kỷ lục
        spawner.StartGame();
    }

    // ===== [MỚI - KỶ LỤC] Đọc / ghi điểm cao nhất =====
    // PlayerPrefs = "cuốn sổ nhỏ" Unity lưu trên máy, tắt game mở lại vẫn còn.
    // Mỗi chủ đề có 1 "khóa" (key) riêng, ví dụ "HighScore_TOEIC", "HighScore_IT"...
    private static string HighScoreKey(Topic topic)
    {
        return "HighScore_" + topic;
    }

    private static int GetHighScore(Topic topic)
    {
        return PlayerPrefs.GetInt(HighScoreKey(topic), 0); // Chưa có thì trả về 0
    }

    // Trả về true nếu điểm lần này phá kỷ lục cũ (và lưu luôn kỷ lục mới)
    private static bool TrySaveHighScore(Topic topic, int score)
    {
        if (score <= GetHighScore(topic)) return false;
        PlayerPrefs.SetInt(HighScoreKey(topic), score);
        PlayerPrefs.Save(); // Ghi xuống ổ đĩa ngay
        return true;
    }

    void Update()
    {
        // ===== [MỚI] 0. Đang ở menu: chờ người chơi nhấn 1, 2 hoặc 3 =====
        if (inMenu)
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)
            { WordBank.currentTopic = Topic.TOEIC; BeginGame(); }
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)
            { WordBank.currentTopic = Topic.IT; BeginGame(); }
            else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame)
            { WordBank.currentTopic = Topic.Travel; BeginGame(); }

            return; // Đang ở menu thì không cập nhật phần còn lại
        }

        // ===== 1. Cập nhật tim, điểm, combo mỗi khung hình =====
        heartsText.text = "Tim: " + tower.hearts + " / " + tower.maxHearts;

        string comboInfo = typing.combo >= 5 ? "  (x2 điểm!)" : "";
        scoreText.text = "Điểm: " + typing.score + "\nCombo: " + typing.combo + comboInfo;

        // [MỚI] Vừa gõ sai (trong 0,6 giây) -> ô Điểm/Combo nháy đỏ
        bool justWrong = Time.time - typing.lastWrongTime < 0.6f;
        scoreText.color = justWrong ? new Color(1f, 0.3f, 0.3f) : Color.white;

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

        // [SỬA] Đang ở màn Game Over:
        //   Enter -> chơi lại CÙNG chủ đề;   M -> về menu chọn chủ đề khác
        if (isGameOver && Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                skipMenu = true;
                RestartScene();
            }
            else if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                skipMenu = false;
                RestartScene();
            }
        }
    }

    // [MỚI] Tải lại màn chơi từ đầu
    private void RestartScene()
    {
        Time.timeScale = 1f; // Cho thời gian chạy lại bình thường
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowGameOver()
    {
        isGameOver = true;

        // [MỚI - KỶ LỤC] Kiểm tra phá kỷ lục trước khi hiện màn hình
        Topic topic = WordBank.currentTopic;
        bool newRecord = TrySaveHighScore(topic, typing.score);

        string text = "<size=90><color=#FF5555>GAME OVER</color></size>\n\n";
        text += "Điểm: " + typing.score + "     Đạt tới: Wave " + spawner.currentWave + "\n";
        if (newRecord)
            text += "<size=56><color=#FFD700>KỶ LỤC MỚI!</color></size>\n\n";
        else
            text += "Kỷ lục (" + WordBank.TopicName(topic) + "): " + GetHighScore(topic) + "\n\n";
        text += "<color=#FFD700>Những từ bạn cần ôn lại:</color>\n";
        foreach (string w in tower.missedWords)
        {
            text += "• " + w + "\n";
        }
        text += "\n<size=36>ENTER: chơi lại chủ đề này     M: về menu chọn chủ đề</size>"; // [SỬA]

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

        // [MỚI] Dưới góc tim: tên chủ đề đang chơi
        topicText = CreateText(canvasObj.transform, "Topic", new Vector2(0, 1), new Vector2(40, -90),
                               new Vector2(700, 60), 34, TextAlignmentOptions.TopLeft);
        topicText.color = new Color(0.7f, 0.9f, 1f);

        // Màn hình Game Over: 1 tấm nền tối phủ kín màn hình + 1 ô chữ
        gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.85f));
        gameOverText = CreateText(gameOverPanel.transform, "GameOverText", new Vector2(0.5f, 0.5f), Vector2.zero,
                                  new Vector2(1400, 900), 44, TextAlignmentOptions.Center);
        gameOverPanel.SetActive(false);

        // [MỚI] Màn hình Menu chọn chủ đề (nền xanh đậm, che kín màn hình)
        menuPanel = CreatePanel(canvasObj.transform, "MenuPanel", new Color(0.05f, 0.08f, 0.2f, 1f));
        TMP_Text menuText = CreateText(menuPanel.transform, "MenuText", new Vector2(0.5f, 0.5f), Vector2.zero,
                                       new Vector2(1400, 900), 48, TextAlignmentOptions.Center);
        menuText.text =
            "<size=120><color=#FFD700>WORD DEFENDER</color></size>\n" +
            "<size=40>Bảo vệ Tháp Tri Thức bằng bàn phím!</size>\n\n\n" +
            "Chọn chủ đề từ vựng:\n\n" +
            // [SỬA] Thêm kỷ lục của từng chủ đề vào cuối mỗi dòng
            "<color=#FFD700>[1]</color>  TOEIC - Công việc & văn phòng   <size=34><color=#AAAAAA>Kỷ lục: " + GetHighScore(Topic.TOEIC) + "</color></size>\n" +
            "<color=#FFD700>[2]</color>  Công nghệ thông tin   <size=34><color=#AAAAAA>Kỷ lục: " + GetHighScore(Topic.IT) + "</color></size>\n" +
            "<color=#FFD700>[3]</color>  Du lịch   <size=34><color=#AAAAAA>Kỷ lục: " + GetHighScore(Topic.Travel) + "</color></size>\n\n\n" +
            "<size=36>Nhấn phím 1, 2 hoặc 3 để bắt đầu</size>";
        menuPanel.SetActive(false);
    }

    // [MỚI] Hàm phụ: tạo 1 tấm nền phủ kín màn hình (dùng cho Menu và Game Over)
    private GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image bg = panel.AddComponent<Image>();
        bg.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;   // Kéo giãn phủ kín
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
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
