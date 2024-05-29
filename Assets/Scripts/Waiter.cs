using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Waiter : MonoBehaviour
{
    [SerializeField] private UnityEvent whatHappensNext;

    public void Wait(float time)
    {
        StartCoroutine(WaitRoutine(time));
    }

    public IEnumerator WaitRoutine(float time)
    {
        yield return new WaitForSeconds(time);

        whatHappensNext.Invoke();
    }

}
