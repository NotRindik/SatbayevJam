using DG.Tweening;
using System;
using System.Collections;
using Systems;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AttackSystem : BaseSystem, IDisposable
{
    private AttackComponent attackComponent;
    private CharacterController chC;
    private IInputProvider inputProvider;
    private Movement movSys;
    private RotateFaceTo rotSys;
    private MoveComponent movC;
    private Coroutine attkProcess;
    private Collider[] entities = new Collider[10];
    public bool canAttack = true;
    public bool isAttacking => attkProcess != null;

    public override void Initialize(Entity owner)
    {
        attackComponent = owner.GetControllerComponent<AttackComponent>();
        movC = owner.GetControllerComponent<MoveComponent>();
        inputProvider = owner.GetControllerSystem<IInputProvider>();
        movSys = owner.GetControllerSystem<Movement>();
        rotSys = owner.GetControllerSystem<RotateFaceTo>();
        chC = owner.GetComponent<CharacterController>();
        base.Initialize(owner);
        entities = new Collider[10];
        owner.OnGizmosUpdate += OnDrawGizmos;
        canAttack = true;
    }

    public void OnAttack()
    {
        if (!IsActive)
            return;

        if(attkProcess == null && canAttack) 
            attkProcess = owner.StartCoroutine(AttackProcess());
    }

    public IEnumerator AttackProcess()
    {
        canAttack = false;
        int hitCount = Physics.OverlapSphereNonAlloc(owner.transform.position, attackComponent.attackRange, entities, attackComponent.layerMask);
        Entity nearestEnemy = null;
        float nearestDist = float.MaxValue;
        movSys.IsActive = false;
        rotSys.IsActive = false;
        attackComponent.OnAttackStart?.Invoke();
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = entities[i];
            Entity ent = col.GetComponent<Entity>();
            if (ent == null || ent == owner)
                continue;
            float dist = Vector3.Distance(owner.transform.position, ent.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestEnemy = ent;
            }
        }
        Vector3 dashTarget;

        if (nearestEnemy != null)
        {
            dashTarget = owner.transform.position +
                (nearestEnemy.transform.position - owner.transform.position).normalized
                * attackComponent.dashDist;

            Vector3 lookDir = (nearestEnemy.transform.position - owner.transform.position).normalized;
            owner.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
        else
        {
            dashTarget = owner.transform.position +
                movC.dir.normalized * attackComponent.dashDist;
        }

        owner.StartCoroutine(DashCoroutine(chC, dashTarget, attackComponent.timeBefHit / 2));

        yield return new WaitForSecondsRealtime(attackComponent.timeBefHit/3);

        if (nearestEnemy != null)
        {
            var healthSystem = nearestEnemy.GetControllerSystem<HealthSystem>();
            if (healthSystem != null)
            {
                Debug.Log(nearestEnemy.gameObject.name);
                new Damage(attackComponent.damage).ApplyDamage(healthSystem, new HitInfo(owner));
            }
        }

        attkProcess = null;
        movSys.IsActive = true;
        rotSys.IsActive = true;
        attackComponent.OnAttackEnd?.Invoke();
        yield return new WaitForSecondsRealtime(attackComponent.delay);
        canAttack = true;
    }
    private IEnumerator DashCoroutine(CharacterController cc, Vector3 target, float duration)
    {
        Vector3 startPos = cc.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Плавное перемещение с Ease Out
            Vector3 newPos = Vector3.Lerp(startPos, target, 1 - Mathf.Pow(1 - t, 2));
            Vector3 moveDelta = newPos - cc.transform.position;
            cc.Move(moveDelta);

            yield return null;
        }

        // Финальная позиция
        Vector3 finalDelta = target - cc.transform.position;
        cc.Move(finalDelta);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(owner.transform.position, attackComponent.attackRange);
    }
    public void Dispose()
    {
        owner.OnGizmosUpdate -= OnDrawGizmos;
    }
}

public struct AttackComponent : IComponent
{
    public float attackRange,timeBefHit,delay,dashDist;
    public DamageComponent damage;
    public LayerMask layerMask;
    public UnityEvent OnAttackStart, OnAttackEnd;
}