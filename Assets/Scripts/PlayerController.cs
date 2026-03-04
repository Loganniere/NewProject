using UnityEngine;

/// <summary>
/// Controls player movement, jumping, and physics interactions.
/// The player is a sphere that rolls around collecting coins.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float maxVelocity = 15f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.6f;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isDead;

    public static PlayerController Instance { get; private set; }

    public bool IsDead => isDead;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        if (isDead || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        CheckGrounded();
        HandleJump();
    }

    private void FixedUpdate()
    {
        if (isDead || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Move relative to camera direction
        Vector3 camForward = cameraTransform != null
            ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
            : Vector3.forward;
        Vector3 camRight = cameraTransform != null
            ? Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized
            : Vector3.right;

        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            rb.AddForce(moveDir * moveSpeed, ForceMode.Force);
        }

        // Clamp horizontal velocity
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > maxVelocity)
        {
            Vector3 limited = flatVel.normalized * maxVelocity;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
    }

    private void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(
            transform.position - Vector3.up * 0.5f,
            groundCheckRadius,
            groundLayer
        );
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        GameManager.Instance.OnPlayerDied();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Die();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position - Vector3.up * 0.5f, groundCheckRadius);
    }
}
