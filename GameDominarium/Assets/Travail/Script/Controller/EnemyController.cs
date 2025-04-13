using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnnemyController : MonoBehaviour
{
    public enum STATE
    {
        NONE,
        INIT,
        IDLE,
        FOLLOW,
        DEATH
    }

    [SerializeField] private STATE _state = STATE.NONE;
    [SerializeField] private bool showDebugInfo = false;

    public int id;
    public bool randomAllow;

    private Rigidbody2D _rgbd2D;
    private BoxCollider2D _collider2D;
    private SpriteRenderer _spriteRend;
    private EnemyData data;
    private Transform _playerTransform;
    private float _idleTimer;
    private float _pursuitTimer;
    private bool _isFollowing = false;
    private float _currentSpeed = 0f;

    private void Awake()
    {
        TryGetComponent(out _collider2D);
        TryGetComponent(out _rgbd2D);
        TryGetComponent(out _spriteRend);

        _rgbd2D.isKinematic = false;
        _rgbd2D.gravityScale = 0;
        _rgbd2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rgbd2D.freezeRotation = true;
    }

    void Start()
    {
        if (DatabaseManager.Instance != null)
        {
            data = DatabaseManager.Instance.GetData(id, randomAllow);
        }
        else
        {
            Debug.LogError("DatabaseManager.Instance n'est pas trouvé !");
            enabled = false;
            return;
        }

         if (data == null)
         {
             Debug.LogError($"Impossible de charger EnemyData pour id {id}, random {randomAllow}");
             enabled = false;
             return;
         }

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
        else
        {
             Debug.LogWarning($"Joueur avec tag 'Player' non trouvé !");
        }

        Init();
    }

    private void Init()
    {
        _state = STATE.INIT;

        name = data.Label;
        transform.localScale = Vector3.one * data.scaleCoef;

        if (data.sprite != null)
        {
            _spriteRend.sprite = data.sprite;
            _collider2D.size = data.sprite.bounds.size;
            _collider2D.offset = Vector2.zero;
        }

        _spriteRend.color = data.color;

        _idleTimer = 0f;
        _pursuitTimer = 0f;
        _isFollowing = false;
        _currentSpeed = 0f;
        _rgbd2D.linearVelocity = Vector2.zero;
        _rgbd2D.angularVelocity = 0f;

        _state = STATE.IDLE;
    }

    void Update()
    {
        if (_state <= STATE.INIT || _state == STATE.DEATH || data == null)
            return;

        switch (_state)
        {
            case STATE.IDLE:
                HandleIdleState();
                break;
            case STATE.FOLLOW:
                HandleFollowState();
                break;
        }

        if (showDebugInfo) Debug.Log($"{name}: {_state}, Speed: {_currentSpeed:F2}, Following: {_isFollowing}, Vel: {_rgbd2D.linearVelocity.magnitude:F2}, Pos: {transform.position}");
    }

    private void FixedUpdate()
    {
        if (_state <= STATE.INIT || _state == STATE.DEATH || data == null)
        {
            if(_state != STATE.DEATH && _rgbd2D != null) _rgbd2D.linearVelocity = Vector2.zero; // Sécurité
            return;
        }

        float accelRate = data.speed > 0 ? data.speed * 2f : 4f;
        float decelRate = data.speed > 0 ? data.speed : 2.0f;

        if (_isFollowing && _state == STATE.FOLLOW)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, data.speed, accelRate * Time.fixedDeltaTime);
        }
        else
        {
             _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, decelRate * Time.fixedDeltaTime);
        }

        Vector2 currentDirection = transform.up; // Direction actuelle basée sur la rotation du sprite
        Vector2 finalDirection = currentDirection; // Direction par défaut

        if (_isFollowing && _state == STATE.FOLLOW && _playerTransform != null)
        {
            Vector2 directionToPlayer = (_playerTransform.position - transform.position).normalized;

            if (directionToPlayer != Vector2.zero)
            {
                float angleDifference = Vector2.SignedAngle(currentDirection, directionToPlayer);
                float maxDegreesDelta = data.turnSpeed * Time.fixedDeltaTime;
                float angleChange = Mathf.Clamp(angleDifference, -maxDegreesDelta, maxDegreesDelta);

                Quaternion rotation = Quaternion.Euler(0, 0, angleChange);
                finalDirection = (rotation * currentDirection).normalized; // Calculer la nouvelle direction
            }
        }

        Vector2 newVelocity = finalDirection * _currentSpeed; // Calculer la vélocité finale
        _rgbd2D.linearVelocity = newVelocity; // Appliquer la vélocité

        // Mettre à jour la rotation pour correspondre à la direction du mouvement
        if (finalDirection.sqrMagnitude > 0.01f)
        {
            float facingAngle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg - 90f;
            _rgbd2D.MoveRotation(facingAngle);
        }

        // Vérifier si on doit passer à IDLE après avoir décéléré
        if (!_isFollowing && _currentSpeed <= 0.01f && _state != STATE.IDLE)
        {
            GoToIdleState();
        }
    }


    private void HandleIdleState()
    {
        _idleTimer += Time.deltaTime;

        if (_playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            if (distanceToPlayer <= data.detectionRange)
            {
                _state = STATE.FOLLOW;
                _pursuitTimer = 0f;
                _isFollowing = true;
                _idleTimer = 0f;
                RotateTowardsPlayerInstantly();
            }
        }
    }

    private void HandleFollowState()
    {
        bool shouldStopFollowing = false;

        if (_playerTransform == null)
        {
            shouldStopFollowing = true;
        }
        else
        {
            _pursuitTimer += Time.deltaTime;
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            float loseRange = data.detectionRange * 1.2f;

            if (distanceToPlayer > loseRange || _pursuitTimer > data.pursuitDuration)
            {
                shouldStopFollowing = true;
            }
        }

        if (shouldStopFollowing)
        {
            _isFollowing = false;
            _pursuitTimer = 0f;
        }
        else
        {
             _isFollowing = true;
        }
    }

    private void RotateTowardsPlayerInstantly()
    {
         if (_playerTransform == null || _rgbd2D == null) return;
         Vector2 direction = (_playerTransform.position - transform.position).normalized;
         if (direction != Vector2.zero)
         {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            _rgbd2D.MoveRotation(targetAngle);
            // Pas besoin de modifier transform.up ici, FixedUpdate utilise la rotation du Rigidbody
         }
    }

    private void GoToIdleState()
    {
        _state = STATE.IDLE;
        _isFollowing = false;
        _idleTimer = 0f;
        _currentSpeed = 0f;
        if(_rgbd2D != null) {
             _rgbd2D.linearVelocity = Vector2.zero;
             _rgbd2D.angularVelocity = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_state == STATE.DEATH || data == null) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(data.damage);
            }
            Die();
        }
    }

    public void Die()
    {
        if (_state == STATE.DEATH) return;

        _state = STATE.DEATH;
        _isFollowing = false;
        _currentSpeed = 0f;

        if(_rgbd2D != null) {
             _rgbd2D.linearVelocity = Vector2.zero;
             _rgbd2D.angularVelocity = 0f;
             _rgbd2D.isKinematic = true;
        }
        if(_collider2D != null) {
             _collider2D.enabled = false;
        }

        Destroy(gameObject, 0.1f);
    }

    public void TakeDamage(int amount)
    {
        if (_state == STATE.DEATH || data == null) return;

        data.pv -= amount;
        if (data.pv <= 0)
        {
             Die();
        }
    }

    private void OnDrawGizmosSelected()
    {
        EnemyData displayData = data;

        #if UNITY_EDITOR
        if (displayData == null && !Application.isPlaying) {
             DatabaseManager dbm = FindObjectOfType<DatabaseManager>();
             if(dbm != null) {
                 try {
                     displayData = dbm.GetData(id, randomAllow);
                 } catch { }
             }
        }
        #endif

        if (displayData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, displayData.detectionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, displayData.detectionRange * 1.2f);
        }
        else
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, 5f);
        }

        #if UNITY_EDITOR
        if (showDebugInfo)
        {
            string label = $"State: {_state}\nSpeed: {_currentSpeed.ToString("F1")}\nFollowing: {_isFollowing}";
            if (displayData == null && !Application.isPlaying) label += "\n(Data not loaded)";
             UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, label);
        }
        #endif
    }
}