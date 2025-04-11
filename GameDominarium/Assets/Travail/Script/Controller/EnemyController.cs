using UnityEngine;

public class EnnemyController : MonoBehaviour
{

    public enum STATE
    {
        NONE,
        INIT,
        IDLE,
        MOVE,
        FOLLOW,
        FIRE,
        DEATH
    }

    [SerializeField] private STATE _state;


    public int id;
    public bool randomAllow;
    private Rigidbody2D _rgbd2D;
    private BoxCollider2D _collider2D;
    private SpriteRenderer _spriteRend;
    private EnemyData data;

    private float _countdown;

    private void Awake()
    {
        TryGetComponent(out _collider2D);
        TryGetComponent(out _rgbd2D);
        TryGetComponent(out _spriteRend);
    }
    void Start()
    {
        data = DatabaseManager.Instance.GetData(id, randomAllow);
        Init();
    }

    private void Init()
    {
        _state = STATE.INIT;

        name = data.Label;
        transform.localScale = Vector3.one * data.scaleCoef;

        _collider2D.size = data.sprite.bounds.size;
        _spriteRend.sprite = data.sprite;
        _spriteRend.color = data.color;

        _state = STATE.IDLE;
    }

    void Update()
    {
        if (_state < STATE.INIT)
            return;
        
        switch (_state)
        {
            case STATE.IDLE:
                if(_countdown > data.durationIDLE)
                    _state = STATE.MOVE;
                
                _countdown += Time.deltaTime;
                break;

            case STATE.MOVE:
                transform.position += Vector3.right * Time.deltaTime * data.speed;
                break;
            case STATE.FOLLOW:
                break;
            case STATE.FIRE:
                break;
            case STATE.DEATH:
                break;
        }
    }
}
