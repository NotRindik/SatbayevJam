using System;
using System.Collections;
using Systems;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyEntity : SavingEntity, ReInitAfterRePlay
{
    protected AnimationComponent animationComponent;
    public MeshTrail_Script meshTrail;
    protected GravitySystem gvS;
    protected MoveComponent movC;
    protected Movement movSys;
    protected HealthSystem healthSys;
    protected AttackSystem atkSys;
    protected HealthComponent healthComponent;
    protected CharacterController chC;

    protected BaseInputProvider input;
    protected int _combo;
    int lTemp;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<BaseInputProvider>();
        healthSys = GetControllerSystem<HealthSystem>();
        movSys = GetControllerSystem<Movement>();
        atkSys = GetControllerSystem<AttackSystem>();
        animationComponent = GetControllerComponent<AnimationComponent>();
        healthComponent = GetControllerComponent<HealthComponent>();
        movC = GetControllerComponent<MoveComponent>();
        gvS = GetControllerSystem<GravitySystem>();
        lTemp = gameObject.layer;
        input.GetState().Dash.started += c =>
        {
            meshTrail.Activate(0.2f);
        };


        animationComponent.CrossFade("Idle", 0);
    }

    public override void Update()
    {
        base.Update();
        AnimSates();
    }

    public virtual void AnimSates()
    {
        if (healthComponent.currHealth <= 0)
        {
            animationComponent.CrossFade("Death", 0.3f);
            gameObject.layer = 0;
            movSys.IsActive = false;
            input.isActive = false;
            return;
        }
        else
        {
            input.isActive = true;
            gameObject.layer = lTemp;
        }

        if (movC.dir != Vector3.zero)
        {
            animationComponent.CrossFade("Walk", 0.3f);
        }
        else
        {
            animationComponent.CrossFade("Idle", 0.3f);
        }
    }

    public virtual void ReInit()
    {
        healthSys.Initialize(this);
        healthComponent = GetControllerComponent<HealthComponent>();
    }
}

public class ShooterAI : IInputProvider
{
    public InputState inputState;

    public ShooterAI()
    {
    }

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