using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Sprite[] _gameOverSprites;

    [Header("Animation Easing Settings")]
    [SerializeField, Range(0f, 2f)] private float _enableTime = 0.5f;
    [SerializeField, Range(0f, 2f)] private float _disableTime = 0f;

    private Image _gameOverImage;
    private Player _player;
    private RespawnManager _respawnManager;

    private Coroutine _easingCoroutine;


    private void Awake()
    {
        _gameOverImage = GetComponent<Image>();
    }

    private void Start()
    {
        _respawnManager = RespawnManager.Instance;

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        _player.OnDeath += EnableUI;
        _respawnManager.OnRespawnUIEvent += DisableUI;
    }

    private void EnableUI()
    {
        SetRandomSprite();

        if (_easingCoroutine != null)
        {
            StopCoroutine(_easingCoroutine);
            _easingCoroutine = null;
        }

        _easingCoroutine = StartCoroutine(EaseImage(_enableTime, 1f));
    }

    private void DisableUI()
    {
        if (_easingCoroutine != null)
        {
            StopCoroutine(_easingCoroutine);
            _easingCoroutine = null;
        }

        _easingCoroutine = StartCoroutine(EaseImage(_disableTime, 0f));
    }

    private void SetRandomSprite()
    {
        if (_gameOverImage == null || _gameOverSprites == null || _gameOverSprites.Length == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, _gameOverSprites.Length);
        _gameOverImage.sprite = _gameOverSprites[randomIndex];
    }

    private System.Collections.IEnumerator EaseImage(float duration, float targetScale)
    {
        // 0으로 나누기 오류, 음수로 인한 오작동이 발생하지 않게 예외 처리
        if (duration <= 0f)
        {
            transform.localScale = Vector3.one * targetScale;
            yield break;
        }

        float startScale = transform.localScale.x;

        float timer = 0;
        while (timer <= duration)
        {
            // 일시정지가 있을 수 있으니 unscaled 사용
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // Ease-out Sine 사용
            float easedT = Mathf.Sin((t * Mathf.PI) / 2f);

            transform.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, easedT);

            yield return null;
        }
    }
}