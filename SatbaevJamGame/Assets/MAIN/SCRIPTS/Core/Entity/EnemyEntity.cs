using System;
using Systems;
using UnityEngine;

public class EnemyEntity : SavingEntity, ReInitAfterRePlay
{
    private AnimationComponent animationComponent;
    public MeshTrail_Script meshTrail;
    private GravitySystem gvS;
    private MoveComponent movC;
    private AttackSystem atkSys;
    private HealthComponent healthComponent;

    private IInputProvider input;
    private int _combo;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<IInputProvider>();
        atkSys = GetControllerSystem<AttackSystem>();
        animationComponent = GetControllerComponent<AnimationComponent>();
        healthComponent = GetControllerComponent<HealthComponent>();
        movC = GetControllerComponent<MoveComponent>();
        gvS = GetControllerSystem<GravitySystem>();

        input.GetState().Dash.started += c =>
        {
            meshTrail.Activate(0.2f);
        };
        _combo = 1;
        input.GetState().Attack.started += c =>
        {
            if (!atkSys.canAttack)
                return;
            atkSys.OnAttack();

            animationComponent.CrossFade($"Attack{_combo}", 0.1f);
            _combo++;
            if (_combo == 3)
                _combo = 1;
        };

        animationComponent.CrossFade("Idle", 0);
    }

    public override void Update()
    {
        base.Update();
        AnimSates();
        /*        if (!gvS.isGround)
                {
                    animationComponent.CrossFade("Fall", 0);
                }*/
    }

    private void AnimSates()
    {
        if (healthComponent.currHealth <= 0)
        {
            animationComponent.CrossFade("Death", 0.3f);
            return;
        }

        if(atkSys != null) 
            if (atkSys.isAttacking)
                return;
        if (movC.dir != Vector3.zero)
        {
            animationComponent.CrossFade("Walk", 0.3f);
        }
        else
        {
            animationComponent.CrossFade("Idle", 0.3f);
        }
    }

    public void ReInit()
    {
        EntitySetup();
        healthComponent = GetControllerComponent<HealthComponent>();
    }
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