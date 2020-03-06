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
    /// <summary>
    /// A player controllable character
    /// </summary>
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

        /// <summary>
        /// Dummies can't die, aren't player controlled and ignored by ScoreBoard
        /// </summary>
        public bool isDummy;

        /// <summary>
        /// If attacked, requests assist from these towers
        /// </summary>
        protected UnitList<Tower> nearbyAlliedTowers;


        private Vector3 spawnpoint;

        private NavMeshAgent agent;

        /// <summary>
        /// Shows the current path on minimap
        /// </summary>
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

        //TODO WIP
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

        PhotonTransformView transformView;


        [Space]
        [SerializeField, Tooltip("Shows remaining respawn time on a bar and text")]
        private BarTextTimer respawnHUDPrefab;

        private RangeIndicator rangeIndicator;

        /// <summary>
        /// Sets up variables / skills, event subscriptions if local player
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            SetupSkills();

            agent = GetComponent<NavMeshAgent>();
            lr = GetComponentInChildren<LineRenderer>();
            transformView = GetComponent<PhotonTransformView>();

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

        /// <summary>
        /// Called every levelUp, Increments AvailableSkillPoints which calls an action that the Interface listens to if local player
        /// </summary>
        /// <param name="levelReached">The level that was reached</param>
        private void AddAvailableSkillPoint(int levelReached)
        {
            AvailableSkillPoints++;
        }

        /// <summary>
        /// Levels up the given skill, called by SkillDisplay buttons from interface
        /// </summary>
        /// <param name="skill"></param>
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

        /// <summary>
        /// Sets the lineRenderer points to the corners of NavmeshAgents path
        /// </summary>
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

        /// <summary>
        /// Initializes all skills
        /// </summary>
        private void SetupSkills()
        {
            foreach (var skill in Skills)
            {
                skill.Initialize(this);
            }
        }


        //TODO cast fail feedback
        /// <summary>
        /// Tries to cast the first ability on hovered unit / mouse position
        /// </summary>
        /// <param name="hovered">Currently hovered unit (can be null)</param>
        /// <param name="mousePos">Current mouse position</param>
        /// <returns></returns>
        public bool CastQ(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count == 0) return false;
            return Skills[0].TryCast(hovered, mousePos);
        }

        /// <summary>
        /// Tries to cast the second ability on hovered unit / mouse position
        /// </summary>
        /// <param name="hovered">Currently hovered unit (can be null)</param>
        /// <param name="mousePos">Current mouse position</param>
        /// <returns></returns>
        public bool CastW(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 1) return false;
            return Skills[1].TryCast(hovered, mousePos);
        }

        /// <summary>
        /// Tries to cast the third ability on hovered unit / mouse position
        /// </summary>
        /// <param name="hovered">Currently hovered unit (can be null)</param>
        /// <param name="mousePos">Current mouse position</param>
        /// <returns></returns>
        public bool CastE(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 2) return false;
            return Skills[2].TryCast(hovered, mousePos);
        }

        /// <summary>
        /// Tries to cast the fourth ability on hovered unit / mouse position
        /// </summary>
        /// <param name="hovered">Currently hovered unit (can be null)</param>
        /// <param name="mousePos">Current mouse position</param>
        /// <returns></returns>
        public bool CastR(Unit hovered, Vector3 mousePos)
        {
            if (Skills.Count <= 3) return false;
            return Skills[3].TryCast(hovered, mousePos);
        }

        /// <summary>
        /// Only initializes if this is a dummy (PlayerController usually initializes champs)
        /// </summary>
        protected override void Start()
        {
            if (isDummy) Initialize();
        }

        /// <summary>
        /// Called by PlayerController (which distrubutes players to teams), sets own TeamID to given value and initializes afterwards
        /// </summary>
        /// <param name="_teamID">New TeamID</param>
        [PunRPC]
        public void SetTeamID(short _teamID)
        {
            teamID = (TeamID)_teamID;
            Initialize();
        }

        /// <summary>
        /// Increases xp by given value, can trigger levelUp
        /// </summary>
        /// <param name="amount"></param>
        [PunRPC]
        public void ReceiveXP(float amount)
        {
            stats.XP += amount;
        }

        /// <summary>
        /// Disables collision, sets animation trigger or hides mesh, stops attacking, starts respawn process
        /// </summary>
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
                OnDeathEvent?.Invoke();
            }
        }

        /// <summary>
        /// Moves to spawnpoint, sets animation trigger or shows mesh, refills hp / resource
        /// </summary>
        [PunRPC]
        public void OnRespawnRPC()
        {
            transformView.SetNetworkPosition(spawnpoint);
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

        /// <summary>
        /// Get the currently needed time to respawn (usually scales with level)
        /// </summary>
        /// <returns></returns>
        protected float GetRespawnTime()
        {
            return 5 + 3 * stats.Lvl - 1;
        }

        //TODO fix sometimes remaining untargetable after respawning for others?
        /// <summary>
        /// Creates a BarTextTimer showing remaining time, waits for current GetRespawnTime(), then reenables collision for this client and calls OnRespawnRPC() for everyone
        /// </summary>
        /// <returns></returns>
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


            photonView.RPC(nameof(OnRespawnRPC), RpcTarget.All);

            movement.EnableCollision();
        }

        public Action OnRespawn;

        /// <summary>
        /// Requests all nearby towers to attack given champ
        /// </summary>
        /// <param name="attacker">Champ for towers to attack</param>
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

        public void RemoveNearbyAlliedTower(Tower tower)
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

        public override float GetXPNeededForLevel(int level)
        {
            return (level - 1) * 25;
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

        /// <summary>
        /// Toggles range indicator on/off if its range matches given float
        /// </summary>
        /// <param name="on">Toggle on or off?</param>
        /// <param name="range">The range of the indicator to toggle off</param>
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

        /// <summary>
        /// Makes ScoreBoard recalculate all values
        /// </summary>
        private void RefreshScoreBoard()
        {
            ScoreBoard.Instance.Refresh();
        }

        /// <summary>
        /// Changes respective score values (own deaths and killer kills if it is a champ)
        /// </summary>
        /// <param name="killer">The unit that dealt lethal damage</param>
        public void OnKilled(Unit killer)
        {
            Deaths++;
            if (killer is Champ)
            {
                ((Champ)killer).Kills++;
            }
            RefreshScoreBoard();
        }

        /// <summary>
        /// Increments minions killed count
        /// </summary>
        public void OnKillMinion()
        {
            MinionsKilled++;
            RefreshScoreBoard();
        }

        /// <summary>
        /// Increments towers killed count
        /// </summary>
        public void OnKillTower()
        {
            TowersKilled++;
            RefreshScoreBoard();
        }


        protected override void Die(Unit killer)
        {
            if (isDummy) return;
            if (IsDead) return;
            GameLogger.Log(this, LogActionType.die, transform.position, killer);
            OnKilled(killer);
            base.Die(killer);
        }

    }
}
