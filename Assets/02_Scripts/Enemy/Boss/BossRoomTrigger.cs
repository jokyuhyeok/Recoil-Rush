using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("보스 체력바 UI 오브젝트")]
    [Tooltip("하이어라키에 있는 BossUI를 끌어다 넣으세요.")]
    [SerializeField] private GameObject _bossUIObject;

    [Header("보스 본체 오브젝트")]
    [Tooltip("미리 왼쪽에 배치해둔 보스 본체를 끌어다 넣으세요. (평소엔 꺼두세요)")]
    [SerializeField] private GameObject _bossObject;

    private bool _hasTriggered = false;

    // =================================================================
    // 리스폰 매니저의 이벤트 구독 (시작할 때 연결, 파괴될 때 해제)
    // =================================================================
    private void Start()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnEvent += ResetBossRoom;
        }
    }

    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.OnRespawnEvent -= ResetBossRoom;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_hasTriggered && collision.CompareTag("Player"))
        {
            _hasTriggered = true;

            // 보스 체력바 켜기
            if (_bossUIObject != null)
            {
                _bossUIObject.SetActive(true);
            }

            // 보스 본체 켜기
            if (_bossObject != null)
            {
                // 켜지는 순간 BossController의 Start()가 실행되며 알아서 패턴을 시작
                _bossObject.SetActive(true);

                AudioManager.Instance.PlaySFX(SoundEffectType.BOSS_APPEAR);
            }

            Debug.Log("<color=cyan>보스방 입장! 체력바 활성화</color>");
        }
    }

    private void ResetBossRoom(Vector3 respawnPos)
    {
        Debug.Log("플레이어 부활 감지: 보스방을 초기화합니다.");

        // 1. 트리거 상태 초기화 (다시 밟을 수 있게)
        _hasTriggered = false;

        // 2. 보스 체력바 끄기
        if (_bossUIObject != null)
        {
            _bossUIObject.SetActive(false);
        }

        // 3. 보스 본체 초기화 및 숨기기
        if (_bossObject != null)
        {
            BossController controller = _bossObject.GetComponent<BossController>();
            if (controller != null)
            {
                controller.ResetBoss();
            }
        }

        // 4. 맵에 남아있는 보스의 투사체(레이저, 구체 등) 청소
        GameObject[] bossBullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (GameObject bullet in bossBullets)
        {
            Destroy(bullet);
        }
    }
}