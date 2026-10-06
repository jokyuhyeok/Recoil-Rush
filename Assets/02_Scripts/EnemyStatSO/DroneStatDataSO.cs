using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Drone")]
public class DroneStatDataSO : EnemyStatDataSO
{
    [Header("드론 전용 비행 데이터")]
    [Tooltip("둥둥 떠다니는 높이 폭입니다.")]
    public float HoverHeight = 0.5f;

    [Tooltip("둥둥 떠다니는 속도입니다.")]
    public float HoverSpeed = 2f;

    [Header("사격 및 AI 밸런스")]
    [Tooltip("사격 전, 에너지를 모으는 준비 시간입니다.")]
    public float ChargeTime = 1f;

    [Tooltip("공격 쿨타임입니다.")]
    public float AttackCooldown = 2f;

    [Tooltip("드론이 아래로 쏠 수 있는 최대 각도입니다.")]
    public float MinAttackAngle = -60f;

    [Tooltip("드론이 위로 쏠 수 있는 최대 각도입니다.")]
    public float MaxAttackAngle = 15f;

    [Tooltip("사격을 가로막는 장애물입니다.")]
    public LayerMask ObstacleMask;

    [Header("프리팹 및 시각 효과 (VFX)")]
    [Tooltip("드론 전용 총알입니다./ 03_Prefabs -> Enemy -> EnemyBullet -> EnemyBullet_Drone")]
    public EnemyBullet CannonPrefab;

    [Tooltip("추락 후 공중 대폭발 이펙트입니다./ 03_Prefabs -> Enemy -> EnemyParticles -> VFX_DroneExplosion3")]
    public GameObject ExplosionPrefab;
}