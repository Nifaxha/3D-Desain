using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement3D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Character Controller Settings")]
    public bool autoSetupController = true;
    public float controllerRadius = 0.5f;
    public float controllerHeight = 2f;
    public Vector3 controllerCenter = new Vector3(0f, 1f, 0f);
    public float skinWidth = 0.08f;
    public float stepOffset = 0.3f;
    public float slopeLimit = 45f;
    public float minMoveDistance = 0.001f;

    // --- TAMBAHAN UNTUK ABILITY ---
    [Header("Ability Status")]
    public bool canDoubleJump = false;
    public bool isBalloon = false;
    private int jumpCount = 0;
    // ------------------------------

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // --- TAMBAHAN UNTUK ANIMASI ---
    private Animator animator;
    // ------------------------------

    void Awake()
    {
        SetupController();
        
        // --- TAMBAHAN UNTUK ANIMASI ---
        // Mencari komponen Animator di object Player atau child object-nya (seperti Armature)
        animator = GetComponentInChildren<Animator>();
        // ------------------------------
    }

    void OnValidate()
    {
        SetupController();
    }

    void SetupController()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        if (autoSetupController && controller != null)
        {
            controller.radius = controllerRadius;
            controller.height = controllerHeight;
            controller.center = controllerCenter;
            controller.skinWidth = skinWidth;
            controller.stepOffset = stepOffset;
            controller.slopeLimit = slopeLimit;
            controller.minMoveDistance = minMoveDistance;
        }
    }

    void Update()
    {
        if (controller == null)
        {
            SetupController();
            if (controller == null) return;
        }

        isGrounded = controller.isGrounded;

        // Reset gravitasi dan lompatan saat menyentuh tanah
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0; // Reset jatah lompat
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(horizontal, 0f, vertical).normalized;

        // --- TAMBAHAN UNTUK ANIMASI ---
        // Cek apakah player sedang bergerak (input tombol ditekan)
        bool isMoving = move.magnitude > 0.1f;
        
        // Kirim status isMoving (true/false) ke parameter "IsWalking" di Animator
        if (animator != null)
        {
            animator.SetBool("IsWalking", isMoving);
        }
        // ------------------------------

        if (isMoving) // <-- Diubah menggunakan variabel isMoving
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * move.z + camRight * move.x;

            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // --- LOGIKA DOUBLE JUMP ---
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpCount = 1;

                // --- TAMBAHAN TRIGGER ANIMASI LOMPAT ---
                if (animator != null)
                {
                    animator.SetTrigger("JumpTrigger");
                }
                // ---------------------------------------
            }
            else if (canDoubleJump && jumpCount < 2)
            {
                // Lompatan kedua saat di udara
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpCount = 2;

                // --- TAMBAHAN TRIGGER ANIMASI DOUBLE JUMP ---
                if (animator != null)
                {
                    animator.SetTrigger("JumpTrigger");
                }
                // --------------------------------------------
            }
        }
        // --------------------------

        // --- LOGIKA BALLOON (Melayang) ---
        float appliedGravity = gravity;
        if (isBalloon && velocity.y < 0) 
        {
            // Jika status balon aktif dan player sedang jatuh, kurangi efek gravitasi drastis
            appliedGravity = gravity * 0.2f; 
        }
        // ---------------------------------

        velocity.y += appliedGravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}