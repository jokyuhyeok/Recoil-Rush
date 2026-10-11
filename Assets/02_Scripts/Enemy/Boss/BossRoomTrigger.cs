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
}