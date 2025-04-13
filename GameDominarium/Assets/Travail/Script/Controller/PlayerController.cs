using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("HEALTH")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int currentHealth;
    private Vector3 startPosition;
    private bool isDead = false;

    [Header("MOVE")]
    [SerializeField] private float _speedMove = 8;
    [SerializeField] private float _runSpeedMultiplier = 1.5f;
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;

    [Header("JUMP")]
    [SerializeField] private float _minForceJump = 12;
    [SerializeField] private float _maxForceJump = 14;
    [SerializeField] private float _jumpHoldTime = 0.2f;
    [SerializeField] private int _limitJump = 1;
    private int _currentJump;
    private bool _hasJumpedSinceGrounded = false;
    private float _jumpStartTime;
    private bool _isJumping;

    [Header("WALL")]
    [SerializeField] private LayerMask _detectWall;
    [SerializeField] private float distanceDW = 0f;
    [SerializeField] private float _minWallJumpHorizontalForce = 5f;
    [SerializeField] private float _maxWallJumpHorizontalForce = 5f;
    [SerializeField] private float _minWallJumpVerticalForce = 4f;
    [SerializeField] private float _maxWallJumpVerticalForce = 20f;
    [SerializeField] private float _wallSlideSpeed = 4f;
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

    [Header("DASH")]
    [SerializeField] private float _dashSpeed = 17f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private KeyCode _dashKey = KeyCode.LeftControl;
    [SerializeField] [Range(0f, 1f)] private float _dashEndMomentumFactor = 0.4f;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private bool _isDashing;
    private Vector2 _dashDirection;
    private Color _initialColor;

    private Rigidbody2D _myRgbd2D;
    private SpriteRenderer _spriteRend;
    private Collider2D _collider;

    private bool _isSprinting;

    [Header("COLOR")]
    [SerializeField] private Color _sprintColor = Color.red;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _dashColor = Color.blue;

    void Awake()
    {
        _myRgbd2D = GetComponent<Rigidbody2D>();
        _spriteRend = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        if (_collider == null)
        {
            Debug.LogError("PlayerController: Aucun Collider2D trouvé sur le joueur !", this);
            enabled = false; return;
        }
        if (_myRgbd2D == null)
        {
            Debug.LogError("PlayerController: Aucun Rigidbody2D trouvé sur le joueur !", this);
            enabled = false; return;
        }
        if (_spriteRend == null)
        {
            Debug.LogError("PlayerController: Aucun SpriteRenderer trouvé sur le joueur !", this);
            enabled = false; return;
        }

        currentHealth = maxHealth;
        startPosition = transform.position;

        _spriteRend.color = _normalColor;
        _initialColor = _normalColor;
    }

    void Update()
    {
        if (isDead) return;

        Move();
        WallSlide();
        HandleJumpInput();
        HandleDashInput();

        if (_timeSinceLastWallJump > 0) _timeSinceLastWallJump -= Time.deltaTime;
        if (_timeToStopStick > 0) _timeToStopStick -= Time.deltaTime;
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;

        if (!_isDashing)
        {
            if (_isSprinting)
            {
                _spriteRend.color = _sprintColor;
                 _initialColor = _sprintColor;
            }
            else
            {
                _spriteRend.color = _normalColor;
                _initialColor = _normalColor;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Correction: Utiliser velocity au lieu de linearVelocity
        if (_myRgbd2D.linearVelocity.y < 0 && !_isWallSliding)
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.fixedDeltaTime;
        }
        else if (_myRgbd2D.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !_isWallJumping)
        {
            _myRgbd2D.linearVelocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.fixedDeltaTime;
        }

        if (_isDashing)
        {
            _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed; // Correction: Utiliser velocity
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
    }

    void Move()
    {
        if (_isDashing) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        _isSprinting = Input.GetKey(_runKey);
        float targetSpeed = moveInput * _speedMove * (_isSprinting ? _runSpeedMultiplier : 1);

        float targetVelocityX = targetSpeed;
        if (_timeToStopStick > 0 && _isWallSliding)
        {
            targetVelocityX = 0;
        }

        _myRgbd2D.linearVelocity = new Vector2(targetVelocityX, _myRgbd2D.linearVelocity.y); // Correction: Utiliser velocity

        if (moveInput != 0 && !_isWallSliding)
            _spriteRend.flipX = moveInput < 0;
    }

    #region Jump Logic
    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_isWallSliding && _timeToStopStick <= 0 && (_infiniteWallJumps || _currentWallJumps < _maxWallJumps))
            {
                StartWallJump();
            }
            else if (_currentJump < _limitJump)
            {
                StartJump();
            }
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (_isJumping) ContinueJump();
            else if (_isWallJumping) ContinueWallJump();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (_isJumping) EndJump();
            if (_isWallJumping) EndWallJump();
        }
    }

    void StartJump()
    {
        _jumpStartTime = Time.time;
        _isJumping = true;
        _isGrounded = false;
        _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, _minForceJump); // Correction: Utiliser velocity
        _currentJump++;
        _hasJumpedSinceGrounded = true;
    }

    void ContinueJump()
    {
        if (_isJumping && Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float jumpForce = Mathf.Lerp(_minForceJump, _maxForceJump, (Time.time - _jumpStartTime) / _jumpHoldTime);
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, jumpForce); // Correction: Utiliser velocity
        }
        else if (_isJumping)
        {
            EndJump();
        }
    }

    void EndJump()
    {
        _isJumping = false;
    }
    #endregion

    #region Wall Logic
    void WallSlide()
    {
        bool wasWallSliding = _isWallSliding;
        _isWallSliding = false;
        _wallDirection = 0;

        if (!_isGrounded && _myRgbd2D.linearVelocity.y <= 0.1f) // Correction: Utiliser velocity
        {
             float checkDistance = _collider.bounds.extents.x + 0.1f;
             int checkDir = (_spriteRend.flipX ? -1 : 1);
             RaycastHit2D wallHit = Physics2D.Raycast(_collider.bounds.center, Vector2.right * checkDir, checkDistance, _detectWall);

             if (wallHit.collider != null)
             {
                 _isWallSliding = true;
                 _wallDirection = -checkDir;
             }
        }

        if (_isWallSliding)
        {
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, Mathf.Max(_myRgbd2D.linearVelocity.y, -_wallSlideSpeed)); // Correction: Utiliser velocity

            if (!wasWallSliding)
            {
                _currentJump = 0;
                _currentWallJumps = 0;
                _hasJumpedSinceGrounded = false;
            }

             if (Input.GetAxisRaw("Horizontal") * _wallDirection > 0)
             {
                if (!wasWallSliding)
                {
                    _timeToStopStick = _wallStickTime;
                    _myRgbd2D.linearVelocity = new Vector2(0, _myRgbd2D.linearVelocity.y); // Correction: Utiliser velocity
                }
             } else {
                 _timeToStopStick = 0;
             }
        }
        else
        {
            _timeToStopStick = 0;
        }
    }

    void StartWallJump()
    {
        _timeSinceLastWallJump = 0.2f;
        _timeToStopStick = 0;
        _currentWallJumps++;

        _jumpStartTime = Time.time;
        _isWallJumping = true;
        _isWallSliding = false;

        float horizontalForce = _wallDirection * _minWallJumpHorizontalForce;
        float verticalForce = _minWallJumpVerticalForce;
        _myRgbd2D.linearVelocity = new Vector2(horizontalForce, verticalForce); // Correction: Utiliser velocity

        _hasJumpedSinceGrounded = true;

        _spriteRend.flipX = _wallDirection > 0;

        EndJump();
    }

    void ContinueWallJump()
    {
        if (_isWallJumping && Time.time - _jumpStartTime < _jumpHoldTime)
        {
            float verticalForce = Mathf.Lerp(_minWallJumpVerticalForce, _maxWallJumpVerticalForce, (Time.time - _jumpStartTime) / _jumpHoldTime);
            _myRgbd2D.linearVelocity = new Vector2(_myRgbd2D.linearVelocity.x, verticalForce); // Correction: Utiliser velocity
        }
        else if (_isWallJumping)
        {
            EndWallJump();
        }
    }

    void EndWallJump()
    {
        _isWallJumping = false;
    }
    #endregion

    #region Dash Logic
    void HandleDashInput()
    {
        if (Input.GetKeyDown(_dashKey) && _dashCooldownTimer <= 0 && !_isDashing)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0))
            {
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
        _spriteRend.color = _dashColor;
    }

    void EndDash()
    {
        _isDashing = false;
        _myRgbd2D.linearVelocity = _dashDirection * _dashSpeed * _dashEndMomentumFactor; // Correction: Utiliser velocity
    }
    #endregion

    #region Collision & Ground Check
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGrounded(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!_isGrounded)
        {
             CheckGrounded(collision);
        }
    }

    private void CheckGrounded(Collision2D collision)
    {
         bool grounded = false;
         int groundLayer = LayerMask.NameToLayer("Ground");

         foreach (ContactPoint2D point in collision.contacts)
         {
             if (point.normal.y > 0.7f && collision.gameObject.layer == groundLayer)
             {
                 grounded = true;
                 break;
             }
         }

         if (grounded && !_isGrounded)
         {
             _isGrounded = true;
             _currentJump = 0;
             _currentWallJumps = 0;
             _hasJumpedSinceGrounded = false;
             _isWallJumping = false;
         }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (collision.gameObject.layer == groundLayer)
        {
             if(_isGrounded)
             {
                 _isGrounded = false;
             }
        }
    }
    #endregion

    #region Health & Death System
    public void TakeDamage(int damageAmount)
    {
        if (isDead || _isDashing) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}/{maxHealth}");

        if (currentHealth > 0)
        {
             CancelInvoke("RestoreColorAfterDamage");
             _spriteRend.color = Color.red;
             Invoke("RestoreColorAfterDamage", 0.15f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void RestoreColorAfterDamage()
    {
         if (!_isDashing)
         {
            _spriteRend.color = _isSprinting ? _sprintColor : _normalColor;
         }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died! Resetting...");
        CancelInvoke("RestoreColorAfterDamage");

        _myRgbd2D.linearVelocity = Vector2.zero; // Correction: Utiliser velocity
        _myRgbd2D.angularVelocity = 0f;
        _myRgbd2D.isKinematic = true;

        transform.position = startPosition;
        currentHealth = maxHealth;

        _isDashing = false; _isJumping = false; _isWallJumping = false; _isWallSliding = false;
        _currentJump = 0; _currentWallJumps = 0;
        _dashCooldownTimer = 0;
        _timeToStopStick = 0; _timeSinceLastWallJump = 0;
        _spriteRend.color = _normalColor;
        _initialColor = _normalColor;
        _spriteRend.flipX = false;

        Invoke("Revive", 0.05f);
    }

    private void Revive()
    {
        _myRgbd2D.isKinematic = false;
        isDead = false;
        Debug.Log("Player Revived.");
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"Player healed {amount} HP. Current Health: {currentHealth}/{maxHealth}");
    }
    #endregion

    #region Triggers & Other
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Finish"))
        {
            Debug.Log("Niveau terminé !");
            if (GameManager.Instance != null) { GameManager.Instance.StopBloc(); }
            else { Debug.LogError("GameManager.Instance non trouvé !"); }
        }

        if (collision.CompareTag("HealthPickup"))
        {
            Heal(5);
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("DeathZone"))
        {
             Debug.Log("Entered Death Zone!");
             TakeDamage(maxHealth * 2);
        }
    }

    public bool IsDashing() { return _isDashing; }
    #endregion
}