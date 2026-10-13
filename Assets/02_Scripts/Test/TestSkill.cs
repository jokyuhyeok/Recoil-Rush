using UnityEngine;

public class TestSkill : MonoBehaviour
{
    private BossLaserTest _bossLaserTest;
    private void Start()
    {
        _bossLaserTest = GetComponent<BossLaserTest>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _bossLaserTest.StartSkill();
        }
    }
}
