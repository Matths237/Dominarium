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
    private const string PLAYER_TAG = "Player";

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        currentSpeed = baseSpeed;
    }

    void FixedUpdate()
    {
        if (Mathf.Approximately(currentSpeed, 0f) || !canMove)
            return;

        Vector2 newPosition = rb.position + Vector2.down * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    public void SetSpeed(float speed)
    {
        baseSpeed = speed;
        if (!isCurrentlyStopping)
        {
            currentSpeed = speed;
        }
    }

    public void StopTemporarily(float stopDuration, float resumeDelay)
    {
        if (stopCoroutineReference != null)
        {
            StopCoroutine(stopCoroutineReference);
        }
        stopCoroutineReference = StartCoroutine(StopAndResumeSequence(stopDuration, resumeDelay));
    }

    private Coroutine stopCoroutineReference = null;

    private IEnumerator StopAndResumeSequence(float pauseTime, float fadeTime)
    {
        isCurrentlyStopping = true;
        float originalSpeedBeforeStop = currentSpeed;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            ContactPoint2D[] contacts = collision.contacts;
            bool landedOnTop = false;
            foreach (ContactPoint2D contact in contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    landedOnTop = true;
                    break;
                }
            }

            if (landedOnTop)
            {
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(PLAYER_TAG))
        {
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}