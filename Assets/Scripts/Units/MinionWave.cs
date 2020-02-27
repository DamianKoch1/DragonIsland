using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    /// <summary>
    /// Used to spawn each assigned minion after a delay, then destroys self
    /// </summary>
    public class MinionWave : MonoBehaviour
    {
        [SerializeField]
        private List<Minion> minions;

        [SerializeField]
        private float spawnDeltaTime = 1;

        private LaneID targetLane;

        /// <summary>
        /// If master client, saves target lane and parent to spawn minions to
        /// </summary>
        /// <param name="lane">lane to spawn minions in</param>
        /// <param name="spawnParent">parent for spawned minions</param>
        public void Initialize(LaneID lane, Transform spawnParent)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject);
                return;
            }
            else
            {

                targetLane = lane;
                StartCoroutine(BeginSpawning(spawnParent));
            }
        }

        /// <summary>
        /// Spawns each assigned minion with spawnDeltaTime delay, then network destroys self if master client
        /// </summary>
        /// <param name="spawnParent"></param>
        /// <returns></returns>
        private IEnumerator BeginSpawning(Transform spawnParent)
        {
            float time = 0;
            int minionCounter = 0;
            while (minionCounter < minions.Count)
            {
                if (time >= spawnDeltaTime)
                {
                    SpawnMinion(minionCounter, spawnParent);
                    time = 0;
                    minionCounter++;
                    yield return null;
                }
                time += Time.deltaTime;
                yield return null;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        /// <summary>
        /// If master client, spawns the assigned minion with index as a child of parent
        /// </summary>
        /// <param name="index">index of minion to spawn</param>
        /// <param name="parent">parent for spawned minion</param>
        private void SpawnMinion(int index, Transform parent)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            var minion = PhotonNetwork.Instantiate(minions[index].gameObject.name, transform.position, Quaternion.identity).GetComponent<Minion>();
            minion.transform.SetParent(parent);
            minion.TargetLane = targetLane;
        }
    }
}
