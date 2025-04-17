using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Coin, StopTrigger }

    public PlayerController playerController;

    [Header("General Settings")]
    [SerializeField] private CollectibleType type = CollectibleType.Coin;
    [SerializeField] private string playerTag = "Player";

    [Header("Settings for Coin Type")]
    [SerializeField] private int coinValue = 1;

    [Header("Settings for Stop Trigger Type")]
    [SerializeField] private float stopDuration = 2.0f;
    [SerializeField] private float resumeDelay = 0.5f; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            switch (type)
            {
                case CollectibleType.Coin:
                    CollectCoin();
                    break;
                case CollectibleType.StopTrigger:
                    TriggerStopSequence(); 
                    break;
            }
            Destroy(gameObject);
        }
    }

    private void CollectCoin()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.AddCoin(coinValue);
        }
    }

    private void TriggerStopSequence()
    {
        Platform[] platforms = FindObjectsOfType<Platform>();
        if (platforms.Length > 0)
        {
            foreach (var platform in platforms)
            {
                platform.StopTemporarily(stopDuration, resumeDelay);
            }
        }

        Spawner spawner = FindObjectOfType<Spawner>(); 
        if (spawner != null)
        {
            spawner.PauseSpawning(stopDuration);
        }
    }

    public CollectibleType GetCollectibleType() => type;
    public int GetCoinValue() => coinValue;
    public float GetStopDuration() => stopDuration;
    public float GetResumeDelay() => resumeDelay;
}