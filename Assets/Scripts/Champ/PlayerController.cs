using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MOBA.Logging;

namespace MOBA
{
    /// <summary>
    /// Controls the local player
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private static PlayerController instance;

        public static PlayerController Instance
        {
            set => instance = value;
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<PlayerController>();
                    instance.Initialize();
                }
                return instance;
            }
        }

        [HideInInspector]
        public Unit hovered;

        [SerializeField]
        private ChampCamera cam;


        [SerializeField]
        private Vector3 camOffset;

        [SerializeField]
        private Vector3 camRotation;

        [Space]
        [SerializeField]
        private Champ player;

        private PhotonView playerView;

        [SerializeField]
        private Interface ui;

        public Interface UI => ui;

        public static Champ Player => Instance.player;

        [Space, Tooltip("Default colors for ally / enemy / own / ... hp bar / outline")]
        public DefaultColors defaultColors;

        public Shader outline;

        [SerializeField]
        private ParticleSystem moveClickVfx;

        [SerializeField]
        private ParticleSystem atkMoveClickVfx;


        [Header("Keybinds")]

        [SerializeField]
        private KeyCode attackMove;


        private void Start()
        {
            if (Instance && Instance != this) Destroy(gameObject);
        }

        /// <summary>
        /// Spawns a player, assigns it to a team depending on if client actor number is even, initializes it, the camera and ui
        /// </summary>
        public void Initialize()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.LocalPlayer.ActorNumber % 2 == 0)
                {
                    player = PhotonNetwork.Instantiate("TestChamp", Base.InstanceRed.Spawnpoint.position, Quaternion.identity).GetComponent<Champ>();
                    playerView = PhotonView.Get(player);
                    playerView.RPC(nameof(player.SetTeamID), RpcTarget.All, (short)TeamID.red);
                }
                else
                {
                    player = PhotonNetwork.Instantiate("TestChamp", Base.InstanceBlue.Spawnpoint.position, Quaternion.identity).GetComponent<Champ>();
                    playerView = PhotonView.Get(player);
                    playerView.RPC(nameof(player.SetTeamID), RpcTarget.All, (short)TeamID.blue);
                }
            }
            cam.Initialize(player, camOffset, Quaternion.Euler(camRotation));
            ui?.Initialize(player);
        }

        /// <summary>
        /// Raycasts mouse position onto ground plane
        /// </summary>
        /// <param name="result">Resulting position on ground plane</param>
        /// <returns></returns>
        public bool GetMouseWorldPos(out Vector3 mouseWorldPos)
        {
            return cam.GetCursorToGroundPoint(out mouseWorldPos);
        }

        /// <summary>
        /// Shows stats of hovered unit
        /// </summary>
        private void OnSelectPressed()
        {
            if (!hovered)
            {
                ui.HideTargetStats();
                return;
            }
            ui.ShowTargetStats(hovered);
        }

        private void OnMovePressed()
        {
            AttackOrMove(true);
        }

        private void OnMoveHeld()
        {
            AttackOrMove();
        }

        /// <summary>
        /// Player moves to mouse ground position or attacks hovered unit if it is a valid target
        /// </summary>
        /// <param name="spawnClickVFX">Spawn a click vfx at mouse ground position?</param>
        private void AttackOrMove(bool spawnClickVFX = false)
        {
            if (!GetMouseWorldPos(out var targetPos)) return;

            if (Minimap.Instance.IsCursorOnMinimap(out var mouseWorldPos))
            {
                targetPos = mouseWorldPos;
            }
            else if (hovered)
            {
                if (hovered.Targetable)
                {
                    if (player.CurrentAttackTarget == hovered) return;
                    player.StartAttacking(hovered);
                    GameLogger.Log(player, LogActionType.attack, targetPos, hovered);
                    return;
                }
                else
                {
                    targetPos = hovered.GetGroundPos();
                }
            }
            player.MoveTo(targetPos);
            GameLogger.Log(player, LogActionType.move, targetPos);
            if (spawnClickVFX)
            {
                Instantiate(moveClickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
            }
        }


        /// <summary>
        /// If there are valid targets near mouse ground position, player attacks the closest, if not player moves there
        /// </summary>
        private void OnAttackMovePressed()
        {
            if (!GetMouseWorldPos(out var targetPos)) return;
            if (Minimap.Instance.IsCursorOnMinimap(out var mouseWorldPos))
            {
                targetPos = mouseWorldPos;
            }
            else if (hovered)
            {
                if (hovered.Targetable)
                {
                    player.StartAttacking(hovered);
                    GameLogger.Log(player, LogActionType.attack, targetPos, hovered);
                }
                return;
            }
            Instantiate(atkMoveClickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = player.GetTargetableEnemiesInRange<Unit>(targetPos, 10, true);
            switch (targets.Count())
            {
                case 0:
                    player.MoveTo(targetPos);
                    GameLogger.Log(player, LogActionType.move, targetPos);
                    break;
                case 1:
                    var target = targets[0];
                    player.StartAttacking(targets[0]);
                    GameLogger.Log(player, LogActionType.attack, targetPos, target);
                    break;
                default:
                    var closestTarget = targets.GetClosestUnitFrom<Unit>(targetPos);
                    player.StartAttacking(closestTarget);
                    GameLogger.Log(player, LogActionType.attack, targetPos, closestTarget);
                    break;
            }
        }

        private void ProcessPlayerInput()
        {
            if (player.IsDead) return;

            ProcessMovementInput();

            ProcessSkillInput();
        }

        private void ProcessMovementInput()
        {
            if (Input.GetMouseButtonDown(1))
            {
                player.OnMovementCommand?.Invoke();
                OnMovePressed();
            }
            else if (Input.GetMouseButton(1))
            {
                OnMoveHeld();
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnSelectPressed();
            }

            if (Input.GetKeyDown(attackMove))
            {
                player.OnMovementCommand?.Invoke();
                OnAttackMovePressed();
                player.ToggleRangeIndicator(true, player.Stats.AtkRange);
            }
            else if (Input.GetKeyUp(attackMove))
            {
                player.ToggleRangeIndicator(false, player.Stats.AtkRange);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                player.OnMovementCommand?.Invoke();
                player.Stop();
            }
        }

        private void ProcessSkillInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (GetMouseWorldPos(out var targetPos))
                {
                    if (!player.CastQ(hovered, targetPos))
                    {
                        player.ToggleRangeIndicator(true, player.Skills[0].CastRange);
                    }
                    else
                    {
                        GameLogger.Log(player, LogActionType.Q, targetPos, hovered);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.Q))
            {
                player.ToggleRangeIndicator(false, player.Skills[0].CastRange);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (GetMouseWorldPos(out var targetPos))
                {
                    if (!player.CastW(hovered, targetPos))
                    {
                        player.ToggleRangeIndicator(true, player.Skills[1].CastRange);
                    }
                    else
                    {
                        GameLogger.Log(player, LogActionType.W, targetPos, hovered);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.W))
            {
                player.ToggleRangeIndicator(false, player.Skills[1].CastRange);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (GetMouseWorldPos(out var targetPos))
                {
                    if (!player.CastE(hovered, targetPos))
                    {
                        player.ToggleRangeIndicator(true, player.Skills[2].CastRange);
                    }
                    else
                    {
                        GameLogger.Log(player, LogActionType.E, targetPos, hovered);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.E))
            {
                player.ToggleRangeIndicator(false, player.Skills[2].CastRange);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GetMouseWorldPos(out var targetPos))
                {
                    if (!player.CastR(hovered, targetPos))
                    {
                        player.ToggleRangeIndicator(true, player.Skills[3].CastRange);
                    }
                    else
                    {
                        GameLogger.Log(player, LogActionType.R, targetPos, hovered);
                    }
                }
            }
            if (Input.GetKeyUp(KeyCode.R))
            {
                player.ToggleRangeIndicator(false, player.Skills[3].CastRange);
            }
        }

        private void ProcessDebugInput()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonNetwork.IsMasterClient) return;
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (Time.timeScale < 8)
                {
                    Time.timeScale *= 2;
                }
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (Time.timeScale > 0.25f)
                {
                    Time.timeScale /= 2;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                player.Stats.DebugMode();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                player.LevelUp();
            }
        }

        private void Update()
        {
            ProcessPlayerInput();
            ProcessDebugInput();
        }

        /// <summary>
        /// Previews camera position / rotation using assigned offset / rotation values
        /// </summary>
        private void OnValidate()
        {
            if (!cam) return;
            cam.transform.position = camOffset;
            cam.transform.rotation = Quaternion.Euler(camRotation);
        }
    }

}


