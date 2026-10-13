using UnityEngine;

public class GameSceneBGM : MonoBehaviour
{
    [Header("Starting BGM")]
    [SerializeField] private BackgroundMusicType _startBGM = BackgroundMusicType.STAGE1;

    private void Start()
    {
        if (AudioManager.Instance == null)
            return;

        // 사망 상태에서는 STAGE1 BGM을 호출하지 않음
        if (AudioManager.Instance.IsDeathAudioPlaying)
            return;

        AudioManager.Instance.PlayBGM(_startBGM);
    }
}