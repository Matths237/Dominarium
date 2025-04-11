using UnityEngine;

public class PlatformBasic : MonoBehaviour
{
    public bool IsMoved {private get; set;} = true;
    public float speed = 4;
    void Update()
    {
        if (!IsMoved)
            return;
        transform.position -= transform.up * speed * Time.deltaTime;
    }

    
}
