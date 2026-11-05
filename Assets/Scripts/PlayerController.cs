using UnityEngine;

/// <summary>Keyboard platform movement and a mouse-driven, orbiting arm.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float jumpSpeed = 8.5f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Visuals and collision")]
    [SerializeField] private Transform arm;
    [SerializeField] private float bodyRadius = 0.4f;
    [SerializeField] private float armLength = 0.18f;
    [SerializeField] private LayerMask groundLayers = 1 << 6;

    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private Camera viewCamera;
    private Vector2 spawnPoint;
    private float moveInput;
    private float lastGroundedTime = float.NegativeInfinity;
    private float jumpRequestedTime = float.NegativeInfinity;

    public void Configure(Transform armTransform) => arm = armTransform;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        spawnPoint = body.position;
        viewCamera = Camera.main;
    }

    private void Update()
    {
        moveInput = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                  - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            jumpRequestedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.R) || body.position.y < -8f)
            Respawn();

        AimArm();
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        if (grounded && body.linearVelocity.y <= 0.1f)
            lastGroundedTime = Time.time;

        Vector2 velocity = body.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        if (Time.time - jumpRequestedTime <= jumpBufferTime && Time.time - lastGroundedTime <= coyoteTime)
        {
            velocity.y = jumpSpeed;
            jumpRequestedTime = float.NegativeInfinity;
            lastGroundedTime = float.NegativeInfinity;
        }
        body.linearVelocity = velocity;
    }

    private bool IsGrounded()
    {
        // A narrow box cast below the capsule checks only the surfaces, never the player.
        Bounds bounds = capsule.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.08f);
        return Physics2D.BoxCast(origin, new Vector2(bounds.size.x * 0.8f, 0.08f), 0f,
            Vector2.down, 0.12f, groundLayers).collider != null;
    }

    private void AimArm()
    {
        if (arm == null) return;
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera == null) return;

        Vector3 screen = Input.mousePosition;
        screen.z = -viewCamera.transform.position.z;
        Vector3 cursor = viewCamera.ScreenToWorldPoint(screen);
        Vector2 direction = (Vector2)(cursor - transform.position);
        if (direction.sqrMagnitude < 0.0001f) return;
        direction.Normalize();
        // Slight overlap hides any seam between the round body and its arm.
        arm.localPosition = direction * (bodyRadius + armLength * 0.5f - 0.02f);
        arm.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void Respawn()
    {
        body.position = spawnPoint;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        lastGroundedTime = float.NegativeInfinity;
        jumpRequestedTime = float.NegativeInfinity;
    }
}
