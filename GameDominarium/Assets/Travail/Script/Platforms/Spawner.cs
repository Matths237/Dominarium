using UnityEngine;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct CubeProbability
    {
        public GameObject cubePrefab;
        [Range(0f, 1f)]
        public float probability;
    }

    public List<CubeProbability> cubePrefabs;
    public float spawnRate = 2f;
    public float screenWidthPercentage = 1f;
    public float minHorizontalSpacing = 2f;

    private float nextSpawnTime = 0f;
    private float screenWidth;
    private float lastSpawnX;

    [SerializeField] private bool isPaused = false;
    [SerializeField] private float pauseDuration;
    [SerializeField] private float resumeTime;

    void Start()
    {
        screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(screenWidthPercentage, 0, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3((1 - screenWidthPercentage), 0, 0)).x / 2;
        spawnRate = Mathf.Abs(spawnRate);
        NormalizeProbabilities();
        lastSpawnX = Camera.main.transform.position.x; 
    }

    void Update()
    {
        if (isPaused)
        {
            if (Time.time >= resumeTime)
            {
                Resume();
            }
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnCube();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnCube()
    {
        float randomX;
        bool validPosition = false;
        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            randomX = Random.Range(-screenWidth, screenWidth);

            if (Mathf.Abs(randomX - lastSpawnX) >= minHorizontalSpacing || attempts == 0) 
            {
                validPosition = true;
            }
            attempts++;
            if(attempts > maxAttempts){
                validPosition = true; 
            }
        } while (!validPosition && attempts <= maxAttempts);


        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, Mathf.Abs(transform.position.z - Camera.main.transform.position.z)));
        spawnPosition.x = randomX;
        spawnPosition.z = 0; 

        GameObject cubeToSpawn = ChooseCube();

        if (cubeToSpawn != null)
        {
            Instantiate(cubeToSpawn, spawnPosition, Quaternion.identity);
            
        }

        lastSpawnX = randomX;
    }

    GameObject ChooseCube()
    {
        float randomNumber = Random.value;
        float cumulativeProbability = 0f;

        foreach (CubeProbability cubeProb in cubePrefabs)
        {
            cumulativeProbability += cubeProb.probability;
            if (randomNumber <= cumulativeProbability)
            {
                return cubeProb.cubePrefab;
            }
        }

        if (cubePrefabs.Count > 0 && cubePrefabs[cubePrefabs.Count - 1].probability > 0)
             return cubePrefabs[cubePrefabs.Count - 1].cubePrefab; 

        return null;
    }

    void NormalizeProbabilities()
    {
        float totalProbability = 0f;
        foreach (CubeProbability cubeProb in cubePrefabs)
        {
            totalProbability += cubeProb.probability;
        }

        if (totalProbability > 0f && Mathf.Abs(totalProbability - 1.0f) > 0.001f)
        {
             List<CubeProbability> normalizedList = new List<CubeProbability>();
             for (int i = 0; i < cubePrefabs.Count; i++)
             {
                 CubeProbability cubeProb = cubePrefabs[i];
                 cubeProb.probability /= totalProbability;
                 normalizedList.Add(cubeProb);
             }
             cubePrefabs = normalizedList;
        }
    }


    public void StopTemporarily(float stopTime, float resumeTime)
    {
        isPaused = true;
        pauseDuration = stopTime;
        this.resumeTime = Time.time + resumeTime;
    }


    private void Resume()
    {
        isPaused = false;
    }
}