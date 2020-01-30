using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA.Logging
{
    public class LogData
    {
        [BsonId]
        public ObjectId id;

        public float timeStamp;

        public string sourceName;

        public string type;

        public float eventPosX;
        public float eventPosZ;

        public string otherName;

        public LogData(Unit source, LogActionType _type, Vector3 eventPos, Unit other)
        {
            id = ObjectId.GenerateNewId();
            timeStamp = Time.timeSinceLevelLoad.Truncate(1);
            sourceName = source.gameObject.name;
            type = _type.ToString();
            eventPosX = eventPos.x.Truncate(1);
            eventPosZ = eventPos.z.Truncate(1);
            if (other)
            {
                otherName = other.gameObject.name;
            }
            else otherName = "";
        }

        public override string ToString()
        {
            return new TimeSpan(0, 0, (int)timeStamp) + " " + sourceName + " " + type + " " + new Vector2(eventPosX, eventPosZ) + " " + otherName;
        }

        public string ToString(string extraActions, string extraOtherNames)
        {
            return new TimeSpan(0, 0, (int)timeStamp) + " " + sourceName + " " + type + extraActions + " " + new Vector2(eventPosX, eventPosZ) + " " + otherName + extraOtherNames;
        }
    }

    [Flags]
    public enum LogActionType : short
    {
        none = 0,
        move = 1,
        attack = 2,
        kill = 4,
        die = 8,
        levelUp = 16,
        Q = 32,
        W = 64,
        E = 128,
        R = 256
    }
}
