using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{

    //TODO end game on destroy
    /// <summary>
    /// Responsible for spawning waves, has spawnpoint for allied champs
    /// </summary>
    public class Base : Structure
    {
        [Space]

        [SerializeField]
        private MinionWave defaultWave;

        [SerializeField]
        private MinionWave empoweredWave;

        [Space]
        [SerializeField]
        private Transform spawnpoint;

        public Transform Spawnpoint => spawnpoint;

        [Space]
        [SerializeField]
        private Transform spawnParent;

        [SerializeField]
        private LaneWaypoint topSpawn;

        [SerializeField]
        private LaneWaypoint midSpawn;

        [SerializeField]
        private LaneWaypoint botSpawn;

        [Space]

        [SerializeField]
        private float waveSpawnDeltaTime = 15;
        private float timeSinceSpawn;

        private static Base instanceBlue;
        private static Base instanceRed;

        public static Base InstanceBlue
        {
            private set
            {
                instanceBlue = value;
            }
            get
            {
                if (!instanceBlue)
                {
                    var bases = FindObjectsOfType<Base>().ToList();
                    InstanceBlue = bases.Find(b => b.teamID == TeamID.blue);
                    InstanceRed = bases.Find(b => b.teamID == TeamID.red);
                }
                return instanceBlue;
            }
        }

        public static Base InstanceRed
        {
            private set
            {
                instanceRed = value;
            }
            get
            {
                if (!instanceRed)
                {
                    var bases = FindObjectsOfType<Base>().ToList();
                    InstanceBlue = bases.Find(b => b.teamID == TeamID.blue);
                    InstanceRed = bases.Find(b => b.teamID == TeamID.red);
                }
                return instanceRed;
            }
        }

        //TODO WIP
        /// <summary>
        /// Returns to lobby
        /// </summary>
        protected override void OnDeath()
        {
            PhotonNetwork.LoadLevel("Lobby");
        }

        /// <summary>
        /// Gets the allied base for given unit
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>Returns units allied base, null if unit is neither red nor blue</returns>
        public static Base GetAllied(Unit unit)
        {
            if (unit.IsAlly(InstanceBlue))
            {
                return InstanceBlue;
            }
            else if (unit.IsAlly(InstanceRed))
            {
                return InstanceRed;
            }
            return null;
        }

        /// <summary>
        /// Initializes wave spawning if master client
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            topSpawn.Initialize(LaneID.top);
            botSpawn.Initialize(LaneID.bot);
            midSpawn.Initialize(LaneID.mid);

            if (!PhotonNetwork.IsMasterClient) return;
            timeSinceSpawn = waveSpawnDeltaTime;

        }

        protected override void Update()
        {
            base.Update();

            if (!PhotonNetwork.IsMasterClient) return;
            if (timeSinceSpawn >= waveSpawnDeltaTime)
            {
                SpawnWaves();
                timeSinceSpawn = 0;
            }
            else
            {
                timeSinceSpawn += Time.deltaTime;
            }
        }

      
        /// <summary>
        /// If master client, network instantiates minion wave in each lane
        /// </summary>
        private void SpawnWaves()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            MinionWave wave;
            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, topSpawn.transform.position, topSpawn.transform.rotation).GetComponent<MinionWave>();
            wave.Initialize(LaneID.top, spawnParent);

            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, midSpawn.transform.position, midSpawn.transform.rotation).GetComponent<MinionWave>();
            wave.Initialize(LaneID.mid, spawnParent);

            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, botSpawn.transform.position, botSpawn.transform.rotation).GetComponent<MinionWave>();
            wave.Initialize(LaneID.bot, spawnParent);
        }

        public override float GetXPReward()
        {
            return 1;
        }

        public override int GetGoldReward()
        {
            return 1;
        }

    }
}
