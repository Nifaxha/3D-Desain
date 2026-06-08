using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    [Header("Box Settings")]
    public float abilityDuration = 10f; 

    [Header("Fall Settings")]
    public float fallSpeed = 5f; // Kecepatan jatuhnya box
    public float lifeTime = 8f;  // Box hilang jika tidak diambil

    private void Start()
    {
        // Box otomatis hilang setelah sekian detik jika tidak diambil
        Destroy(gameObject, lifeTime); 
    }

    private void Update()
    {
        // Logika untuk membuat box jatuh ke bawah
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCollector collector = other.GetComponent<PlayerCollector>();

            if (collector != null)
            {
                collector.ActivateRandomAbility(abilityDuration);
                Destroy(gameObject);
            }
        }
        // Tambahan: Hancur jika menyentuh tanah (misal tag "Ground")
        else if (other.CompareTag("Ground") || other.CompareTag("Lava"))
        {
            Destroy(gameObject);
        }
    }
}