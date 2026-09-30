using UnityEngine;

public class RespawnCheckpoint : MonoBehaviour
{
    [SerializeField] private TestBullet _bulletPrefab;

    private bool _isActive = false;
    private Animator _animator;
    private Transform _target;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_isActive)
            return;

        if (collider.CompareTag("Bullet") == false)
            return;

        _isActive = true;
        RespawnManager.Instance.UpdateCheckpoint(transform.position);

        _animator.SetTrigger("Revenge");
    }

    // 애니메이션 이벤트, Checkpoint_Revenge 클립에서 제어
    private void OnRevenge()
    {
        TestBullet bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        bullet.FireBullet(_target.position - transform.position);
    }
}