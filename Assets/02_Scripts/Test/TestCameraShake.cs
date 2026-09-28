using UnityEngine;

public class TestCameraShake : MonoBehaviour
{
    [Header("공통")]
    [SerializeField] private float _shakeDuration = 1f;

    [Header("일반")]
    [Tooltip("일반 Shake에 사용하는 힘")]
    [SerializeField] private float _shakeForce = 0.2f;

    [Header("Perlin")]
    [Tooltip("Perlin Shake에 사용하는 힘. 일반 Shake와 비슷하려면 2~3배 정도 주면 됩니다.")]
    [SerializeField] private float _perlinShakeForce = 0.5f;
    [Tooltip("Perlin Shake에만 사용되는 파장. 높을 수록 골고루 흔들립니다.")]
    [SerializeField] private float _perlinShakeFrequency = 60f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CameraController.Instance.StartShake(_shakeForce, _shakeDuration);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CameraController.Instance.StartPerlinShake(_perlinShakeForce, _perlinShakeFrequency, 1f);
        }
    }
}