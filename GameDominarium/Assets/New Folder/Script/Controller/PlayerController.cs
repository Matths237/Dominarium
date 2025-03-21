using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("MOVE")]
    [SerializeField] private float _speedMove = 6;
    [SerializeField] private float _runSpeedMultiplier = 1.5f;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;

    [Header("JUMP")]
    [SerializeField] private float _minForceJump = 8;
    [SerializeField] private float _maxForceJump = 15;
    [SerializeField] private float _jumpHoldTime = 0.2f;
    [SerializeField] private int _limitJump = 2;
    private int _currentJump;
    private bool _hasJumpedSinceGrounded = false;
    private float _jumpStartTime;
    private bool _isJumping;

    [Header("WALL")]
    [SerializeField] private LayerMask _detectWall;
    [SerializeField] private float distanceDW = 0.6f;
    [SerializeField] private float _minWallJumpHorizontalForce = 5f;
    [SerializeField] private float _maxWallJumpHorizontalForce = 12f;
    [SerializeField] private float _minWallJumpVerticalForce = 8f;
    [SerializeField] private float _maxWallJumpVerticalForce = 15f;
    [SerializeField] private float _wallSlideSpeed = 2f;
    [SerializeField] private float _wallStickTime = 0.1f;
    [SerializeField] private bool _infiniteWallJumps = true;
    [SerializeField] private int _maxWallJumps = 3;
    private int _currentWallJumps;
    private bool _isWallSliding;
    private float _wallDirection;
    private float _timeSinceLastWallJump;
    private float _timeToStopStick;
    private bool _isGrounded;
    private bool _isWallJumping;

    [Header("COLOR")]
    [SerializeField] private Color _sprintColor = Color.red;  // Couleur lors du sprint
    [SerializeField] private Color _normalColor = Color.white; // Couleur normale
    [SerializeField] private Color _dashColor = Color.blue;  // Couleur lors du dash

    [Header("DASH")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftControl; // Nouvelle variable pour la touche de dash
    private float _dashTimer;
    private float _dashCooldownTimer;
    private bool _isDashing;
    private Vector2 _dashDirection;
    private Color _initialColor; // Stocke la couleur de départ

    private Rigidbody2D _myRgbd2D;
    private SpriteRenderer _spriteRend;
    private bool _isSprinting;

    void Awake()
    {
        _myRgbd2D = GetComponent<Rigidbody2D>();
        _spriteRend = GetComponent<SpriteRenderer>();

        // Assurez-vous que la couleur de départ est la couleur normale.
        _spriteRend.color = _normalColor;
        _initialColor = _normalColor; // Stocker la couleur de départ
    }

    void Update()
    {
        Move();
        WallSlide();
        HandleJumpInput();
        HandleDashInput();

        if (_timeSinceLastWallJump > 0)
        {
            _timeSinceLastWallJump -= Time.deltaTime;
        }

        if (_timeToStopStick > 0)
        {
            _timeToStopStick -= Time.deltaTime;
        }

        if (_isGrounded && !_hasJumpedSinceGrounded && _myRgbd2D.linearVelocity.y < -0.1f)
        {
            _currentJump++;
            _hasJumpedSinceGrounded = true;
        }

        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        // Maintient la couleur du dash si on est en train de dasher
        if (_isDashing)
        {
            _spriteRend.color = _dashColor;
        }
    }

    void FixedUpdate()
    {
        if (_myRgbd2D.linearVelocity.y < 0)
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        }
        else if (_myRgbd2D.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }

        if (_isDashing)
        {
            _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed;
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        _isSprinting = Input.GetKey(_runKey);
        float targetSpeed = moveInput * _speedMove * (_isSprinting ? _runSpeedMultiplier : 1);
        float currentSpeed = _myRgbd2D.linearVelocity.x;
        float accelerationRate = Mathf.Abs(moveInput) > 0.01f ? 50 : 50;
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        if (moveInput != 0)
            _spriteRend.flipX = moveInput < 0;

        if (_timeSinceLastWallJump > 0)
        {
            newSpeed *= 0.7f;
        }

        if (!_isDashing) // Ne pas affecter le mouvement pendant le dash
        {
            _myRgbd2D.linearVelocity = new Vector2(newSpeed, _myRgbd2D.linearVelocity.y);
        }

        // Gérer le changement de couleur lors du sprint
        if (_isSprinting)
        {
            _spriteRend.color = _sprintColor;
            _initialColor = _sprintColor;  // Mettre à jour _initialColor si le joueur sprint
        }
        else
        {
            _spriteRend.color = _normalColor;
            _initialColor = _normalColor; // Mettre à jour _initialColor si le joueur ne sprint pas
        }
    }

    void StartJump()
    {
        _jumpStartTime = Time.time;
        _isJumping = true;
        _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _minForceJump);
        _currentJump++;
    }

    void ContinueJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float jumpForce = Mathf.Lerp(_minForceJump, _maxForceJump, (Time.time - _jumpStartTime) / _jumpHoldTime);
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, jumpForce);
        }
        else
        {
            EndJump();
        }
    }

    void EndJump()
    {
        _isJumping = false;
        if (_myRgbd2D.linearVelocity.y > 0)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _myRgbd2D.linearVelocity.y * 0.5f);
        }
    }

    void WallSlide()
    {
        _isWallSliding = false;

        bool isTouchingWallRight = Physics2D.OverlapBox(new Vector2(_spriteRend.bounds.max.x + distanceDW / 2, _spriteRend.bounds.center.y), new Vector2(distanceDW, _spriteRend.bounds.size.y * 0.9f), 0, _detectWall);
        bool isTouchingWallLeft = Physics2D.OverlapBox(new Vector2(_spriteRend.bounds.min.x - distanceDW / 2, _spriteRend.bounds.center.y), new Vector2(distanceDW, _spriteRend.bounds.size.y * 0.9f), 0, _detectWall);


        if (isTouchingWallRight)
        {
            _isWallSliding = true;
            _wallDirection = -1;
        }
        else if (isTouchingWallLeft)
        {
            _isWallSliding = true;
            _wallDirection = 1;
        }

        if (_isWallSliding)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, Mathf.Clamp(_myRgbd2D.linearVelocity.y, -_wallSlideSpeed, float.MaxValue));
        }
    }


    void StartWallJump()
    {
        _timeSinceLastWallJump = 0.2f;
        _timeToStopStick = _wallStickTime;
        _currentWallJumps++;

        _jumpStartTime = Time.time;
        _isWallJumping = true;

        float horizontalForce = _wallDirection * _minWallJumpHorizontalForce;
        float verticalForce = _minWallJumpVerticalForce;
        _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce);
        _currentJump++;
        _hasJumpedSinceGrounded = true;
        EndJump();
    }


    void ContinueWallJump()
    {
        if (Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float horizontalForce = _wallDirection * Mathf.Lerp(_minWallJumpHorizontalForce, _maxWallJumpHorizontalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);
            float verticalForce = Mathf.Lerp(_minWallJumpVerticalForce, _maxWallJumpVerticalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);

            _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce);
        }
        else
        {
            EndWallJump();
        }
    }

    void EndWallJump()
    {
        _isWallJumping = false;
        if (_myRgbd2D.linearVelocity.y > 0)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _myRgbd2D.linearVelocity.y * 0.5f);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGrounded = true;
            _currentJump = 0;
            _currentWallJumps = 0;
            _hasJumpedSinceGrounded = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _isGrounded = false;
        }
    }

    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isWallSliding && (_infiniteWallJumps || _currentWallJumps < _maxWallJumps))
            {
                if (_timeToStopStick <= 0)
                {
                    StartWallJump();
                }
            }
            else if (_currentJump < _limitJump)
            {
                StartJump();
                _hasJumpedSinceGrounded = true;
            }
        }

        if (Input.GetKey(KeyCode.Space) && _isJumping)
        {
            ContinueJump();
        }

        if (Input.GetKey(KeyCode.Space) && _isWallJumping)
        {
            ContinueWallJump();
        }


        if (Input.GetKeyUp(KeyCode.Space))
        {
            EndJump();
            EndWallJump();
        }
    }

    void HandleDashInput()
    {
        if (Input.GetKeyDown(_dashKey) && _dashCooldownTimer <= 0) // Utilisation de _dashKey
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (horizontalInput == 0 && verticalInput == 0)
            {
                // Si aucune direction n'est donnée, dasher dans la direction du regard
                _dashDirection = _spriteRend.flipX ? Vector2.left : Vector2.right;
            }
            else
            {
                _dashDirection = new Vector2(horizontalInput, verticalInput).normalized;
            }

            StartDash(_dashDirection);
        }
    }

    void StartDash(Vector2 direction)
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        _dashDirection = direction;
        _dashCooldownTimer = _dashCooldown;
        _initialColor = _spriteRend.color;  // Stocke la couleur actuelle avant de dasher
        _spriteRend.color = _dashColor; // Changer la couleur au début du dash
    }

    void EndDash()
    {
        _isDashing = false;
        _myRgbd2D.linearVelocity = Vector2.zero; // Arrêter le dash instantanément

        // Restaurer la couleur correcte à la fin du dash
        if (_isSprinting)
        {
            _spriteRend.color = _sprintColor; // Restaurer la couleur de sprint si le joueur sprint toujours
        }
        else
        {
            _spriteRend.color = _normalColor; // Sinon, restaurer la couleur normale
        }
    }
}