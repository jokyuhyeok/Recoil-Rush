using UnityEngine;
using System.Collections;

public class Cmd_BackwardDash : IBossCommand
{
    private float _dashSpeed;
    private float _dashDuration;
    private float timer = 0f;
    
    // 생성자입니다.
    public Cmd_BackwardDash(float dashSpeed, float dashDuration)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
    }

    public IEnumerator Execute(BossController boss)
    {
        Debug.Log("<color=cyan>보스: 후방 돌진 시작! [잔상 효과 발동]</color>");

        float directionX = 1f;
        
        // [연출] 여기에 잔상을 남기는 코드를 추가할 예정입니다.

        boss.rb.linearVelocity = new Vector2(directionX * _dashSpeed, boss.rb.linearVelocity.y);
        
        while (timer < _dashDuration)
        {
            if (boss.isHittingRightWall)
            {
                Debug.Log("쾅! 등 뒤의 벽에 부딪혀서 후진을 멈춥니다.");
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        
        // [연출] 잔상 효과를 끄는 코드를 추가할 예정입니다.
        Debug.Log("<color=cyan>보스: 후방 돌진 종료... [잔상 효과 끄기]");
    }
}
