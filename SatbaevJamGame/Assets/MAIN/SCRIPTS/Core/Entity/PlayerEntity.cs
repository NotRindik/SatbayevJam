using System;
using Systems;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerEntity : SavingEntity
{
    private AnimationComponent animationComponent = new AnimationComponent();
    private IInputProvider input;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<IInputProvider>();

        input.GetState().Move.started += c =>
        {
            animationComponent.CrossFade("Move", 0);
        };

        input.GetState().Move.canceled += c =>
        {
            animationComponent.CrossFade("Idle", 0);
        };
    }
}
