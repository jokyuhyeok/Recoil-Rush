using System.Collections;
using UnityEngine;

public class FireShot : MonoBehaviour
{
    [Header("Flame")]
    [SerializeField] private GameObject _flamePrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Spawn Interval")]
    [SerializeField] private float _minInterval = 2f;
    [SerializeField] private float _maxInterval = 5f;

    [Header("Shoot Power")]
    [SerializeField] private float _minXPower = 2f;
    [SerializeField] private float _maxXPower = 4f;

    [SerializeField] private float _minYPower = 5f;
    [SerializeField] private float _maxYPower = 8f;

    private GameManager _gameManager;


    private void Start()
    {
        if (GameManager.Instance != null)
        {
            _gameManager = GameManager.Instance;
        }

        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            if (_gameManager.IsCutscene)
            {
                yield return null;
            }

            else
            {
                float waitTime = Random.Range(
                    _minInterval,
                    _maxInterval
                );

                yield return new WaitForSeconds(waitTime);

                ShootFlame();
            }
        }
    }

    private void ShootFlame()
    {
        if (_gameManager.IsCutscene)
            return;

        GameObject flame = Instantiate(
            _flamePrefab,
            _firePoint.position,
            Quaternion.identity
        );

        Rigidbody2D rb = flame.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Flame Prefab에 Rigidbody2D가 없음!");
            return;
        }

        // 좌우 랜덤
        float direction = Random.value < 0.5f ? -1f : 1f;

        float xPower =
            Random.Range(_minXPower, _maxXPower)
            * direction;

        float yPower =
            Random.Range(_minYPower, _maxYPower);

        rb.AddForce(
            new Vector2(xPower, yPower),
            ForceMode2D.Impulse
        );
    }
}