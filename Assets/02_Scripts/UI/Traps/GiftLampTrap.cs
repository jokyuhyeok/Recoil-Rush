using System.Collections;
using UnityEngine;

public class GiftLampTrap : MonoBehaviour
{
    [Header("Box")]
    [SerializeField] private Animator _boxAnimator;

    [Header("Lamp Spawn")]
    [SerializeField] private GameObject _lampPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _lampSpeed = 25f;
    [SerializeField] private float _spawnDelay = 0.2f;

    private bool _activated;
    private GameObject _spawnedLamp;


    private void Awake()
    {
        ResetTrap();
    }


    private IEnumerator Start()
    {
        while (RespawnManager.Instance == null)
            yield return null;

        RespawnManager.Instance.OnRespawnPlayerEvent += OnRespawnPlayer;
    }


    private void OnDestroy()
    {
        if (RespawnManager.Instance != null)
            RespawnManager.Instance.OnRespawnPlayerEvent -= OnRespawnPlayer;
    }


    private void OnRespawnPlayer(Vector3 respawnPosition)
    {
        ResetTrap();
    }


    // 플레이어가 상자 근처에 왔을 때 호출
    public void ActivateTrap()
    {
        if (_activated)
            return;

        _activated = true;

        Debug.Log("[GiftLampTrap] 상자 열기");

        if (_boxAnimator != null)
        {
            _boxAnimator.ResetTrigger("Open");
            _boxAnimator.SetTrigger("Open");
        }
    }

    public void SpawnLamp()
    {
        StartCoroutine(SpawnLampRoutine());
    }

    private IEnumerator SpawnLampRoutine()
    {
        yield return new WaitForSeconds(_spawnDelay);

        if (!_activated)
            yield break;

        if (_spawnedLamp != null)
            yield break;

        if (_lampPrefab == null || _spawnPoint == null)
            yield break;

        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            yield break;

        // 상자 위치에서 가로등 생성
        _spawnedLamp = Instantiate(
            _lampPrefab,
            _spawnPoint.position,
            _spawnPoint.rotation
        );

        // ★ 상자가 플레이어 방향 계산
        Vector2 direction =
            ((Vector2)player.transform.position -
             (Vector2)_spawnPoint.position).normalized;

        // ★ 생성된 가로등에 발사 명령
        TrapBullet trapBullet =
            _spawnedLamp.GetComponent<TrapBullet>();

        if (trapBullet != null)
        {
            trapBullet.Fire(direction, _lampSpeed);
        }
    }


    public void ResetTrap()
    {
        _activated = false;

        if (_spawnedLamp != null)
        {
            Destroy(_spawnedLamp);
            _spawnedLamp = null;
        }

        if (_boxAnimator != null)
        {
            // Animator 완전 초기화
            _boxAnimator.Rebind();
            _boxAnimator.Update(0f);

            // 닫힌 상태 강제 재생
            _boxAnimator.Play("Box_Idle", 0, 0f);
            _boxAnimator.Update(0f);
        }
    }
}