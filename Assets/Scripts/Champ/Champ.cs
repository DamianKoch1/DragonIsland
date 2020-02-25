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

        private NavMeshAgent agent;

        private LineRenderer lr;


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

        public int MinionsKilled { get; private set; }
        public int TowersKilled { get; private set; }
        public int Kills { get; private set; }

        //wip
        public int Assists { get; private set; }
        public int Deaths { get; private set; }

        private int availableSkillPoints;
        public int AvailableSkillPoints
        {
            get => availableSkillPoints;
            private set
            {
                availableSkillPoints = value;
                OnSkillPointsChanged?.Invoke(availableSkillPoints);
            }
        }

        public Action<int> OnSkillPointsChanged;

        public bool CanLevelUltimate { get; private set; }

        [Space]
        [SerializeField]
        private BarTextTimer respawnHUDPrefab;

        private RangeIndicator rangeIndicator;


        public override void Initialize()
        {
            base.Initialize();

            SetupSkills();

            agent = GetComponent<NavMeshAgent>();
            lr = GetComponentInChildren<LineRenderer>();

            stats.OnLevelUp += (_) => RefreshScoreBoard();

            ScoreBoard.Instance.AddDisplay(this);

            if (!photonView.IsMine)
            {
                lr.enabled = false;
                return;
            }
            OnAttackedByChamp += RequestTowerAssist;

            nearbyAlliedTowers = new UnitList<Tower>();

            AvailableSkillPoints = 1;
            stats.OnLevelUp += AddAvailableSkillPoint;

            spawnpoint = Base.GetAllied(this).Spawnpoint.position;

            Gold = 0;


            OnUnitTick += () => Gold += GOLDPERSEC;
            rangeIndicator = GetComponentInChildren<RangeIndicator>();
            rangeIndicator.Initialize(this, stats.AtkRange);
        }

        private void AddAvailableSkillPoint(int levelReached)
        {
            if (levelReached == 6 || levelReached == 11 || levelReached == 16)
            {
                CanLevelUltimate = true;
            }
            AvailableSkillPoints++;
        }

        public void SpendSkillPoint(Skill skill)
        {
            if (!skills.Contains(skill)) return;
            skill.LevelUp();
            AvailableSkillPoints--;
        }


        protected override void Update()
        {
            base.Update();

            UpdateMinimapPath();
        }

        private void UpdateMinimapPath()
        {
            if (!photonView.IsMine) return;
            if (!CanMove && lr.enabled)
            {
                lr.enabled = false;
                return;
            }
            else if (CanMove && !lr.enabled)
            {
                lr.enabled = true;
            }
            var pathPoints = agent.path.corners;
            lr.positionCount = pathPoints.Length;
            for (int i = 0; i < pathPoints.Length; i++)
            {
                lr.SetPosition(i, pathPoints[i] += Vector3.up * 10);
            }
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


        public void ToggleRangeIndicator(bool on, float range = -1)
        {
            if (on)
            {
                rangeIndicator.ToggleOn(range);
            }
            else
            {
                rangeIndicator.ToggleOff(range);
            }
        }

        private void RefreshScoreBoard()
        {
            ScoreBoard.Instance.Refresh();
        }

        [PunRPC]
        public void OnKilled(int killerID)
        {
            Deaths++;
            var killer = killerID.GetUnitByID();
            if (killer is Champ)
            {
                ((Champ)killer).Kills++;
            }
            RefreshScoreBoard();
        }

        [PunRPC]
        public void OnKillMinion()
        {
            MinionsKilled++;
            RefreshScoreBoard();
        }

        [PunRPC]
        public void OnKillTower()
        {
            TowersKilled++;
            RefreshScoreBoard();
        }


        protected override void Die(Unit killer)
        {
            if (isDummy) return;
            GameLogger.Log(this, LogActionType.die, transform.position, killer);
            photonView.RPC(nameof(OnKilled), RpcTarget.All, killer.GetViewID());
            base.Die(killer);
        }

    }
}
