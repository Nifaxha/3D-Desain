using System.Collections;
using UnityEngine;

public class FallingFloor : MonoBehaviour
{
    public enum FloorState
    {
        Stable,
        Warning,
        Falling,
        Waiting,
        Rising
    }

    [Header("References")]
    public Renderer[] meshRenderers;
    public Collider floorCollider;
    public FloorFallBalanceManager balanceManager;

    [Header("Timing")]
    public float warningDuration = 2f;
    public float fallDuration = 2f;
    public float respawnDelay = 1f;
    public float riseDuration = 1.5f;

    [Header("Shake Settings")]
    public float shakeAmount = 0.1f;
    public float shakeSpeed = 20f;

    [Header("Fall Settings")]
    public bool useVisualDrop = true;
    public float dropDistance = 5f;
    public float dropSpeed = 8f;

    [Header("Rise Settings")]
    public bool useSmoothRise = true;
    public float riseSpeed = 4f;

    [Header("Balance Control")]
    public bool useBalanceManager = true;
    public bool autoFindBalanceManager = true;
    public bool retryWhenBlocked = true;
    public float customRetryDelay = 0.5f;

    private Vector3 originalPosition;
    private Vector3 fallenPosition;

    private FloorState currentState = FloorState.Stable;
    private bool triggerPending = false;
    private Coroutine retryCoroutine;
    private Coroutine floorRoutineCoroutine;

    public FloorState CurrentState => currentState;

    public bool IsBusy =>
        currentState == FloorState.Warning ||
        currentState == FloorState.Falling ||
        currentState == FloorState.Waiting ||
        currentState == FloorState.Rising;

    public bool IsActive => currentState == FloorState.Stable;

    public bool IsSafeForRespawn =>
        currentState == FloorState.Stable &&
        floorCollider != null &&
        floorCollider.enabled &&
        transform.position.y >= originalPosition.y - 0.05f;

    private void Awake()
    {
        originalPosition = transform.position;
        fallenPosition = originalPosition + Vector3.down * dropDistance;

        if (floorCollider == null)
            floorCollider = GetComponent<Collider>();

        if (autoFindBalanceManager && balanceManager == null)
            balanceManager = FindObjectOfType<FloorFallBalanceManager>();

        if (useBalanceManager && balanceManager != null)
            balanceManager.RegisterFloor(this);
    }

    private void OnDestroy()
    {
        if (balanceManager != null)
            balanceManager.UnregisterFloor(this);
    }

    public void TriggerFloor()
    {
        if (currentState != FloorState.Stable)
            return;

        if (triggerPending)
            return;

        if (!useBalanceManager || balanceManager == null)
        {
            StartFloorNow();
            return;
        }

        float suggestedDelay;
        string blockReason;
        bool canTrigger = balanceManager.CanTriggerFloor(this, out suggestedDelay, out blockReason);

        if (canTrigger)
        {
            StartFloorNow();
        }
        else if (retryWhenBlocked)
        {
            float delay = Mathf.Max(customRetryDelay, suggestedDelay);

            if (retryCoroutine != null)
                StopCoroutine(retryCoroutine);

            retryCoroutine = StartCoroutine(RetryTrigger(delay));
        }
    }

    private IEnumerator RetryTrigger(float delay)
    {
        triggerPending = true;
        yield return new WaitForSeconds(delay);
        triggerPending = false;
        retryCoroutine = null;

        if (currentState == FloorState.Stable)
        {
            TriggerFloor();
        }
    }

    private void StartFloorNow()
    {
        if (currentState != FloorState.Stable)
            return;

        if (useBalanceManager && balanceManager != null)
        {
            balanceManager.NotifyFloorTriggered(this);
        }

        if (floorRoutineCoroutine != null)
            StopCoroutine(floorRoutineCoroutine);

        floorRoutineCoroutine = StartCoroutine(FloorRoutine());
    }

    private IEnumerator FloorRoutine()
    {
        currentState = FloorState.Warning;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEarthquake(transform.position);
        }

        float timer = 0f;
        while (timer < warningDuration)
        {
            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            float z = Mathf.Cos(Time.time * shakeSpeed) * shakeAmount;

            transform.position = originalPosition + new Vector3(x, 0f, z);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;

        currentState = FloorState.Falling;

        if (floorCollider != null)
            floorCollider.enabled = false;

        if (useVisualDrop)
        {
            while (Vector3.Distance(transform.position, fallenPosition) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    fallenPosition,
                    dropSpeed * Time.deltaTime
                );

                yield return null;
            }

            transform.position = fallenPosition;
        }
        else
        {
            SetRenderersVisible(false);
            transform.position = fallenPosition;
        }

        currentState = FloorState.Waiting;

        yield return new WaitForSeconds(fallDuration);
        yield return new WaitForSeconds(respawnDelay);

        if (!useVisualDrop)
        {
            SetRenderersVisible(true);
        }

        currentState = FloorState.Rising;

        if (useSmoothRise)
        {
            if (riseDuration > 0f)
            {
                float riseTimer = 0f;
                Vector3 startPos = transform.position;

                while (riseTimer < riseDuration)
                {
                    riseTimer += Time.deltaTime;
                    float t = riseTimer / riseDuration;
                    transform.position = Vector3.Lerp(startPos, originalPosition, t);
                    yield return null;
                }

                transform.position = originalPosition;
            }
            else
            {
                while (Vector3.Distance(transform.position, originalPosition) > 0.02f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        originalPosition,
                        riseSpeed * Time.deltaTime
                    );

                    yield return null;
                }

                transform.position = originalPosition;
            }
        }
        else
        {
            transform.position = originalPosition;
        }

        if (floorCollider != null)
            floorCollider.enabled = true;

        currentState = FloorState.Stable;
        floorRoutineCoroutine = null;
    }

    private void SetRenderersVisible(bool visible)
    {
        if (meshRenderers == null || meshRenderers.Length == 0)
            return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null)
            {
                meshRenderers[i].enabled = visible;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (balanceManager == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, balanceManager.nearbyCheckRadius);
    }
}