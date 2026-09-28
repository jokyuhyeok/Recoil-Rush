using UnityEngine;
using System.Collections; // 코루틴을 사용하기 위해 추가
using System.Collections.Generic; // Queue를 사용하기 위해 추가

public class BossController : MonoBehaviour
{
    // 패턴들을 순서대로 담아놓을 대기열입니다. 
    private Queue<IBossCommand> _commandQueue = new Queue<IBossCommand>();

    // 현재 보스가 패턴 하나를 실행 중인지 확인용 변수입니다.
    private bool _isExecuting = false;

    private void Start()
    {
        StartCoroutine(ProcessCommands());
    }

    // 새로운 패턴을 queue에 줄 세울 때 사용하는 메서드입니다. 
    private void EnqueueCommand(IBossCommand command)
    {
        _commandQueue.Enqueue(command);
    }

    // 핵심 코루틴입니다. (계속 큐를 감시하고 실행하는 루프)
    private IEnumerator ProcessCommands()
    {
        // 보스가 살아있는동안 계속 반복합니다.
        while (true)
        {
            if ((!_isExecuting) && (_commandQueue.Count > 0))
            {
                _isExecuting = true;
                // queue의 맨 앞에서 패턴을 하나 꺼냅니다.
                IBossCommand currentCommand = _commandQueue.Dequeue();
                
                // 꺼낸 패턴을 실행합니다. (패턴이 끝날 때까지 대기) 
                yield return StartCoroutine(currentCommand.Execute(this));
                _isExecuting = false;
            }

            // queue가 비어있거나 패턴이 실행 중이라면 다음 프레임까지 대기합니다. 
            yield return null;
        }
    }
}
