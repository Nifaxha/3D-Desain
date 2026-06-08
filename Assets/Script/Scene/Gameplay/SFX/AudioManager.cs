using UnityEngine;
using UnityEngine.Audio; // <-- Tambahan wajib untuk menggunakan Audio Mixer

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer Groups")]
    [Tooltip("Masukkan grup BGM dari Audio Mixer ke sini")]
    public AudioMixerGroup bgmMixerGroup;
    [Tooltip("Masukkan grup SFX dari Audio Mixer ke sini")]
    public AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    public AudioSource uiSource;
    public AudioSource bgmSource; // AudioSource khusus untuk BGM

    [Header("Background Music (BGM)")]
    public AudioClip menuBGM;
    [Range(0f, 1f)] public float menuBGMVolume = 0.5f;
    
    public AudioClip gameplayBGM;
    [Range(0f, 1f)] public float gameplayBGMVolume = 0.5f;

    [Header("UI SFX")]
    public AudioClip uiClickClip;
    [Range(0f, 1f)] public float uiClickVolume = 1f;

    [Header("Gameplay SFX")]
    public AudioClip volcanoEruptClip;
    [Range(0f, 1f)] public float volcanoEruptVolume = 1f;

    public AudioClip correctFoodClip;
    [Range(0f, 1f)] public float correctFoodVolume = 1f;

    public AudioClip wrongFoodClip;
    [Range(0f, 1f)] public float wrongFoodVolume = 1f;

    public AudioClip respawnClip;
    [Range(0f, 1f)] public float respawnVolume = 1f;

    public AudioClip earthquakeClip;
    [Range(0f, 1f)] public float earthquakeVolume = 1f;

    public AudioClip boulderFallClip;
    [Range(0f, 1f)] public float boulderFallVolume = 1f;

   private void Awake()
    {
        // Memastikan hanya ada satu AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- TAMBAHAN BARU: Membuat AudioSource otomatis jika di Inspector masih kosong ---
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true; // Musik BGM otomatis diatur agar berulang (loop)
            bgmSource.playOnAwake = false;
        }
        
        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // Otomatis menyambungkan UI dan BGM source ke Mixer Group yang tepat saat game mulai
        if (bgmSource != null && bgmMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }
        
        if (uiSource != null && sfxMixerGroup != null)
        {
            uiSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    public void PlayMenuBGM()
    {
        if (bgmSource == null || menuBGM == null) return;
        bgmSource.clip = menuBGM;
        bgmSource.volume = menuBGMVolume;
        bgmSource.Play();
    }

    public void PlayGameplayBGM()
    {
        if (bgmSource == null || gameplayBGM == null) return;
        bgmSource.clip = gameplayBGM;
        bgmSource.volume = gameplayBGMVolume;
        bgmSource.Play();
    }

    public void PlayUIClick()
    {
        Play2D(uiClickClip, uiClickVolume);
    }

    public void PlayVolcanoErupt(Vector3 position)
    {
        Play3D(volcanoEruptClip, volcanoEruptVolume, position);
    }

    public void PlayCorrectFood(Vector3 position)
    {
        Play3D(correctFoodClip, correctFoodVolume, position);
    }

    public void PlayWrongFood(Vector3 position)
    {
        Play3D(wrongFoodClip, wrongFoodVolume, position);
    }

    public void PlayRespawn(Vector3 position)
    {
        Play3D(respawnClip, respawnVolume, position);
    }

    public void PlayEarthquake(Vector3 position)
    {
        Play3D(earthquakeClip, earthquakeVolume, position);
    }

    public void PlayBoulderFall(Vector3 position)
    {
        Play3D(boulderFallClip, boulderFallVolume, position);
    }

    private void Play2D(AudioClip clip, float volume)
    {
        if (clip == null || uiSource == null) return;
        uiSource.PlayOneShot(clip, volume);
    }

    private void Play3D(AudioClip clip, float volume, Vector3 position)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("Temp3DAudio_" + clip.name);
        tempAudio.transform.position = position;

        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f; // 1 = 3D (suara bergantung jarak)
        source.maxDistance = 500f; 
        source.rolloffMode = AudioRolloffMode.Linear;

        // --- INI BAGIAN TERPENTING: Menyambungkan suara 3D ke SFX Mixer ---
        if (sfxMixerGroup != null)
        {
            source.outputAudioMixerGroup = sfxMixerGroup;
        }

        source.Play();
        Destroy(tempAudio, clip.length);
    }
}