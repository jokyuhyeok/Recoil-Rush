using UnityEngine;

// 보스의 돌진 패턴 시에 뒤에 잔상 효과를 담당합니다. 
public class BossGhostEffect : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed = 3f;
    private SpriteRenderer _sr;

    public void Setup(Sprite currentSprite, Transform bossTransform)
    {
        _sr = GetComponent<SpriteRenderer>();

        _sr.sprite = currentSprite;
        transform.position = bossTransform.position;
        transform.localScale = bossTransform.localScale;

        _sr.color = new Color(0.5f, 0.7f, 0.9f, 0.5f);
    }

    private void Update()
    {
        // 매 프레임마다 투명도를 깎아냅니다.
        Color color = _sr.color;
        color.a -= Time.deltaTime * _fadeSpeed;
        _sr.color = color;

        // 완전히 투명해지면 파괴합니다. (추후에 오브젝트 풀링으로 업그레이드 할 예정입니다.)
        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}