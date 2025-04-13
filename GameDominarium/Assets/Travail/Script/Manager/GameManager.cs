using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private float _timeStop;

    [SerializeField] private float _timeReprise;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            gameObject.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    public void StopBloc()
    {
        Destroy(gameObject); 
        Platform[] platforms = FindObjectsOfType<Platform>();
        foreach (var platform in platforms)
        {
            platform.StopTemporarily(_timeStop, _timeReprise);
        }
        Spawner spawner = FindObjectOfType<Spawner>();
        if (spawner != null)
        {
            spawner.StopTemporarily(_timeStop, _timeReprise);
        }
    }
}