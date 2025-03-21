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
    public float spawnRate = 1f;
    public float screenWidthPercentage = 0.8f;
    public float cubeLifeTime = 5f;
    public float minHorizontalSpacing = 1f; // Nouvelle variable

    private float nextSpawnTime = 0f;
    private float screenWidth;
    private float lastSpawnX; // Memorise la position du dernier spawn

    void Start()
    {
        screenWidth = Camera.main.ViewportToWorldPoint(new Vector3(screenWidthPercentage, 0, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3((1 - screenWidthPercentage), 0, 0)).x / 2;
        spawnRate = Mathf.Abs(spawnRate);
        NormalizeProbabilities();
        lastSpawnX = 0; // Initialiser
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
        float randomX;
        bool validPosition = false;

        // Trouver une position valide jusqu'à un nombre limite d'essais
        int attempts = 0;
        do
        {
            randomX = Random.Range(-screenWidth, screenWidth);
            //Verifier si la position n'est pas trop proche de la derniere
            if (Mathf.Abs(randomX - lastSpawnX) >= minHorizontalSpacing)
            {
                validPosition = true;
            }
            attempts++;
            if(attempts > 20){ // Eviter une boucle infinie si la position est toujours invalide
                randomX = Random.Range(-screenWidth, screenWidth);
                validPosition = true; // Forcer une position meme si pas parfaite
                Debug.LogWarning("Impossible de trouver une position espacée après plusieurs tentatives.");
            }
        } while (!validPosition);
        

        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.1f, 0));
        spawnPosition.x = randomX;
        spawnPosition.z = 0;

        GameObject cubeToSpawn = ChooseCube();

        if (cubeToSpawn != null)
        {
            GameObject newCube = Instantiate(cubeToSpawn, spawnPosition, Quaternion.identity);
            Destroy(newCube, cubeLifeTime);
        }
        else
        {
            Debug.LogWarning("Aucun cube à spawner ! Vérifiez les probabilités.");
        }

        lastSpawnX = randomX; // Mettre a jour la position du dernier spawn
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

        return null;
    }

    void NormalizeProbabilities()
    {
        float totalProbability = 0f;
        foreach (CubeProbability cubeProb in cubePrefabs)
        {
            totalProbability += cubeProb.probability;
        }

        if (totalProbability > 0f)
        {
            for (int i = 0; i < cubePrefabs.Count; i++)
            {
                CubeProbability cubeProb = cubePrefabs[i];
                cubeProb.probability /= totalProbability;
                cubePrefabs[i] = cubeProb;
            }
        }
        else
        {
            Debug.LogWarning("Somme des probabilités est zéro.  Assurez-vous d'avoir des probabilités valides.");
        }
    }
}