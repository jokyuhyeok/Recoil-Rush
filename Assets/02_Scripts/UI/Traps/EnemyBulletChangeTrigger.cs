using UnityEngine;

public class EnemyBulletChangeTrigger : MonoBehaviour
{
    [Header("Enemy Stat")]
    [SerializeField] private MachineGunStatDataSO _enemyStat;

    [Header("Bullet Prefabs")]
    [SerializeField] private EnemyBullet _originalBulletPrefab;
    [SerializeField] private EnemyBullet _streetLampPrefab;

    private bool _isActivated = false;
    private RespawnManager _respawnManager;

    private void OnEnable()
    {
        // 게임 시작 또는 트리거 재활성화 시 초기화
        ResetBullet();
    }

    private void Start()
    {
        // 리스폰 이벤트 연결
        _respawnManager = RespawnManager.Instance;

        if (_respawnManager != null)
        {
            _respawnManager.OnRespawnPlayerEvent += OnRespawnPlayer;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 총알만 반응
        if (!collision.CompareTag("playerbullet"))
            return;

        // 중복 실행 방지
        if (_isActivated)
            return;

        if (_enemyStat == null || _streetLampPrefab == null)
        {
            Debug.LogWarning("스탯 또는 가로등 프리팹이 연결되지 않았습니다!");
            return;
        }

        _isActivated = true;

        // 총알 프리팹을 가로등으로 변경
        _enemyStat.BulletPrefab = _streetLampPrefab;

        Debug.Log("트리거 작동! 가로등 공격으로 변경!");
    }

    // 플레이어 리스폰 시 호출
    private void OnRespawnPlayer(Vector3 respawnPosition)
    {
        ResetBullet();
    }

    // 기존 총알로 복구
    private void ResetBullet()
    {
        if (_enemyStat == null || _originalBulletPrefab == null)
            return;

        _enemyStat.BulletPrefab = _originalBulletPrefab;

        // 다시 트리거 작동 가능
        _isActivated = false;

        Debug.Log("기관총 총알 초기화 완료!");
    }

    private void OnDisable()
    {
        // 이벤트 중복 등록 방지
        if (_respawnManager != null)
        {
            _respawnManager.OnRespawnPlayerEvent -= OnRespawnPlayer;
            _respawnManager = null;
        }

        // 비활성화 시에도 초기화
        ResetBullet();
    }

    private void OnDestroy()
    {
        // 씬 종료 시 초기화
        ResetBullet();
    }
}