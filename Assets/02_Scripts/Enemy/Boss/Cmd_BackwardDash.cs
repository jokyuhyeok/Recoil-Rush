using UnityEngine;
using System.Collections;

public class Cmd_BackwardDash : IBossCommand
{
    private float _dashSpeed;
    private float _dashDuration;
    
    // 생성자입니다.
    public Cmd_BackwardDash(float dashSpeed, float dashDuration)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.anim.Play("BackMove");
        Debug.Log("<color=cyan>보스: 후방 돌진 시작! [잔상 효과 발동]</color>");

        float directionX = 1f;
        float timer = 0f;
        
        // [연출] 여기에 잔상을 남기는 코드를 추가할 예정입니다.
        
        while (timer < _dashDuration)
        {
            if ((directionX < 0 && boss.isHittingLeftWall) || (directionX > 0 && boss.isHittingRightWall))
            {
                Debug.Log("쾅! 등 뒤의 벽에 박고 앞으로 통통 튕겨납니다!");
                
                // 역방향으로 튕겨나가는 힘 적용
                boss.rb.linearVelocity = new Vector2(directionX * _dashSpeed, boss.rb.linearVelocity.y);
                
                yield return new WaitForSeconds(0.5f);
                break; 
            }
            
            boss.rb.linearVelocity = new Vector2(directionX * _dashSpeed, boss.rb.linearVelocity.y);
            
            timer += Time.deltaTime;
            yield return null;
        }

        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        
        // [연출] 잔상 효과를 끄는 코드를 추가할 예정입니다.
        Debug.Log("<color=cyan>보스: 후방 돌진 종료... [잔상 효과 끄기]");
    }
}
