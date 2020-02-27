using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Shows all rooms in the lobby
    /// </summary>
    public class RoomList : MonoBehaviourPunCallbacks
    {

        [SerializeField, Tooltip("Room views spawn as childs of this")]
        private Transform roomViewParent;

        [SerializeField]
        private RoomView roomViewPrefab;

        private List<RoomView> roomViews = new List<RoomView>();

        /// <summary>
        /// Toggles room list off
        /// </summary>
        public override void OnJoinedRoom()
        {
            roomViewParent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Toggles room list on
        /// </summary>
        public override void OnLeftRoom()
        {
            roomViewParent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Clears room list because we didn't receive room list updates while in room
        /// </summary>
        public override void OnJoinedLobby()
        {
            Clear();
        }

        /// <summary>
        /// Clears the room list
        /// </summary>
        /// <param name="cause"></param>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Clear();
        }

        /// <summary>
        /// Updates respective room view for updated rooms
        /// </summary>
        /// <param name="roomList">Updated rooms</param>
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (var room in roomList)
            {
                OnRoomUpdate(room);
            }
        }

        /// <summary>
        /// Destroys view if room is removed from list, otherwise updates info displayed, creates a new view if necessary
        /// </summary>
        /// <param name="room"></param>
        private void OnRoomUpdate(RoomInfo room)
        {
            var existingView = roomViews.Find(r => r.RoomName == room.Name);
            if (room.RemovedFromList)
            {
                if (existingView)
                {
                    RemoveRoomView(existingView);
                }
            }
            else
            {
                if (existingView)
                {
                    RefreshView(existingView, room);
                }
                else
                {
                    AddNewView(room);
                }
            }
        }

        /// <summary>
        /// Removes a given view from saved roomViews and destroys it
        /// </summary>
        /// <param name="view"></param>
        private void RemoveRoomView(RoomView view)
        {
            roomViews.Remove(view);
            Destroy(view.gameObject);
        }

        /// <summary>
        /// Refreshes given view using given RoomInfo
        /// </summary>
        /// <param name="view">RoomView to refresh</param>
        /// <param name="room">RoomInfo to display</param>
        private void RefreshView(RoomView view, RoomInfo room)
        {
            view.Refresh(room);
        }

        /// <summary>
        /// Spawns a new view displaying the given RoomInfo
        /// </summary>
        /// <param name="room">RoomInfo to display</param>
        private void AddNewView(RoomInfo room)
        {
            var newView = Instantiate(roomViewPrefab.gameObject, roomViewParent).GetComponent<RoomView>();
            newView.Refresh(room);
            roomViews.Add(newView);
        }

        /// <summary>
        /// Destroys all roomViews
        /// </summary>
        private void Clear()
        {
            while (roomViews.Count != 0)
            {
                RemoveRoomView(roomViews[0]);
            }
        }
    }
}
