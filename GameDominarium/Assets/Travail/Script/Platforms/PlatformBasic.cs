using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Platform : MonoBehaviour
{
    public float speed = 4f;
    private float currentSpeed = 4f;
    private bool canMove = true;
    private const string DESPAWN_TAG = "Despawn"; 

    void Start()
    {
        currentSpeed = speed; 
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null && !rb.isKinematic)
        {

        }
    }

    void Update()
    {
        if (!canMove || Mathf.Approximately(currentSpeed, 0f))
            return;

        transform.position -= transform.up * currentSpeed * Time.deltaTime;
    }

    public void StopTemporarily(float pauseDuration, float resumeDuration)
    {
        StartCoroutine(SlowAndResume(pauseDuration, resumeDuration));
    }

    private System.Collections.IEnumerator SlowAndResume(float pauseTime, float fadeTime)
    {
        float originalSpeed = currentSpeed; 
        currentSpeed = 0f;


        yield return new WaitForSeconds(pauseTime);


        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0f, originalSpeed, elapsed / fadeTime);
            yield return null;
        }

        currentSpeed = originalSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(DESPAWN_TAG))
        {
            Destroy(gameObject);
        }
    }
}