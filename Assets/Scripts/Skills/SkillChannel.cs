using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO cancellable when moving
    public class SkillChannel : SkillToggleable
    {
        [Space]
        [SerializeField]
        private bool cancellable;

        protected override void Initialize()
        {
            base.Initialize();
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
