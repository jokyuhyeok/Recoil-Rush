using UnityEngine;
using System.Collections;

// Boss의 "전방 돌진" 메서드 입니다.
public class Cmd_ForwardDash : IBossCommand
{
    private float _dashSpeed;
    private float _dashDuration;
    private float _chargeTime;
    private float _ghostInterval;

    // 생성자입니다.
    public Cmd_ForwardDash(float dashSpeed, float dashDuration, float chargeTime, float ghostInterval)
    {
        _dashSpeed = dashSpeed;
        _dashDuration = dashDuration;
        _chargeTime = chargeTime;
        _ghostInterval = ghostInterval;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.anim.Play("FrontMove");
        
        Debug.Log($"<color=red>보스: 제트팩 충전 중 {_chargeTime}초 후 발사합니다!</color>");
        boss.rb.linearVelocity = Vector2.zero;
        
        // 여기에 장전 중인 파티클을 넣으면 됩니다. 
        
        yield return new WaitForSeconds(_chargeTime);
        
        Debug.Log("<color=red>보스: 전방 돌진 시작!</color>");

        float directionX = -1f;
        float timer = 0f;
        float ghostTimer = 0f;
        
        while (timer < _dashDuration)
        {
            if ((directionX < 0 && boss.isHittingLeftWall) || (directionX > 0 && boss.isHittingRightWall))
            {
                Debug.Log("앞쪽 벽에 박았다!");
                boss.rb.linearVelocity = new Vector2(-directionX * _dashSpeed * 0.2f, boss.rb.linearVelocity.y);
                yield return new WaitForSeconds(0.5f);
                break;
            }

            boss.rb.linearVelocity = new Vector2(directionX * _dashSpeed, boss.rb.linearVelocity.y);
            ghostTimer += Time.deltaTime;
            if (ghostTimer >= _ghostInterval)
            {
                boss.SpawnGhost();
                ghostTimer = 0f;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        boss.rb.linearVelocity = new Vector2(0f, boss.rb.linearVelocity.y);
        Debug.Log("<color=red>보스: 전진 돌진 종료!</color>");
    }
}
