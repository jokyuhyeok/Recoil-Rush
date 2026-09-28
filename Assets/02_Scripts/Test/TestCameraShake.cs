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
    [Tooltip("Perlin Shake에만 사용되는 거친 정도. 높을 수록 부드럽습니다.")]
    [SerializeField] private float _perlinShakeRoughness = 60f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CameraController.Instance.StartShake(0.2f, 1f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CameraController.Instance.StartPerlinShake(0.5f, 60f, 1f);
        }
    }
}