using MOBA.Logging;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{
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


        public bool isDummy;

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




        public override void Initialize()
        {
            base.Initialize();

            ToggleRangeIndicator(false);
            SetupSkills();

            if (!photonView.IsMine) return;
            OnAttackedByChamp += RequestTowerAssist;

            nearbyAlliedTowers = new UnitList<Tower>();


            spawnpoint = Base.GetAllied(this).Spawnpoint.position;

            Gold = 0;


            OnUnitTick += () => Gold += GOLDPERSEC;

        }

        private void SetupSkills()
        {
            foreach (var skill in Skills)
            {
                skill.Initialize(this);
            }
        }


        //TODO cast fail feedback
        public bool CastQ(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count == 0) return false;
            return Skills[0].TryCast(hovered, mousePos);
        }

        public bool CastW(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 1) return false;
            return Skills[1].TryCast(hovered, mousePos);
        }

        public bool CastE(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 2) return false;
            return Skills[2].TryCast(hovered, mousePos);
        }

        public bool CastR(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 3) return false;
            return Skills[3].TryCast(hovered, mousePos);
        }

        protected override void Start()
        {
            if (isDummy) Initialize();
        }

        [PunRPC]
        public void SetTeamID(short _teamID)
        {
            teamID = (TeamID)_teamID;
            Initialize();
        }

        [PunRPC]
        public void ReceiveXP(float amount)
        {
            stats.XP += amount;
        }

        protected override void OnDeath()
        {
            movement.DisableCollision();
            if (Animator)
            {
                Animator.SetTrigger("Death");
            }
            else mesh.SetActive(false);
            attacking?.Stop();
            if (photonView.IsMine)
            {
                StartCoroutine(Respawn());
            }
            else
            {
                OnBeforeDeath?.Invoke();
            }
        }

        [PunRPC]
        public void OnRespawnRPC()
        {
            OnRespawn?.Invoke();
            transform.position = spawnpoint;
            if (Animator)
            {
                Animator.SetTrigger("Respawn");
            }
            else mesh.SetActive(true);
            IsDead = false;
            stats.HP = stats.MaxHP;
            stats.Resource = stats.MaxResource;
            OnRespawn?.Invoke();
        }


        protected float GetRespawnTime()
        {
            return 5 + 3 * stats.Lvl - 1;
        }

        //add to inhib, make IRespawning
        private IEnumerator Respawn()
        {
            float remainingTime = GetRespawnTime();

            BarTextTimer respawnHUD = respawnHUD = Instantiate(respawnHUDPrefab.gameObject).GetComponent<BarTextTimer>();
            respawnHUD.Initialize(remainingTime);

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
            photonView.RPC(nameof(OnRespawnRPC), RpcTarget.Others);
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
            if (!photonView.IsMine) return;
            if (!nearbyAlliedTowers.Contains(tower))
            {
                nearbyAlliedTowers.Add(tower);
            }
        }

        public void RemoveNearbyTower(Tower tower)
        {
            if (!photonView.IsMine) return;
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
            var statBarsGO = Resources.Load<GameObject>("ChampStatBars");
            statBarsInstance = Instantiate(statBarsGO, transform.parent);
            statBarsInstance.GetComponent<ChampStatBars>()?.Initialize(this, 0.5f, 1.2f, true);
        }


        public void ToggleRangeIndicator(bool show)
        {
            if (!attacking?.RangeIndicator) return;
            attacking.RangeIndicator.gameObject.SetActive(show);
        }

        [PunRPC]
        public void OnMoveCommand(Vector3 mousePos)
        {
            if (IsAttacking())
            {
                StopAttacking();
            }
            MoveTo(mousePos);
        }

        [PunRPC]
        public void OnAttackCommand(int targetViewID)
        {
            StartAttacking(targetViewID.GetUnitByID());
        }

        protected override void Die(Unit killer)
        {
            GameLogger.Log(this, LogActionType.die, transform.position, killer);
            base.Die(killer);
        }

    }
}
