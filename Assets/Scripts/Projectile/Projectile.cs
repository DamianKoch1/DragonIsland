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
        HitModeCount = 7

    }

    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        protected bool destroyOnNonTargetHit = false;

        [SerializeField]
        protected bool canHitStructures = false;

        [SerializeField]
        protected Movement movement;

        [SerializeField]
        protected bool isHoming;

        [SerializeField]
        protected float speed;

        [SerializeField]
        protected HitMode hitMode;

        protected Unit target;

        protected Vector3 targetPos;

        protected Unit owner;

        protected float damage;

        protected DamageType dmgType;

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;

            if (unit is Structure)
            {
                if (!canHitStructures) return;
            }

            switch (hitMode)
            {
                case HitMode.invalid:
                    print("encountered invalid projectile hit mode!");
                    break;
                case HitMode.targetOnly:
                    if (unit == target)
                    {
                        OnHit(unit);
                    }
                    break;
                case HitMode.enemyChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (owner.IsEnemy(unit))
                        {
                            OnHit(unit);
                        }
                    }
                    break;
                case HitMode.enemyUnits:
                    if (owner.IsEnemy(unit))
                    {
                        OnHit(unit);
                    }
                    break;
                case HitMode.alliedChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (owner.IsAlly(unit))
                        {
                            OnHit(unit);
                        }
                    }
                    break;
                case HitMode.alliedUnits:
                    if (owner.IsAlly(unit))
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
                case HitMode.HitModeCount:
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnHit(Unit unit)
        {
            var dmg = new Damage(damage, dmgType, owner, unit);
            dmg.Inflict();

            if (unit == target)
            {
                OnHitTarget();
            }
            else if (destroyOnNonTargetHit)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnHitMonster(Monster monster)
        {
            var dmg = new Damage(damage, dmgType, owner, monster);
            dmg.Inflict();

            if (monster == target)
            {
                OnHitTarget();
            }
            else if (destroyOnNonTargetHit)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnReachedDestination()
        {
            Destroy(gameObject);
        }

        protected virtual void OnHitTarget()
        {
            Destroy(gameObject);
        }

        protected Projectile Spawn(Vector3 position, Unit _owner, float _damage, float _speed, HitMode _hitMode, DamageType _dmgType, bool _destroyOnNonTargetHit = false, bool _canHitStructures = false)
        {
            Projectile instance = Instantiate(gameObject, position, Quaternion.identity).GetComponent<Projectile>();
            instance.owner = _owner;
            instance.damage = _damage;
            instance.speed = _speed;
            instance.hitMode = _hitMode;
            instance.dmgType = _dmgType;
            instance.destroyOnNonTargetHit = _destroyOnNonTargetHit;
            instance.canHitStructures = _canHitStructures;
            instance.movement.Initialize(_speed);
            return instance;
        }

        public void SpawnHoming(Unit _target, Vector3 position, Unit _owner, float _damage, float _speed, HitMode _hitMode, DamageType _dmgType, bool _destroyOnNonTargetHit = false, bool _canHitStructures = false)
        {
            Projectile instance = Spawn(position, _owner, _damage, _speed, _hitMode, _dmgType, _destroyOnNonTargetHit, _canHitStructures);
            instance.target = _target;
            instance.isHoming = true;
        }

        /// <summary>
        /// Doesn't work with hitMode targetOnly.
        /// </summary>
        /// <param name="_targetPos"></param>
        /// <param name="position"></param>
        /// <param name="_owner"></param>
        /// <param name="_damage"></param>
        /// <param name="_speed"></param>
        /// <param name="_hitMode"></param>
        /// <param name="_dmgType"></param>
        /// <param name="_destroyOnNonTargetHit"></param>
        /// <param name="_canHitStructures"></param>
        public void SpawnSkillshot(Vector3 _targetPos, Vector3 position, Unit _owner, float _damage, float _speed, HitMode _hitMode, DamageType _dmgType, bool _destroyOnNonTargetHit = false, bool _canHitStructures = false)
        {
            Projectile instance = Spawn(position, _owner, _damage, _speed, _hitMode, _dmgType, _destroyOnNonTargetHit, _canHitStructures);
            instance.targetPos = _targetPos;
            instance.isHoming = false;
        }

        public void SpawnSkillshot(Unit _target, Vector3 position, Unit _owner, float _damage, float _speed, HitMode _hitMode, DamageType _dmgType, bool _destroyOnNonTargetHit = false, bool _canHitStructures = false)
        {
            Projectile instance = Spawn(position, _owner, _damage, _speed, _hitMode, _dmgType, _destroyOnNonTargetHit, _canHitStructures);
            instance.targetPos = _target.transform.position;
            instance.target = _target;
            instance.isHoming = false;
        }

        protected virtual void Update()
        {
            if (isHoming)
            {
                if (!target)
                {
                    Destroy(gameObject);
                    return;
                }
                movement.MoveTo(target.transform.position);
            }
            else
            {
                movement.MoveTo(targetPos);
                if (Vector3.Distance(transform.position, targetPos) <= 0.1f)
                {
                    OnReachedDestination();
                }
            }
        }


    }
}
