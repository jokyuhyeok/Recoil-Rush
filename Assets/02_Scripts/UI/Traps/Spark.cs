using UnityEngine;

public class Spark : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 3f;

    private void Start()
    {
        // 맵 밖으로 날아가도 자동 제거
        Destroy(gameObject, _lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 바닥에 닿으면 즉시 삭제
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
