using System.Collections;
using UnityEngine;

public class LanePlayerMovement : MonoBehaviour
{
    public Transform[] lanePositions;
    public float moveSpeed = 8f;
    public int currentLane = 2;

    private bool isMoving = false;

    void Update()
    {
        if (isMoving) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLane(-1);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveLane(1);
        }
    }

    void MoveLane(int direction)
    {
        int targetLane = currentLane + direction;

        if (targetLane < 0 || targetLane >= lanePositions.Length)
            return;

        currentLane = targetLane;
        StartCoroutine(MoveToPosition(lanePositions[currentLane].position));
    }

    IEnumerator MoveToPosition(Vector3 targetPos)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }
}