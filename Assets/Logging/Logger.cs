using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Logging
{
    /// <summary>
    /// Used to put certain actions into a MongoDB database
    /// </summary>
    public static class GameLogger
    {
        private static Database database;
        private static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database();
                }
                return database;
            }
        }

        public static bool enabled = true;

        /// <summary>
        /// Creates a LogData with given arguments and saves it to the current collection
        /// </summary>
        /// <param name="source">Source Unit</param>
        /// <param name="type">Event type (move / attack / etc)</param>
        /// <param name="eventPos">Event position (usually use mouse position)</param>
        /// <param name="other">Other unit (attacked / killed / ... unit)</param>
        public static void Log(Unit source, LogActionType type, Vector3 eventPos, Unit other = null)
        {
            if (!enabled) return;
            if (!Photon.Pun.PhotonNetwork.IsMasterClient) return;
            var data = new LogData(source, type, eventPos, other);
            Database.Save(ref data);
        }

        /// <summary>
        /// Loads all LogData from collection with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<LogData> Load(string name)
        {
            if (!enabled) return null;
            return Database.Load(name);
        }

        /// <summary>
        /// Removes last saved collection
        /// </summary>
        public static void RemoveLast()
        {
            if (!enabled) return;
            Database.RemoveLast();
        }

        /// <summary>
        /// Removes all collections
        /// </summary>
        public static void Clear()
        {
            if (!enabled) return;
            Database.Clear();
        }

        /// <summary>
        /// Gets a list with available log names
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCollectionNames()
        {
            if (!enabled) return null;
            return Database.LogNames;
        }

    }
}
