using UnityEngine;

using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private int coinsCollected = 0;
    public int CoinsCollected => coinsCollected;
    private UnityEvent<int> OnCoinsChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            OnCoinsChanged?.Invoke(coinsCollected);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoin(int amount)
    {
        if (amount > 0)
        {
            coinsCollected += amount;
            OnCoinsChanged?.Invoke(coinsCollected);
        }
    }
}