using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingMelee : Attacking
    {
        [SerializeField]
        private GameObject mesh;

        private Vector3 startScale;

        [SerializeField]
        private AnimationCurve attackAnimCurve;

        public override void Attack(Unit target)
        {
            if (animator)
            {
                animator?.SetTrigger("attack");
            }
            StartCoroutine(AttackAnim(target));
        }

        public override void Initialize(Unit _owner)
        {
            base.Initialize(_owner);
            startScale = mesh.transform.localScale;
        }

        private IEnumerator AttackAnim(Unit target)
        {
            float time = 0;
            while (time < 1 / owner.Stats.AtkSpeed)
            {
                if (!target) break;
                mesh.transform.localScale = startScale + Vector3.one * attackAnimCurve.Evaluate(time * owner.Stats.AtkSpeed);
                if (time < 0.5f / owner.Stats.AtkSpeed)
                {
                    if (time + Time.deltaTime >= 0.5f / owner.Stats.AtkSpeed)
                    {
                        var dmg = new Damage(owner.Stats.AtkDmg, DamageType.physical, owner, target);
                        dmg.Inflict();
                    }
                }
                time += Time.deltaTime;
                yield return null;
                continue;
            }
            AttackAnimFinished();
        }
    }
}
