using System.Collections.Generic;
using UnityEngine;

// SoundManager = Bộ quản lý âm thanh dùng chung (static, không cần gắn vào GameObject).
// Gọi ở bất kỳ đâu: SoundManager.Play("explode");  SoundManager.PlayMusic();
// File âm thanh đặt trong Assets/Resources/Sounds/ (type, wrong, explode, hit, golden, wave, gameover, music)
public static class SoundManager
{
    private static AudioSource sfxSource;    // "Loa" phát hiệu ứng âm thanh ngắn
    private static AudioSource musicSource;  // "Loa" phát nhạc nền (lặp lại)
    private static Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>(); // Lưu lại clip đã tải

    public static float sfxVolume = 0.8f;
    public static float musicVolume = 0.35f;

    // Tạo 1 GameObject chứa 2 "loa". DontDestroyOnLoad = không bị xóa khi tải lại màn chơi,
    // nhờ vậy nhạc nền phát liên tục, không bị ngắt mỗi lần chơi lại.
    private static void EnsureSources()
    {
        if (sfxSource != null) return;

        GameObject obj = new GameObject("SoundManager");
        Object.DontDestroyOnLoad(obj);
        sfxSource = obj.AddComponent<AudioSource>();
        musicSource = obj.AddComponent<AudioSource>();
        musicSource.loop = true;
    }

    // Tải clip từ Resources/Sounds (chỉ tải 1 lần, lần sau lấy từ bộ nhớ đệm)
    private static AudioClip GetClip(string name)
    {
        if (cache.TryGetValue(name, out AudioClip clip)) return clip;
        clip = Resources.Load<AudioClip>("Sounds/" + name);
        if (clip == null) Debug.LogWarning("Không tìm thấy âm thanh: Resources/Sounds/" + name);
        cache[name] = clip;
        return clip;
    }

    // Phát 1 hiệu ứng âm thanh ngắn. PlayOneShot cho phép nhiều tiếng chồng lên nhau.
    public static void Play(string name, float volume = 1f)
    {
        EnsureSources();
        AudioClip clip = GetClip(name);
        if (clip != null) sfxSource.PlayOneShot(clip, volume * sfxVolume);
    }

    // Bật nhạc nền (nếu đang phát rồi thì thôi, không phát lại từ đầu)
    public static void PlayMusic()
    {
        EnsureSources();
        AudioClip clip = GetClip("music");
        if (clip == null || musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public static void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}
