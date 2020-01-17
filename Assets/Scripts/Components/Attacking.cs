using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public abstract class Attacking : MonoBehaviour
    {
        [HideInInspector]
        public Unit target;

        protected Unit owner;

        protected Coroutine attacking;

        public void StartAttacking(Unit target)
        {
            if (attacking != null)
            {
                StopCoroutine(attacking);
            }
            attacking = StartCoroutine(Attack());
        }

        protected IEnumerator Attack()
        {
            float time = 0;
            while (true)
            {
                if (!target) break;
                if (!owner.canAttack) continue;
                if (Vector3.Distance(owner.transform.position, target.transform.position) > owner.AtkRange)
                {
                    owner.MoveTo(target.transform.position);
                    continue;
                }
                if (time >= 1 / owner.AtkSpeed)
                {
                    Attack(target);
                }
                time += Time.deltaTime;
                yield return null;
            }
        }

        public abstract void Attack(Unit target);

        public virtual void Initialize(Unit _owner)
        {
            owner = _owner;
        }
    }

}