using UnityEngine;

public class PlatformBasic : MonoBehaviour
{
    public float speed = 4f;
    private float currentSpeed = 4f;
    private bool canMove = true;

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
        float originalSpeed = speed;
        currentSpeed = 0f;
        canMove = false;

        yield return new WaitForSeconds(pauseTime);

        canMove = true;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0f, originalSpeed, elapsed / fadeTime);
            yield return null;
        }

        currentSpeed = originalSpeed;
    }
}
