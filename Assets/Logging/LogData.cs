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
    }

    public enum LogActionType
    {
        invalid = -1,
        move = 0,
        attack = 1,
        kill = 2,
        die = 3,
        levelUp = 4,
        Q = 5,
        W = 6,
        E = 7,
        R = 8
    }
}
