using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Logging
{
    public class Database
    {
        MongoClient client;
        IMongoDatabase database;
        string gameID;

        public List<string> LogNames
        {
            private set;
            get;
        }

        /// <summary>
        /// Tries to create a MongoClient to localhost and retrieve all names of existing logs, disables logging on failure
        /// </summary>
        public Database()
        {
            var settings = MongoClientSettings.FromConnectionString("mongodb://127.0.0.1:27017");

            settings.MaxConnectionLifeTime = new TimeSpan(2, 0, 0);

            var timeout = new TimeSpan(0, 0, 0, 0, 250);
            settings.ConnectTimeout = timeout;
            settings.HeartbeatTimeout = timeout;
            settings.ServerSelectionTimeout = timeout;
            settings.SocketTimeout = timeout;
            settings.WaitQueueTimeout = timeout;

            try
            {
                client = new MongoClient(settings);
                database = client.GetDatabase("MOBA");

                LogNames = new List<string>();
                foreach (var name in database.ListCollectionNames().ToEnumerable())
                {
                    LogNames.Add(name);
                }
            }
            catch
            {
                Debug.LogWarning("Couldn't reach logging database, logging has been disabled.");
                GameLogger.enabled = false;
            }

        }

        /// <summary>
        /// Create a collection for the current game using a GUID as name
        /// </summary>
        private void SetupLogging()
        {
            if (!GameLogger.enabled) return;
            gameID = Guid.NewGuid().ToString();

            database.CreateCollection(gameID);
            LogNames.Add(gameID);
        }

        /// <summary>
        /// Saves a LogData to the current collection
        /// </summary>
        /// <param name="data"></param>
        public void Save(ref LogData data)
        {
            if (!GameLogger.enabled) return;

            if (string.IsNullOrEmpty(gameID))
            {
                SetupLogging();
            }
            var collection = database.GetCollection<LogData>(gameID);
            collection.ReplaceOne(Builders<LogData>.Filter.Eq("id", data.id), data, new UpdateOptions() { IsUpsert = true });
        }

        /// <summary>
        /// Loads all LogData from collection with given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<LogData> Load(string name)
        {
            if (!GameLogger.enabled) return null;

            var collection = database.GetCollection<LogData>(name);
            return collection.Find(Builders<LogData>.Filter.Empty).ToEnumerable();
        }

        /// <summary>
        /// Removes last saved collection
        /// </summary>
        public void RemoveLast()
        {
            if (!GameLogger.enabled) return;
            var names = database.ListCollectionNames().ToList();
            var lastName = names[names.Count - 1];
            LogNames.Remove(lastName);
            database.DropCollection(lastName);
        }

        /// <summary>
        /// Removes all collections
        /// </summary>
        public void Clear()
        {
            if (!GameLogger.enabled) return;
            foreach (var name in database.ListCollectionNames().ToEnumerable())
            {
                LogNames.Clear();
                database.DropCollection(name);
            }
        }
    }
}
