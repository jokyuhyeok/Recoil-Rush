using UnityEngine;
using System.Collections;

public class TimeBombDrone : MonoBehaviour
{
    [SerializeField] private float _autoDestroyTime = 6.0f;
    [SerializeField] private float _touchedDestroyTime = 1.0f;
    [SerializeField] private float _layerOnTime = 0.2f;
    [SerializeField] private float _moveSpeed = 2.0f;
    [SerializeField] private float _freezeTime = 1.5f;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _explosionArea;
    [SerializeField] private GameObject _sparkVFX; // 드론들이 멈출 때 넣을 번개 파티클입니다.
    [SerializeField] private Animator _animator;

    private Coroutine _autoDestroyCoroutine;
    private bool _isTriggered = false; // 이미 밟혔는지 체크하는 bool입니다.
    private bool _isFrozen = false; // 감전되어서 멈췄는지 여부입니다.
    private Vector3 _moveDirection;

    private void Start()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_animator == null) _animator = GetComponent<Animator>();

        if (transform.position.x < 0f)
            _moveDirection = Vector3.right; // 왼쪽에서 태어났으면 오른쪽으로
        else
            _moveDirection = Vector3.left; // 오른쪽에서 태어났으면 왼쪽으로

        StartCoroutine(SelfFreezeTimer());
        _autoDestroyCoroutine = StartCoroutine(ExplosionTimer(_autoDestroyTime));
    }

    private void Update()
    {
        if (_isFrozen || _isTriggered) return;

        // 정해진 방향으로 횡이동합니다.
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private IEnumerator SelfFreezeTimer()
    {
        // _freezeTime만큼 날아가면서 대기합니다.
        yield return new WaitForSeconds(_freezeTime);

        // 만약 멈추기도 전에 플레이어가 위에서 밟아버렸다면 무시합니다.
        if (_isTriggered) yield break;

        _isFrozen = true;
        Debug.Log($"<color=yellow>드론: {_freezeTime}초 경과! 스스로 감전 정지!</color>");

        // 멈추는 순간 지직거리는 번개 파티클을 켜줍니다.
        if (_sparkVFX != null)
        {
            _sparkVFX.SetActive(true);
        }

        if (_animator != null)
        {
            _animator.SetTrigger("freeze");
        }
    }

    // 플레이어가 밟았을 때 일정 시간 뒤에 폭발합니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isTriggered && collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 위에서 밟았는지 확인합니다.
            // collision.contacts[0].normal.y => 충돌한 면의 방향이랍니다. 
            // 누르는 힘을 받으면 -1에 가까워진대요. 어느정도 밟으면 발동하게 0.7로 설정했습니다. 
            if (collision.contacts[0].normal.y < -0.7f)
            {
                _isTriggered = true;
                Debug.Log("<color=orange>드론: 플레이어가 밟았다! 폭발 카운트다운에 들어갑니다.</color>");

                if (_autoDestroyCoroutine != null)
                    StopCoroutine(_autoDestroyCoroutine);

                _spriteRenderer.color = Color.red;
                if (_animator != null)
                {
                    _animator.SetTrigger("warning");
                }

                StartCoroutine(ExplosionTimer(_touchedDestroyTime));
            }
        }
    }

    private IEnumerator ExplosionTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    // 폭발 시 일정 범위의 Layer가 Dangerous로 설정됩니다. 
    private void Explode()
    {
        Debug.Log("<color=red>드론 폭발! (Dangerous 영역 생성)</color>");

        if (_explosionArea != null)
        {
            // 꺼져있던 폭발 영역을 켭니다. (Dangerous 레이어로 바뀌는 시간!)
            _explosionArea.SetActive(true);

            // 드론이 파괴될 때 같이 파괴되지 않도록 분리합니다.  
            _explosionArea.transform.SetParent(null);

            // 폭발 영역은 일정 시간 뒤에 스스로 사라지게 합니다.
            Destroy(_explosionArea, _layerOnTime);
        }

        Destroy(gameObject);
    }
}