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

        private Vector3 mousePos;

        [Space]
        public DefaultColors defaultColors;

        public Shader outline;

        [SerializeField]
        private ParticleSystem moveClickVfx;

        [SerializeField]
        private ParticleSystem atkMoveClickVfx;

        [Header("Settings")]

        [SerializeField, Range(0.1f, 5)]
        private float scrollSpeed = 0.4f;

        [Header("Keybinds")]

        [SerializeField]
        private KeyCode attackMove;


        private void Start()
        {
            if (Instance && Instance != this) Destroy(gameObject);
        }

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

        public bool GetMouseWorldPos(out Vector3 mouseWorldPos)
        {
            return cam.GetCursorToWorldPoint(out mouseWorldPos);
        }


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
            if (hovered)
            {
                player.OnAttackCommand(hovered.GetViewID());
                GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
            }
            else if (GetMouseWorldPos(out mousePos))
            {
                player.OnMoveCommand(mousePos);
                GameLogger.Log(player, LogActionType.move, mousePos);
            }
        }

        private void OnMoveHeld()
        {
            if (hovered)
            {
                player.OnAttackCommand(hovered.GetViewID());
                GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
            }
            else if (GetMouseWorldPos(out mousePos))
            {
                player.OnMoveCommand(mousePos);
                GameLogger.Log(player, LogActionType.move, mousePos);
            }
        }

        private void OnAttackMovePressed()
        {
            if (hovered)
            {
                if (hovered.Targetable)
                {
                    player.OnAttackCommand(hovered.GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
                }
                return;
            }
            if (!GetMouseWorldPos(out mousePos)) return;
            Instantiate(atkMoveClickVfx, mousePos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = player.GetTargetableEnemiesInAtkRange<Unit>(mousePos);
            switch (targets.Count())
            {
                case 0:
                    player.OnMoveCommand(mousePos);
                    GameLogger.Log(player, LogActionType.move, mousePos);
                    break;
                case 1:
                    var target = targets[0];
                    player.OnAttackCommand(targets[0].GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, target);
                    break;
                default:
                    var closestTarget = targets.GetClosestUnitFrom<Unit>(mousePos);
                    player.OnAttackCommand(closestTarget.GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, closestTarget);
                    break;
            }
        }

        private void ProcessPlayerInput()
        {
            if (player.IsDead) return;
            if (Input.GetMouseButtonDown(1))
            {
                OnMovePressed();
                Instantiate(moveClickVfx, mousePos + Vector3.up * 0.2f, Quaternion.identity);
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
                OnAttackMovePressed();
                player.ToggleRangeIndicator(true);
            }
            else if (Input.GetKeyUp(attackMove))
            {
                player.ToggleRangeIndicator(false);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (GetMouseWorldPos(out mousePos))
                {
                    player.CastQ(hovered, mousePos);
                    GameLogger.Log(player, LogActionType.Q, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (GetMouseWorldPos(out mousePos))
                {
                    player.CastW(hovered, mousePos);
                    GameLogger.Log(player, LogActionType.W, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (GetMouseWorldPos(out mousePos))
                {
                    player.CastE(hovered, mousePos);
                    GameLogger.Log(player, LogActionType.E, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (GetMouseWorldPos(out mousePos))
                {
                    player.CastR(hovered, mousePos);
                    GameLogger.Log(player, LogActionType.R, mousePos, hovered);
                }
            }
        }

        private void ProcessCamInput()
        {
            var scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                cam.AddDistanceFactor(-scrollAxis * scrollSpeed);
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

        public void UpdateMousePos()
        {
            GetMouseWorldPos(out mousePos);
        }

        private void Update()
        {
            UpdateMousePos();
            ProcessPlayerInput();
            ProcessCamInput();
            ProcessDebugInput();
        }

        private void OnValidate()
        {
            if (!cam) return;
            cam.transform.position = camOffset;
            cam.transform.rotation = Quaternion.Euler(camRotation);
        }
    }

}


