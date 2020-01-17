using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public abstract class Attacking : MonoBehaviour
    {
        
        public Unit target
        {
            get;
            protected set;
        }

        protected Unit owner;

        protected Coroutine attacking;

        public void StartAttacking(Unit _target)
        {
            if (attacking != null)
            {
                StopCoroutine(attacking);
            }
            target = _target;
            attacking = StartCoroutine(ChaseAndAttack());
        }

        protected IEnumerator ChaseAndAttack()
        {
            float time = 0;
            while (true)
            {
                if (!target) break;
                if (!owner.canAttack) yield return null;
                if (Vector3.Distance(owner.transform.position, target.transform.position) > owner.AtkRange)
                {
                    owner.MoveTo(target.transform.position);
                    time += Time.deltaTime;
                    yield return null;
                }
                if (time >= 1 / owner.AtkSpeed)
                {
                    Attack(target);
                    time = 0;
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