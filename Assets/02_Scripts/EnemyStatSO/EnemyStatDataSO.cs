using UnityEngine;

public class EnemyStatDataSO : ScriptableObject
{
    [Header("공통 스탯")]
    public int MaxHp = 1;

    public float Speed = 1f;
    public float DetectRange = 5f;
    public float AttackRange = 3f;
    public float AlarmTime = 1f;
}