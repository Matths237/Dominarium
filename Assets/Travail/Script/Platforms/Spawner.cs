using UnityEngine;
using System.Collections.Generic; 

public class Spawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [Tooltip("Référence vers l'asset PlatformDatabase contenant les types de plateformes.")]
    public PlatformDatabase platformDatabase;

    public float spawnRate = 2f;
    [Range(0f, 1f)]
    public float screenWidthPercentage = 0.7f; 
    public float minHorizontalSpacing = 2f;
    public float spawnHeightOffset = 1.1f; 
    private float nextSpawnTime = 0f;
    private float screenWorldWidth; 
    private float lastSpawnX;


    void Start()
    {
        if (platformDatabase == null)
        {
            this.enabled = false; 
            return;
        }

        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        screenWorldWidth = 2f * halfWidth * screenWidthPercentage;

        spawnRate = Mathf.Abs(spawnRate); 


        lastSpawnX = Camera.main.transform.position.x;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnPlatform(); 
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnPlatform()
    {
        if (platformDatabase == null || platformDatabase.platformTypes.Count == 0) return; 

        PlatformData selectedPlatformData = platformDatabase.ChoosePlatform();
        if (selectedPlatformData == null || selectedPlatformData.platformPrefab == null)
        {
            return; 
        }

        float randomX;
        int attempts = 0;
        const int maxAttempts = 20;
        float cameraLeftEdge = Camera.main.transform.position.x - (screenWorldWidth / 2f);
        float cameraRightEdge = Camera.main.transform.position.x + (screenWorldWidth / 2f);

        do
        {
            randomX = Random.Range(cameraLeftEdge, cameraRightEdge);
            attempts++;
        } while (attempts > 1 && Mathf.Abs(randomX - lastSpawnX) < minHorizontalSpacing && attempts <= maxAttempts);
        Vector3 spawnPositionViewport = new Vector3(0.5f, spawnHeightOffset, Mathf.Abs(transform.position.z - Camera.main.transform.position.z));
        Vector3 spawnPositionWorld = Camera.main.ViewportToWorldPoint(spawnPositionViewport);
        spawnPositionWorld.x = randomX;
        spawnPositionWorld.z = 0; 

        GameObject newPlatformObject = Instantiate(selectedPlatformData.platformPrefab, spawnPositionWorld, Quaternion.identity);

        Platform platformComponent = newPlatformObject.GetComponent<Platform>();
        if (platformComponent != null)
        {

            platformComponent.SetSpeed(selectedPlatformData.fallSpeed);
        }

        lastSpawnX = randomX;
    }
}