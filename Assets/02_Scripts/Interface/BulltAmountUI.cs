using UnityEngine;
using UnityEngine.UI;

public class BulletsAmountUI : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Animator _animator;

    public void Fire()
    {
        _animator.SetTrigger("Fire");
    }

    public void ResetBullet()
    {
        _animator.Rebind();
        _animator.Update(0f);
    }
}