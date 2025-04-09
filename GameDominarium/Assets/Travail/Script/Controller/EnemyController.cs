using UnityEngine;

public class EnnemyController : MonoBehaviour
{
    public int id;
    public bool randomAllow;
    private Rigidbody2D _rgbd2D;
    private BoxCollider2D _collider2D;
    private SpriteRenderer _spriteRend;
    private EnemyData data;

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
        name = data.Label;
        transform.localScale = Vector3.one * data.scaleCoef;

        _collider2D.size = data.sprite.bounds.size;
        _spriteRend.sprite = data.sprite;
        _spriteRend.color = data.color;
    }
}
