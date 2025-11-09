using System;
using TMPro;
using UnityEngine;
using Utilities;

namespace TimeSystem
{
    public class TimeController : MonoBehaviour
    {
        #region Time Controller

        private bool isTimeRunning;
        private float time;     // 2 seconds = 1 minute
        private TimeSpan initialTimeSpan = new(0, 7, 0); 
        private float decreaseRate;

        private const float SecondsPerMinute = 2;
        private float secondsPerMinuteTimer;

        private int initialTime;
        private int finalTime;

        #endregion

        #region Time View

        [SerializeField] private GameObject timePanel;
        [SerializeField] private TMP_Text timeText;

        #endregion

        private void Awake()
        {
            isTimeRunning = false;
        }

        private void Start()
        {
            Actions.OnStartTimeAction += InitializeTime;
            Actions.OnIncreaseTimeAction += IncreaseTime;
            Actions.OnDecreaseTimeAction += DecreaseTime;
            
            UpdateTimeText();
        }

        private void InitializeTime()
        {
            isTimeRunning = true;
            initialTime = 0;
            finalTime = 240;

            secondsPerMinuteTimer = 0f;

            initialTimeSpan = new TimeSpan(0, 7, 0);
            time = initialTime;
            timePanel.SetActive(true);
        }

        private void IncreaseTime(int value)
        {
            time -= value;
            UpdateTimeText();
        }

        private void DecreaseTime(int value)
        {
            time += value;
            UpdateTimeText();
        }
        
        public int GetTime() => Mathf.FloorToInt(time);
        private void FixedUpdate()
        {
            UpdateTime();
        }
        
        private void UpdateTime()
        {
            if (!isTimeRunning) return;
            
            if (secondsPerMinuteTimer < SecondsPerMinute)
            {
                secondsPerMinuteTimer += Time.fixedDeltaTime; 
                return;
            }
            time += 1f;
            secondsPerMinuteTimer = 0f;
            
            UpdateTimeText();
            
            if(time < finalTime) return;
            isTimeRunning = false;
            Actions.OnStopTimeAction.Invoke();
        }

        #region Time View

        private void UpdateTimeText()
        {
            var timeSpan = TimeSpan.FromSeconds(time);
            var totalTimeSpan = initialTimeSpan + timeSpan;
            var formattedTime = $"{totalTimeSpan.Minutes:D2}:{totalTimeSpan.Seconds:D2}";
            timeText.text = formattedTime;
        }

        #endregion

        private void OnDestroy()
        {
            Actions.OnStartTimeAction -= InitializeTime;
            Actions.OnIncreaseTimeAction -= IncreaseTime;
            Actions.OnDecreaseTimeAction -= DecreaseTime;
        }

        [ContextMenu("Start Time")]
        public void StartTime()
        {
            Actions.OnStartTimeAction.Invoke();
        }
    }
}
