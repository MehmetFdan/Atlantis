using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Oyun içerisindeki tüm ses işlemlerini yöneten sınıf
/// </summary>
public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip[] clips;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
        public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        public float minDistance = 1f;
        public float maxDistance = 50f;
    }
    
    [Header("Sound Settings")]
    [Tooltip("Önceden tanımlanmış ses efektleri")]
    [SerializeField] private SoundEffect[] soundEffects;
    
    [Tooltip("Ses havuzundaki maksimum ses kaynağı sayısı")]
    [SerializeField] private int maxAudioSources = 20;
    
    [Header("Music Settings")]
    [Tooltip("Oyun müzikleri")]
    [SerializeField] private AudioClip[] musicTracks;
    
    [Tooltip("Müzik ses seviyesi")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;
    
    // Singleton instance
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<AudioManager>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("AudioManager");
                    _instance = singletonObject.AddComponent<AudioManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            
            return _instance;
        }
    }
    
    // Kullanılmayan ses kaynakları havuzu
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    
    // Aktif ses kaynakları listesi
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    
    // Müzik için özel ses kaynağı
    private AudioSource musicSource;
    
    // Ses efektleri sözlüğü (hızlı erişim için)
    private Dictionary<string, SoundEffect> soundEffectDictionary = new Dictionary<string, SoundEffect>();
    
    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ses efektleri sözlüğünü doldur
        foreach (SoundEffect effect in soundEffects)
        {
            soundEffectDictionary[effect.name] = effect;
        }
        
        // Ses kaynakları havuzunu oluştur
        InitializeAudioSourcePool();
        
        // Müzik ses kaynağını oluştur
        InitializeMusicSource();
    }
    
    /// <summary>
    /// Ses kaynakları havuzunu oluşturur
    /// </summary>
    private void InitializeAudioSourcePool()
    {
        for (int i = 0; i < maxAudioSources; i++)
        {
            GameObject audioSourceObj = new GameObject("AudioSource_" + i);
            audioSourceObj.transform.parent = transform;
            
            AudioSource source = audioSourceObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            
            audioSourcePool.Enqueue(source);
        }
    }
    
    /// <summary>
    /// Müzik ses kaynağını oluşturur
    /// </summary>
    private void InitializeMusicSource()
    {
        GameObject musicSourceObj = new GameObject("MusicSource");
        musicSourceObj.transform.parent = transform;
        
        musicSource = musicSourceObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f; // 2D sound
    }
    
    /// <summary>
    /// Ses efekti çalar (2D, pozisyonsuz)
    /// </summary>
    /// <param name="soundName">Ses efektinin adı</param>
    public void PlaySound(string soundName)
    {
        PlaySound(soundName, Vector3.zero, false);
    }
    
    /// <summary>
    /// Ses efekti çalar (3D, pozisyonlu)
    /// </summary>
    /// <param name="soundName">Ses efektinin adı</param>
    /// <param name="position">Dünya pozisyonu</param>
    public void PlaySound(string soundName, Vector3 position)
    {
        PlaySound(soundName, position, true);
    }
    
    /// <summary>
    /// Ses efekti çalar
    /// </summary>
    /// <param name="soundName">Ses efektinin adı</param>
    /// <param name="position">Dünya pozisyonu</param>
    /// <param name="spatialize">3D ses kullanılsın mı?</param>
    public void PlaySound(string soundName, Vector3 position, bool spatialize)
    {
        if (!soundEffectDictionary.TryGetValue(soundName, out SoundEffect effect))
        {
            Debug.LogWarning($"Sound effect '{soundName}' not found!");
            return;
        }
        
        if (effect.clips == null || effect.clips.Length == 0)
        {
            Debug.LogWarning($"No audio clips assigned to sound effect '{soundName}'!");
            return;
        }
        
        // Ses klibini rastgele seç
        AudioClip clipToPlay = effect.clips[Random.Range(0, effect.clips.Length)];
        
        // Havuzdan ses kaynağı al
        AudioSource source = GetAudioSourceFromPool();
        if (source == null)
        {
            Debug.LogWarning("Could not get audio source from pool. Pool might be exhausted.");
            return;
        }
        
        // Ses kaynağını ayarla
        source.clip = clipToPlay;
        source.volume = effect.volume;
        source.pitch = Random.Range(effect.pitch * 0.9f, effect.pitch * 1.1f); // Biraz rastgeleleştir
        source.loop = effect.loop;
        
        // 3D ses ayarları
        if (spatialize)
        {
            source.spatialBlend = effect.spatialBlend;
            source.minDistance = effect.minDistance;
            source.maxDistance = effect.maxDistance;
            source.transform.position = position;
        }
        else
        {
            source.spatialBlend = 0f; // 2D
        }
        
        // Sesi çal
        source.Play();
        
        // Eğer ses döngüsel değilse, tamamlandığında havuza geri döndür
        if (!effect.loop)
        {
            StartCoroutine(ReturnToPoolWhenFinished(source));
        }
        else
        {
            // Döngüsel sesin durdurulması için bir yol sağla
            // Burada döngüsel sesler için referans tutabilirsiniz
        }
    }
    
    /// <summary>
    /// Müzik parçası çalar
    /// </summary>
    /// <param name="trackIndex">Müzik parçası indeksi</param>
    /// <param name="fadeInDuration">Müziğin yavaşça girme süresi (saniye)</param>
    public void PlayMusic(int trackIndex, float fadeInDuration = 1f)
    {
        if (musicTracks == null || trackIndex < 0 || trackIndex >= musicTracks.Length)
        {
            Debug.LogWarning("Invalid music track index or no music tracks defined!");
            return;
        }
        
        StartCoroutine(FadeMusicTrack(musicTracks[trackIndex], fadeInDuration));
    }
    
    /// <summary>
    /// Müziği durdurur
    /// </summary>
    /// <param name="fadeOutDuration">Müziğin yavaşça çıkma süresi (saniye)</param>
    public void StopMusic(float fadeOutDuration = 1f)
    {
        StartCoroutine(FadeMusicVolume(0f, fadeOutDuration));
    }
    
    /// <summary>
    /// Havuzdan kullanılabilir bir ses kaynağı getirir
    /// </summary>
    private AudioSource GetAudioSourceFromPool()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource source = audioSourcePool.Dequeue();
            activeAudioSources.Add(source);
            return source;
        }
        else
        {
            // Havuzda ses kaynağı kalmadıysa, çalmayı bitirmiş olan bir kaynağı bul
            for (int i = 0; i < activeAudioSources.Count; i++)
            {
                if (!activeAudioSources[i].isPlaying)
                {
                    return activeAudioSources[i];
                }
            }
            
            // Hala bulunamadıysa, null döndür
            return null;
        }
    }
    
    /// <summary>
    /// Ses kaynağını havuza geri döndürür
    /// </summary>
    private System.Collections.IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        // Ses bitene kadar bekle
        yield return new WaitForSeconds(source.clip.length);
        
        // Ses kaynağını sıfırla ve havuza döndür
        source.clip = null;
        source.loop = false;
        
        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }
    
    /// <summary>
    /// Müzik parçasını değiştirmek için
    /// </summary>
    private System.Collections.IEnumerator FadeMusicTrack(AudioClip newTrack, float fadeDuration)
    {
        // Eğer hali hazırda müzik çalıyorsa, sesini kapa
        if (musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeMusicVolume(0f, fadeDuration));
        }
        
        // Yeni parçayı ayarla
        musicSource.clip = newTrack;
        musicSource.Play();
        
        // Sesi yavaşça aç
        yield return StartCoroutine(FadeMusicVolume(musicVolume, fadeDuration));
    }
    
    /// <summary>
    /// Müzik ses seviyesini değiştirmek için
    /// </summary>
    private System.Collections.IEnumerator FadeMusicVolume(float targetVolume, float fadeDuration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }
        
        musicSource.volume = targetVolume;
        
        // Eğer ses sıfıra indiyse, müziği durdur
        if (targetVolume <= 0f)
        {
            musicSource.Stop();
        }
    }
} 