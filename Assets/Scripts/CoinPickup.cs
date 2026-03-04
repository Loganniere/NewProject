using System.Collections;
using UnityEngine;

/// <summary>
/// Coin collectible: rotates, bobs, plays effects and notifies GameManager on pickup.
/// </summary>
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobHeight = 0.3f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;

    private Vector3 startPosition;
    private bool collected;
    private Renderer coinRenderer;
    private Collider coinCollider;

    private void Awake()
    {
        coinRenderer = GetComponent<Renderer>();
        coinCollider = GetComponent<Collider>();
        startPosition = transform.position;
    }

    private void Update()
    {
        if (collected) return;

        // Rotate around Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        collected = true;

        // Disable visuals and physics immediately
        if (coinRenderer) coinRenderer.enabled = false;
        if (coinCollider) coinCollider.enabled = false;

        // Notify game manager
        GameManager.Instance?.CollectCoin(scoreValue);

        // Spawn particle effect
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Play sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Destroy after a short delay (to let effects play)
        Destroy(gameObject, 1f);
    }
}
