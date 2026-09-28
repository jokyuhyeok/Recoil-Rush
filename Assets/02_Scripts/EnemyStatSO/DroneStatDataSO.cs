using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStat", menuName = "Enemy/Stat/Drone")]
public class DroneStatDataSO : EnemyStatDataSO
{
    [Header("드론 전용 데이터")]
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 2f;
    public float chargeTime = 1f;
    public float attackCooldown = 2f;
}
