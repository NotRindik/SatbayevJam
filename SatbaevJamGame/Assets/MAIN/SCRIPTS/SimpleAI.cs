using System.Collections;
using Systems;
using UnityEngine;
using UnityEngine.Windows;

public class SimpleShooterAI : EnemyEntity
{
    private ShootData shootData;
    public override void Start()
    {
        base.Start();
        input = GetControllerSystem<SimpleShooter>();
        shootData = GetControllerComponent<ShootData>();
    }

    public override void Update()
    {
        base.Update();
        for (int i = 0; i < shootData.hits.Length; i++)
        {
            shootData.hits[i] = null;
        }
        Physics.OverlapSphereNonAlloc(transform.position, shootData.radiusToShoot,shootData.hits,shootData.TragetLayer);
        shootData.Traget = shootData.hits[0]?.gameObject;
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
    private Coroutine loop;
    public ShootData shootData;
    public override void Initialize(Entity obj)
    {
        base.Initialize(obj);
        shootData = obj.GetControllerComponent<ShootData>();
        shooterData = obj.GetControllerComponent<PatrolData>();
        nextPatrolPoint = 0;
        StartLoop();
    }

    public void StartLoop()
    {
        if(loop == null) 
            loop = Entity.StartCoroutine(AILoop());
    }
    public void StopLoop()
    {
        Entity.StopCoroutine(loop);
        loop = null;
    }

    public IEnumerator AILoop()
    {
        while (true)
        {
            if (!isActive)
            {
                yield return null;
                continue;
            }

            if (shootData.Traget != null)
            {
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

                shootData.pist.Shoot();
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
            yield return null;
        }
    }
}

public class ShootData : IComponent
{
    public GameObject Traget;
    public Collider[] hits = new Collider[10];
    public float radiusToShoot;
    public LayerMask TragetLayer;
    public Pistol pist;
    public float rotateSpeed = 2;
}