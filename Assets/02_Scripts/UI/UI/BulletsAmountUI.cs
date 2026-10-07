using UnityEngine;
using UnityEngine.UI;

public class BulletsAmountUI : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private Image _bulletImage;
    [SerializeField] private Animator _animator;

    [Header("Sprite")]
    [SerializeField] private Sprite _loadedSprite;
    [SerializeField] private Sprite _spentSprite;

    private static readonly int FireStateHash = Animator.StringToHash("Fire");

    public void Fire()
    {
        if (_animator == null)
            return;

        _animator.enabled = true;
        _animator.Play(FireStateHash, 0, 0f);
    }

    public void SetSpentInstant()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_bulletImage != null && _spentSprite != null)
        {
            _bulletImage.sprite = _spentSprite;
        }
    }

    public void ResetBullet()
    {
        if (_animator != null)
        {
            _animator.enabled = false;
        }

        if (_bulletImage != null && _loadedSprite != null)
        {
            _bulletImage.sprite = _loadedSprite;
        }
    }
}