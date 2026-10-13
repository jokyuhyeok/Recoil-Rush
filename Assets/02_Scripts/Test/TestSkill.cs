using UnityEngine;

public class TestSkill : MonoBehaviour
{
    private BossLaserTest _bossLaserTest;
    private BossController _bossController;

    private void Start()
    {
        _bossLaserTest = GetComponent<BossLaserTest>();
        _bossController = GetComponent<BossController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _bossLaserTest.StartSkill(_bossController);
        }
    }
}