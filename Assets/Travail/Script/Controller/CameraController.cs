using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Shake Parameters")]
    [SerializeField] private float defaultShakeDuration = 0.15f;
    [SerializeField] private float defaultShakeMagnitude = 0.3f; 
    [SerializeField] private float dampingSpeed = 1.0f;

    private Vector3 initialPosition; 
    private float currentShakeDuration = 0f;  
    private float currentShakeMagnitude = 0f;

    void Awake()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        if (currentShakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
            transform.localPosition = initialPosition + shakeOffset;
            currentShakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            currentShakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    public void TriggerShake()
    {
        currentShakeDuration = defaultShakeDuration;
        currentShakeMagnitude = defaultShakeMagnitude;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        currentShakeMagnitude = magnitude;

    }
}