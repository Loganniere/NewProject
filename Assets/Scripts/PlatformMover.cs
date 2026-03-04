using UnityEngine;

/// <summary>
/// Moving platform that oscillates between two points.
/// Carries the player smoothly by reparenting on contact.
/// </summary>
public class PlatformMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    [SerializeField] private float speed = 2f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 pointA;
    private Vector3 pointB;
    private float t;
    private bool goingForward = true;

    private void Start()
    {
        pointA = transform.position;
        pointB = transform.position + moveOffset;
    }

    private void Update()
    {
        t += Time.deltaTime * speed * (goingForward ? 1f : -1f);

        if (t >= 1f) { t = 1f; goingForward = false; }
        if (t <= 0f) { t = 0f; goingForward = true; }

        float curved = moveCurve.Evaluate(t);
        transform.position = Vector3.Lerp(pointA, pointB, curved);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            collision.transform.SetParent(null);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 start = Application.isPlaying ? pointA : transform.position;
        Vector3 end = Application.isPlaying ? pointB : transform.position + moveOffset;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(start, 0.3f);
        Gizmos.DrawWireSphere(end, 0.3f);
    }
}
