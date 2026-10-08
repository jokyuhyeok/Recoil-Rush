using UnityEngine;

public class DoorAutoMove : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private bool _isLocked = false;

    public void Open()
    {
        if (_isLocked)
            return;

        _animator.SetBool("IsOpen", true);
    }

    public void CloseAndLock()
    {
        _isLocked = true;
        _animator.SetBool("IsOpen", false);
    }

    public void ResetDoor()
    {
        _isLocked = false;
        _animator.SetBool("IsOpen", false);
    }
}
