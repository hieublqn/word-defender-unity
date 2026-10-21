using System.Collections.Generic;
using UnityEngine;

// [MỚI] Các chủ đề từ vựng người chơi có thể chọn ở menu
public enum Topic
{
    TOEIC,   // Công việc & văn phòng
    IT,      // Công nghệ thông tin
    Travel   // Du lịch
}

// WordBank = Kho từ vựng.
// "static class" = lớp dùng chung, không cần gắn vào GameObject nào.
// Script khác gọi trực tiếp: WordBank.GetRandomWord(3, 4, ...)
public static class WordBank
{
    // [MỚI] Chủ đề đang chơi. "static" nên giá trị được GIỮ LẠI kể cả khi tải lại màn chơi.
    public static Topic currentTopic = Topic.TOEIC;

    // [MỚI] Mỗi chủ đề là 1 danh sách riêng. Mỗi phần tử = { từ tiếng Anh, nghĩa tiếng Việt }
    // Mỗi chủ đề đều có đủ từ ngắn (3-4 chữ), vừa (5-6 chữ) và dài (7+ chữ) cho các wave.
    private static readonly string[,] toeicWords =
    {
        { "fee", "phí" },            { "tax", "thuế" },              { "job", "công việc" },
        { "pay", "trả tiền" },       { "sale", "doanh số" },         { "deal", "thỏa thuận" },
        { "loan", "khoản vay" },     { "bank", "ngân hàng" },        { "rent", "tiền thuê" },
        { "cash", "tiền mặt" },      { "team", "đội nhóm" },         { "task", "nhiệm vụ" },
        { "memo", "bản ghi nhớ" },
        { "offer", "lời đề nghị" },  { "staff", "nhân viên" },       { "order", "đơn hàng" },
        { "price", "giá cả" },       { "client", "khách hàng" },     { "budget", "ngân sách" },
        { "office", "văn phòng" },   { "salary", "lương" },          { "refund", "hoàn tiền" },
        { "report", "báo cáo" },     { "supply", "nguồn cung" },
        { "meeting", "cuộc họp" },   { "contract", "hợp đồng" },     { "customer", "khách hàng" },
        { "employee", "nhân viên" }, { "schedule", "lịch trình" },   { "deadline", "hạn chót" },
        { "interview", "phỏng vấn" },{ "purchase", "mua hàng" },     { "manager", "quản lý" },
        { "discount", "giảm giá" },  { "invoice", "hóa đơn" },       { "warranty", "bảo hành" },
        { "agreement", "hợp đồng, thỏa thuận" },
    };

    private static readonly string[,] itWords =
    {
        { "code", "mã nguồn" },      { "data", "dữ liệu" },          { "file", "tệp tin" },
        { "bug", "lỗi phần mềm" },   { "app", "ứng dụng" },          { "link", "đường liên kết" },
        { "byte", "byte" },          { "chip", "con chip" },         { "disk", "ổ đĩa" },
        { "port", "cổng kết nối" },  { "web", "trang web" },         { "user", "người dùng" },
        { "menu", "thực đơn, menu" },
        { "mouse", "con chuột" },    { "cloud", "đám mây" },         { "input", "đầu vào" },
        { "output", "đầu ra" },      { "server", "máy chủ" },        { "screen", "màn hình" },
        { "folder", "thư mục" },     { "laptop", "máy tính xách tay" },{ "router", "bộ định tuyến" },
        { "update", "cập nhật" },    { "delete", "xóa" },            { "script", "kịch bản lệnh" },
        { "keyboard", "bàn phím" },  { "database", "cơ sở dữ liệu" },{ "software", "phần mềm" },
        { "hardware", "phần cứng" }, { "network", "mạng máy tính" }, { "internet", "mạng internet" },
        { "password", "mật khẩu" },  { "computer", "máy tính" },     { "function", "hàm" },
        { "variable", "biến" },      { "download", "tải xuống" },    { "security", "bảo mật" },
        { "algorithm", "thuật toán" },
    };

    private static readonly string[,] travelWords =
    {
        { "bus", "xe buýt" },        { "map", "bản đồ" },            { "car", "xe hơi" },
        { "sea", "biển" },           { "taxi", "xe taxi" },          { "trip", "chuyến đi" },
        { "tour", "chuyến tham quan" },{ "room", "căn phòng" },      { "ship", "con tàu" },
        { "food", "đồ ăn" },         { "city", "thành phố" },        { "bag", "cái túi" },
        { "hotel", "khách sạn" },    { "beach", "bãi biển" },        { "ticket", "vé" },
        { "travel", "du lịch" },     { "island", "hòn đảo" },        { "museum", "bảo tàng" },
        { "camera", "máy ảnh" },     { "guide", "hướng dẫn viên" },  { "train", "tàu hỏa" },
        { "plane", "máy bay" },      { "visit", "tham quan" },       { "flight", "chuyến bay" },
        { "airport", "sân bay" },    { "passport", "hộ chiếu" },     { "luggage", "hành lý" },
        { "holiday", "kỳ nghỉ" },    { "souvenir", "quà lưu niệm" }, { "vacation", "kỳ nghỉ" },
        { "tourist", "khách du lịch" },{ "landmark", "địa danh" },   { "journey", "hành trình" },
        { "reception", "quầy lễ tân" },{ "departure", "khởi hành" }, { "restaurant", "nhà hàng" },
        { "adventure", "cuộc phiêu lưu" },
    };

    // [MỚI] Tên chủ đề để hiện trên giao diện
    public static string TopicName(Topic topic)
    {
        switch (topic)
        {
            case Topic.TOEIC: return "TOEIC";
            case Topic.IT:    return "Công nghệ thông tin";
            default:          return "Du lịch";
        }
    }

    // [MỚI] Lấy danh sách từ của chủ đề đang chọn
    private static string[,] CurrentList()
    {
        switch (currentTopic)
        {
            case Topic.TOEIC: return toeicWords;
            case Topic.IT:    return itWords;
            default:          return travelWords;
        }
    }

    // Lấy ngẫu nhiên 1 từ có độ dài từ minLength đến maxLength (trong chủ đề đang chọn).
    // "avoidFirstLetters": những chữ cái đầu KHÔNG nên trùng (để người chơi khỏi nhầm quái).
    // Trả về mảng 2 phần tử: [0] = từ, [1] = nghĩa
    public static string[] GetRandomWord(int minLength, int maxLength, HashSet<char> avoidFirstLetters)
    {
        string[,] words = CurrentList(); // [MỚI] dùng danh sách của chủ đề hiện tại

        // Bước 1: lọc ra các từ có độ dài phù hợp
        List<int> candidates = new List<int>();
        for (int i = 0; i < words.GetLength(0); i++)
        {
            int len = words[i, 0].Length;
            if (len >= minLength && len <= maxLength)
                candidates.Add(i);
        }

        // Bước 2: thử tối đa 10 lần để chọn từ có chữ đầu chưa xuất hiện trên màn hình
        int pick = candidates[Random.Range(0, candidates.Count)];
        for (int attempt = 0; attempt < 10; attempt++)
        {
            int index = candidates[Random.Range(0, candidates.Count)];
            pick = index;
            if (!avoidFirstLetters.Contains(words[index, 0][0])) break;
        }

        return new string[] { words[pick, 0], words[pick, 1] };
    }
}
