using UnityEngine;

public class DoorAutoMove : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private Collider2D _collider;
    private bool _isLocked = false;


    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public void Open()
    {
        if (_isLocked)
            return;

        _collider.enabled = false;
        _animator.SetBool("IsOpen", true);
    }

    public void CloseAndLock()
    {
        _isLocked = true;
        _collider.enabled = true;
        _animator.SetBool("IsOpen", false);
    }

    public void ResetDoor()
    {
        _isLocked = false;
        _collider.enabled = true;
        _animator.SetBool("IsOpen", false);
    }
}