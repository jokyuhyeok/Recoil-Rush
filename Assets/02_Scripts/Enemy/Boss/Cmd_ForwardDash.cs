using UnityEngine;
using System.Collections;

// Boss의 "전방 돌진" 메서드 입니다.
public class Cmd_ForwardDash : IBossCommand
{
    private float _dashSpeed;
    private float _dashDuration;

    // 생성자입니다.
    public Cmd_ForwardDash(float dashSpeed, float dashDuration)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
    }

    public IEnumerator Execute(BossController boss)
    {
        Debug.Log("<color=red>보스: 전방 돌진 시작!</color>");

        float directionX = -1f;
        float timer = 0f;
        
        while (timer < _dashDuration)
        {
            if (boss.isHittingLeftWall)
            {
                Debug.Log("쾅! 벽에 부딪혀서 돌진을 멈춥니다.");
                break;
            }
            
            boss.rb.linearVelocity = new Vector2(directionX * _dashSpeed, boss.rb.linearVelocity.y);
            
            timer += Time.deltaTime;
            yield return null; 
        }

        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        Debug.Log("<color=red>보스: 전진 돌진 종료!</color>");
    }
}
