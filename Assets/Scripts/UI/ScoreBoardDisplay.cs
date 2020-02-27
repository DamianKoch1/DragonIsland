using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Used to display champ stats like KDA
    /// </summary>
    public class ScoreBoardDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text minionsKilledText;

        [SerializeField]
        private Text KDAText;

        public Champ Target { get; private set; }

        /// <summary>
        /// Saves champ to display stats for, slightly darkens if it is local player
        /// </summary>
        /// <param name="_target"></param>
        public void SetTargetChamp(Champ _target)
        {
            Target = _target;
            if (PhotonView.Get(Target).IsMine)
            {
                GetComponent<Image>().color += new Color(0, 0, 0, 0.1f);
            }
            Refresh();
        }

        /// <summary>
        /// Updates texts to match target stats
        /// </summary>
        public void Refresh()
        {
            levelText.text = Target.Stats.Lvl + "";
            minionsKilledText.text = Target.MinionsKilled + "";
            KDAText.text = Target.Kills + " / " + Target.Deaths + " / " + Target.Assists;
        }
    }
}
