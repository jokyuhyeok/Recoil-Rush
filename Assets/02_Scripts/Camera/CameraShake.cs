using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector2 _offset = Vector2.zero;
    public Vector2 Offset => _offset;

    // 진동의 크기
    private float _force;

    // 진동 시간
    private float _timer;
    private float _duration;

    // Perlin Noise 이동 속도
    private float _roughness;

    // Perlin Noise 좌표
    private float _tick = 0f;

    // X/Y가 같은 Noise를 사용하지 않도록 서로 다른 시작점 사용
    private float _seedX;
    private float _seedY;

    private bool _isPerlin;


    private void Awake()
    {
        _seedX = Random.Range(0f, 1000f);
        _seedY = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        if (_timer <= 0f)
        {
            _offset = Vector2.zero;
            return;
        }

        _timer -= Time.deltaTime;

        if (_isPerlin)
        {
            UpdatePerlinShake();
        }
        else
        {
            UpdateRandomShake();
        }
    }

    private void UpdatePerlinShake()
    {
        _tick += Time.deltaTime * _roughness;

        float x = (Mathf.PerlinNoise(_seedX + _tick, 0f) - 0.5f) * 2f;
        float y = (Mathf.PerlinNoise(_seedY + _tick, 0f) - 0.5f) * 2f;

        Vector2 shake = new Vector2(x, y);
        shake = Vector2.ClampMagnitude(shake, 1f);

        // 1 → 0으로 감소
        float fade = Mathf.Clamp01(_timer / _duration);

        _offset = shake * _force * fade;
    }

    private void UpdateRandomShake()
    {
        float fade = Mathf.Clamp01(_timer / _duration);

        _offset = Random.insideUnitCircle * _force * fade;
    }

    public void StartShake(float force, float duration)
    {
        _isPerlin = false;

        _force = force;
        _duration = duration;
        _timer = duration;
    }

    public void StartPerlinShake(float force, float roughness, float duration)
    {
        _isPerlin = true;

        _force = force;
        _roughness = roughness;
        _duration = duration;
        _timer = duration;
    }
}