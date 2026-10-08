using UnityEngine;

// 보스의 돌진 시, 잔상 효과
public class BossGhostEffect : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 3f;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Setup(Sprite currentSprite, Transform bossTransform)
    {
        _sr.sprite = currentSprite;
        transform.position = bossTransform.position;
        transform.localScale = bossTransform.localScale;
        _sr.color = new Color(0.5f, 0.7f, 0.9f, 0.8f);
    }

    private void Update()
    {
        // 매 프레임마다 투명도를 깎아냅니다.
        Color color = _sr.color;
        color.a -= Time.deltaTime * _fadeSpeed;
        _sr.color = color;

        // 완전히 투명해지면 파괴
        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}