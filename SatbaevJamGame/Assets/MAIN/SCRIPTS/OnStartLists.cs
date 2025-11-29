using UnityEngine;
using UnityEngine.Events;

public class OnStartLists : MonoBehaviour
{
    [Header("Выполнить при входе в триггер")]
    public UnityEvent onTriggerEnterEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onTriggerEnterEvent.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
