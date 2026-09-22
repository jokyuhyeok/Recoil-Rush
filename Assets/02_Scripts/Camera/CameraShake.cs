using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // 진동의 크기
    [SerializeField] private float _shakeForce = 0.05f;
    // 진동의 빈도
    private float _shakeRoughness = 60f;
    // 진동 지속 시간
    [SerializeField] private float _shakeDuration = 0.5f;
    private float _shakeTimer = 0f;

    private CameraLookAhead _lookAhead;
    private float _tick;
    private Coroutine _shakeCoroutine;

    private void Start()
    {
        _lookAhead = GetComponent<CameraLookAhead>();
        _tick = Random.Range(-100f, 100f);
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartShakeCamera();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartShakeCameraWithPerlinNoise();
        }
        */
    }

    public void StartShakeCamera()
    {
        _shakeTimer = _shakeDuration;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeCamera());
    }

    // 랜덤한 원 위의 점 = 비교적 뚝뚝 끊기는 느낌
    IEnumerator ShakeCamera()
    {
        while (_shakeTimer > 0)
        {
            transform.position = _lookAhead.BasePosition + (Vector3)Random.insideUnitCircle * _shakeForce;

            _shakeTimer -= Time.deltaTime;
            yield return null;
        }

        transform.position = _lookAhead.BasePosition;
    }

    public void StartShakeCameraWithPerlinNoise()
    {
        _shakeTimer = _shakeDuration;

        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeCameraWithPerlinNoise());
    }

    // Perlin Noise 사용: 더 부드러운 느낌
    // (Coherent Noise의 특징, 어떤 두 점이든 연속성이 있음)
    IEnumerator ShakeCameraWithPerlinNoise()
    {
        while (_shakeTimer > 0)
        {
            _tick += Time.deltaTime * _shakeRoughness;

            // 0 ~ 1 범위를 -0.5 ~ 0.5로 조정
            float x = Mathf.PerlinNoise(_tick, 0f) - 0.5f;
            float y = Mathf.PerlinNoise(0f, _tick) - 0.5f;

            Vector3 shakeOffset = new Vector3(x, y, 0f) * _shakeForce;
            transform.position = _lookAhead.BasePosition + shakeOffset;

            _shakeTimer -= Time.deltaTime;
            yield return null;
        }

        transform.position = _lookAhead.BasePosition;
    }
}