using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{
    [RequireComponent(typeof(NavMovement))]
    public class Champ : Unit
    {
        public const float GOLDPERSEC = 1;

        protected float currGold;

        public float Gold
        {
            set
            {
                if (currGold != value)
                {
                    currGold = value;
                    OnGoldChanged?.Invoke(currGold.Truncate(0));
                }
            }
            get => currGold;
        }

        public Action<float> OnGoldChanged;

        protected UnitList<Tower> nearbyAlliedTowers;


        private Vector3 spawnpoint;

      

        private List<Skill> skills;

        public List<Skill> Skills
        {
            get
            {
                if (skills == null)
                {
                    skills = new List<Skill>(GetComponentsInChildren<Skill>());
                }
                return skills;
            }
            private set
            {
                skills = value;
            }
        }

        //move to interface, shouldnt be in every champ
        [Space]
        [SerializeField]
        private BarTextTimer respawnHUDPrefab;

     


        protected override void Initialize()
        {
            base.Initialize();

            OnAttackedByChamp += RequestTowerAssist;

            nearbyAlliedTowers = new UnitList<Tower>();

            ToggleRangeIndicator(false);

            spawnpoint = Base.GetAllied(this).Spawnpoint.position;

            Gold = 0;


            OnUnitTick += () => Gold += GOLDPERSEC;

            SetupSkills();
        }

        private void SetupSkills()
        {
            foreach (var skill in Skills)
            {
                skill.SetOwner(this);
            }
        }

        public bool CastQ()
        {
            if (Skills.Count == 0) return false;
            return Skills[0].TryCast();
        }

        public bool CastW()
        {
            if (Skills.Count <= 1) return false;
            return Skills[1].TryCast();
        }

        public bool CastE()
        {
            if (Skills.Count <= 2) return false;
            return Skills[2].TryCast();
        }

        public bool CastR()
        {
            if (Skills.Count <= 3) return false;
            return Skills[3].TryCast();
        }


        protected override void OnDeath()
        {
            movement.DisableCollision();
            mesh.SetActive(false);
            attacking?.Stop();
            StartCoroutine(Respawn());
        }


        protected float GetRespawnTime()
        {
            return 5 + 3 * stats.Lvl-1;
        }

        //add to inhib, make IRespawning
        private IEnumerator Respawn()
        {
            float remainingTime = GetRespawnTime();

            BarTextTimer respawnHUD = null;
            if (this == PlayerController.Player)
            {
                respawnHUD = Instantiate(respawnHUDPrefab.gameObject).GetComponent<BarTextTimer>();
                respawnHUD.Initialize(remainingTime);
            }

            while (remainingTime > 0)
            {
                respawnHUD?.SetRemainingTime(remainingTime);

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (respawnHUD)
            {
                Destroy(respawnHUD.gameObject);
            }

            transform.position = spawnpoint;
            movement.EnableCollision();
            mesh.SetActive(true);
            IsDead = false;
            stats.HP = stats.MaxHP;
            stats.Resource = stats.MaxResource;
            OnRespawn?.Invoke();
        }

        public Action OnRespawn;

        protected void RequestTowerAssist(Champ attacker)
        {
            if (nearbyAlliedTowers.Count() == 0) return;
            foreach (Tower tower in nearbyAlliedTowers)
            {
                tower.TryAttack(attacker);
            }
        }

        public void AddNearbyAlliedTower(Tower tower)
        {
            if (!nearbyAlliedTowers.Contains(tower))
            {
                nearbyAlliedTowers.Add(tower);
            }
        }

        public void RemoveNearbyTower(Tower tower)
        {
            if (nearbyAlliedTowers.Contains(tower))
            {
                nearbyAlliedTowers.Remove(tower);
            }
        }

        protected override void ShowOutlines()
        {
            if (PlayerController.Player == this) return;
            base.ShowOutlines();
        }

        protected override void HideOutlines()
        {
            if (PlayerController.Player == this) return;
            base.HideOutlines();
        }

        protected override Color GetOutlineColor()
        {
            if (PlayerController.Player == this)
            {
                return PlayerController.Instance.defaultColors.ownOutline;
            }
            return base.GetOutlineColor();
        }

        public override Color GetHPColor()
        {
            if (PlayerController.Player == this)
            {
                return PlayerController.Instance.defaultColors.ownHP;
            }
            if (this.IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyChampHP;
            }
            return PlayerController.Instance.defaultColors.enemyChampHP;
        }

        public override float GetXPReward()
        {
            return stats.Lvl * 50;
        }

        public override int GetGoldReward()
        {
            return 350;
        }

        protected override void SetupBars()
        {
            statBarsInstance = Instantiate(statBarsPrefab);
            statBarsInstance.GetComponent<ChampStatBars>()?.Initialize(this, 0.5f, 1.2f, true);
        }


        public void ToggleRangeIndicator(bool show)
        {
            if (!attacking?.RangeIndicator) return;
            attacking.RangeIndicator.gameObject.SetActive(show);
        }
    }
}
