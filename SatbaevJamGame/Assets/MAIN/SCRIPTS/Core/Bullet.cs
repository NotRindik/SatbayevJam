using DG.Tweening.Core.Easing;
using Systems;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public AudioClip shot;

    public BulletComponent bc;
    private void Start()
    {
        AudioManager.instance.PlayAudioClip(shot);

        Destroy(gameObject, bc.lifeTime);

    }
    private void Update()
    {
        transform.position += transform.forward * bc.speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        if (((1 << layer) & bc.DestroyLayer) != 0)
        {
            Destroy(gameObject);
            return;
        }

        if (((1 << layer) & bc.DamageLayer) != 0)
        {
            if(other.gameObject.TryGetComponent<Entity>(out Entity ent))
            {
                var hp = ent.GetControllerSystem<HealthSystem>();

                new Damage(bc.damage).ApplyDamage(hp,new HitInfo(gameObject.transform.position));
                FindAnyObjectByType<PlayerUIManager>().AddTime(15);

            }
            Destroy(gameObject);
        }
    }
}
[System.Serializable]
public struct   BulletComponent : IComponent
{
    public float speed;
    public float lifeTime;
    public Vector3 dir;
    public LayerMask DestroyLayer;
    public LayerMask DamageLayer;
    public DamageComponent damage;
}
