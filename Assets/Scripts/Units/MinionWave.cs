using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    public class MinionWave : MonoBehaviour
    {
        [SerializeField]
        private List<Minion> minions;

        [SerializeField]
        private float spawnDeltaTime = 1;

        private LaneID targetLane;

        public void Initialize(LaneID lane, Transform spawnParent)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject);
                return;
            }
            targetLane = lane;
            StartCoroutine(BeginSpawning(spawnParent));
        }

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
            Destroy(gameObject);
        }

        private void SpawnMinion(int index, Transform parent)
        {
            var minion = PhotonNetwork.Instantiate(minions[index].gameObject.name, transform.position, Quaternion.identity).GetComponent<Minion>();
            minion.transform.SetParent(parent);
            minion.TargetLane = targetLane;
        }
    }
}
