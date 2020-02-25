using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ScoreBoardDisplay : MonoBehaviour
    {
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text minionsKilledText;

        [SerializeField]
        private Text KDAText;

        private Champ target;

        public void SetTargetChamp(Champ _target)
        {
            target = _target;
            Refresh();
        }

        public void Refresh()
        {
            levelText.text = target.Stats.Lvl + "";
            minionsKilledText.text = target.MinionsKilled + "";
            KDAText.text = target.Kills + " / " + target.Deaths + " / " + target.Assists;
        }
    }
}
