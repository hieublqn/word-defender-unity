using System.Collections.Generic;
using UnityEngine;

// WordBank = Kho từ vựng.
// "static class" = lớp dùng chung, không cần gắn vào GameObject nào.
// Script khác gọi trực tiếp: WordBank.GetRandomWord(3, 4)
public static class WordBank
{
    // Mỗi phần tử là 1 cặp { từ tiếng Anh, nghĩa tiếng Việt }
    private static readonly string[,] words =
    {
        // --- Từ ngắn (3-4 chữ) ---
        { "cat", "con mèo" },        { "dog", "con chó" },        { "sun", "mặt trời" },
        { "map", "bản đồ" },         { "key", "chìa khóa" },      { "job", "công việc" },
        { "bus", "xe buýt" },        { "pen", "cái bút" },        { "fee", "phí" },
        { "tax", "thuế" },           { "fish", "con cá" },        { "book", "quyển sách" },
        { "milk", "sữa" },           { "rain", "mưa" },           { "door", "cửa ra vào" },
        { "hotel", "khách sạn" },    { "code", "mã nguồn" },      { "data", "dữ liệu" },
        { "file", "tệp tin" },       { "wifi", "mạng không dây" },

        // --- Từ vừa (5-6 chữ) ---
        { "apple", "quả táo" },      { "water", "nước" },         { "house", "ngôi nhà" },
        { "money", "tiền" },         { "train", "tàu hỏa" },      { "phone", "điện thoại" },
        { "table", "cái bàn" },      { "music", "âm nhạc" },      { "banana", "quả chuối" },
        { "office", "văn phòng" },   { "ticket", "vé" },          { "travel", "du lịch" },
        { "server", "máy chủ" },     { "screen", "màn hình" },    { "invoice", "hóa đơn" },
        { "garden", "khu vườn" },    { "island", "hòn đảo" },     { "mouse", "con chuột" },

        // --- Từ dài (7 chữ trở lên) ---
        { "meeting", "cuộc họp" },         { "customer", "khách hàng" },
        { "computer", "máy tính" },        { "keyboard", "bàn phím" },
        { "database", "cơ sở dữ liệu" },   { "schedule", "lịch trình" },
        { "airport", "sân bay" },          { "passport", "hộ chiếu" },
        { "employee", "nhân viên" },       { "software", "phần mềm" },
        { "contract", "hợp đồng" },        { "internet", "mạng internet" },
        { "restaurant", "nhà hàng" },      { "department", "phòng ban" },
    };

    // Lấy ngẫu nhiên 1 từ có độ dài từ minLength đến maxLength.
    // "avoidFirstLetters": những chữ cái đầu KHÔNG nên trùng (để người chơi khỏi nhầm quái).
    // Trả về mảng 2 phần tử: [0] = từ, [1] = nghĩa
    public static string[] GetRandomWord(int minLength, int maxLength, HashSet<char> avoidFirstLetters)
    {
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
