using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnRate = 1f;
    public float screenWidthPercentage = 0.8f;
    public float fallSpeed = 5f; 
    public float cubeLifeTime = 5f;
    private float nextSpawnTime = 0f;
    private float screenWidth;

    void Start()
    {
        screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(screenWidthPercentage, 0, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3((1 - screenWidthPercentage), 0, 0)).x / 2;
        spawnRate = Mathf.Abs(spawnRate);
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnCube();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnCube()
    {
        float randomX = Random.Range(-screenWidth, screenWidth);

        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0));
        spawnPosition.x = randomX;
        spawnPosition.z = 0;

        GameObject newCube = Instantiate(cubePrefab, spawnPosition, Quaternion.identity);

        Rigidbody rb = newCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;

            rb.linearVelocity = Vector3.down * fallSpeed;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.isKinematic = true; 
        }

        Destroy(newCube, cubeLifeTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float width = Camera.main.ViewportToWorldPoint(new Vector3(screenWidthPercentage, 0, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3((1 - screenWidthPercentage), 0, 0)).x;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 5, new Vector3(width, 1, 1));
    }
}