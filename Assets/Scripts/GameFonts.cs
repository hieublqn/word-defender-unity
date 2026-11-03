using UnityEngine;
using TMPro;

// GameFonts = Quản lý font chữ của game (static, không cần gắn vào GameObject).
//   - Title: Bungee         -> tiêu đề lớn (WORD DEFENDER, WAVE, GAME OVER)
//   - Body : Be Vietnam Pro -> mọi chữ còn lại (từ trên đầu quái, menu, điểm...)
// File font (.ttf) đặt trong Assets/Resources/Fonts/.
// Cả 2 font dùng giấy phép SIL Open Font License (miễn phí), tải từ Google Fonts.
public static class GameFonts
{
    private static TMP_FontAsset title;
    private static TMP_FontAsset body;

    public static TMP_FontAsset Title
    {
        get
        {
            if (title == null) title = Create("Fonts/Bungee-Regular");
            return title;
        }
    }

    public static TMP_FontAsset Body
    {
        get
        {
            if (body == null) body = Create("Fonts/BeVietnamPro-Bold");
            return body;
        }
    }

    // Tạo "Font Asset" cho TextMeshPro từ file .ttf ngay khi game chạy.
    // Đây là font ĐỘNG (dynamic): chữ nào cần hiển thị (kể cả chữ có dấu tiếng Việt)
    // sẽ tự được vẽ thêm vào bảng ký tự lúc chạy, không cần chuẩn bị trước.
    private static TMP_FontAsset Create(string path)
    {
        Font font = Resources.Load<Font>(path);
        if (font == null)
        {
            Debug.LogWarning("Không tìm thấy font: Resources/" + path + " -> dùng font mặc định");
            return null;
        }
        TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font);
        asset.name = font.name;
        return asset;
    }

    // Hàm tiện ích: gán font cho 1 ô chữ (nếu tải font thất bại thì giữ font mặc định)
    public static void Apply(TMP_Text text, bool isTitle = false)
    {
        TMP_FontAsset f = isTitle ? Title : Body;
        if (f != null) text.font = f;
    }
}
