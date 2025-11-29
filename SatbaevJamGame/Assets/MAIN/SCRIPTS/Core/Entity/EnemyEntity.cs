using System;
using UnityEngine;

public class EnemyEntity : SavingEntity
{
}

public class ShooterAI : IInputProvider
{
    public InputState inputState;
    public InputState GetState() => inputState;

    public void Initialize(Entity obj)
    {
        inputState = new InputState();
    }

    void IDisposable.Dispose()
    {
        GetState().Dispose();
    }
}