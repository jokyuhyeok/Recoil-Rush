using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Image _gameOverImage;
    [SerializeField] private Sprite[] _gameOverSprites;

    private Player _player;
    private RespawnManager _respawnManager;

    // 테스트용
    [SerializeField] private bool _isTestMode = false;


    private void Awake()
    {
        if (_gameOverImage != null)
            return;

        _gameOverImage = GetComponent<Image>();
    }

    private void Start()
    {
        _respawnManager = RespawnManager.Instance;

        if (_isTestMode)
        {
            _respawnManager.OnRespawnUIEvent += EnableUI;
            return;
        }

        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        //_player.OnDeadEvent += EnableUI;
        _respawnManager.OnRespawnUIEvent += DisableUI;
    }

    private void EnableUI()
    {
        SetRandomSprite();
        _gameOverImage.enabled = true;

        if (_isTestMode)
        {
            _respawnManager.OnRespawnUIEvent -= EnableUI;
            _respawnManager.OnRespawnUIEvent += DisableUI;
        }
    }

    private void DisableUI()
    {
        _gameOverImage.enabled = false;

        if (_isTestMode)
        {
            _respawnManager.OnRespawnUIEvent -= DisableUI;
            _respawnManager.OnRespawnUIEvent += EnableUI;
        }
    }

    private void SetRandomSprite()
    {
        if (_gameOverImage == null || _gameOverSprites == null || _gameOverSprites.Length == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, _gameOverSprites.Length);
        _gameOverImage.sprite = _gameOverSprites[randomIndex];
    }
}