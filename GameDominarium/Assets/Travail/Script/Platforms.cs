using UnityEngine;

public class Platforms : MonoBehaviour
{
    public float speed = 4;
    void Update()
    {
        transform.position -= transform.up * speed * Time.deltaTime;
    }
}
