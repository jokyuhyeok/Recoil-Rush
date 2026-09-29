using UnityEngine;

public class Shotgun : Weapon
{
    protected override void Fire()
    {
        CameraController.Instance?.StartPerlinShake(0.15f, 15f, 0.3f);

        Bullet centerBullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        centerBullet.SetWeapon(this);

        float randomLeftAngle = Random.Range(0f, 15f);
        // Quaternion끼리의 *는 곱셈이 아닌 합치는 연산
        Quaternion leftRotation = _firePoint.rotation * Quaternion.Euler(0f, 0f, randomLeftAngle);

        Bullet leftBullet = Instantiate(_bulletPrefab, _firePoint.position, leftRotation);
        leftBullet.SetWeapon(this);

        float randomRightAngle = Random.Range(0f, 15f);
        Quaternion rightRotation = _firePoint.rotation * Quaternion.Euler(0f, 0f, -randomRightAngle);

        Bullet rightBullet = Instantiate(_bulletPrefab, _firePoint.position, rightRotation);
        rightBullet.SetWeapon(this);
    }
}
