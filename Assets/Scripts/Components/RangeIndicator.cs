using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO stop using 1 indicator for everything
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer sprite;


        [SerializeField]
        private float alpha = 0.2f;

        public void Initialize(Unit owner, float range, bool toggledOn = false)
        {
            Color color;
            if (PlayerController.Player.IsAlly(owner))
            {
               color = PlayerController.Instance.defaultColors.allyStructureHP;
            }
            else
            {
                color = PlayerController.Instance.defaultColors.enemyStructureHP;
            }
            color.a = alpha;
            sprite.color = color;

            if (toggledOn)
            {
                ToggleOn(range);
            }
            else
            {
                SetRange(range);
                ToggleOff();
            }
        }

        public void ToggleOn(float newRange)
        {
            if (newRange < 0) return;
            sprite.enabled = true;
            SetRange(newRange);
        }

        public void ToggleOff(float range = -1)
        {
            if (range > 0)
            {
                if (range != transform.localScale.x) return;
            }
            sprite.enabled = false;
        }

        public void SetRange(float range)
        {
            if (transform.localScale.x == range) return;
            transform.localScale = new Vector3(range, 1, range);
        }
    }
}
