using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MOBA.Logging
{
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

        public void LoadLobby()
        {
            SceneManager.LoadScene("Lobby");
        }

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

        public void ShowLogOnTimeline(string name)
        {
            logButtonParent.gameObject.SetActive(false);
            timeline.gameObject.SetActive(true);
            timeline.ShowLog(name);
        }

        public void HideTimeline()
        {
            logButtonParent.gameObject.SetActive(true);
            timeline.gameObject.SetActive(false);
            timeline.Clear();
        }
    }
}
