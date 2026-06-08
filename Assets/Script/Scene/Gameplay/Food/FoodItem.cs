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
    public FoodType foodType = FoodType.Normal;

    [Tooltip("Poin untuk makanan biasa yang valid dan bukan request aktif.")]
    public int normalPickupScore = 1;

    public float lifeTime = 6f;

    [Header("Wrong Food / Trash Food")]
    [Tooltip("Centang jika item ini adalah makanan salah, misalnya tulang ikan / tulang ayam.")]
    public bool isBadFood = false;

    [Tooltip("Damage ke player jika item ini makanan salah.")]
    public int badFoodDamage = 1;

    [Header("NPC Request Integration")]
    [Tooltip("Identitas makanan ini untuk sistem request NPC.")]
    public OrderFoodData orderFoodData;

    [Tooltip("Jika aktif, makanan ini bisa dicek ke request NPC.")]
    public bool useForNpcRequest = true;

    [Tooltip("Kalau item tidak cocok request aktif, item tetap dihancurkan atau tidak.")]
    public bool destroyEvenIfNotNeededByRequest = true;

    [Header("Fall Speed Control")]
    public bool useControlledFallSpeed = false;
    public float baseFallSpeed = 5f;

    private bool isCollected = false;
    private Collider itemCollider;
    private Renderer[] itemRenderers;
    private Rigidbody itemRigidbody;
    private NPCRequestQueueManager cachedRequestManager;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRenderers = GetComponentsInChildren<Renderer>(true);
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

        // 1. Kalau ini makanan salah / sampah, langsung damage
        if (isBadFood || foodType == FoodType.Bomb)
        {
            collector.CollectWrongFood(this, badFoodDamage);
            DisableItemPhysicsAndVisual();
            Destroy(gameObject);
            return;
        }

        bool matchedRequest = false;

        // 2. Coba cocokkan ke request aktif
        if (useForNpcRequest && orderFoodData != null && cachedRequestManager != null)
        {
            matchedRequest = cachedRequestManager.TryConsumeFood(orderFoodData);
        }

        // 3. Kalau cocok request:
        //    - bunyi benar
        //    - TIDAK tambah score di sini
        //    - karena score request sudah ditangani NPCRequestQueueManager
        if (matchedRequest)
        {
            collector.CollectCorrectRequestFood(this);
        }
        else
        {
            // 4. Kalau tidak cocok request tapi ini makanan valid,
            //    tetap dianggap benar sebagai makanan biasa
            collector.CollectNormalFood(this, normalPickupScore);
        }

        DisableItemPhysicsAndVisual();

        if (useForNpcRequest)
        {
            if (matchedRequest || destroyEvenIfNotNeededByRequest)
            {
                Destroy(gameObject);
            }
            else
            {
                RestoreItem();
            }

            return;
        }

        Destroy(gameObject);
    }

    private void DisableItemPhysicsAndVisual()
    {
        if (itemCollider != null)
            itemCollider.enabled = false;

        if (itemRenderers != null)
        {
            for (int i = 0; i < itemRenderers.Length; i++)
            {
                if (itemRenderers[i] != null)
                    itemRenderers[i].enabled = false;
            }
        }

        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
            itemRigidbody.isKinematic = true;
        }
    }

    private void RestoreItem()
    {
        isCollected = false;

        if (itemCollider != null)
            itemCollider.enabled = true;

        if (itemRenderers != null)
        {
            for (int i = 0; i < itemRenderers.Length; i++)
            {
                if (itemRenderers[i] != null)
                    itemRenderers[i].enabled = true;
            }
        }

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
        }
    }
}