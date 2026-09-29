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
        Debug.Log($"<color=yellow>보스: 기본 사격 시작! {_bulletCount}발</color>");

        for (int i = 0; i < _bulletCount; i++)
        {
            float count = Mathf.Max(1f, _bulletCount - 1);
            float t = (float)i / count;
            float currentAngle = Mathf.Lerp(_startAngle,  _endAngle, t);

            GameObject bullet = Object.Instantiate(boss.BulletPrefab, boss.FirePoint.position, Quaternion.identity);
            Vector2 fireDirection = Quaternion.Euler(0, 0, currentAngle) * Vector2.left;
            
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = fireDirection * _bulletSpeed;
            }

            yield return new WaitForSeconds(_fireDelay);
        }
        
        Debug.Log("<color=yellow>보스: 기본 사격 종료!</color>");
    }
}
