using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MOBA.Logging
{
    /// <summary>
    /// Used to view all available logs or show one detailed
    /// </summary>
    public class LogDisplay : MonoBehaviour
    {
        [SerializeField]
        private int maxDisplayedCount = 40;

        [SerializeField]
        private GridLayoutGroup logButtonParent;

        [SerializeField]
        private GameObject logButtonPrefab;

        [SerializeField]
        private Timeline timeline;

        [ContextMenu("Remove last log")]
        public void RemoveLastLog()
        {
            GameLogger.RemoveLast();
        }

        [ContextMenu("Clear logs")]
        public void ClearLogs()
        {
            GameLogger.Clear();
        }

        /// <summary>
        /// Returns to lobby 
        /// </summary>
        public void LoadLobby()
        {
            SceneManager.LoadScene("Lobby");
        }

        /// <summary>
        /// Instantiates a button for each existing log
        /// </summary>
        private void Start()
        {
            if (!GameLogger.enabled) return;
            int instantiatedButtonsCount = 0;
            foreach (var logName in GameLogger.GetCollectionNames())
            {
                instantiatedButtonsCount++;
                var btn = Instantiate(logButtonPrefab, logButtonParent.transform);
                btn.GetComponentInChildren<Text>().text = logName;
                btn.GetComponent<Button>().onClick.AddListener( () => ShowLogOnTimeline(logName) );
                if (instantiatedButtonsCount >= maxDisplayedCount) return;
            }
        }

        /// <summary>
        /// Shows the log with given name on a timeline
        /// </summary>
        /// <param name="name"></param>
        public void ShowLogOnTimeline(string name)
        {
            logButtonParent.gameObject.SetActive(false);
            timeline.gameObject.SetActive(true);
            timeline.ShowLog(name);
        }

        /// <summary>
        /// Hides the timeline, shows the log buttons again
        /// </summary>
        public void HideTimeline()
        {
            logButtonParent.gameObject.SetActive(true);
            timeline.gameObject.SetActive(false);
            timeline.Clear();
        }
    }
}
