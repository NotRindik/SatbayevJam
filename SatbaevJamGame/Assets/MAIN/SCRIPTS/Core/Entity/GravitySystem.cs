using System;
using Systems;
using UnityEngine;

public class GravitySystem : BaseSystem,IDisposable
{
    private CharacterController characterController;
    public GravityComponent gravityComponent;
    Vector3 velocity;

    public override void Initialize(Entity owner)
    {
        base.Initialize(owner);
        characterController = owner.GetComponent<CharacterController>();
        gravityComponent = owner.GetControllerComponent<GravityComponent>();
        owner.OnUpdate += Update;
    }
    public override void OnUpdate()
    {
        velocity.y += gravityComponent.gravity * Time.deltaTime * gravityComponent.multiplier;
        characterController.Move(velocity * Time.deltaTime);
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    public void Dispose()
    {
        owner.OnUpdate -= Update;
    }
}

public struct GravityComponent : IComponent
{
    public float gravity;
    public float multiplier;
}