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

        protected float timeSinceAttack;

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
            while (true)
            {
                if (!target) break;
                if (!owner.canAttack) yield return null;
                if (Vector3.Distance(owner.transform.position, target.transform.position) > owner.AtkRange)
                {
                    owner.MoveTo(target.transform.position);
                    yield return null;
                }
                if (timeSinceAttack >= 1 / owner.AtkSpeed)
                {
                    Attack(target);
                    timeSinceAttack = 0;
                }
                yield return null;
            }
        }

        public void StopAttacking()
        {
            if (attacking != null)
            {
                StopCoroutine(attacking);
            }
            target = null;
        }

        public abstract void Attack(Unit target);

        public virtual void Initialize(Unit _owner)
        {
            owner = _owner;
            timeSinceAttack = 100;
        }

        private void Update()
        {
            timeSinceAttack += Time.deltaTime;
        }
    }

}