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

    //TODO onhit effects
    [Serializable]
    public class ProjectileProperties
    {
        public bool isHoming;

        public float speed;

        public float size;

        public HitMode hitMode;

        public DamageType dmgType;

        [Tooltip("Changing this within AttackingRanged has no effect.")]
        public float baseDamage;

        [Range(-1, 60)]
        public float lifespan = -1;

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

        protected Vector3 targetDir;

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
                var dmg = new Damage(properties.baseDamage /*TODO add scaling here*/, properties.dmgType, owner, unit);
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

        private void OnHitMonster(Monster monster)
        {
            var dmg = new Damage(properties.baseDamage /*TODO add scaling here*/, properties.dmgType, owner, monster);
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


        private void OnHitTarget()
        {
            Destroy(gameObject);
        }

        private void Initialize(Unit _owner, Vector3 position, ProjectileProperties _properties)
        {
            properties = _properties;
            movement.SetSpeed(properties.speed);
            transform.localScale *= properties.size;
            owner = _owner;
            ownerTeamID = owner.TeamID;
            onHitEffects = new List<SkillEffect>(GetComponents<SkillEffect>());
            foreach (var effect in onHitEffects)
            {
                effect.Initialize(owner, 0);
            }
        }

        /// <summary>
        /// Spawns a stillstanding but initialized projectile.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        private Projectile Spawn(Unit _owner, Vector3 position, ProjectileProperties _properties, Scalings _scalings)
        {
            Projectile instance = Instantiate(gameObject, position, Quaternion.identity).GetComponent<Projectile>();
            instance.Initialize(_owner, position, _properties);
            return instance;
        }

        /// <summary>
        /// Spawns a projectile flying after a unit or to its position at spawn time with given properties, _properties determine whether it is homing.
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_target"></param>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        public Projectile Spawn(Unit _owner, Unit _target, Vector3 position, ProjectileProperties _properties, Scalings _scalings)
        {
            Projectile instance = Spawn(_owner, position, _properties, _scalings);
            instance.target = _target;
            instance.targetPos = _owner.transform.position;
            instance.targetDir = (_target.transform.position - position).normalized;
            instance.targetDir.y = 0;
            return instance;
        }

        /// <summary>
        /// Spawns a projectile flying to a position with given properties, cannot be homing.
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_targetPos"></param>
        /// <param name="position"></param>
        /// <param name="_properties"></param>
        /// <returns></returns>
        public Projectile SpawnSkillshot(Unit _owner, Vector3 _targetPos, Vector3 position, ProjectileProperties _properties, Scalings _scalings)
        {
            Projectile instance = Spawn(_owner, position, _properties, _scalings);
            instance.targetPos = _targetPos;
            instance.targetDir = (_targetPos - position).normalized;
            instance.targetDir.y = 0;
            if (_properties.isHoming)
            {
                instance.properties.isHoming = false;
                Debug.LogWarning(owner.name + " tried to spawn a homing projectile given only a target position instead of a target unit!");
            }
            return instance;
        }


        private void Update()
        {
            Move();

            if (properties.lifespan < 0) return;
            properties.lifespan -= Time.deltaTime;
            if (properties.lifespan < 0)
            {
                Destroy(gameObject);
            }
        }

        private void Move()
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
                movement.MoveTo(transform.position + targetDir);
            }
        }

    }
}
