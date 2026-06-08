using System.Collections;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [Header("Settings")]
    public float respawnDelay = 0.15f;

    private bool isProcessingFall = false;

    private void OnTriggerEnter(Collider other)
    {
        HandleFall(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleFall(other);
    }

    private void HandleFall(Collider other)
    {
        PlayerCollector collector = other.GetComponent<PlayerCollector>();
        PlayerRespawn respawn = other.GetComponent<PlayerRespawn>();

        // Cek jika yang jatuh adalah Player
        if (collector != null && respawn != null)
        {
            if (!isProcessingFall)
            {
                StartCoroutine(FallRoutine(collector, respawn));
            }
        }
        else
        {
            // Jika BUKAN Player (misal: batu, makanan, mystery box), maka Despawn/Hancurkan
            Destroy(other.gameObject);
        }
    }

    private IEnumerator FallRoutine(PlayerCollector collector, PlayerRespawn respawn)
    {
        isProcessingFall = true;

        collector.LoseLifeFromFall();

        if (!collector.isDead)
        {
            yield return new WaitForSeconds(respawnDelay);
            respawn.Respawn();
        }

        yield return new WaitForSeconds(0.2f);
        isProcessingFall = false;
    }
}