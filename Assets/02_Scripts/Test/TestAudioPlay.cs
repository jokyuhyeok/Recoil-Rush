using UnityEngine;

public class TestAudioPlay : MonoBehaviour
{
    private AudioManager _manager;

    private void Start()
    {
        _manager = AudioManager.Instance;
        Debug.Log("<color=#FAD656>Test Audio Play</color>");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _manager.PlayBGM(BackgroundMusicType.TITLE);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            //_manager.PlaySFX(SoundEffectType.SHOOT);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _manager.PlaySFX(SoundEffectType.PLAYER_HIT);
        }
    }
}