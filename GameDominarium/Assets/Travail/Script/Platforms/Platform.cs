using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    [SerializeField] 
    private float baseSpeed = 4f;
    private float currentSpeed; 

    private bool canMove = true;
    private bool isCurrentlyStopping = false; 

    private const string DESPAWN_TAG = "Despawn";

    void Awake() 
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; 
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        if (Mathf.Approximately(currentSpeed, 0f))
            return;
        transform.Translate(Vector3.down * currentSpeed * Time.deltaTime, Space.World);
    }
    public void SetSpeed(float speed)
    {
        baseSpeed = speed;
        if (!isCurrentlyStopping)
        {
            currentSpeed = speed;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="stopDuration"></param>
    /// <param name="resumeDelay"></param>
    public void StopTemporarily(float stopDuration, float resumeDelay)
    {
        if (stopCoroutineReference != null)
        {
            StopCoroutine(stopCoroutineReference);
        }
        stopCoroutineReference = StartCoroutine(StopAndResumeSequence(stopDuration, resumeDelay));
    }

    private Coroutine stopCoroutineReference = null;

    /// <summary>
    /// </summary>
    /// <param name="pauseTime"></param>
    /// <param name="fadeTime"></param>
    private IEnumerator StopAndResumeSequence(float pauseTime, float fadeTime)
    {
        isCurrentlyStopping = true;
        currentSpeed = 0f;

        if (pauseTime > 0)
        {
            yield return new WaitForSeconds(pauseTime);
        }

        if (fadeTime > 0)
        {
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                currentSpeed = Mathf.Lerp(0f, baseSpeed, elapsed / fadeTime);
                yield return null; 
            }
        }

        currentSpeed = baseSpeed;
        isCurrentlyStopping = false;
        stopCoroutineReference = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(DESPAWN_TAG))
        {

            Destroy(gameObject);
        }
    }
}