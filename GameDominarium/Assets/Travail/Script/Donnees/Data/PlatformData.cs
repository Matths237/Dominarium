using UnityEngine;

[CreateAssetMenu(fileName = "NewPlatformData", menuName = "Gameplay/Platform Data")]
public class PlatformData : ScriptableObject
{
    [Header("Spawning")]
    public GameObject platformPrefab; 
    [Range(0f, 1f)]
    public float spawnProbability = 1f; 

    [Header("Gameplay")]
    public float fallSpeed = 4f; 

}