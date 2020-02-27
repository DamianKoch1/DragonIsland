using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public enum UntargetableUntilMode
    {
        invalid = -1,
        allDestroyed = 0,
        anyDestroyed = 1,
    }

    /// <summary>
    /// Base class for buildings
    /// </summary>
    public abstract class Structure : Unit
    {
        [Space]

        [SerializeField, Tooltip("Structures that either all or any of have to be destroyed before this becomes targetable")]
        protected List<Structure> isUntargetableUntilDestroyed;

        [SerializeField, Tooltip("How does this react to an assigned previous structure being destroyed?")]
        private UntargetableUntilMode targetableUntilMode;

        protected bool isDestroyed;

        /// <summary>
        /// Goes untargetable if condition structures aren't destroyed
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            isDestroyed = false;
            if (isUntargetableUntilDestroyed.Count > 0)
            {
                Targetable = false;
                damageable = false;
                foreach (var structure in isUntargetableUntilDestroyed)
                {
                    structure.OnDeathEvent += CheckTargetableRequirements;
                }
            }
            CanMove = false;
        }

        protected override void Die(Unit killer)
        {
            isDestroyed = true;
            base.Die(killer);
        }

        public override Color GetHPColor()
        {
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyStructureHP;
            }
            return PlayerController.Instance.defaultColors.enemyStructureHP;
        }

        /// <summary>
        /// Checks if this should become targetable depending on targetableUntilMode, sends SetTargetable rpc to all if yes
        /// </summary>
        private void CheckTargetableRequirements()
        {
            switch (targetableUntilMode)
            {
                case UntargetableUntilMode.allDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed) return;
                    }
                    photonView.RPC(nameof(SetTargetable), RpcTarget.All);
                    return;
                case UntargetableUntilMode.anyDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed) continue;
                        photonView.RPC(nameof(SetTargetable), RpcTarget.All);
                        return;
                    }
                    return;
                default:
                    Debug.LogWarning("encountered invalid TargetableUntilMode!");
                    return;
            }
        }

        /// <summary>
        /// Sets this to targetable and damageable
        /// </summary>
        [PunRPC]
        public void SetTargetable()
        {
            Targetable = true;
            damageable = true;
        }

        protected override void SetupBars()
        {
            var statBarsGO = Resources.Load<GameObject>("StatBars");
            statBarsInstance = Instantiate(statBarsGO, transform.parent);
            statBarsInstance.GetComponent<UnitStatBars>()?.Initialize(this, 1, 2, true);
        }
    }
}
