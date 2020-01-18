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

        protected Animator animator;

        protected Coroutine chaseAndAttack;
        public bool IsAttacking()
        {
            if (!target) return false;
            return chaseAndAttack != null;
        }

        protected float timeSinceAttack;

        public Unit CurrentTarget
        {
            protected set
            {
                target = value;
            }
            get => target;
        }


        public void StartAttacking(Unit _target)
        {
            if (!_target)
            {
                Stop();
                return;
            }
            if (chaseAndAttack != null)
            {
                StopCoroutine(chaseAndAttack);
            }
            target = _target;
            owner.CanMove = false;
            chaseAndAttack = StartCoroutine(ChaseAndAttack());
        }

        protected IEnumerator ChaseAndAttack()
        {
            while (true)
            {
                if (!target) break;
                if (!owner.canAttack) yield return null;
                if (Vector3.Distance(owner.transform.position, target.transform.position) > owner.AtkRange)
                {
                    owner.CanMove = true;
                    owner.MoveTo(target.transform.position);
                    yield return null;
                }
                else owner.CanMove = false;
                if (timeSinceAttack >= 1 / owner.AtkSpeed)
                {
                    Attack(target);
                    owner.CanMove = false;
                    timeSinceAttack = 0;
                }
                yield return null;
            }
        }

        public void Stop()
        {
            if (chaseAndAttack != null)
            {
                StopCoroutine(chaseAndAttack);
                chaseAndAttack = null;
            }
            target = null;
            owner.CanMove = true;
        }

        public abstract void Attack(Unit target);

        public virtual void Initialize(Unit _owner)
        {
            owner = _owner;
            timeSinceAttack = 1 / owner.AtkSpeed;
            animator = GetComponent<Animator>();
        }

        protected void AttackAnimFinished()
        {
        }

        private void Update()
        {
            timeSinceAttack += Time.deltaTime;
        }
    }

}