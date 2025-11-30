using System;
using System.Collections;
using Systems;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerEntity : SavingEntity, ReInitAfterRePlay
{
    private AnimationComponent animationComponent;
    public MeshTrail_Script meshTrail;
    private GravitySystem gvS;
    private MoveComponent movC;
    private Movement movSys;
    private RotateFaceTo RotateFaceTo;
    private AttackSystem atkSys;
    private HealthComponent healthComponent;
    private HealthSystem hsSys;
    public Pistol pistol;
    public AudioClip slash;
    public AudioClip dash;
    public AudioClip walk;
    public float animationLen;
    private bool shootAble;
    private IInputProvider input;
    private int _combo;
    public float dashtimeconsume = 15f;

    public bool Died;
    public LayerMask tempLayer;

    private Action<InputContext> dashAction,attackAction,shootAction;
    private Action<HitInfo> onDie;
    //public GameObject timemana;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<IInputProvider>();
        hsSys = GetControllerSystem<HealthSystem>();
        atkSys = GetControllerSystem<AttackSystem>();
        RotateFaceTo = GetControllerSystem<RotateFaceTo>();
        movSys = GetControllerSystem<Movement>();
        animationComponent = GetControllerComponent<AnimationComponent>();
        healthComponent = GetControllerComponent<HealthComponent>();
        movC = GetControllerComponent<MoveComponent>();
        tempLayer = gameObject.layer;
        gvS = GetControllerSystem<GravitySystem>();
        shootAble = true;

        dashAction = c =>
        {
            if (Died)
                return;
            AudioManager.instance.PlayAudioClip(dash);
            FindAnyObjectByType<PlayerUIManager>().SpendTime(dashtimeconsume);
            meshTrail.Activate(0.2f);
        };
        attackAction = c =>
        {
            if (Died)
                return;
            if (!atkSys.canAttack)
                return;
            atkSys.OnAttack();

            animationComponent.CrossFade($"Attack{_combo}", 0.1f);
            AudioManager.instance.PlayAudioClip(slash);
            _combo++;
            if (_combo == 3)
                _combo = 1;
        };

        shootAction = c =>
        {
            if (Died)
                return;
            if (shootAble)
            {
                shootAble = false;
                movSys.IsActive = false;
                animationComponent.CrossFade($"Shoot", 0);
                StartCoroutine(ShootAnim());
            }
        };

        onDie = c => 
        {
            Died = true;
            animationComponent.CrossFade($"Death", 0);
            gameObject.layer = 0;
            movSys.IsActive = false;
            RotateFaceTo.IsActive = false;
        };

        hsSys.OnDie += onDie;
        input.GetState().Dash.started += dashAction;
        _combo = 1;

        input.GetState().Attack.started += attackAction;

        input.GetState().Shoot.started += shootAction;

        animationComponent.CrossFade("Idle", 0);
    }

    public override void Update()
    {
        base.Update();
        AnimSates();
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

        if (healthComponent.currHealth <= 0)
        {
            return;
        }
        else
        {
            if(animationComponent.currentState == "Death")
            {
                hsSys.IsActive = false;
/*                StartCoroutine(std.Utilities.Invoke(() => { hsSys.IsActive = true; gameObject.layer = tempLayer; }, 4));*/
                hsSys.IsActive = true; gameObject.layer = tempLayer;
                Died = false;
                movSys.IsActive = true;
                RotateFaceTo.IsActive = true;
            }
        }

            if (!shootAble)
            return;

        if (atkSys.isAttacking)
            return;
        if (movC.dir != Vector3.zero)
        {
            if (animationComponent.currentState != "Walk")
            {
                animationComponent.CrossFade("Walk", 0.3f);
                StartCoroutine(std.Utilities.InvokeRepeatedly(()=> AudioManager.instance.PlayAudioClip(walk), 0.3f,() => animationComponent.currentState != "Walk"));
            }
        }
        else
        {
            animationComponent.CrossFade("Idle", 0.3f);
        }
    }

    protected override void ReferenceClean()
    {
        input.GetState().Dash.started -= dashAction;

        input.GetState().Attack.started -= attackAction;

        input.GetState().Shoot.started -= shootAction;
        base.ReferenceClean();
    }

    public void ReInit()
    {
        hsSys.Initialize(this);
        healthComponent = GetControllerComponent<HealthComponent>();
    }
}
