using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
    [Header("Выполнить при входе в триггер")]
    public UnityEvent onTriggerEnterEvent;
    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsValidLayer(other.gameObject.layer))        // Запускаем все действия, добавленные в инспекторе
            onTriggerEnterEvent.Invoke();
    }
    private bool IsValidLayer(int layer)
    {
        return layer == playerLayer;
    }
}
