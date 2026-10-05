using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Rifle")]
public class RifleStatDataSO : EnemyStatDataSO
{
    [Header("사격 및 AI 밸런스")]
    public float FireRate = 1f;

    [Tooltip("총을 쏜 반동에 밀려날 힘의 정도입니다.")]
    public float ShootRecoilPower = 3f;

    [Tooltip("위아래로 총을 쏠 수 있는 최대 각도입니다.")]
    public float MaxShootAngle = 45f;

    [Tooltip("총알을 가로막는 장애물 레이어입니다.")]
    public LayerMask ObstacleMask;

    [Header("프리팹 및 시각 효과 (VFX)")]
    [Tooltip("소총 총알 프리팹/ 03_Prefabs -> Enemy -> EnemyBullet -> EnemyBullet_Rifle")]
    public EnemyBullet BulletPrefab;

    [Tooltip("소총 총구에서 나오는 연기 VFX/ 03_Prefabs -> Enemy -> EnemyParticles -> VFX_RifleFireSmoke")]
    public GameObject MuzzleFlashParticle;

    [Tooltip("소총 사망 시 폭발 VFX/  03_Prefabs -> Enemy -> EnemyParticles -> VFX_EnemyDeath")]
    public GameObject DeathParticle;
}