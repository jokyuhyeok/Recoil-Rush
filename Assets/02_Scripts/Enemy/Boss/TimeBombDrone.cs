using UnityEngine;
using System.Collections;

public class TimeBombDrone : MonoBehaviour
{
    [SerializeField] private float _autoDestroyTime = 6.0f;
    [SerializeField] private float _steppedDestroyTime = 1.0f;

    private Coroutine _deathTimer;
    private bool _isTriggered = false;  // 이미 밟혔는지 체크하는 bool입니다.

    private void Start()
    {
        // 태어나자마자 자폭 타이머를 켭니다.
        _deathTimer = StartCoroutine(ExplodeAfterDelay(_autoDestroyTime));
    }

    // 플레이어가 밟았을 때 일정 시간 뒤에 폭발합니다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_isTriggered && collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 위에서 밟았는지 Y축 계산으로 확인하는 로직이 들어갈 곳입니다.
            if (collision.transform.position.y > transform.position.y + 0.1f)
            {
                _isTriggered = true;
                Debug.Log("<color=red>드론: 밟혔다! 1초 뒤 폭발 카운트다운 시작!</color>");

                // 시각적 경고 => 애니메이션으로 변경 가능합니다. 
                GetComponent<SpriteRenderer>().color = Color.red;

                if (_deathTimer != null)
                {
                    StopCoroutine(_deathTimer);
                }
                
                // 타이머 폭발, 데미지 코드를 여기에 작성합니다. 
                StartCoroutine(ExplodeAfterDelay(_steppedDestroyTime));
            }
        }
    }

    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("드론 폭발 이펙트 및 데미지 발생!");
        // 여기에는 폭발 이펙트 생성 코드가 들어갑니다.
        // 여기에는 플레이어 데미지를 주는 코드를 넣습니다.
        
        Destroy(gameObject);
    }
}
