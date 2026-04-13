using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandslideTrapManager : MonoBehaviour
{
    [Header("Trap References")]
    public List<LandslideTrap> traps = new List<LandslideTrap>();

    [Header("Spawn Logic")]
    public float minInterval = 6f;
    public float maxInterval = 10f;

    [Header("Trigger Count")]
    public int minTrapsPerWave = 1;
    public int maxTrapsPerWave = 1;

    [Header("Auto Find")]
    public bool autoCollectOnStart = true;

    private Coroutine loopCoroutine;

    private void Start()
    {
        if (autoCollectOnStart && traps.Count == 0)
        {
            AutoCollectTraps();
        }

        loopCoroutine = StartCoroutine(TrapLoop());
    }

    private IEnumerator TrapLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            TriggerRandomTraps();
        }
    }

    public void AutoCollectTraps()
    {
        traps.Clear();

        LandslideTrap[] found = FindObjectsOfType<LandslideTrap>();
        for (int i = 0; i < found.Length; i++)
        {
            if (!traps.Contains(found[i]))
                traps.Add(found[i]);
        }
    }

    private void TriggerRandomTraps()
    {
        List<LandslideTrap> available = new List<LandslideTrap>();

        for (int i = 0; i < traps.Count; i++)
        {
            if (traps[i] != null && !traps[i].IsBusy)
                available.Add(traps[i]);
        }

        if (available.Count == 0)
            return;

        int amount = Random.Range(minTrapsPerWave, maxTrapsPerWave + 1);
        amount = Mathf.Clamp(amount, 1, available.Count);

        Shuffle(available);

        for (int i = 0; i < amount; i++)
        {
            available[i].TriggerTrap();
        }
    }

    private void Shuffle<T>(List<T> list)
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