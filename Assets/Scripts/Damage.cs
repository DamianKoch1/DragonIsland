using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public enum DamageType
    {
        invalid = -1,
        physical = 0,
        magical = 1,
        piercing = 2,
    }

    public class Damage
    {
        private float baseDmg;

        private DamageType dmgType = DamageType.invalid;

        private Unit instigator;

        private List<Unit> targets;

        private float GetDamageTo(Unit target)
        {
            float dmg = baseDmg * instigator.amplifiers.dealtDmg;
            float defense = 0;
            switch (dmgType)
            {
                case DamageType.physical:
                    defense = target.Armor * instigator.PercentArmorPen - instigator.FlatArmorPen;
                    break;
                case DamageType.magical:
                    defense = target.MagicRes * instigator.PercentMagicPen - instigator.FlatMagicPen;
                    break;
                case DamageType.piercing:
                    defense = 0;
                    break;
                default:
                    Debug.LogWarning("Warning: found Damage with invalid type!");
                    break;
            }
            if (defense < 0)
            {
                return dmg * (2 - 100 / (100 - defense));
            }
            return dmg * (100 / 100 + defense);
        }

        public void Inflict()
        {
            foreach (var target in targets)
            {
                InflictTo(target);
            }
        }

        private void InflictTo(Unit target)
        {
            target.ReceiveDamage(instigator, GetDamageTo(target), dmgType);
        }

        public Damage(float _baseDmg, DamageType _dmgType, Unit _instigator, List<Unit> _targets)
        {
            baseDmg = _baseDmg;
            dmgType = _dmgType;
            instigator = _instigator;
            targets = _targets;
        }

        public Damage(float _baseDmg, DamageType _dmgType, Unit _instigator, Unit _target)
        {
            baseDmg = _baseDmg;
            dmgType = _dmgType;
            instigator = _instigator;
            targets = new List<Unit> { _target };
        }

    }
}
