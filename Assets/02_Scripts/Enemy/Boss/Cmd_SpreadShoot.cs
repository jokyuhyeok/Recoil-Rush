using UnityEngine;
using System.Collections;

public class Cmd_SpreadShoot : IBossCommand
{
    private int _bulletCount;
    private float _startAngle;
    private float _endAngle;
    private float _fireDelay;
    private float _bulletSpeed;

    public Cmd_SpreadShoot(int bulletCount, float startAngle, float endAngle, float fireDelay, float bulletSpeed)
    {
        _bulletCount = bulletCount;
        _startAngle = startAngle;
        _endAngle = endAngle;
        _fireDelay = fireDelay;
        _bulletSpeed = bulletSpeed;
    }

    public IEnumerator Execute(BossController boss)
    {
        boss.anim.Play("Fire");
        boss.rb.linearVelocity = Vector2.zero;
        Debug.Log("<color=yellow>보스: 사격 폼 잡기</color>");
        yield return new WaitForSeconds(0.8f);
        
        Debug.Log($"<color=yellow>보스: 기본 사격 시작! {_bulletCount}발</color>");

        for (int i = 0; i < _bulletCount; i++)
        {
            float count = Mathf.Max(1f, _bulletCount - 1);
            float t = (float)i / count;
            float currentAngle = Mathf.Lerp(_startAngle,  _endAngle, t);
            
            Vector2 fireDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.left;

            foreach (Transform firePoint in boss.FirePoints)
            {
                GameObject bullet = Object.Instantiate(boss.BulletPrefab, firePoint.position, Quaternion.identity);

                BossBullet bossBullet = bullet.GetComponent<BossBullet>();
                if (bossBullet != null)
                {
                    bossBullet.FireBullet(fireDirection, _bulletSpeed);
                }
            }

            yield return new WaitForSeconds(_fireDelay);
        }
        
        Debug.Log("<color=yellow>보스: 기본 사격 종료!</color>");
    }
}
