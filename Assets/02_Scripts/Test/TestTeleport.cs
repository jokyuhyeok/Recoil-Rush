using Unity.Cinemachine;
using UnityEngine;

public class TestTeleport : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;

    private Player _player;


    private void Awake()
    {
        _player = FindFirstObjectByType<Player>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Backslash))
            return;

        _player.transform.position = this.transform.position;
        _player.UnlockWeapon(WeaponType.Shotgun);
        _camera.gameObject.SetActive(true);
    }
}