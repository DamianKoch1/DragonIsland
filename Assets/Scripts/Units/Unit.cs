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

    /// <summary>
    /// Base class for all units
    /// </summary>
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

        /// <summary>
        /// Setting this enables / disables movement
        /// </summary>
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

        /// <summary>
        /// Setting this calls OnBecome(Un)Targetable, setting to false hides outlines if this was hovered
        /// </summary>
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

        [SerializeField, Tooltip("Parameters: Speed (float, 0-1), Triggers: Death, Respawn, Win, Atk (if only one atk animation, else: Atk1, Atk2, ...)")]
        private Animator animator;

        public Animator Animator => animator;

        public bool IsDead { protected set; get; }


        /// <summary>
        /// Calculates xp needed for next level, scales with level (default: (level-1)^2 * 100))
        /// </summary>
        /// <param name="level">level to calculate needed xp for</param>
        /// <returns></returns>
        public virtual float GetXPNeededForLevel(int level)
        {
            return (level - 1) * (level - 1) * 100;
        }

        /// <summary>
        /// Levels up stats
        /// </summary>
        public void LevelUp()
        {
            stats.LevelUp();
        }


        //TODO WIP
        public void UpdateStats()
        {
            //items + buffs + base + perLvl * (lvl-1)
        }

        /// <summary>
        /// Returns movement component destination
        /// </summary>
        /// <returns></returns>
        public Vector3 GetDestination()
        {
            if (!movement) return Vector3.zero;
            return movement.TargetPos;
        }

        /// <summary>
        /// If can attack and target is valid and enemy, attack it
        /// </summary>
        /// <param name="target"></param>
        public void StartAttacking(Unit target)
        {
            if (!canAttack) return;
            if (!attacking) return;
            if (!target) return;
            if (!this.IsEnemy(target)) return;
            attacking.StartAttacking(target);
        }


        /// <summary>
        /// Returns attacking components attacking status
        /// </summary>
        /// <returns></returns>
        public bool IsAttacking()
        {
            if (!attacking) return false;
            return attacking.IsAttacking();
        }

        /// <summary>
        /// Stops attacking
        /// </summary>
        public void StopAttacking()
        {
            attacking.Stop();
        }


        public Unit CurrentAttackTarget => attacking.target;

        /// <summary>
        /// Adds buff component to buffs slot
        /// </summary>
        /// <typeparam name="T">type of buff to add</typeparam>
        /// <returns></returns>
        public T AddBuff<T>() where T : Buff
        {
            return BuffsSlot.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Try to add custom buff to buffs slot
        /// </summary>
        /// <param name="buffType">type of buff to add, must derive from CustomBuff</param>
        /// <returns></returns>
        public CustomBuff AddCustomBuff(Type buffType)
        {
            if (!buffType.IsSubclassOf(typeof(CustomBuff))) return null;
            return (CustomBuff)BuffsSlot.gameObject.AddComponent(buffType);
        }




        /// <summary>
        /// Avoid calling this directly, create a new Damage() and use Inflict() on it. Reduces hp by amount, calls OnReceiveDamage and OnDealDamage (for instigator), if hp are empty now calls Die
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

        /// <summary>
        /// If controlled by local client, distributes xp reward to nearby enemy champs, grants gold reward to killer champ, calls OnDeath / OnDeathEvent
        /// </summary>
        /// <param name="killer"></param>
        protected virtual void Die(Unit killer)
        {
            if (!photonView.IsMine)
            {
                IsDead = true;
                OnDeath();
                return;
            };
            var xpEligibleChamps = this.GetUnitsInRange<Champ>(xpRewardRange).FindEnemies(this);
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
            OnDeathEvent?.Invoke();
            IsDead = true;
            OnDeath();
        }




        /// <summary>
        /// Is called when this unit dies
        /// </summary>
        public Action OnDeathEvent;

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

        /// <summary>
        /// Plays death animation and waits for 3 seconds, then destroys this object unless overridden
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DeathAnim()
        {
            Animator.SetTrigger("Death");
            yield return new WaitForSeconds(3);
            Destroy(gameObject);
        }

        public Action<Unit, float, DamageType> OnDealDamage;

        /// <summary>
        /// Initializes
        /// </summary>
        protected virtual void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Sets up bars / stats / components / outline materials
        /// </summary>
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

        /// <summary>
        /// Instantiates stat bars for this unit and saves / initializes them
        /// </summary>
        protected virtual void SetupBars()
        {
            var statBarsGO = Resources.Load<GameObject>("StatBars");
            statBarsInstance = Instantiate(statBarsGO, transform.parent);
            statBarsInstance.GetComponent<UnitStatBars>()?.Initialize(this);
        }

        /// <summary>
        /// For each material on the mesh, create a variant with the outline shader in PlayerController and store it and the original material
        /// </summary>
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


        /// <summary>
        /// Retrieve the outline color for this unit from a scriptable object in PlayerController depending on team relation to local player
        /// </summary>
        /// <returns></returns>
        protected virtual Color GetOutlineColor()
        {
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyOutline;
            }
            return PlayerController.Instance.defaultColors.enemyOutline;
        }

        /// <summary>
        /// Retrieve the hp bar color for this unit from a scriptable object in PlayerController depending on team relation to local player
        /// </summary>
        /// <returns></returns>
        public virtual Color GetHPColor()
        {
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyMinionHP;
            }
            return PlayerController.Instance.defaultColors.enemyMinionHP;
        }

        /// <summary>
        /// Sets this as the hovered unit, shows outlines unless untargetable and enemy of local player
        /// </summary>
        private void OnMouseEnter()
        {
            PlayerController.Instance.hovered = this;
            if (!Targetable)
            {
                if (!this.IsAlly(PlayerController.Player)) return;
            }
            ShowOutlines();
        }

        /// <summary>
        /// Swaps each renderer material to its stored outlined version
        /// </summary>
        protected virtual void ShowOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = outlineMaterials[i];
            }
        }

        /// <summary>
        /// Reverts each rendered material to its stored original one
        /// </summary>
        protected virtual void HideOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = defaultMaterials[i];
            }
        }

        /// <summary>
        /// Hides outlines, if this was the hovered unit, clear hovered unit
        /// </summary>
        private void OnMouseExit()
        {
            if (PlayerController.Instance.hovered == this)
            {
                PlayerController.Instance.hovered = null;
                HideOutlines();
            }
        }


        public Action OnMovementCommand;

        /// <summary>
        /// Moves to destination if possible
        /// </summary>
        /// <param name="destination"></param>
        public void MoveTo(Vector3 destination)
        {
            if (!CanMove) return;
            if (!movement) return;
           
            movement.MoveTo(destination);
        }

        /// <summary>
        /// Stops movement if possible
        /// </summary>
        public void Stop()
        {
            movement?.Stop();
        }

        /// <summary>
        /// If local unit, apply regeneration ticks depending on times
        /// </summary>
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

        /// <summary>
        /// If missing hp or resource, send ApplyRegenerationRPC to everyone
        /// </summary>
        protected virtual void ApplyRegeneration()
        {
            if (IsDead) return;
            if (stats.HP >= stats.MaxHP)
            {
                if (stats.Resource >= stats.MaxResource)
                {
                    return;
                }
            }
            photonView.RPC(nameof(ApplyRegenerationRPC), RpcTarget.All);
        }

        /// <summary>
        /// Applies respective hp / resource regeneration
        /// </summary>
        [PunRPC]
        public void ApplyRegenerationRPC()
        {
            stats.ApplyHPReg();
            stats.ApplyResourceReg();
        }

        /// <summary>
        /// Overrride this to set the amount of xp distributed to nearby enemy champs on death
        /// </summary>
        /// <returns></returns>
        public virtual float GetXPReward()
        {
            return 0;
        }

        /// <summary>
        /// Override this to set the amount of gold a champ receives upon killing this
        /// </summary>
        /// <returns></returns>
        public virtual int GetGoldReward()
        {
            return 0;
        }

        /// <summary>
        /// Shows attack range (red), radius (yellow)
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.AtkRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        /// <summary>
        /// Sends a NetworkTeleport rpc to targetPos for everyone, tries to resume movement afterwards
        /// </summary>
        /// <param name="targetPos"></param>
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
                //prevent running back to source if not previously moving
                continueMove = Vector3.Distance(this.GetGroundPos(), destination) > 1f;
            }

            movement.DisableCollision();
            photonView.RPC(nameof(NetworkTeleport), RpcTarget.All, targetPos);
            movement.EnableCollision();

            if (continueMove)
            {
                MoveTo(destination);
            }
            else
            {
                Stop();
            }

        }

        /// <summary>
        /// Instantly moves to target position, overwrites network position to prevent unwanted interpolation
        /// </summary>
        /// <param name="targetPos"></param>
        [PunRPC]
        public void NetworkTeleport(Vector3 targetPos)
        {
            transform.position = targetPos;
            GetComponent<PhotonTransformView>().SetNetworkPosition(targetPos);
        }

        /// <summary>
        /// Match rangeIndicator range with attack range
        /// </summary>
        protected virtual void OnValidate()
        {
            GetComponentInChildren<RangeIndicator>()?.SetRange(stats.AtkRange);
        }
    }
}