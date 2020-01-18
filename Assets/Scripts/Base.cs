using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class Base : Structure
    {
        [SerializeField]
        private MinionWave defaultWave;

        [SerializeField]
        private MinionWave empoweredWave;


        [SerializeField]
        private LaneWaypoint topSpawn;

        [SerializeField]
        private LaneWaypoint midSpawn;

        [SerializeField]
        private LaneWaypoint botSpawn;


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
                                break;
                        }
                    }
                }
                return instanceRed;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            topSpawn.Initialize(LaneID.top);
            botSpawn.Initialize(LaneID.bot);
            midSpawn.Initialize(LaneID.mid);
            timeSinceSpawn = waveSpawnDeltaTime;

        }

        protected override void Update()
        {
            base.Update();

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
            MinionWave wave;
            wave = Instantiate(defaultWave, topSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
            wave.Initialize(LaneID.top);

            wave = Instantiate(defaultWave, midSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
            wave.Initialize(LaneID.mid);

            wave = Instantiate(defaultWave, botSpawn.transform.position, Quaternion.identity).GetComponent<MinionWave>();
            wave.Initialize(LaneID.bot);
        }
    }
}
