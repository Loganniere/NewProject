using UnityEngine;

/// <summary>
/// Smooth third-person camera that orbits around the player.
/// Supports mouse/touch input to rotate the view.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 8f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float scrollSensitivity = 5f;

    [Header("Vertical Limits")]
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 8f;
    [SerializeField] private float rotationSmoothing = 8f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionPadding = 0.3f;

    private float yaw;
    private float pitch = 20f;
    private Vector3 smoothPosition;
    private Quaternion smoothRotation;

    private void Start()
    {
        if (target == null && PlayerController.Instance != null)
            target = PlayerController.Instance.transform;

        // Initialize to current rotation
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        smoothPosition = transform.position;
        smoothRotation = transform.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCamera();
    }

    private void HandleInput()
    {
        // Right mouse button or always in locked mode
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        // Scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * scrollSensitivity;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
        }
    }

    private void UpdateCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * distance;

        // Camera collision: avoid clipping through walls
        float actualDistance = distance;
        Vector3 dir = (desiredPosition - target.position).normalized;
        if (Physics.SphereCast(target.position, collisionPadding, dir, out RaycastHit hit,
            distance, collisionMask))
        {
            actualDistance = hit.distance - collisionPadding;
        }

        Vector3 targetPosition = target.position - rotation * Vector3.forward * actualDistance;

        // Smooth follow
        smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, Time.deltaTime * positionSmoothing);
        smoothRotation = Quaternion.Slerp(smoothRotation, rotation, Time.deltaTime * rotationSmoothing);

        transform.position = smoothPosition;
        transform.rotation = smoothRotation;
    }
}
