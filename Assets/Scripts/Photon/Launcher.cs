using MOBA.Logging;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";


    void Awake()
    {
        btnText = startBtn.GetComponentInChildren<Text>();
        startBtn.onClick.AddListener(Connect);
        if (!enableLogging)
        {
            GameLogger.enabled = false;
            Destroy(logViewBtn.gameObject);
        }

        //TODO rewrite this, using this to force rejoin when loaded from finished game
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            return;
        }

        if (autoConnect) Connect();
    }



    [SerializeField]
    private Text roomDisplay;

    private Text btnText;

    [SerializeField]
    private Button startBtn;

    [SerializeField]
    private Button createRoomBtn;

    [SerializeField]
    private Button leaveBtn;

    [SerializeField]
    private Button logViewBtn;

    [SerializeField]
    private bool autoConnect;

    [SerializeField]
    private bool enableLogging;

    /// <summary>
    /// Connects to master
    /// </summary>
    public void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        startBtn.interactable = false;
        startBtn.onClick.RemoveAllListeners();
        if (PhotonNetwork.IsConnected)
        {
            roomDisplay.text = "Connected";
        }
        else
        {
            roomDisplay.text = "Connecting...";
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    /// <summary>
    /// Resets button states
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        print(cause);
        btnText.text = "Connect";
        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(Connect);
        leaveBtn.interactable = false;
        createRoomBtn.interactable = false;

    }

    /// <summary>
    /// Joins lobby automatically
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        roomDisplay.text = "Joining Lobby...";
    }

    /// <summary>
    /// Toggles createRoomBtn on
    /// </summary>
    public override void OnJoinedLobby()
    {
        roomDisplay.text = "Connected";
        createRoomBtn.interactable = true;
    }

    /// <summary>
    /// Creates a new room with new GUID as name
    /// </summary>
    public void CreateRoom()
    {
        roomDisplay.text = "Creating room...";
        PhotonNetwork.CreateRoom(Guid.NewGuid().ToString(), new RoomOptions() { MaxPlayers = 10 });
    }

    /// <summary>
    /// Disables leave / create buttons, displays current room info
    /// </summary>
    public override void OnJoinedRoom()
    {
        leaveBtn.interactable = true;
        createRoomBtn.interactable = false;

        RefreshStartButton();

        startBtn.onClick.AddListener(TryLoadLevel);

        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    /// <summary>
    /// Refreshes current room info
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    /// <summary>
    /// Refreshes current room info
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
        RefreshStartButton();
    }

    /// <summary>
    /// Toggle startBtn text / interactable depending on if client is master client
    /// </summary>
    private void RefreshStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            btnText.text = "Start game";
            startBtn.interactable = true;
        }
        else
        {
            btnText.text = "Only masterClient can start!";
            startBtn.interactable = false;
        }
    }

    /// <summary>
    /// Reset button states
    /// </summary>
    public override void OnLeftRoom()
    {
        leaveBtn.interactable = false;
        createRoomBtn.interactable = true;
        startBtn.interactable = false;
        btnText.text = "Connect";
    }

    /// <summary>
    /// Leaves current room, transfers master client to other player in room if possible and necessary
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerListOthers.Length > 0)
            {
                TransferMasterClient(PhotonNetwork.PlayerListOthers[0]);
            }
        }
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Transfers master client to target player
    /// </summary>
    /// <param name="target">New master client</param>
    private void TransferMasterClient(Player target)
    {
        PhotonNetwork.SetMasterClient(target);
    }

    /// <summary>
    /// Closes current room and loads the game scene
    /// </summary>
    public void TryLoadLevel()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Game");
    }

    /// <summary>
    /// Disconnects and loads the log view
    /// </summary>
    public void LoadLogView()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.AutomaticallySyncScene = false;
        }
        SceneManager.LoadScene("LogViewer");
    }

    private void OnValidate()
    {
        if (!logViewBtn) return;
        logViewBtn.gameObject.SetActive(enableLogging);
    }
}
