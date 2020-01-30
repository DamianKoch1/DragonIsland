using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA.Logging
{
    public class Timeline : MonoBehaviour
    {
        [SerializeField]
        private GameObject eventTextPrefab;

        [SerializeField]
        private Text lastTimeText;

        private List<GameObject> evtTextInstances;

        public LogActionType displayedActions;

        float width;

        float lastTime;

        [SerializeField]
        private float minDistance = 10;

        private void Update()
        {
            //UpdateTime();           
        }

        private void UpdateTime()
        {
            float newLastTime = lastTime + Time.deltaTime;
            lastTimeText.text = new TimeSpan(0, 0, (int)newLastTime).ToString();
            foreach (var instance in evtTextInstances)
            {
                var prevPos = instance.transform.localPosition;
                var time = (prevPos.x / width) * lastTime;
                float newX = width * time / newLastTime;
                prevPos.x = newX;
                instance.transform.localPosition = prevPos;
            }
            lastTime = newLastTime;
        }

        public void ShowLog(string name)
        {
            width = GetComponent<RectTransform>().rect.width;
            evtTextInstances = new List<GameObject>();

            var logEvents = GameLogger.Load(name).ToList();
            lastTime = logEvents[logEvents.Count - 1].timeStamp;
            lastTimeText.text = new TimeSpan(0, 0, (int)lastTime).ToString();

            float prevPosX = -100000;

            string storedActions = "";
            string storedOtherNames = "";
            LogActionType prevType = 0;

            foreach (var evt in logEvents)
            {
                var type = (LogActionType)Enum.Parse(typeof(LogActionType), evt.type);
                if (displayedActions.HasFlag(type))
                {
                    var posX = width * evt.timeStamp / lastTime;
                    var distance = posX - prevPosX;
                    var evtText = Instantiate(eventTextPrefab, transform);
                    evtText.transform.localPosition += posX * Vector3.right;
                    evtTextInstances.Add(evtText);

                    if (distance < minDistance)
                    {
                        evtText.GetComponent<Text>().text = "";
                        if (prevType == type) continue;
                        if (!storedActions.Contains(evt.type))
                        {
                            storedActions += ", " + evt.type;
                        }
                        if (!string.IsNullOrEmpty(evt.otherName))
                        {
                            if (!storedOtherNames.Contains(evt.otherName))
                            {
                                storedOtherNames += ", " + evt.otherName;
                            }
                        }
                        continue;
                    }
                    prevPosX = posX;
                    prevType = type;

                    evtText.GetComponent<Text>().text = evt.ToString(storedActions, storedOtherNames);
                    storedActions = "";
                    storedOtherNames = "";
                }
            }
        }

        public void Clear()
        {
            foreach (var instance in evtTextInstances)
            {
                Destroy(instance);
            }
        }
    }
}
