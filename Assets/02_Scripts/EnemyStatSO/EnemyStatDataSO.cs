using UnityEngine;

public class EnemyStatDataSO : ScriptableObject
{
    [Header("공통 스탯")]
    public int maxHp = 1;

    public float speed = 1f;
    public float detectRange = 5f;
    public float attackRange = 3f;
    public float alarmTime = 1f;
}