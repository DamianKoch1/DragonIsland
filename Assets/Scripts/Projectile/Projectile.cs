using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public enum HitMode
    {
        invalid = -1,
        targetOnly = 0,
        enemyChamps = 1,
        enemyUnits = 2,
        alliedChamps = 3,
        alliedUnits = 4,
        monsters = 5,
        anyUnit = 6,
        ownerOnly = 7
    }

    [Serializable]
    public class ProjectileProperties
    {
        public bool isHoming;

        public float speed;

        public float size;

        public HitMode hitMode;

        public DamageType dmgType;

        public float baseDamage;

        [HideInInspector]
        public float bonusDamage;

        [Space]
        public bool hitUntargetables = false;

        public bool destroyOnNonTargetHit = false;

        public bool canHitStructures = false;
    }

    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        protected ProjectileProperties properties;

        [SerializeField]
        protected Movement movement;

        protected Unit owner;

        /// <summary>
        /// Used to store team id of owner in case owner is destroyed on hit.
        /// </summary>
        protected TeamID ownerTeamID;

        protected Unit target;

        protected Vector3 targetPos;

        protected List<SkillEffect> onHitEffects;

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;

            if (unit is Structure)
            {
                if (!properties.canHitStructures) return;
            }

            switch (properties.hitMode)
            {
                case HitMode.targetOnly:
                    if (unit == target)
                    {
                        OnHit(unit);
                    }
                    break;
                case HitMode.enemyChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (unit.IsEnemy(ownerTeamID))
                        {
                            OnHit(unit);
                        }
                    }
                    break;
                case HitMode.enemyUnits:
                    if (unit.IsEnemy(ownerTeamID))
                    {
                        OnHit(unit);
                    }
                    break;
                case HitMode.alliedChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (unit.IsAlly(ownerTeamID))
                        {
                            OnHit(unit);
                        }
                    }
                    break;
                case HitMode.alliedUnits:
                    if (unit.IsAlly(ownerTeamID))
                    {
                        OnHit(unit);
                    }
                    break;
                case HitMode.monsters:
                    if (unit is Monster)
                    {
                        OnHitMonster((Monster)unit);
                    }
                    break;
                case HitMode.anyUnit:
                    if (unit is Monster)
                    {
                        OnHitMonster((Monster)unit);
                    }
                    else
                    {
                        OnHit(unit);
                    }
                    break;
                default:
                    Debug.LogError(owner.name + "spawned projectile with invalid hit mode!");
                    break;
            }
        }

        protected virtual void OnHit(Unit unit)
        {
            if (properties.hitUntargetables || unit.damageable)
            {
                var dmg = new Damage(properties.baseDamage + properties.bonusDamage, properties.dmgType, owner, unit);
                dmg.Inflict();
            }

            if (unit == target)
            {
                OnHitTarget();
            }
            else if (properties.destroyOnNonTargetHit)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnHitMonster(Monster monster)
        {
            var dmg = new Damage(properties.baseDamage + properties.bonusDamage, properties.dmgType, owner, monster);
            dmg.Inflict();

            if (monster == target)
            {
                OnHitTarget();
            }
            else if (properties.destroyOnNonTargetHit)
            {
                Destroy(gameObject);
            }
        }


        protected virtual void OnHitTarget()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Spawns a stillstanding projectile.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        protected Projectile Spawn(Unit _owner, Vector3 position, ProjectileProperties _properties)
        {
            Projectile instance = Instantiate(gameObject, position, Quaternion.identity).GetComponent<Projectile>();
            instance.properties = _properties;
            instance.owner = _owner;
            instance.ownerTeamID = _owner.TeamID;
            instance.onHitEffects = new List<SkillEffect>(instance.GetComponents<SkillEffect>());
            foreach (var effect in instance.onHitEffects)
            {
                effect.Initialize(instance.owner, 0);
            }
            return instance;
        }

        /// <summary>
        /// Spawns a projectile with given properties, _properties determine whether it is homing.
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_target"></param>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        public Projectile Spawn(Unit _owner, Unit _target, Vector3 position, ProjectileProperties _properties, float _bonusDamage = 0)
        {
            Projectile instance = Spawn(_owner, position, _properties);
            instance.target = _target;
            instance.targetPos = _owner.transform.position;
            instance.properties.bonusDamage = _bonusDamage;
            return instance;
        }

        /// <summary>
        /// Spawns a projectile with given properties, cannot be homing.
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_targetPos"></param>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        public Projectile SpawnSkillshot(Unit _owner, Vector3 _targetPos, Vector3 position, ProjectileProperties _properties, float _bonusDamage = 0)
        {
            Projectile instance = Spawn(_owner, position, _properties);
            instance.targetPos = _targetPos;
            if (_properties.isHoming)
            {
                instance.properties.isHoming = false;
                Debug.LogWarning(owner.name + " tried to spawn a homing projectile given only a target position instead of a unit!");
            }
            instance.properties.bonusDamage = _bonusDamage;
            return instance;
        }


        protected virtual void Update()
        {
            if (properties.isHoming)
            {
                if (!target || target.IsDead)
                {
                    Destroy(gameObject);
                    return;
                }
                if (!properties.hitUntargetables)
                {
                    if (!target.Targetable)
                    {
                        Destroy(gameObject);
                    }
                }
                movement.MoveTo(target.transform.position);
            }
            else
            {
                movement.MoveTo(targetPos);
            }
        }


    }
}
