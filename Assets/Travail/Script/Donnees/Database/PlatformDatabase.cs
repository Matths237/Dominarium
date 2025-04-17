using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "PlatformDatabase", menuName = "Gameplay/Platform Database")]
public class PlatformDatabase : ScriptableObject
{
    public List<PlatformData> platformTypes = new List<PlatformData>();

    private bool probabilitiesNormalized = false;
    private float totalProbability = 0f;

    private void OnValidate()
    {
        NormalizeProbabilities();
    }

    private void NormalizeProbabilities()
    {
        if (platformTypes == null || platformTypes.Count == 0)
        {
            totalProbability = 0;
            probabilitiesNormalized = false;
            return;
        }

        totalProbability = platformTypes.Sum(data => data.spawnProbability);

        if (totalProbability <= 0)
        {
             Debug.LogWarning("La somme des probabilités dans PlatformDatabase est <= 0. Aucune plateforme ne pourra spawner par probabilité.", this);
             probabilitiesNormalized = false;
             return;
        }

        probabilitiesNormalized = true; 
    }

    public PlatformData ChoosePlatform()
    {
        if (!probabilitiesNormalized || totalProbability <= 0f || platformTypes.Count == 0)
        {
            NormalizeProbabilities();
            if (!probabilitiesNormalized || totalProbability <= 0f || platformTypes.Count == 0)
            {
                 Debug.LogError("Impossible de choisir une plateforme : aucune donnée valide ou somme de probabilité nulle dans PlatformDatabase.", this);
                 return null; 
            }
        }

        float randomNumber = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        foreach (PlatformData platformData in platformTypes)
        {
            cumulativeProbability += platformData.spawnProbability;
            if (randomNumber <= cumulativeProbability)
            {
                if (platformData.platformPrefab == null)
                {
                    Debug.LogWarning($"PlatformData '{platformData.name}' a une probabilité mais pas de Prefab assigné.", platformData);
                    continue; 
                }
                return platformData;
            }
        }
        for (int i = platformTypes.Count - 1; i >= 0; i--)
        {
            if (platformTypes[i].spawnProbability > 0 && platformTypes[i].platformPrefab != null)
                return platformTypes[i];
        }

        Debug.LogError("N'a pas pu choisir de plateforme malgré une probabilité totale > 0.", this);
        return null; 
    }
}