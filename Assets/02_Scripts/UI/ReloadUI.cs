using UnityEngine;

public class ReloadUI : MonoBehaviour
{
    [SerializeField] private GameObject _reloadUIObject;
    [SerializeField] private Transform _handleUI;
    [SerializeField] private float _startX = -0.43f;
    [SerializeField] private float _endX = 0.465f;

    private Player _player;


    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        bool isReloading = _player.IsReloading;

        _reloadUIObject.gameObject.SetActive(isReloading);

        if (!isReloading)
            return;

        float progress = 1f - Mathf.Clamp01(_player.ReloadTimer / _player.ReloadTime);

        Vector3 position = _handleUI.localPosition;
        position.x = Mathf.Lerp(_startX, _endX, progress);

        _handleUI.localPosition = position;
    }
}