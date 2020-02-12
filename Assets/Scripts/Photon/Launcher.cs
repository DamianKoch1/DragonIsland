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
        //TODO rewrite this, using this when loaded from finished game
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            return;
        }
        if (autoJoin) Connect();
    }



    [SerializeField]
    private Text roomDisplay;

    private Text btnText;

    [SerializeField]
    private Button startBtn;

    [SerializeField]
    private Button logViewBtn;

    [SerializeField]
    private bool autoJoin;

    [SerializeField]
    private bool enableLogging;

    public void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        startBtn.interactable = false;
        startBtn.onClick.RemoveAllListeners();
        if (PhotonNetwork.IsConnected)
        {
            roomDisplay.text = "Finding room...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            roomDisplay.text = "Connecting...";
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print(cause);
    }

    public override void OnConnectedToMaster()
    {
        roomDisplay.text = "Finding room...";
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        roomDisplay.text = "Creating room...";
        PhotonNetwork.CreateRoom(Guid.NewGuid().ToString(), new RoomOptions() { MaxPlayers = 10 });
    }

    public override void OnJoinedRoom()
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

        startBtn.onClick.AddListener(TryLoadLevel);

        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public void TryLoadLevel()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Game");
    }

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
