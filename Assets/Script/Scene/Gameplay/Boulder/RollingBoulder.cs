using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollingBoulder : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 18f;
    public float maxSpeed = 12f;
    public float lifeTime = 8f;

    [Header("Damage")]
    public int damage = 1;
    public bool destroyOnHitPlayer = false;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isInitialized = false;
    private bool hasHitPlayer = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Vector3 direction)
    {
        moveDirection = direction.normalized;
        isInitialized = true;

        if (rb != null)
        {
            rb.AddForce(moveDirection * moveForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        if (!isInitialized || rb == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(moveDirection * moveForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHitPlayer) return;

        PlayerCollector player = collision.collider.GetComponent<PlayerCollector>();
        if (player != null)
        {
            hasHitPlayer = true;
            player.TakeDamage(damage);

            if (destroyOnHitPlayer)
            {
                Destroy(gameObject);
            }
        }
    }
}