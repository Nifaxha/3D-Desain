using System.Collections.Generic;
using UnityEngine;

public class FloorFallBalanceManager : MonoBehaviour
{
    public static FloorFallBalanceManager Instance { get; private set; }

    [Header("Global Balance")]
    [Tooltip("Maksimum lantai yang boleh sibuk sekaligus.")]
    public int maxBusyFloorsGlobal = 2;

    [Tooltip("Jika rasio lantai sibuk sudah melewati ini, trigger baru dibatasi.")]
    [Range(0.1f, 1f)]
    public float maxBusyRatio = 0.6f;

    [Header("Recent Trigger Balance")]
    [Tooltip("Jendela waktu untuk menghitung trigger yang baru saja terjadi.")]
    public float recentTriggerWindow = 1.5f;

    [Tooltip("Maksimum trigger baru dalam recentTriggerWindow.")]
    public int maxRecentTriggers = 2;

    [Header("Nearby Balance")]
    [Tooltip("Cek area sekitar kandidat floor.")]
    public float nearbyCheckRadius = 6f;

    [Tooltip("Maksimum floor sibuk di sekitar kandidat.")]
    public int maxNearbyBusyFloors = 2;

    [Header("Retry Suggestion")]
    [Tooltip("Delay saran jika trigger ditolak.")]
    public float blockedRetryDelay = 0.5f;

    private readonly List<FallingFloor> registeredFloors = new List<FallingFloor>();
    private readonly List<float> recentTriggerTimes = new List<float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterFloor(FallingFloor floor)
    {
        if (floor == null) return;

        if (!registeredFloors.Contains(floor))
        {
            registeredFloors.Add(floor);
        }
    }

    public void UnregisterFloor(FallingFloor floor)
    {
        if (floor == null) return;
        registeredFloors.Remove(floor);
    }

    public void NotifyFloorTriggered(FallingFloor floor)
    {
        CleanupOldTriggerTimes();
        recentTriggerTimes.Add(Time.time);
    }

    public bool CanTriggerFloor(FallingFloor candidate, out float suggestedDelay, out string reason)
    {
        suggestedDelay = blockedRetryDelay;
        reason = string.Empty;

        if (candidate == null)
        {
            reason = "Candidate floor null.";
            return false;
        }

        CleanupMissingFloors();
        CleanupOldTriggerTimes();

        int totalFloors = registeredFloors.Count;
        int busyFloors = GetBusyFloorCount();

        if (busyFloors >= maxBusyFloorsGlobal)
        {
            reason = "Busy floor global sudah mencapai batas.";
            return false;
        }

        if (totalFloors > 0)
        {
            float busyRatio = (float)busyFloors / totalFloors;
            if (busyRatio >= maxBusyRatio)
            {
                reason = "Rasio floor sibuk terlalu tinggi.";
                return false;
            }
        }

        if (recentTriggerTimes.Count >= maxRecentTriggers)
        {
            reason = "Trigger floor dalam waktu berdekatan terlalu banyak.";
            return false;
        }

        int nearbyBusy = CountNearbyBusyFloors(candidate.transform.position, nearbyCheckRadius, candidate);
        if (nearbyBusy >= maxNearbyBusyFloors)
        {
            reason = "Floor sibuk di area sekitar sudah terlalu banyak.";
            return false;
        }

        return true;
    }

    public int GetBusyFloorCount()
    {
        CleanupMissingFloors();

        int count = 0;
        for (int i = 0; i < registeredFloors.Count; i++)
        {
            if (registeredFloors[i] != null && registeredFloors[i].IsBusy)
            {
                count++;
            }
        }

        return count;
    }

    public int CountNearbyBusyFloors(Vector3 center, float radius, FallingFloor ignoreFloor = null)
    {
        CleanupMissingFloors();

        int count = 0;
        float sqrRadius = radius * radius;

        for (int i = 0; i < registeredFloors.Count; i++)
        {
            FallingFloor floor = registeredFloors[i];
            if (floor == null) continue;
            if (floor == ignoreFloor) continue;
            if (!floor.IsBusy) continue;

            float sqrDistance = (floor.transform.position - center).sqrMagnitude;
            if (sqrDistance <= sqrRadius)
            {
                count++;
            }
        }

        return count;
    }

    private void CleanupOldTriggerTimes()
    {
        float minTime = Time.time - recentTriggerWindow;

        for (int i = recentTriggerTimes.Count - 1; i >= 0; i--)
        {
            if (recentTriggerTimes[i] < minTime)
            {
                recentTriggerTimes.RemoveAt(i);
            }
        }
    }

    private void CleanupMissingFloors()
    {
        for (int i = registeredFloors.Count - 1; i >= 0; i--)
        {
            if (registeredFloors[i] == null)
            {
                registeredFloors.RemoveAt(i);
            }
        }
    }
}