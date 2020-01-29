using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO cancellable by moving
    public class SkillChannel : SkillToggleable
    {
        [Space]
        [SerializeField]
        private bool cancellable;

        public override void Initialize(Unit _owner)
        {
            base.Initialize(_owner);
            OnCast += ActivateEffects;
            OnCastTimeFinished -= ActivateEffects;
            OnCastTimeFinished += ToggleOff;
        }

        protected override bool TryToggleOff()
        {
            if (!cancellable) return false;
            if (castTimeCoroutine != null)
            {
                StopCoroutine(castTimeCoroutine);
            }
            OnCastTimeFinished?.Invoke();
            return true;
        }
    }
}
