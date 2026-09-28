using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector2 _offset = Vector2.zero;
    public Vector2 Offset => _offset;

    // 진동의 크기
    private float _force;
    // 진동 시간 변수
    private float _timer = 0f;

    // 진동의 빈도 (Perlin Noise 버전에서만 사용)
    private float _roughness;
    // 무작위 틱 (Perlin Noise 버전에서만 사용)
    private float _tick;

    private bool _isPerlin = false;


    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f)
        {
            _offset = Vector2.zero;
            return;
        }

        if (_isPerlin)
        {
            _tick += Time.deltaTime * _roughness;

            // 0 ~ 1 범위를 -1 ~ 1로 조정
            float x = (Mathf.PerlinNoise(_tick, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, _tick) - 0.5f) * 2f;

            // 전체 크기 1로 보정
            Vector2 shake = new Vector2(x, y);
            shake = Vector2.ClampMagnitude(shake, 1f);

            _offset = shake * _force;
        }

        else
        {
            _offset = Random.insideUnitCircle * _force;
        }
    }

    public void StartShake(float force, float duration)
    {
        _isPerlin = false;
        _force = force;
        _timer = duration;
    }

    public void StartPerlinShake(float force, float roughness, float duration)
    {
        _isPerlin = true;
        _tick = Random.Range(-100f, 100f);
        _force = force;
        _roughness = roughness;
        _timer = duration;
    }
}