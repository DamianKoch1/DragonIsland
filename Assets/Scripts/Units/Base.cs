using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    //TODO end game on destroy
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
                    foreach (var teamBase in FindObjectsOfType<Base>())
                    {
                        switch (teamBase.TeamID)
                        {
                            case TeamID.blue:
                                instanceBlue = teamBase;
                                break;
                            case TeamID.red:
                                instanceRed = teamBase;
                                break;
                            default:
                                Debug.LogWarning("Warning: " + teamBase.name + " had TeamID other than blue/red.");
                                break;
                        }
                    }
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
                    foreach (var teamBase in FindObjectsOfType<Base>())
                    {
                        switch (teamBase.TeamID)
                        {
                            case TeamID.blue:
                                instanceBlue = teamBase;
                                break;
                            case TeamID.red:
                                instanceRed = teamBase;
                                break;
                            default:
                                Debug.LogWarning("Warning: " + teamBase.name + " had TeamID other than blue/red.");
                                break;
                        }
                    }
                }
                return instanceRed;
            }
        }

        protected override void OnDeath()
        {
            PhotonNetwork.LoadLevel("Lobby");
        }

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

      

        private void SpawnWaves()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            MinionWave wave;
            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, topSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
            wave.Initialize(LaneID.top, spawnParent);

            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, midSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
            wave.Initialize(LaneID.mid, spawnParent);

            wave = PhotonNetwork.Instantiate(defaultWave.gameObject.name, botSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
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
