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

        private void Start()
        {
           // gameObject.SetActive(false);
        }

        public void ShowLog(string name)
        {
            evtTextInstances = new List<GameObject>();

            var logEvents = GameLogger.Load(name).ToList();
            float lastTime = logEvents[logEvents.Count - 1].timeStamp;
            lastTimeText.text = new TimeSpan(0, 0, (int)lastTime).ToString();

            foreach (var evt in logEvents)
            {
                var type = (LogActionType)Enum.Parse(typeof(LogActionType), evt.type);
                switch (type)
                {
                    case LogActionType.invalid:
                        break;
                    //case LogActionType.move:
                    //    break;
                    //case LogActionType.attack:
                    //    break;
                    //case LogActionType.kill:
                    //    break;
                    //case LogActionType.die:
                    //    break;
                    //case LogActionType.levelUp:
                    //    break;
                    //case LogActionType.Q:
                    //    break;
                    //case LogActionType.W:
                    //    break;
                    //case LogActionType.E:
                    //    break;
                    //case LogActionType.R:
                    //    break;
                    default:
                        var evtText = Instantiate(eventTextPrefab, transform);
                        evtText.transform.localPosition += GetComponent<RectTransform>().rect.width * Vector3.right * evt.timeStamp / lastTime;
                        evtText.GetComponent<Text>().text = evt.ToString();
                        evtTextInstances.Add(evtText);
                        break;
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
