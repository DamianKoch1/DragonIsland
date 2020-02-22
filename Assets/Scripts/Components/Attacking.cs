using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [DisallowMultipleComponent]
    public abstract class Attacking : MonoBehaviour
    {

        public Unit target
        {
            get;
            protected set;
        }

        /// <summary>
        /// The view id of the unit the current attack will hit, doesnt change with target after attack anim started
        /// </summary>
        protected int currTargetViewID;

        protected Unit owner;

        protected Animator animator;

        protected Coroutine chaseAndAttack;
        public bool IsAttacking()
        {
            if (!target) return false;
            if (target.IsDead) return false;
            return chaseAndAttack != null;
        }

        protected float timeSinceAttack;


        //TODO to be used for autoattackMove, automatically attacks entering units
        [SerializeField]
        private SphereCollider atkTrigger;

        public SphereCollider AtkTrigger => atkTrigger;


        [SerializeField]
        protected bool lookAtTarget = true;


        protected PhotonView photonView;

        [SerializeField]
        protected Scalings attackScaling = new Scalings() { ad = 1 };

        [SerializeField]
        private int atkAnimCount = 1;

        public void StartAttacking(Unit _target)
        {
            if (target == _target) return;
            if (!owner.canAttack) return;
            if (!_target)
            {
                Stop();
                return;
            }
            if (_target.IsDead)
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
                if (!target)
                {
                    Stop();
                    break;
                }
                if (target.IsDead)
                {
                    Stop();
                    break;
                }
                if (owner.IsDead)
                {
                    Stop();
                    break;
                }
                if (lookAtTarget)
                {
                    transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
                }
                if (!owner.canAttack)
                {
                    yield return null;
                    continue;
                }
                if (Vector3.Distance(owner.GetGroundPos(), target.GetGroundPos()) > owner.Stats.AtkRange + target.Radius)
                {
                    owner.CanMove = true;
                    owner.MoveTo(target.GetGroundPos());
                    yield return null;
                    continue;
                }
                else owner.CanMove = false;
                if (timeSinceAttack >= 1 / owner.Stats.AtkSpeed)
                {
                    photonView.RPC(nameof(Attack), RpcTarget.All, target.GetViewID());
                    owner.CanMove = false;
                    timeSinceAttack = 0;
                }
                yield return null;
                continue;
            }
        }

        public void Stop()
        {
            if (chaseAndAttack != null)
            {
                StopCoroutine(chaseAndAttack);
                chaseAndAttack = null;
                target = null;
                owner.CanMove = true;
                owner.Stop();
            }
        }

        [PunRPC]
        protected void Attack(int targetViewID)
        {
            currTargetViewID = targetViewID;
            if (!owner.Animator)
            {
                OnAtkAnimNotify();
                return;
            }
            if (atkAnimCount == 1)
            {
                owner.Animator.SetTrigger("Atk");
            }
            else
            {
                owner.Animator.SetTrigger("Atk" + Random.Range(1, atkAnimCount+1));
            }
        }

        public abstract void OnAtkAnimNotify();


        public virtual void Initialize(Unit _owner)
        {
            owner = _owner;
            photonView = owner.GetComponent<PhotonView>();
            timeSinceAttack = 1 / owner.Stats.AtkSpeed;
            animator = GetComponent<Animator>();
            owner.OnMovementCommand += Stop;
        }

        private void Update()
        {
            timeSinceAttack += Time.deltaTime;
        }
    }

}