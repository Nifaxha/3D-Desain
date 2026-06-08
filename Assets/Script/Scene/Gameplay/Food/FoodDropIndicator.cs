using UnityEngine;

public class FoodDropIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [Tooltip("Masukkan Prefab tanda X Anda ke sini")]
    public GameObject indicatorPrefab;
    
    [Tooltip("Pilih layer tanah/lantai di sini agar tanda X tidak muncul di udara")]
    public LayerMask groundLayer;
    
    [Tooltip("Jarak tinggi tanda X dari tanah agar tidak berkedip (Z-fighting)")]
    public float yOffset = 0.05f;

    [Header("Sprite Settings")]
    [Tooltip("Centang ini jika prefab indikator adalah gambar 2D (.png) agar posisinya 'tidur' rata di tanah")]
    public bool is2DSprite = true;

    [Header("Custom Rotation")]
    [Tooltip("Ubah nilai X, Y, Z ini untuk menyesuaikan arah rotasi indikator secara manual")]
    public Vector3 customRotation = Vector3.zero;

    // Menyimpan referensi objek tanda X yang sudah dibuat di area game
    private GameObject spawnedIndicator;

    private void Start()
    {
        // Ketika makanan ini (prefab) muncul/spawn, buat juga tanda X-nya
        if (indicatorPrefab != null)
        {
            spawnedIndicator = Instantiate(indicatorPrefab);
            
            // Sembunyikan dulu sampai kita menemukan posisi tanah di bawah makanan
            spawnedIndicator.SetActive(false); 
        }
        else
        {
            Debug.LogWarning("Prefab Indikator X belum dimasukkan ke script pada " + gameObject.name);
        }
    }

    private void Update()
    {
        // Jika indikator belum ada, tidak perlu lanjut
        if (spawnedIndicator == null) return;

        // Menembakkan sinar (Raycast) lurus ke bawah dari posisi makanan
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Jika mengenai tanah, aktifkan tanda X
            if (!spawnedIndicator.activeSelf)
            {
                spawnedIndicator.SetActive(true);
            }

            // Pindahkan tanda X ke titik tempat sinar mengenai tanah, lalu naikkan sedikit (yOffset)
            spawnedIndicator.transform.position = hit.point + (Vector3.up * yOffset);

            // Hitung kemiringan tanah (jaga-jaga jika tanah Anda menanjak/menurun)
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // Tentukan rotasi dasar (jika gambar 2D, buat posisinya 'tidur' dengan rotasi X 90)
            Quaternion baseRotation = is2DSprite ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

            // Buat rotasi tambahan berdasarkan variabel customRotation yang kita atur di Inspector
            Quaternion extraRotation = Quaternion.Euler(customRotation.x, customRotation.y, customRotation.z);

            // Terapkan gabungan semua rotasi tersebut ke objek tanda X
            spawnedIndicator.transform.rotation = slopeRotation * baseRotation * extraRotation;
        }
        else
        {
            // Jika di bawahnya tidak ada tanah, sembunyikan tanda X
            if (spawnedIndicator.activeSelf)
            {
                spawnedIndicator.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        // Hancurkan tanda X saat makanan hancur/ditangkap
        if (spawnedIndicator != null)
        {
            Destroy(spawnedIndicator);
        }
    }
}