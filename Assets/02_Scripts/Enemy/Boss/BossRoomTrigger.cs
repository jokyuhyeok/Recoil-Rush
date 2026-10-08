using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    [Header("보스 체력바 UI 오브젝트")]
    [Tooltip("하이어라키에 있는 BossUI를 끌어다 넣으세요.")]
    [SerializeField] private GameObject _bossUIObject;

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

            Debug.Log("<color=cyan>보스방 입장! 체력바 활성화</color>");

            // 필요하다면 이 부분에 보스 AI를 깨우는 코드를 추가할 수 있습니다.
            // 예: BossController.SetPhase(1);
        }
    }
}