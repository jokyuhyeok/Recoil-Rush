using UnityEngine;
using System.Collections;

public class BossBomb : MonoBehaviour
{
    [SerializeField] private int _maxBounces = 4;

    private int _bounceCount = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=orange>수류탄에 플레이어가 맞았다!</color>");
            
            // 플레이어 사망 처리 로직이 들어갈 자리입니다. (Layer를 이용해서 처리한다고 함.)

            Destroy(gameObject);
            return;
        }

        _bounceCount++;
        Debug.Log($"<color=orange>수류탄: {_bounceCount}회 튕김!</color>");

        if (_bounceCount >= _maxBounces)
        {
            Debug.Log("<color=orange>수류탄: 폭발 횟수 도달! 쾅!</color>");
            
            // 오브젝트 기준 일정 범위 내에 player가 있으면 데미지를 준다.
            
            Destroy(gameObject);
        }
    }
}
