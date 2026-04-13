using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRespawn : MonoBehaviour
{
    [Header("Random Respawn Points")]
    public Transform[] respawnPoints;

    [Header("Respawn Settings")]
    public float respawnYOffset = 0.2f;
    public bool avoidInactiveFloor = true;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Respawn()
    {
        Transform chosenPoint = GetRandomRespawnPoint();

        if (chosenPoint == null)
        {
            Debug.LogWarning("Tidak ada respawn point yang aman. Player tidak di-respawn.");
            return;
        }

        controller.enabled = false;
        transform.position = chosenPoint.position + Vector3.up * respawnYOffset;
        controller.enabled = true;
    }

    private Transform GetRandomRespawnPoint()
    {
        if (respawnPoints == null || respawnPoints.Length == 0)
        {
            return null;
        }

        if (!avoidInactiveFloor)
        {
            int randomIndex = Random.Range(0, respawnPoints.Length);
            return respawnPoints[randomIndex];
        }

        List<Transform> validPoints = GetValidRespawnPoints();

        if (validPoints.Count == 0)
        {
            return null;
        }

        int validIndex = Random.Range(0, validPoints.Count);
        return validPoints[validIndex];
    }

    private List<Transform> GetValidRespawnPoints()
    {
        List<Transform> validPoints = new List<Transform>();

        for (int i = 0; i < respawnPoints.Length; i++)
        {
            Transform point = respawnPoints[i];

            if (point == null)
                continue;

            RespawnPointLink link = point.GetComponent<RespawnPointLink>();
            FallingFloor floor = null;

            if (link != null)
            {
                floor = link.linkedFloor;
            }

            if (floor == null)
            {
                floor = point.GetComponentInParent<FallingFloor>();
            }

            // Kalau point tidak terhubung ke floor manapun, anggap valid
            if (floor == null)
            {
                validPoints.Add(point);
                continue;
            }

            // Hanya boleh respawn jika floor benar-benar aman
            if (floor.IsSafeForRespawn)
            {
                validPoints.Add(point);
            }
        }

        return validPoints;
    }
}