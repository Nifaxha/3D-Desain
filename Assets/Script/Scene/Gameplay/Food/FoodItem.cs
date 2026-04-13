using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public enum FoodType
    {
        Normal,
        Burger,
        Golden,
        Bomb,
        SpeedBoost,
        Shield
    }

    public static float GlobalFallSpeedMultiplier = 1f;

    [Header("Food Data")]
    public FoodType foodType;
    public int scoreValue = 1;
    public float lifeTime = 6f;

    [Header("NPC Order Integration")]
    public bool useNpcOrderFood = false;
    public OrderFoodData orderFoodData;
    public bool skipDefaultCollectLogicWhenNpcFood = true;
    public bool destroyEvenIfNotNeededByRequest = true;

    [Header("Fall Speed Control")]
    public bool useControlledFallSpeed = false;
    public float baseFallSpeed = 5f;

    private bool isCollected = false;
    private Collider itemCollider;
    private Renderer itemRenderer;
    private Rigidbody itemRigidbody;
    private NPCRequestQueueManager cachedRequestManager;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRenderer = GetComponent<Renderer>();
        itemRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cachedRequestManager = FindObjectOfType<NPCRequestQueueManager>();
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!useControlledFallSpeed)
            return;

        if (isCollected)
            return;

        float speed = baseFallSpeed * Mathf.Max(0.01f, GlobalFallSpeedMultiplier);
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
            return;

        PlayerCollector collector = other.GetComponent<PlayerCollector>();
        if (collector == null)
            return;

        isCollected = true;

        bool matchedRequest = false;

        if (useNpcOrderFood && orderFoodData != null && cachedRequestManager != null)
        {
            matchedRequest = cachedRequestManager.TryConsumeFood(orderFoodData);
        }

        if (!useNpcOrderFood || !skipDefaultCollectLogicWhenNpcFood)
        {
            collector.Collect(this);
        }

        DisableItemPhysicsAndVisual();

        if (!useNpcOrderFood)
        {
            Destroy(gameObject);
            return;
        }

        if (matchedRequest)
        {
            Destroy(gameObject);
            return;
        }

        if (destroyEvenIfNotNeededByRequest)
        {
            Destroy(gameObject);
        }
        else
        {
            isCollected = false;

            if (itemCollider != null)
                itemCollider.enabled = true;

            if (itemRenderer != null)
                itemRenderer.enabled = true;

            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = false;
            }
        }
    }

    private void DisableItemPhysicsAndVisual()
    {
        if (itemCollider != null)
            itemCollider.enabled = false;

        if (itemRenderer != null)
            itemRenderer.enabled = false;

        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
            itemRigidbody.isKinematic = true;
        }
    }
}