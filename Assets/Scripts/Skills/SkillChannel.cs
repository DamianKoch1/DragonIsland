using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class SkillChannel : SkillToggleable
    {
        [Space]
        [SerializeField]
        private bool cancellable;

        protected override void Initialize()
        {
            isReady = true;
            Rank = 1;
            effects = new List<SkillEffect>(GetComponents<SkillEffect>());
            OnCast += ActivateEffects;
            OnCastTimeFinished += ToggleOff;
        }

        protected override bool TryToggleOff()
        {
            if (!cancellable) return false;
            OnCastTimeFinished?.Invoke();
            return true;
        }
    }
}
