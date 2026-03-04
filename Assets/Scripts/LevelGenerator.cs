using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Procedurally generates the level layout at runtime:
/// platforms, coins, obstacles, and start/end markers.
/// All objects are created with primitives so no external assets are required.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private int platformCount = 12;
    [SerializeField] private float platformSpacing = 6f;
    [SerializeField] private float heightVariation = 2f;
    [SerializeField] private Vector2 platformSizeRange = new Vector2(3f, 8f);

    [Header("Coins")]
    [SerializeField] private int coinsPerPlatform = 1;

    [Header("Colors")]
    [SerializeField] private Color platformColor = new Color(0.3f, 0.7f, 0.3f);
    [SerializeField] private Color coinColor = new Color(1f, 0.85f, 0f);
    [SerializeField] private Color playerColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color skyColor = new Color(0.4f, 0.7f, 1f);

    private List<GameObject> generatedObjects = new List<GameObject>();

    private void Start()
    {
        Camera.main.backgroundColor = skyColor;
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        CreateStartPlatform();
        CreateMainPlatforms();
        SetupPlayer();
        SetupLights();
        SetupDeathZone();

        // Tell GameManager the total coin count
        int total = FindObjectsOfType<CoinPickup>().Length;
        // GameManager reads totalCoins from inspector, but we log it for validation
        Debug.Log($"[LevelGenerator] Generated {total} coins across {platformCount} platforms.");
    }

    // ------- Builders -------

    private void CreateStartPlatform()
    {
        GameObject start = CreatePlatform(Vector3.zero, new Vector3(10f, 1f, 10f), "StartPlatform");

        // Respawn marker
        GameObject respawn = new GameObject("RespawnPoint");
        respawn.transform.position = new Vector3(0f, 2f, 0f);
        generatedObjects.Add(respawn);

        // Wire respawn point to GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            var field = typeof(GameManager).GetField("respawnPoint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(gm, respawn.transform);
        }
    }

    private void CreateMainPlatforms()
    {
        Vector3 lastPos = Vector3.zero;
        float yOffset = 0f;

        for (int i = 1; i <= platformCount; i++)
        {
            float angle = i * 30f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * platformSpacing * i * 0.4f;
            float z = Mathf.Sin(angle) * platformSpacing * i * 0.4f;
            yOffset += Random.Range(-heightVariation, heightVariation);
            yOffset = Mathf.Clamp(yOffset, -3f, 8f);

            float sizeX = Random.Range(platformSizeRange.x, platformSizeRange.y);
            float sizeZ = Random.Range(platformSizeRange.x, platformSizeRange.y);
            Vector3 pos = new Vector3(x, yOffset, z);
            GameObject platform = CreatePlatform(pos, new Vector3(sizeX, 1f, sizeZ));

            // 30% chance of moving platform
            if (Random.value < 0.3f)
            {
                PlatformMover mover = platform.AddComponent<PlatformMover>();
            }

            // Spawn coins on this platform
            SpawnCoinsOnPlatform(pos, sizeX, sizeZ);

            lastPos = pos;
        }
    }

    private GameObject CreatePlatform(Vector3 position, Vector3 size, string objName = "Platform")
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.position = position;
        go.transform.localScale = size;
        go.layer = LayerMask.NameToLayer("Default");

        // Assign tag for ground check
        go.tag = "Ground";

        ApplyColor(go, platformColor);
        generatedObjects.Add(go);
        return go;
    }

    private void SpawnCoinsOnPlatform(Vector3 platformCenter, float sizeX, float sizeZ)
    {
        for (int c = 0; c < coinsPerPlatform; c++)
        {
            float rx = Random.Range(-sizeX * 0.4f, sizeX * 0.4f);
            float rz = Random.Range(-sizeZ * 0.4f, sizeZ * 0.4f);
            Vector3 coinPos = new Vector3(
                platformCenter.x + rx,
                platformCenter.y + 1.5f,
                platformCenter.z + rz
            );

            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.position = coinPos;
            coin.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.tag = "Coin";

            ApplyColor(coin, coinColor);

            // Replace box collider with trigger
            Destroy(coin.GetComponent<Collider>());
            SphereCollider sc = coin.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 1f;

            coin.AddComponent<CoinPickup>();
            generatedObjects.Add(coin);
        }
    }

    private void SetupPlayer()
    {
        // Find or create player sphere
        PlayerController existing = FindObjectOfType<PlayerController>();
        if (existing != null) return;

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 2f, 0f);
        player.transform.localScale = Vector3.one;
        player.tag = "Player";

        ApplyColor(player, playerColor);

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;

        PlayerController pc = player.AddComponent<PlayerController>();

        // Set up camera to follow player
        Camera cam = Camera.main;
        if (cam != null)
        {
            CameraController cc = cam.GetComponent<CameraController>();
            if (cc == null) cc = cam.gameObject.AddComponent<CameraController>();

            var targetField = typeof(CameraController).GetField("target",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetField?.SetValue(cc, player.transform);

            var camField = typeof(PlayerController).GetField("cameraTransform",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            camField?.SetValue(pc, cam.transform);
        }

        generatedObjects.Add(player);
    }

    private void SetupLights()
    {
        // Directional light (sun)
        GameObject sun = new GameObject("Sun");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        generatedObjects.Add(sun);

        // Ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.5f, 0.6f, 0.7f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = skyColor;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.015f;
    }

    private void SetupDeathZone()
    {
        // Invisible kill plane below the level
        GameObject killPlane = new GameObject("DeathZone");
        killPlane.tag = "DeathZone";
        killPlane.transform.position = new Vector3(0f, -15f, 0f);
        BoxCollider bc = killPlane.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(500f, 1f, 500f);
        generatedObjects.Add(killPlane);
    }

    // ------- Helpers -------

    private static void ApplyColor(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Glossiness", 0.5f);
        r.material = mat;
    }
}
