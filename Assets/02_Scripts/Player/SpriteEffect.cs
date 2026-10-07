using UnityEngine;

// SpriteRenderer가 없으면 새로 만듦, SpriteRenderer를 Remove하려고 하면 Unity가 막음
// 단 이걸 붙일 때 기준임
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteEffect : MonoBehaviour
{
    [SerializeField] private Sprite[] _frames;
    [SerializeField] private EffectType _effectType = EffectType.Destroy;
    [SerializeField] private float _frameInterval = 0.1f;

    private SpriteRenderer _spriteRenderer;

    private float _frameTimer;
    private int _frameIndex;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_frames.Length == 0)
        {
            Debug.LogWarning($"{name}: SpriteEffect Frames X");
        }

        Restart();
    }

    private void OnDisable()
    {
        _spriteRenderer.enabled = false;
    }

    private void Update()
    {
        // false 상태면 return;
        if (!_spriteRenderer.enabled)
        {
            return;
        }

        if (_frames.Length == 0)
        {
            End();
            return;
        }

        _frameTimer += Time.deltaTime;

        // while을 사용하여 렉이 있어도 시간이 밀리지 않게 함
        while (_frameTimer >= _frameInterval)
        {
            _frameTimer -= _frameInterval;
            _frameIndex++;

            if (_frameIndex >= _frames.Length)
            {
                End();
                return;
            }

            _spriteRenderer.sprite = _frames[_frameIndex];
        }
    }

    public void Play()
    {
        gameObject.SetActive(true);
        Restart();
    }

    private void Restart()
    {
        _frameTimer = 0f;
        _frameIndex = 0;
        _spriteRenderer.enabled = true;

        if (_frames.Length > 0)
        {
            _spriteRenderer.sprite = _frames[0];
        }
    }

    private void End()
    {
        if (_effectType == EffectType.Destroy)
        {
            Destroy(gameObject);
            return;
        }
        else if (_effectType == EffectType.Disable)
        {
            gameObject.SetActive(false);
        }
    }
}
