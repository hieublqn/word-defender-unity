using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemySpawner = Bộ sinh quái. Gắn vào GameObject "GameManager".
// Nhiệm vụ: tạo quái theo từng đợt (wave). Đợt sau đông hơn, nhanh hơn, từ dài hơn.
public class EnemySpawner : MonoBehaviour
{
    [Header("Mẫu quái (Prefab)")]
    public Enemy enemyPrefab;          // Kéo prefab Enemy vào ô này trong Inspector

    [Header("Vị trí xuất hiện")]
    public float spawnY = 6f;          // Phía trên mép màn hình (camera nhìn thấy từ -5 đến 5)
    public float minX = -6f;           // Giới hạn trái
    public float maxX = 6f;            // Giới hạn phải

    [Header("Trạng thái (chỉ để xem)")]
    public int currentWave = 0;        // Đợt hiện tại

    void Start()
    {
        // Bắt đầu chuỗi các đợt quái.
        // Coroutine = hàm có thể "tạm dừng" (chờ vài giây) rồi chạy tiếp mà không làm đơ game.
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true) // Lặp vô hạn: chơi đến khi hết tim
        {
            currentWave++;

            // ===== Thông số của đợt này (theo bảng trong file Phân tích thiết kế) =====
            int enemyCount = 5 + (currentWave - 1) * 2;                     // 5, 7, 9, 11...
            float speed = 0.6f + (currentWave - 1) * 0.15f;                 // Nhanh dần
            float spawnInterval = Mathf.Max(0.8f, 2.5f - (currentWave - 1) * 0.3f); // Xuất hiện dày dần
            int minLen, maxLen;
            GetWordLength(currentWave, out minLen, out maxLen);

            Debug.Log("===== WAVE " + currentWave + " | " + enemyCount + " quái | tốc độ " + speed + " =====");

            // ===== Sinh từng con quái =====
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy(speed, minLen, maxLen);
                yield return new WaitForSeconds(spawnInterval); // Chờ rồi mới sinh con tiếp theo
            }

            // ===== Chờ người chơi diệt hết quái của đợt này =====
            while (Enemy.All.Count > 0)
            {
                yield return null; // Chờ 1 khung hình rồi kiểm tra lại
            }

            Debug.Log("Hoàn thành Wave " + currentWave + "! Nghỉ 3 giây...");
            yield return new WaitForSeconds(3f);
        }
    }

    // Tạo 1 con quái mới ở vị trí ngẫu nhiên phía trên màn hình
    private void SpawnEnemy(float speed, int minLen, int maxLen)
    {
        // Gom các chữ cái đầu của quái đang có, để từ mới không bị trùng chữ đầu
        HashSet<char> usedFirstLetters = new HashSet<char>();
        foreach (Enemy e in Enemy.All)
            usedFirstLetters.Add(e.word[0]);

        string[] pair = WordBank.GetRandomWord(minLen, maxLen, usedFirstLetters);

        Vector3 position = new Vector3(Random.Range(minX, maxX), spawnY, 0f);

        // Instantiate = tạo bản sao của prefab tại vị trí chỉ định
        Enemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.word = pair[0];
        enemy.meaning = pair[1];
        enemy.speed = speed;
    }

    // Độ dài từ theo từng đợt
    private void GetWordLength(int wave, out int minLen, out int maxLen)
    {
        if (wave == 1)      { minLen = 3; maxLen = 4; }
        else if (wave == 2) { minLen = 3; maxLen = 5; }
        else if (wave == 3) { minLen = 4; maxLen = 6; }
        else                { minLen = 5; maxLen = 10; }
    }
}
