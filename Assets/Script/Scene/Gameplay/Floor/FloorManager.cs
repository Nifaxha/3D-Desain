using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorManager : MonoBehaviour
{
    [Header("Floors")]
    public List<FallingFloor> floors = new List<FallingFloor>();

    [Header("Spawn Logic")]
    public float minInterval = 2f;
    public float maxInterval = 5f;

    [Header("Multiple Fall")]
    public int minFloorsToDrop = 1;
    public int maxFloorsToDrop = 2;

    [Header("Balance")]
    public FloorFallBalanceManager balanceManager;
    public bool autoFindBalanceManager = true;
    public bool respectBalanceLimits = true;
    public bool dynamicDropCount = true;

    [Tooltip("Batas tambahan dari sisi manager agar tidak terlalu agresif.")]
    public int managerHardMaxBusyFloors = 2;

    private Coroutine floorLoopCoroutine;

    private void Awake()
    {
        if (autoFindBalanceManager && balanceManager == null)
        {
            balanceManager = FindObjectOfType<FloorFallBalanceManager>();
        }
    }

    private void Start()
    {
        if (floors.Count == 0)
        {
            AutoCollectFloors();
        }

        floorLoopCoroutine = StartCoroutine(FloorLoop());
    }

    private IEnumerator FloorLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            TriggerRandomFloors();
        }
    }

    private void TriggerRandomFloors()
    {
        List<FallingFloor> availableFloors = floors.FindAll(f => f != null && !f.IsBusy);

        if (availableFloors.Count == 0)
            return;

        int amountToDrop = Random.Range(minFloorsToDrop, maxFloorsToDrop + 1);
        amountToDrop = Mathf.Min(amountToDrop, availableFloors.Count);

        if (dynamicDropCount)
        {
            int busyNow = GetBusyFloorCount();
            int allowedFreeSlots = Mathf.Max(0, managerHardMaxBusyFloors - busyNow);

            if (allowedFreeSlots <= 0)
                return;

            amountToDrop = Mathf.Min(amountToDrop, allowedFreeSlots);
        }

        if (amountToDrop <= 0)
            return;

        ShuffleList(availableFloors);

        int triggeredCount = 0;

        for (int i = 0; i < availableFloors.Count; i++)
        {
            FallingFloor floor = availableFloors[i];
            if (floor == null || floor.IsBusy)
                continue;

            if (respectBalanceLimits && balanceManager != null)
            {
                float suggestedDelay;
                string reason;
                bool canTrigger = balanceManager.CanTriggerFloor(floor, out suggestedDelay, out reason);

                if (!canTrigger)
                    continue;
            }

            floor.TriggerFloor();
            triggeredCount++;

            if (triggeredCount >= amountToDrop)
                break;
        }
    }

    public void AutoCollectFloors()
    {
        floors.Clear();
        FallingFloor[] foundFloors = FindObjectsOfType<FallingFloor>();

        for (int i = 0; i < foundFloors.Length; i++)
        {
            if (!floors.Contains(foundFloors[i]))
            {
                floors.Add(foundFloors[i]);
            }
        }
    }

    public int GetBusyFloorCount()
    {
        int count = 0;

        for (int i = 0; i < floors.Count; i++)
        {
            if (floors[i] != null && floors[i].IsBusy)
            {
                count++;
            }
        }

        return count;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}