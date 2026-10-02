using UnityEngine;
using System.Collections;

// 레이저 프리팹에 들어갈 코드입니다.
public class BossLaser : MonoBehaviour
{
    [SerializeField] private GameObject _warningLine;
    [SerializeField] private GameObject _actualBeam;

    public void FireLaser(float warningTime, float beamTime)
    {
        StartCoroutine(LaserSequence(warningTime, beamTime));
    }

    private IEnumerator LaserSequence(float warningTime, float beamTime)
    {
        // 경고선을 켭니다.
        if (_warningLine != null)
        {
            _warningLine.SetActive(true);
        }

        if (_actualBeam != null)
        {
            _actualBeam.SetActive(false);
        }

        yield return new WaitForSeconds(warningTime);

        // 실제 레이저를 발사합니다.
        if (_warningLine != null)
        {
            _warningLine.SetActive(false);
        }

        if (_actualBeam != null)
        {
            _actualBeam.SetActive(true);
        }

        Debug.Log("<color=magenta>레이저를 발사합니다. 피해보시지!</color>");

        yield return new WaitForSeconds(beamTime);

        // 끝나면 오브젝트를 파괴합니다.
        if (_actualBeam != null)
        {
            _actualBeam.SetActive(false);
        }

        Destroy(gameObject);
    }
}