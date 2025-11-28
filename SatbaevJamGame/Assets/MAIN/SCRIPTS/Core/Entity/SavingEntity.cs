using UnityEngine;
using UnityEngine.Events;

public class SavingEntity : Entity
{
    public virtual void Start()
    {
        TimeDataManager.Instance.RegisterEntity(this);
    }
}

public class ReplayActions : IComponent
{
    public UnityEvent OnReplayStart, OnReplayEnd;
}
