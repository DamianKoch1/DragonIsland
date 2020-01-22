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

        protected List<Tower> nearbyAlliedTowers;


        private Vector3 spawnpoint;



        //TODO make global
        [SerializeField]
        private float goldPerSec = 1;

        public List<Skill> Skills { get; private set; }

        //move to interface, shouldnt be in every champ
        [Space]
        [SerializeField]
        private RespawnHUD respawnHUDPrefab;

        public void StartAttacking(Unit target)
        {
            if (!canAttack) return;
            if (!attacking) return;
            if (!IsEnemy(target)) return;
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

        protected override void Initialize()
        {
            base.Initialize();

            OnAttackedByChamp += RequestTowerAssist;

            nearbyAlliedTowers = new List<Tower>();

            ToggleRangeIndicator(false);

            spawnpoint = Base.GetAllied(this).Spawnpoint.position;

            Gold = 0;

            OnUnitTick += () => Gold += goldPerSec;

            SetupSkills();
        }

        private void SetupSkills()
        {
            Skills = new List<Skill>(GetComponentsInChildren<Skill>());
            foreach (var skill in Skills)
            {
                skill.Initialize(this);
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


        private float GetRespawnTime()
        {
            return 7 + 3 * stats.Lvl;
        }

        //add to inhib, make IRespawning
        private IEnumerator Respawn()
        {
            float remainingTime = GetRespawnTime();

            //move to interface, shouldnt be called by every champ
            RespawnHUD respawnHUD = null;
            if (this == PlayerController.Player)
            {
                respawnHUD = Instantiate(respawnHUDPrefab.gameObject).GetComponent<RespawnHUD>();
                respawnHUD.Initialize(remainingTime);
            }
            //

            while (remainingTime > 0)
            {
                //
                respawnHUD?.SetRemainingTime(remainingTime);
                //

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            //
            if (respawnHUD)
            {
                Destroy(respawnHUD.gameObject);
            }
            //

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
            foreach (var tower in nearbyAlliedTowers)
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
            if (IsAlly(PlayerController.Player))
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
