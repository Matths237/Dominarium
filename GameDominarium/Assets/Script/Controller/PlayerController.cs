using UnityEngine;

public class PlayerController : MonoBehaviour 
{
    [Header("MOVE")]
    [SerializeField]private float speedMove;

    [Header("JUMP")]
    [SerializeField]private float forceJump;

    private Rigidbody2D _myRgbd2D;
    private Collider2D _myCollider2D;
    private SpriteRenderer _spriteRend;
    private Transform _myTransform;
    


    void Awake()
    {
        TryGetComponent(out _myTransform);
        TryGetComponent(out _myCollider2D);
        TryGetComponent(out _myRgbd2D);
        TryGetComponent(out _spriteRend);
    }


    void Update()
    {
        if(Input.GetButton("Horizontal"))
            Move();
        
        if(Input.GetKeyDown(KeyCode.Escape))
            Jump();
    }

    void Move()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
            _spriteRend.flipX = Input.GetAxis("Horizontal") < 0;
        _myTransform.position += Vector3.right * Input.GetAxisRaw("Horizontal") * speedMove * Time.deltaTime;
    }


    void Jump()
    {
        _myRgbd2D.linearVelocity = Vector2.up * forceJump;
    }
}
