using DG.Tweening;
using System.Collections;
using Systems;
using UnityEngine;

public class CreaperEntity : EnemyEntity
{
    private CreeperData creeperData;
    public SkinnedMeshRenderer SkinnedMeshRenderer;
    public GameObject ExplodeSphere;
    public CreeperAI creap;
    public override void Start()
    {
        base.Start();
        creeperData = GetControllerComponent<CreeperData>();
        creap = GetControllerSystem<CreeperAI>();
    }
    public override void Update()
    {
        base.Update();
        for (int i = 0; i < creeperData.hits.Length; i++)
        {
            creeperData.hits[i] = null;
        }
        Physics.OverlapSphereNonAlloc(transform.position, creeperData.ChaseStartDist, creeperData.hits, creeperData.TragetLayer);
        creeperData.Target = creeperData.hits[0]?.gameObject;
    }
    public override void ReInit()
    {
        base.ReInit();
        creeperData = GetControllerComponent<CreeperData>();
        creap.creeperData = creeperData;
        creap.hp = healthComponent;

    }
    public override void AnimSates()
    {
        if(creeperData.isExplode)
        {
            SkinnedMeshRenderer.enabled = false;
        }
        else
        {
            SkinnedMeshRenderer.enabled = true;
        }
        if(creeperData.isExploding)
        {
            if(animationComponent.currentState != "Explode")
            {
                animationComponent.CrossFade("Explode", 0.2f);
                ExplodeSphere.transform
                    .DOScale(Vector3.one * creeperData.explodeDistance, 0.5f)
                    .SetEase(Ease.InBounce)
                    .OnComplete(() =>
                    {
                        ExplodeSphere.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBounce);
                    });
            }
            return;
        }
        if (healthComponent.currHealth <= 0)
        {
            return;
        }
            base.AnimSates();
    }
}
public class CreeperData : IComponent, ICopyable
{
    public GameObject Target;
    public float ChaseStartDist = 10f;
    public Collider[] hits = new Collider[10];
    public LayerMask TragetLayer;

    public float explodeDistance = 1.5f;
    public UnityEngine.Events.UnityEvent<Entity> OnExplode;
    public bool isExploding;
    public bool isExplode;
    public DamageComponent dmg;

    public IComponent Copy()
    {
        var a = new CreeperData();
        a.isExploding = isExploding;
        a.Target = Target;
        a.ChaseStartDist = ChaseStartDist;
        a.TragetLayer = TragetLayer;
        a.hits = hits;
        a.explodeDistance = explodeDistance;
        a.OnExplode = OnExplode;
        a.dmg = dmg;

        return a;
    }
}

public class CreeperAI : BaseInputProvider
{
    private PatrolData patrolData;
    public CreeperData creeperData;
    public HealthComponent hp;

    private int nextPatrolPoint;

    private Coroutine loop;
    private Coroutine rotate;

    private bool isChasing;

    public override void Initialize(Entity obj)
    {
        base.Initialize(obj);

        patrolData = obj.GetControllerComponent<PatrolData>();
        creeperData = obj.GetControllerComponent<CreeperData>();
        hp = obj.GetControllerComponent<HealthComponent>();

        nextPatrolPoint = 0;
        StartLoop();
    }

    public override void Dispose()
    {
        StopLoop();
    }

    public void StartLoop()
    {
        if (loop == null)
            loop = Entity.StartCoroutine(AILoop());
    }

    public void StopLoop()
    {
        if (loop != null) Entity.StopCoroutine(loop);
        if (rotate != null) Entity.StopCoroutine(rotate);

        loop = null;
        rotate = null;
    }


    // ------------------------------- AI LOOP -------------------------------

    public IEnumerator AILoop()
    {
        while (true)
        {
            yield return null;

            if (!isActive || creeperData.isExploding)
                continue;

            // ------------------------- DETECTION LOGIC -------------------------

            if (creeperData.Target != null)
            {
                float dist = Vector3.Distance(
                    new Vector3(Entity.transform.position.x, 0, Entity.transform.position.z),
                    new Vector3(creeperData.Target.transform.position.x, 0, creeperData.Target.transform.position.z)
                );

                // Переключаемся в режим преследования
                isChasing = true;

                // Если в радиусе детонации — взрываемся
                if (dist <= creeperData.explodeDistance/3 || hp.currHealth <= 0)
                {
                    InputState.Move.Update<Vector3>(true, Vector3.zero);
                    creeperData.isExploding = true;
                    yield return new WaitForSeconds(0.5f);
                    for (int i = 0; i < creeperData.hits.Length; i++)
                    {
                        creeperData.hits[i] = null;
                    }
                    Physics.OverlapSphereNonAlloc(Entity.transform.position, creeperData.explodeDistance, creeperData.hits, creeperData.TragetLayer);
                    if(creeperData.hits[0] != null)
                    {

                        if (creeperData.hits[0].gameObject.TryGetComponent<Entity>(out Entity ent))
                        {
                            var hp = ent.GetControllerSystem<HealthSystem>();

                            new Damage(creeperData.dmg).ApplyDamage(hp, new HitInfo(Entity.transform.position));

                        }
                    }
                    creeperData.OnExplode.Invoke(Entity);
                    creeperData.isExplode = true;
                    continue;
                }

                // ------------------------- CHASE -------------------------
                Vector3 chaseDir = creeperData.Target.transform.position - Entity.transform.position;
                chaseDir.y = 0;
                InputState.Move.Update(true, chaseDir.normalized);

                continue;
            }

            // ------------------------- PATROL -------------------------
            isChasing = false;

            var curr = patrolData.patrolTrack[nextPatrolPoint];

            Vector3 dirToPoint = curr.position - Entity.transform.position;
            dirToPoint.y = 0;

            InputState.Move.Update(true, dirToPoint.normalized);

            if (dirToPoint.sqrMagnitude < 1f)
            {
                nextPatrolPoint = (nextPatrolPoint + 1) % patrolData.patrolTrack.Length;
            }
        }
    }
}
