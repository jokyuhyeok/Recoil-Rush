using System.Collections;
using UnityEngine;

public class SequentialSpikeTrap : MonoBehaviour
{
    [Header("Spikes")]
    [SerializeField] private FallingSpike[] _spikes;

    [Header("Timing")]
    [SerializeField] private float _startDelay = 0.2f;
    [SerializeField] private float _interval = 0.15f;

    private bool _activated;
    private Coroutine _sequenceCoroutine;

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

    // 기존 TrapTrigger에서 호출
    public void ActivateTrap()
    {
        if (_activated)
            return;

        _activated = true;

        _sequenceCoroutine = StartCoroutine(DropSequence());
    }

    private IEnumerator DropSequence()
    {
        yield return new WaitForSeconds(_startDelay);

        for (int i = 0; i < _spikes.Length; i++)
        {
            if (_spikes[i] != null)
                _spikes[i].Drop();

            yield return new WaitForSeconds(_interval);
        }
    }

    public void ResetTrap()
    {
        if (_sequenceCoroutine != null)
        {
            StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = null;
        }

        _activated = false;

        foreach (FallingSpike spike in _spikes)
        {
            if (spike != null)
                spike.ResetSpike();
        }
    }
}