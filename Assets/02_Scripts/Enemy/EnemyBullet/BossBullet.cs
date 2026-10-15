using System;
using UnityEngine;
using Unity.Cinemachine;
using Random = UnityEngine.Random;

// 보스 전용 총알 스크립트입니다.
public class BossBullet : MonoBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionShakeForce = 0.8f;

    private Rigidbody2D _rb;
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        RespawnManager.Instance.OnRespawnEvent += ResetBullet;
    }

    private void OnDisable()
    {
        RespawnManager.Instance.OnRespawnEvent -= ResetBullet;
    }

    public void FireBullet(Vector2 direction, float speed)
    {
        _rb.linearVelocity = direction.normalized * speed;

        // 총알이 날아가는 방향과 바라보는 방향을 일치시킵니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger || collision.CompareTag("Enemy")) return;

        if (_impulseSource != null)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f).normalized;
            _impulseSource.GenerateImpulse(randomDir * _explosionShakeForce);
        }

        if (_explosionPrefab == null)
        {
            Debug.LogWarning("보스 총알 폭발 프리팹을 넣어주세요.");
        }

        Quaternion explosionRotation = transform.rotation * Quaternion.Euler(0, 0, 180f);

        // Quaternion.identity 대신 계산한 회전값(explosionRot)을 넣어줍니다!
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, explosionRotation);
        Destroy(explosion, 2.0f);

        AudioManager.Instance.PlaySFX(SoundEffectType.SPREAD_BULLET_EXPLOSION);

        // 총알이 어딘가에(벽이든 플레이어든) 부딪히면 자신을 파괴합니다.
        Destroy(gameObject);
    }

    private void ResetBullet()
    {
        Destroy(gameObject);
    }
}