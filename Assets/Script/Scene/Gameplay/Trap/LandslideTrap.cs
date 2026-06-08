using System.Collections;
using UnityEngine;

public class LandslideTrap : MonoBehaviour
{
    [Header("References")]
    public Transform landslideVisual;
    public Renderer landslideRenderer;
    public Transform boulderSpawnPoint;
    public Transform boulderTargetDirection;
    public GameObject boulderPrefab;

    [Header("Warning")]
    public float warningDuration = 2f;
    public Color normalColor = new Color(0.45f, 0.3f, 0.15f);
    public Color warningColor = Color.yellow;

    [Header("Landslide Animation")]
    public float slideDistance = 2f;
    public float slideDuration = 1f;
    public float resetDelay = 1.5f;

    [Header("Boulder Spawn")]
    public int minBoulderCount = 1;
    public int maxBoulderCount = 3;
    public float delayBetweenBoulders = 0.3f;
    public float boulderSpawnSpreadX = 1.5f;
    public float boulderSpawnSpreadZ = 0.5f;

    [Header("Boulder Target Area")]
    public bool useRandomTargetArea = true;
    public float targetAreaWidth = 8f;
    public float targetAreaLength = 4f;
    public float targetAreaHeight = 1f; // Tinggi Gizmo yang baru kita buat sebelumnya
    public bool drawTargetAreaGizmo = true;

    private Vector3 landslideStartPos;
    private Vector3 landslideEndPos;
    private Material landslideMaterial;
    private bool isBusy = false;

    public bool IsBusy => isBusy;

    private void Awake()
    {
        if (landslideVisual != null)
        {
            landslideStartPos = landslideVisual.position;
            landslideEndPos = landslideStartPos + Vector3.down * slideDistance;
        }

        if (landslideRenderer == null && landslideVisual != null)
        {
            landslideRenderer = landslideVisual.GetComponent<Renderer>();
        }

        if (landslideRenderer != null)
        {
            landslideMaterial = landslideRenderer.material;
            landslideMaterial.color = normalColor;
        }
    }

    public void TriggerTrap()
    {
        if (!isBusy)
        {
            StartCoroutine(LandslideRoutine());
        }
    }

    private IEnumerator LandslideRoutine()
    {
        isBusy = true;

        float warningTimer = 0f;
        while (warningTimer < warningDuration)
        {
            if (landslideMaterial != null)
            {
                float blink = Mathf.PingPong(Time.time * 4f, 1f);
                landslideMaterial.color = Color.Lerp(normalColor, warningColor, blink);
            }

            warningTimer += Time.deltaTime;
            yield return null;
        }

        if (landslideMaterial != null)
        {
            landslideMaterial.color = warningColor;
        }

        if (AudioManager.Instance != null)
        {
            Vector3 sfxPos = landslideVisual != null ? landslideVisual.position : transform.position;
            AudioManager.Instance.PlayVolcanoErupt(sfxPos);
        }

        if (landslideVisual != null)
        {
            float slideTimer = 0f;
            Vector3 startPos = landslideVisual.position;

            while (slideTimer < slideDuration)
            {
                slideTimer += Time.deltaTime;
                float t = slideTimer / slideDuration;
                landslideVisual.position = Vector3.Lerp(startPos, landslideEndPos, t);
                yield return null;
            }

            landslideVisual.position = landslideEndPos;
        }

        int spawnCount = Random.Range(minBoulderCount, maxBoulderCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnBoulder();
            yield return new WaitForSeconds(delayBetweenBoulders);
        }

        yield return new WaitForSeconds(resetDelay);

        if (landslideVisual != null)
        {
            float returnTimer = 0f;
            Vector3 returnStart = landslideVisual.position;

            while (returnTimer < slideDuration)
            {
                returnTimer += Time.deltaTime;
                float t = returnTimer / slideDuration;
                landslideVisual.position = Vector3.Lerp(returnStart, landslideStartPos, t);
                yield return null;
            }

            landslideVisual.position = landslideStartPos;
        }

        if (landslideMaterial != null)
        {
            landslideMaterial.color = normalColor;
        }

        isBusy = false;
    }

    private void SpawnBoulder()
    {
        if (boulderPrefab == null || boulderSpawnPoint == null || boulderTargetDirection == null)
            return;

        Vector3 spawnOffset = new Vector3(
            Random.Range(-boulderSpawnSpreadX, boulderSpawnSpreadX),
            0f,
            Random.Range(-boulderSpawnSpreadZ, boulderSpawnSpreadZ)
        );

        Vector3 spawnPos = boulderSpawnPoint.position + spawnOffset;

        GameObject boulderObject = Instantiate(boulderPrefab, spawnPos, Quaternion.identity);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBoulderFall(spawnPos);
        }

        RollingBoulder rollingBoulder = boulderObject.GetComponent<RollingBoulder>();
        if (rollingBoulder != null)
        {
            Vector3 targetPoint = GetRandomTargetPoint();
            Vector3 direction = (targetPoint - spawnPos).normalized;
            rollingBoulder.Initialize(direction);

            // --- BARU: Mengirimkan info Gizmo Target Area ke Batu ---
            Vector3 targetAreaSize = new Vector3(targetAreaWidth, targetAreaHeight, targetAreaLength);
            rollingBoulder.SetTargetArea(boulderTargetDirection.position, targetAreaSize);
        }
    }

    private Vector3 GetRandomTargetPoint()
    {
        if (boulderTargetDirection == null)
            return transform.position;

        if (!useRandomTargetArea)
            return boulderTargetDirection.position;

        Vector3 center = boulderTargetDirection.position;

        float randomX = Random.Range(-targetAreaWidth * 0.5f, targetAreaWidth * 0.5f);
        float randomZ = Random.Range(-targetAreaLength * 0.5f, targetAreaLength * 0.5f);

        Vector3 randomOffset = new Vector3(randomX, 0f, randomZ);
        return center + randomOffset;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawTargetAreaGizmo || boulderTargetDirection == null)
            return;

        Gizmos.color = Color.cyan;

        Vector3 center = boulderTargetDirection.position;
        Vector3 size = new Vector3(targetAreaWidth, targetAreaHeight, targetAreaLength);

        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.2f);
    }
}