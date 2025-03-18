
using UnityEditor;
using UnityEngine;


public class PlayerController : MonoBehaviour 
{
    [Header("MOVE")]
    [SerializeField]private float _speedMove;


    [Header("JUMP")]
    [SerializeField]private float _forceJump;
    [SerializeField]private int _limitJump = 2;
    private int _currentJump;


    [Header("WALL")]
    [SerializeField]private LayerMask _detectWall;
    public float distanceDW = 1;



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
        
        if(Input.GetKeyDown(KeyCode.Space) && _currentJump < _limitJump)
            Jump();
    }



    void Move()
    {
        Vector3 direction = Input.GetAxisRaw("Horizontal") * Vector2.right;

        var hit = Physics2D.BoxCast(transform.position, Vector2.one, 0f, direction, distanceDW, _detectWall);
        
        if (hit.collider != null)
        return;

        if (!Input.GetKey(KeyCode.LeftControl))
            _spriteRend.flipX = Input.GetAxisRaw("Horizontal") < 0;
        _myTransform.position += direction * _speedMove * Time.deltaTime;
    }



    void Jump()
    {

        _myRgbd2D.linearVelocity = Vector2.up * _forceJump;
        _currentJump++;
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        _currentJump = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        _currentJump = 0;
    }
}
