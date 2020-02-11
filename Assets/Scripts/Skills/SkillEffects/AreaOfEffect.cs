using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    [RequireComponent(typeof(CapsuleCollider))]
    public class AreaOfEffect : MonoBehaviour
    {
        private List<SkillEffect> onHitEffects;

        private Unit owner;

        private Unit target;

        private TeamID ownerTeamID;

        private float lifespan;

        private float timeActive;

        private float timeSinceLastTick;

        private UnitStats ownerStatsAtSpawn;

        private HitMode hitMode;

        private bool canHitStructures;

        private UnitList<Unit> hitables;

        private float tickInterval;

        private PhotonView ownerView;

        [Tooltip("Let child effects use their scaling or override with own?")]
        public bool overrideScalings;

        private bool active = true;

        public void Initialize(Unit _owner, UnitStats ownerStats, TeamID _ownerTeamID, Unit _target, float _lifespan, float size, float _tickInterval, HitMode _hitMode, bool _canHitStructures, Scalings scaling, float delay = 0)
        {
            target = _target;
            ownerTeamID = _ownerTeamID;
            lifespan = _lifespan;
            if (size != 1)
            {
                Vector3 newScale = transform.localScale;
                newScale.x *= size;
                newScale.z *= size;
                transform.localScale = newScale;
            }
            tickInterval = _tickInterval;
            hitMode = _hitMode;
            canHitStructures = _canHitStructures;
            timeActive = 0;
            timeSinceLastTick = 0;
            ownerStatsAtSpawn = ownerStats;
            if (delay > 0)
            {
                StartCoroutine(WaitForDelay(delay));
            }
            if (_owner)
            {
                owner = _owner;
                ownerView = owner.GetComponent<PhotonView>();
                if (ownerView)
                {
                    if (ownerView.IsMine)
                    {
                        onHitEffects = new List<SkillEffect>(GetComponents<SkillEffect>());
                        foreach (var effect in onHitEffects)
                        {
                            effect.Initialize(owner, 0);
                            if (overrideScalings)
                            {
                                effect.SetScaling(scaling);
                            }
                            effect.SetStatsAtActivation(ownerStatsAtSpawn);
                        }
                        hitables = new UnitList<Unit>();
                        Tick();
                    }
                }
            }
        }

        private IEnumerator WaitForDelay(float delay)
        {
            active = false;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(delay);
            active = true;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (!active) return;
            timeSinceLastTick += Time.deltaTime;
            while (timeSinceLastTick >= tickInterval)
            {
                timeSinceLastTick -= tickInterval;
                timeActive += tickInterval;
                Tick();
            }

            if (lifespan < 0) return;
            if (timeActive > lifespan)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ownerView) return;
            if (!ownerView.IsMine) return;
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (unit.IsDead) return;

            if (unit is Structure)
            {
                if (!canHitStructures) return;
            }

            switch (hitMode)
            {
                case HitMode.targetOnly:
                    if (unit == target)
                    {
                        hitables.Add(unit);
                    }
                    break;
                case HitMode.enemyChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (unit.IsEnemy(ownerTeamID))
                        {
                            hitables.Add(unit);
                        }
                    }
                    break;
                case HitMode.enemyUnits:
                    if (unit.IsEnemy(ownerTeamID))
                    {
                        hitables.Add(unit);
                    }
                    break;
                case HitMode.alliedChamps:
                    if (unit.GetComponent<Champ>())
                    {
                        if (unit.IsAlly(ownerTeamID))
                        {
                            hitables.Add(unit);
                        }
                    }
                    break;
                case HitMode.alliedUnits:
                    if (unit.IsAlly(ownerTeamID))
                    {
                        hitables.Add(unit);
                    }
                    break;
                case HitMode.monsters:
                    if (unit is Monster)
                    {
                        hitables.Add(unit);
                    }
                    break;
                case HitMode.anyUnit:
                    hitables.Add(unit);
                    break;
                default:
                    Debug.LogError(owner.name + "spawned area of effect with invalid hit mode!");
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ownerView) return;
            if (!ownerView.IsMine) return;
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (hitables.Contains(unit))
            {
                hitables.Remove(unit);
            }
        }

        private void Tick()
        {
            if (!active) return;
            if (!ownerView) return;
            if (!ownerView.IsMine) return;
            if (hitables.Count() > 0)
            {
                foreach (var effect in onHitEffects)
                {
                    effect.Activate(hitables);
                }
            }

        }

    }
}
