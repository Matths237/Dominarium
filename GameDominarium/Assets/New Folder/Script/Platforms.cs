using UnityEngine;

public class Platforms : MonoBehaviour
{
    public float speed;
    void Update()
    {
        transform.position -= transform.up * speed * Time.deltaTime;
    }
}
