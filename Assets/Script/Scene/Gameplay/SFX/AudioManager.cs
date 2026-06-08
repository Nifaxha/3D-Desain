using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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

    [Header("3D Audio Settings")]
    public float spatialBlend = 1f;
    public float minDistance = 5f;
    public float maxDistance = 30f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Setup UI Audio Source
        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
        }
        uiSource.playOnAwake = false;
        uiSource.loop = false;
        uiSource.spatialBlend = 0f;
        uiSource.ignoreListenerPause = true;

        // Setup BGM Audio Source
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.playOnAwake = false;
        bgmSource.loop = true; // BGM harus di-loop
        bgmSource.spatialBlend = 0f; // BGM selalu 2D

        DontDestroyOnLoad(gameObject);
    }

    // --- BGM METHODS ---

    public void PlayMenuBGM()
    {
        PlayBGM(menuBGM, menuBGMVolume);
    }

    public void PlayGameplayBGM()
    {
        PlayBGM(gameplayBGM, gameplayBGMVolume);
    }

    private void PlayBGM(AudioClip clip, float volume)
    {
        if (clip == null || bgmSource == null) return;

        // Mencegah BGM restart jika track yang sama sudah dimainkan
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    // --- SFX METHODS ---

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
        source.spatialBlend = spatialBlend;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.playOnAwake = false;
        source.loop = false;

        source.Play();
        Destroy(tempAudio, clip.length + 0.1f);
    }
}