using System;
using System.Collections;
using Systems;
using UnityEngine;
using UnityEngine.Windows;

public class SimpleShooterAI : EnemyEntity
{
    private ShootData shootData;
    private bool shootAble;
    private float animationLen;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<SimpleShooter>();
        shootData = GetControllerComponent<ShootData>();
        shootAble = true;
        input.GetState().Shoot.performed += c =>
        {
            if (shootAble)
            {
                shootAble = false;
                movSys.IsActive = false;
                animationComponent.CrossFade($"Shoot", 0);
                StartCoroutine(ShootAnim());
            }
        };
    }
    public IEnumerator ShootAnim()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        shootData.pist.Shoot();
        yield return new WaitForSecondsRealtime(animationLen);
        shootAble = true;
        movSys.IsActive = true;
    }
    public override void AnimSates()
    {
        if (!shootAble)
            return;
        base.AnimSates();
    }
    public override void Update()
    {
        base.Update();
        for (int i = 0; i < shootData.hits.Length; i++)
        {
            shootData.hits[i] = null;
        }
        Physics.OverlapSphereNonAlloc(
            transform.position,
            shootData.radiusToShoot,
            shootData.hits,
            shootData.TragetLayer
        );

        shootData.Traget = null; 

        for (int i = 0; i < shootData.hits.Length; i++)
        {
            Collider col = shootData.hits[i];
            if (col == null) continue;

            Vector3 dir = (col.transform.position - transform.position);
            float dist = dir.magnitude;

            // RAYCAST: проверяем, кто первый между вами и объектом
            if (Physics.Raycast(
                    transform.position,
                    dir.normalized,
                    out RaycastHit hit,
                    dist,
                    shootData.WallLayer | shootData.TragetLayer)) // маска стен + целей
            {
                // если первым попался именно таргет — значит нет стены
                if (hit.collider == col)
                {
                    shootData.Traget = col.gameObject;
                    break;
                }

                // если первым попалась стена — пропускаем эту цель
            }
        }
    }
}
public struct PatrolData : IComponent
{
    public Transform[] patrolTrack;
}

public class SimpleShooter : BaseInputProvider
{
    private PatrolData shooterData;
    private int nextPatrolPoint;
    private Coroutine loop,rotate;
    public ShootData shootData;
    private bool isFire;
    public override void Initialize(Entity obj)
    {
        base.Initialize(obj);
        shootData = obj.GetControllerComponent<ShootData>();
        shooterData = obj.GetControllerComponent<PatrolData>();
        nextPatrolPoint = 0;
        StartLoop();
    }
    public override void Dispose()
    {
        StopLoop();
    }
    public void StartLoop()
    {
        if(loop == null) 
            loop = Entity.StartCoroutine(AILoop());
        if(rotate == null)
            rotate = Entity.StartCoroutine(RotationProcess());
    }
    public void StopLoop()
    {
        Entity.StopCoroutine(loop);
        Entity.StopCoroutine(rotate);
        rotate = null;
        loop = null;
    }

    public IEnumerator RotationProcess()
    {
        while (true)
        {
            yield return null;
            if (!isActive)
            {
                yield return null;
                continue;
            }
            if (shootData.Traget == null || (shootData.stopRotWhileFire && isFire))
                continue;
            Vector3 dir = shootData.Traget.transform.position - Entity.transform.position;
            dir.y = 0;
            InputState.Move.Update<Vector3>(true, Vector3.zero);
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                Entity.transform.rotation = Quaternion.Slerp(
                    Entity.transform.rotation,
                    targetRot,
                    shootData.rotateSpeed * Time.deltaTime
                );
            }
        }
    }

    public IEnumerator AILoop()
    {
        while (true)
        {
        restart:;
            yield return null;
            if (!isActive)
            {
                yield return null;
                continue;
            }

            if (shootData.Traget != null)
            {
                Vector3 dirToTarget = (shootData.Traget.transform.position - Entity.transform.position);
                dirToTarget.y = 0;

                float dot = Vector3.Dot(Entity.transform.forward, dirToTarget.normalized);


                while (dot < 0.5f)
                {
                    yield return null;
                    goto restart;
                }
                yield return new WaitForSeconds(shootData.delay);
                for (int i = 0; i < shootData.shotcount; i++)
                {
                    isFire = true;
                    InputState.Shoot.Update(true, true);
                    yield return new WaitForSeconds(shootData.delayPerShot);
                }
                isFire = false;
            }
            else
            {
                var currTrack = shooterData.patrolTrack[nextPatrolPoint];
                Vector3 dir = (currTrack.position - Entity.transform.position).normalized;
                dir.y = 0;
                InputState.Move.Update<Vector3>(true, dir);
                Vector3 a = Entity.transform.position;
                Vector3 b = currTrack.position;

                a.y = 0;
                b.y = 0;

                if (Vector3.Distance(a, b) < 1)
                {
                    nextPatrolPoint = (nextPatrolPoint + 1) % shooterData.patrolTrack.Length;
                }
            }
        }
    }
}

public class ShootData : IComponent
{
    public GameObject Traget;
    public Collider[] hits = new Collider[10];
    public float radiusToShoot,delay,shotcount,delayPerShot;
    public LayerMask TragetLayer,WallLayer;
    public Pistol pist;
    public float rotateSpeed = 2;
    public bool stopRotWhileFire;
}