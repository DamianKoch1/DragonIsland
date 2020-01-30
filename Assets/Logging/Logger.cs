using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Logging
{
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

        public static void Log(Unit source, LogActionType type, Vector3 mousePos, Unit other = null)
        {
            if (!enabled) return;
            var data = new LogData(source, type, mousePos, other);
            Database.Save(ref data);
        }

        public static IEnumerable<LogData> Load(string name)
        {
            if (!enabled) return null;
            return Database.Load(name);
        }

        public static void RemoveLast()
        {
            if (!enabled) return;
            Database.RemoveLast();
        }

        public static void Clear()
        {
            if (!enabled) return;
            Database.Clear();
        }

        public static List<string> GetCollectionNames()
        {
            if (!enabled) return null;
            return Database.LogNames;
        }

    }
}
