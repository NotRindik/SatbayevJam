using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Systems
{
    public class HealthSystem: BaseSystem
    {
        private HealthComponent _healthComponent;

        public void TakeHit(HitInfo who)
        {
            _healthComponent.currHealth -= who.dmg;
            _healthComponent.OnTakeHit?.Invoke(who);
            if (_healthComponent.currHealth <= 0)
            {
                _healthComponent.OnDie?.Invoke(who);
            }
        }

        public void Heal(float heal)
        {
            _healthComponent.currHealth += heal;
            _healthComponent.OnHeal?.Invoke(heal);
        }

        public override void Initialize(Entity owner)
        {
            base.Initialize(owner);
            _healthComponent = base.owner.GetControllerComponent<HealthComponent>();
            _healthComponent.currHealth = _healthComponent.maxHealth;
        }
    }

    public struct HitInfo
    {
        private Nullable<Vector2> hitPosition;
        public Entity Attacker;
        public float dmg;

        public HitInfo(Vector2 pos)
        {
            hitPosition = pos;
            Attacker = null;
            this.dmg = 0;
        }
        public HitInfo(float dmg)
        {
            hitPosition = null;
            Attacker = null;
            this.dmg = dmg;
        }

        public HitInfo(Entity attacker)
        {
            Attacker = attacker;
            hitPosition = null;
            this.dmg = 0;
        }
        public HitInfo(Entity attacker,float dmg)
        {
            Attacker = attacker;
            hitPosition = null;
            this.dmg = dmg;
        }
        public HitInfo(Vector2 pos, float dmg)
        {
            hitPosition = pos;
            Attacker = null;
            this.dmg = dmg;
        }

        public HitInfo(Entity attacker, Vector2 pos)
        {
            Attacker = attacker;
            hitPosition = pos;
            this.dmg = 0;
        }

        public Vector2 GetHitPos()
        {
            if (hitPosition.HasValue)
                return hitPosition.Value;

            if (Attacker != null)
                return Attacker.transform.position;

            return Vector2.zero;
        }

    }


    [System.Serializable]
    public class HealthComponent : IComponent
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _currHealth;

        public float maxHealth
        {
            get => _maxHealth;
            set
            {
                _maxHealth = value;
                OnMaxHealthDataChanged?.Invoke(_maxHealth);
            }
        }

        public float currHealth
        {
            get => _currHealth;
            set
            {
                _currHealth = value;
                OnCurrHealthDataChanged?.Invoke(_currHealth);
            }
        }
        public Action<float> OnCurrHealthDataChanged;
        public Action<float> OnMaxHealthDataChanged;
        public Action<HitInfo> OnTakeHit;
        public Action<HitInfo> OnDie;
        public Action<float> OnHeal;
    }

    public struct Damage : IDamager
    {
        private DamageComponent _damageComponent;
        public Damage(DamageComponent damageComponent)
        {
            _damageComponent = damageComponent;
        }
        public void ApplyDamage(HealthSystem hp, HitInfo who)
        {

            bool isCrit = UnityEngine.Random.value < _damageComponent.CritChance;
            float damage = isCrit ? _damageComponent.BaseDamage * _damageComponent.CritMultiplier
                                  : _damageComponent.BaseDamage;

            float finalDamage = Mathf.Max(1, damage);
            who.dmg = finalDamage;
            hp.TakeHit(who);
        }

        public float GetDamage()
        {
            return _damageComponent.BaseDamage;
        }
    }

    public interface IDamager
    {
        float GetDamage();

        void ApplyDamage(HealthSystem hp,HitInfo who);
    }


    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct DamageComponent : IComponent
    {
        public float BaseDamage;
        public float CritChance;
        public float CritMultiplier;
        public float Penetration;

        public DamageComponent(float baseDamage, float critChance, float critMultiplier, float penetration)
        {
            BaseDamage = baseDamage;
            CritChance = critChance;
            CritMultiplier = critMultiplier;
            Penetration = penetration;
        }
        public static DamageComponent operator+(DamageComponent damage1, DamageComponent damage2)
        {
            return new DamageComponent(damage1.BaseDamage + damage2.BaseDamage,damage1.CritChance + damage2.CritChance,
                damage1.CritMultiplier + damage2.CritMultiplier,
                damage1.Penetration + damage2.Penetration);
        }

        public static DamageComponent operator *(DamageComponent damage1, DamageComponent damage2)
        {
            return new DamageComponent(damage1.BaseDamage * damage2.BaseDamage, damage1.CritChance * damage2.CritChance,
                damage1.CritMultiplier * damage2.CritMultiplier,
                damage1.Penetration * damage2.Penetration);
        }

    }
}