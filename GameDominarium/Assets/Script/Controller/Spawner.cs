using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject cubePrefab;
    public float spawnRate = 1f;
    public float screenWidthPercentage = 0.8f;
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

        Destroy(newCube, cubeLifeTime);
    }
}