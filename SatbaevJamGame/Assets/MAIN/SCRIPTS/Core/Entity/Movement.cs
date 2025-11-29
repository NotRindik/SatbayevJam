using Sirenix.OdinInspector;
using System;
using System.Collections;
using Systems;
using UnityEngine;

public unsafe class Movement : BaseSystem, IDisposable
{

    private CharacterController _characterController;

    private MoveComponent moveComponent;
    private DashComponent dashComponent;
    private IInputProvider provider;

    public Action<InputContext> moveContext, dashContext;
    public override void Initialize(Entity owner)
    {
        base.Initialize(owner);
        moveComponent = owner.GetControllerComponent<MoveComponent>();
        dashComponent = owner.GetControllerComponent<DashComponent>();
        _characterController = owner.GetComponent<CharacterController>();
        provider = owner.GetControllerSystem<IInputProvider>();
        owner.OnUpdate += Update;
        InitActions();

        provider.GetState().Move.performed += moveContext;
        provider.GetState().Move.canceled += moveContext;
        provider.GetState().Dash.started += dashContext;
    }
    public void InitActions()
    {
        moveContext = c =>
        {
            moveComponent.dir = c.ReadValue<Vector3>();
        };

        dashContext = c =>
        {
            if(dashComponent.isDashing == false)
                dashComponent.dashProcess = owner.StartCoroutine(DashProcess());
        };
    }
    public IEnumerator DashProcess()
    {
        float currTime = dashComponent.time;
        Vector3 temp = moveComponent.dir;
        while (currTime >= 0)
        {
            _characterController.Move(moveComponent.dir * dashComponent.speed * moveComponent.deltaTime);
            currTime -= moveComponent.deltaTime;
            yield return null;
        }
        if(!moveComponent.isTimeNotScale) yield return new WaitForSeconds(dashComponent.delay);
        else yield return new WaitForSecondsRealtime(dashComponent.delay);
        dashComponent.dashProcess = null;
    }
    public override void OnUpdate()
    {
        Vector3 moveDir = moveComponent.dir * moveComponent.deltaTime * moveComponent.speed;
        _characterController.Move(moveDir);
    }

    public void Dispose()
    {
        provider.GetState().Move.performed -= moveContext;
        provider.GetState().Move.canceled -= moveContext;
        provider.GetState().Dash.started -= dashContext;
        owner.OnUpdate -= Update;
    }
}
public interface NotCopy { }
public class MoveComponent : IComponent, ICopyable
{
    public float speed;
    public Vector3 dir;
    public bool isTimeNotScale;
    public float deltaTime => isTimeNotScale ? Time.unscaledDeltaTime : Time.deltaTime;

    public IComponent Copy()
    {
        var copy = new MoveComponent();
        copy.speed = speed;
        copy.isTimeNotScale = isTimeNotScale;
        return copy;
    }
}
public struct DashComponent : IComponent
{
    public float speed,time,delay;
    public Coroutine dashProcess;
    public bool isDashing => dashProcess != null;
}
public interface IComponent
{

}