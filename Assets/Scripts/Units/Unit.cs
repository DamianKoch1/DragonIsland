using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GameLogger = MOBA.Logging.GameLogger;

namespace MOBA
{
    public enum TeamID : short
    {
        invalid = -1,
        blue = 0,
        red = 1,
        neutral = 2,
        passive = 3,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(PhotonTransformView))]
    public class Unit : MonoBehaviour
    {
        public const float TICKINTERVAL = 0.5f;

        [SerializeField]
        protected TeamID teamID = TeamID.invalid;

        public TeamID TeamID => teamID;


        [SerializeField]
        protected UnitStats stats;

        public UnitStats Stats => stats;


        [HideInInspector]
        public Amplifiers amplifiers;

        [HideInInspector]
        public BuffFlags statusEffects;

        protected PhotonView photonView;

        [Space]
        private bool canMove = true;

        public bool CanMove
        {
            set
            {
                if (!movement) return;
                if (canMove != value)
                {
                    if (value)
                    {
                        movement.Enable();
                    }
                    else
                    {
                        movement.Disable();
                    }
                }
                canMove = value;
            }
            get => canMove;
        }

        [SerializeField]
        private bool targetable = true;

        public bool Targetable
        {
            set
            {
                if (targetable != value)
                {
                    if (value)
                    {
                        if (PlayerController.Instance.hovered == this)
                        {
                            ShowOutlines();
                        }
                        OnBecomeTargetable?.Invoke();
                    }
                    else
                    {
                        if (PlayerController.Instance.hovered == this)
                        {
                            HideOutlines();
                        }
                        OnBecomeUntargetable?.Invoke();
                    }
                }
                targetable = value;
            }
            get => targetable;
        }

        public Action OnBecomeUntargetable;
        public Action OnBecomeTargetable;

        public bool damageable = true;

        [HideInInspector]
        public bool canAttack = true;

        [HideInInspector]
        public bool canCast = true;


        [Space]
        [SerializeField, Tooltip("Adjust until yellow gizmo sphere horizontally encapsulates the unit, increases the distance from which this is attackable.")]
        private float radius;

        public float Radius => radius;

        protected float timeSinceLastRegTick = 0;



        [SerializeField]
        private float xpRewardRange = 12;

        [Space]
        [SerializeField]
        protected Movement movement;

        [SerializeField]
        protected Attacking attacking;


        protected GameObject statBarsInstance;


        private BuffsSlot buffsSlot;

        public BuffsSlot BuffsSlot
        {
            private set
            {
                buffsSlot = value;
            }
            get
            {
                if (!buffsSlot)
                {
                    buffsSlot = GetComponentInChildren<BuffsSlot>();
                }
                return buffsSlot;
            }
        }


        [SerializeField]
        protected GameObject mesh;

        private List<Material> defaultMaterials;
        private List<Material> outlineMaterials;
        private List<Renderer> renderers;

        [SerializeField, Range(0, 2)]
        private float outlineWidth = 0.1f;

        [SerializeField]
        private Animator animator;

        public Animator Animator => animator;

        public bool IsDead { protected set; get; }


        public virtual float GetXPNeededForLevel(int level)
        {
            return (level - 1) * (level - 1) * 100;
        }

        public void LevelUp()
        {
            stats.LevelUp();
        }


        //TODO
        public void UpdateStats()
        {
            //items + buffs + base + perLvl * (lvl-1)
        }

        public Vector3 GetDestination()
        {
            if (!movement) return Vector3.zero;
            return movement.TargetPos;
        }

        public void StartAttacking(Unit target)
        {
            if (!canAttack) return;
            if (!attacking) return;
            if (!target) return;
            if (!this.IsEnemy(target)) return;
            attacking.StartAttacking(target);
        }



        public bool IsAttacking()
        {
            if (!attacking) return false;
            return attacking.IsAttacking();
        }

        public void StopAttacking()
        {
            attacking.Stop();
        }


        public Unit CurrentAttackTarget => attacking.target;

        public T AddBuff<T>() where T : Buff
        {
            return BuffsSlot.gameObject.AddComponent<T>();
        }

        public CustomBuff AddCustomBuff(Type buffType)
        {
            if (!buffType.IsSubclassOf(typeof(CustomBuff))) return null;
            return (CustomBuff)BuffsSlot.gameObject.AddComponent(buffType);
        }




        /// <summary>
        /// Avoid calling this directly, create a new Damage() and use Inflict() on it.
        /// </summary>
        /// <param name="instigator"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        [PunRPC]
        public virtual void ReceiveDamage(int instigatorViewID, int amount, short damageType)
        {
            if (!damageable) return;
            var instigator = instigatorViewID.GetUnitByID();
            var type = (DamageType)damageType;
            OnReceiveDamage?.Invoke(instigator, amount, type);
            stats.HP -= amount;
            if (instigator)
            {
                instigator.OnDealDamage?.Invoke(this, amount, type);
                if (instigator is Champ)
                {
                    OnAttackedByChamp?.Invoke((Champ)instigator);
                }
            }
            if (stats.HP == 0)
            {
                Die(instigator);
            }
        }

        public Action<Unit, float, DamageType> OnReceiveDamage;

        public Action<Champ> OnAttackedByChamp;

        protected virtual void Die(Unit killer)
        {
            if (!photonView.IsMine)
            {
                IsDead = true;
                OnDeath();
                return;
            };
            var xpEligibleChamps = this.GetUnitsInRange<Champ>(xpRewardRange).FindAllies(this);
            if (killer is Champ)
            {
                var champ = (Champ)killer;
                champ.Gold += GetGoldReward();
                if (!xpEligibleChamps.Contains(champ))
                {
                    xpEligibleChamps.Add(champ);
                }
                GameLogger.Log(killer, Logging.LogActionType.kill, transform.position, this);
            }
            foreach (Champ champ in xpEligibleChamps)
            {
                champ.photonView.RPC(nameof(champ.ReceiveXP), RpcTarget.All, GetXPReward() / xpEligibleChamps.Count());
            }
            OnBeforeDeath?.Invoke();
            IsDead = true;
            OnDeath();
        }




        /// <summary>
        /// Is called just before OnDeath(), which destroys this game object by default.
        /// </summary>
        public Action OnBeforeDeath;

        /// <summary>
        /// Destroys this gameObject unless overridden.
        /// </summary>
        protected virtual void OnDeath()
        {
            if (Animator)
            {
                StartCoroutine(DeathAnim());
                return;
            }
            Destroy(gameObject);
        }

        protected virtual IEnumerator DeathAnim()
        {
            Animator.SetTrigger("Death");
            yield return new WaitForSeconds(3);
            Destroy(gameObject);
        }

        public Action<Unit, float, DamageType> OnDealDamage;


        protected virtual void Start()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            SetupBars();

            stats.Initialize(this);
            IsDead = false;

            amplifiers = new Amplifiers();
            amplifiers.Reset();

            photonView = PhotonView.Get(this);
            movement?.Initialize(stats.MoveSpeed, this);
            attacking?.Initialize(this);

            if (photonView.IsMine)
            {
                statusEffects = new BuffFlags();

                timeSinceLastRegTick = 0;


                if (movement)
                {
                    stats.OnMoveSpeedChanged += movement.SetSpeed;
                }

                OnUnitTick += ApplyRegeneration;
            }
            else
            {
                movement?.Disable();
            }

            SetupMaterials();
        }

        protected virtual void SetupBars()
        {
            var statBarsGO = Resources.Load<GameObject>("StatBars");
            statBarsInstance = Instantiate(statBarsGO, transform.parent);
            statBarsInstance.GetComponent<UnitStatBars>()?.Initialize(this);
        }

        protected void SetupMaterials()
        {
            defaultMaterials = new List<Material>();
            outlineMaterials = new List<Material>();
            renderers = new List<Renderer>();
            var outlineColor = GetOutlineColor();

            foreach (var renderer in mesh.GetComponentsInChildren<Renderer>())
            {
                renderers.Add(renderer);
                defaultMaterials.Add(new Material(renderer.material));

                var outlineMaterial = new Material(renderer.material);
                outlineMaterial.shader = PlayerController.Instance.outline;
                outlineMaterial.SetColor("_OutlineColor", GetOutlineColor());
                outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
                outlineMaterials.Add(outlineMaterial);
            }
        }



        protected virtual Color GetOutlineColor()
        {
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyOutline;
            }
            return PlayerController.Instance.defaultColors.enemyOutline;
        }

        public virtual Color GetHPColor()
        {
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyMinionHP;
            }
            return PlayerController.Instance.defaultColors.enemyMinionHP;
        }

        private void OnMouseEnter()
        {
            PlayerController.Instance.hovered = this;
            if (!Targetable)
            {
                if (!this.IsAlly(PlayerController.Player)) return;
            }
            ShowOutlines();
        }

        protected virtual void ShowOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = outlineMaterials[i];
            }
        }

        protected virtual void HideOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = defaultMaterials[i];
            }
        }


        private void OnMouseExit()
        {
            if (PlayerController.Instance.hovered == this)
            {
                PlayerController.Instance.hovered = null;
                HideOutlines();
            }
        }


        public Action OnMovementCommand;

        public void MoveTo(Vector3 destination)
        {
            if (!CanMove) return;
            if (!movement) return;
            if (IsAttacking())
            {
                StopAttacking();
            }
            movement.MoveTo(destination);
        }

        public void Stop()
        {
            movement?.Stop();
        }


        protected virtual void Update()
        {
            if (!photonView.IsMine) return;

            while (timeSinceLastRegTick >= TICKINTERVAL)
            {
                OnUnitTick?.Invoke();
                timeSinceLastRegTick -= TICKINTERVAL;
            }
            timeSinceLastRegTick += Time.deltaTime;
        }

        public Action OnUnitTick;

        protected virtual void ApplyRegeneration()
        {
            if (IsDead) return;
            photonView.RPC(nameof(ApplyRegenerationRPC), RpcTarget.All);
        }

        [PunRPC]
        public void ApplyRegenerationRPC()
        {
            stats.ApplyHPReg();
            stats.ApplyResourceReg();
        }

        public virtual float GetXPReward()
        {
            return 0;
        }

        public virtual int GetGoldReward()
        {
            return 0;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.AtkRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public void Teleport(Vector3 targetPos)
        {
            var destination = GetDestination();
            bool continueMove;
            if (destination == Vector3.zero)
            {
                continueMove = false;
            }
            else
            {
                continueMove = Vector3.Distance(this.GetGroundPos(), destination) > 1f;
            }

            movement.DisableCollision();
            photonView.RPC(nameof(NetworkTeleport), RpcTarget.All, targetPos);
            movement.EnableCollision();

            //prevent running back to source if not previously moving
            if (continueMove)
            {
                MoveTo(destination);
            }
            else
            {
                Stop();
            }

        }

        [PunRPC]
        public void NetworkTeleport(Vector3 targetPos)
        {
            transform.position = targetPos;
            GetComponent<PhotonTransformView>().SetNetworkPosition(targetPos);
        }

        protected virtual void OnValidate()
        {
            GetComponentInChildren<RangeIndicator>()?.SetRange(stats.AtkRange);
        }
    }
}