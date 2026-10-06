using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/MachineGun")]
public class MachineGunStatDataSO : EnemyStatDataSO
{
    [Header("기관총 전용 데이터")]
    [Tooltip("총알 간 발사 간격입니다. (연사 속도)")]
    public float FireRate = 1f;

    [Tooltip("탄창 크기입니다.")]
    public int MagazineSize = 1;

    [Tooltip("재장전에 걸리는 시간입니다.")]
    public float ReloadTime = 1f;

    [Tooltip("연사 시에 탄 퍼짐 각도입니다.")]
    public float SpreadAngle = 10f;

    [Header("사격 및 AI 밸런스")]
    [Tooltip("사격 시 총구 반동입니다. (연사하므로 EnemyRifle보다 낮게 설정 추천)")]
    public float ShootRecoilPower = 1f;

    [Tooltip("위아래로 사격 가능한 최대 각도입니다.")]
    public float MaxShootAngle = 45f;

    [Tooltip("총알을 가로막는 장애물 레이어입니다.")]
    public LayerMask ObstacleMask;

    [Header("프리팹 및 시각 효과 (VFX)")]
    [Tooltip("기관총 전용 총알입니다./ 03_Prefabs -> Enemy -> EnemyBullet -> EnemyBullet_MachineGun")]
    public EnemyBullet BulletPrefab;

    [Tooltip("총구에서 나오는 화염 VFX/ 03_Prefabs -> Enemy -> EnemyParticles -> VFX_MachinegunFireSmoke")]
    public GameObject MuzzleFlashParticle;

    [Tooltip("기관총 적 사망 시 폭발 VFX/ 03_Prefabs -> Enemy -> EnemyParticles -> VFX_EnemyDeath")]
    public GameObject DeathParticle;
}