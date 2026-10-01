using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가

// Boss의 모든 패턴(돌진, 후진, 사격, 소환 등)이 반드시 지켜야하는 규격입니다.
public interface IBossCommand
{
    // 패턴은 시간이 걸리는 작업이므로, IEnumerator(코루틴) 형태로 실행하게 하겠습니다. 
    IEnumerator Execute(BossController boss);
}