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
        boss.Anim.Play("FrontMove");
        boss.Rigidbody.linearVelocity = Vector2.zero;
        float directionX = boss.transform.localScale.x > 0 ? -1f : 1f;

        Debug.Log($"<color=red>보스: 제트팩 충전 중.. {_chargeTime}초 후 발사!</color>");

        if (boss.DashChargeEffect == null)
        {
            Debug.LogWarning("보스 차징 VFX를 넣어주세요.");
        }

        boss.DashChargeEffect.SetActive(true);

        CameraController.Instance.StartShake(_chargeTime, 0.5f); // 기를 모을 때 흔들림
        yield return new WaitForSeconds(_chargeTime);

        Debug.Log("<color=red>보스: 전방 돌진 시작!</color>");

        if (boss.DashChargeEffect != null)
        {
            boss.DashChargeEffect.SetActive(false);
        }

        if (boss.DashFireEffect == null)
        {
            Debug.LogWarning("보스 돌진 시 불꽃 VFX를 넣어주세요.");
        }

        boss.DashFireEffect.SetActive(true);

        float timer = 0f;
        float ghostTimer = 0f;

        while (timer < _dashDuration)
        {
            CameraController.Instance.StartPerlinShake(0.1f, 25f, 0.3f);

            if ((directionX < 0 && boss.IsHittingLeftWall) || (directionX > 0 && boss.IsHittingRightWall))
            {
                Debug.Log("앞쪽 벽에 박았다!");

                if (boss.DashFireEffect != null)
                {
                    boss.DashFireEffect.SetActive(false);
                }

                CameraController.Instance.StartShake(0.6f, 1.5f); // 강한 진동

                boss.Rigidbody.linearVelocity =
                    new Vector2(-directionX * _dashSpeed * 0.2f, boss.Rigidbody.linearVelocity.y);
                yield return new WaitForSeconds(0.5f);
                break;
            }

            boss.Rigidbody.linearVelocity = new Vector2(directionX * _dashSpeed, boss.Rigidbody.linearVelocity.y);

            ghostTimer += Time.deltaTime;
            if (ghostTimer >= _ghostInterval)
            {
                boss.SpawnGhost();
                ghostTimer = 0f;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        boss.Rigidbody.linearVelocity = new Vector2(0f, boss.Rigidbody.linearVelocity.y);

        if (boss.DashFireEffect != null)
        {
            boss.DashFireEffect.SetActive(false); // 제트팩 끄기
        }

        Debug.Log("<color=red>보스: 전진 돌진 종료!</color>");
    }
}