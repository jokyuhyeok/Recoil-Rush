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

        float directionX = boss.transform.localScale.x > 0 ? -1f : 1f;