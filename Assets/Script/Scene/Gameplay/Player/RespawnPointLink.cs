using UnityEngine;

public class RespawnPointLink : MonoBehaviour
{
    public FallingFloor linkedFloor;

    private void Awake()
    {
        if (linkedFloor == null)
        {
            linkedFloor = GetComponentInParent<FallingFloor>();
        }
    }
}