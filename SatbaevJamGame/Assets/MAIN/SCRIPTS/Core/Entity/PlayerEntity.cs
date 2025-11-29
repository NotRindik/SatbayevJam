using System.Collections;
using Systems;
using UnityEngine;

public class PlayerEntity : SavingEntity
{
    private AnimationComponent animationComponent;
    public MeshTrail_Script meshTrail;
    private GravitySystem gvS;
    private MoveComponent movC;
    private Movement movSys;
    private RotateFaceTo RotateFaceTo;
    private AttackSystem atkSys;
    private HealthComponent healthComponent;
    public Pistol pistol;
    public AudioClip slash;
    public AudioClip dash;

    public float animationLen;
    private bool shootAble;
    private IInputProvider input;
    private int _combo;
    
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<IInputProvider>();
        atkSys = GetControllerSystem<AttackSystem>();
        RotateFaceTo = GetControllerSystem<RotateFaceTo>();
        movSys = GetControllerSystem<Movement>();
        animationComponent = GetControllerComponent<AnimationComponent>();
        healthComponent = GetControllerComponent<HealthComponent>();
        movC = GetControllerComponent<MoveComponent>();
        gvS = GetControllerSystem<GravitySystem>();
        shootAble = true;
        input.GetState().Dash.started += c =>
        {
            AudioManager.instance.PlayAudioClip(dash);

            meshTrail.Activate(0.2f);
        };
        _combo = 1;
        input.GetState().Attack.started += c =>
        {
            if (!atkSys.canAttack)
                return;
            atkSys.OnAttack();
            
            animationComponent.CrossFade($"Attack{_combo}", 0.1f);
            AudioManager.instance.PlayAudioClip(slash);
            _combo++;
            if (_combo == 3)
                _combo = 1;
        };

        input.GetState().Shoot.started += c =>
        {
            if (shootAble)
            {
                shootAble = false;
                movSys.IsActive = false;
                animationComponent.CrossFade($"Shoot", 0);
                StartCoroutine(ShootAnim());
            }
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

    public IEnumerator ShootAnim()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        RotateFaceTo.IsActive = false;
        pistol.Shoot();
        Vector3 dir = (pistol.targetPoint - transform.position);
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);
        yield return new WaitForSecondsRealtime(animationLen);
        RotateFaceTo.IsActive = true;
        shootAble = true;
        movSys.IsActive = true;
    }
    private void AnimSates()
    {
        if (!shootAble)
            return;
        if(healthComponent.currHealth <= 0)
        {
            animationComponent.CrossFade("Death", 0.3f);
        }

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
}
