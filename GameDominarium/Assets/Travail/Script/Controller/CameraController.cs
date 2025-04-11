using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;

    [SerializeField] private Vector3 _velocity;

    [SerializeField] private Vector2 _sizeBoxCast;

    [SerializeField] private float _smoothTime = 0.1f;

    private void Awake()
    {
        _target = FindObjectOfType<PlayerController>().transform;
    }

    void Start()
    {
        
    }

    void Update()
    {
        var limiteMin = transform.TransformPoint(-_sizeBoxCast / 2);
        var limiteMax = transform.TransformPoint(_sizeBoxCast / 2);

        var isOutLeft = _target.position.x < limiteMin.x;
        var isOutRight = _target.position.x > limiteMax.x;
        var isOutUp = _target.position.y > limiteMax.y;
        var isOutDown = _target.position.y < limiteMin.y;

        float targetOutX = isOutLeft ? limiteMin.x : isOutRight ? limiteMax.x : transform.position.x; 
        float targetOutY = isOutDown ? limiteMin.y : isOutUp ? limiteMax.y : transform.position.y; 

        var targetOut = new Vector3(targetOutX, targetOutY, transform.position.z);
        var smoothPos = Vector3.SmoothDamp(transform.position, targetOut, ref _velocity, _smoothTime);

        Vector3 targetPos = new(smoothPos.x, smoothPos.y, transform.position.z);

        transform.position = targetPos;
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, _sizeBoxCast);
    }
}
