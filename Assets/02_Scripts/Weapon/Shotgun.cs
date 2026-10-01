using UnityEngine;

public class Shotgun : Weapon
{
    [Header("Bullet Spread")]
    [SerializeField] private float _minSpread = 0f;
    [SerializeField] private float _maxSpread = 15f;

    protected override void Fire()
    {
        CameraController.Instance?.StartPerlinShake(0.15f, 15f, 0.3f);

        Bullet centerBullet = Instantiate(_bulletPrefab, _firePoint.position, _firePoint.rotation);
        centerBullet.SetWeapon(this);

        float leftAngle = Random.Range(_minSpread, _maxSpread);
        // Quaternion끼리의 *는 곱셈이 아닌 합치는 연산
        Quaternion leftRotation = _firePoint.rotation * Quaternion.Euler(0f, 0f, leftAngle);

        Bullet leftBullet = Instantiate(_bulletPrefab, _firePoint.position, leftRotation);
        leftBullet.SetWeapon(this);

        float rightAngle = Random.Range(_minSpread, _maxSpread);
        Quaternion rightRotation = _firePoint.rotation * Quaternion.Euler(0f, 0f, -rightAngle);

        Bullet rightBullet = Instantiate(_bulletPrefab, _firePoint.position, rightRotation);
        rightBullet.SetWeapon(this);
    }
}
