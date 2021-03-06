﻿using UnityEngine;
using EA4S.UI;

namespace EA4S.Minigames.Balloons
{
    public class TimerManager : MonoBehaviour
    {
        [HideInInspector]
        public float time;
        //public TextMeshProUGUI timerText;

        private bool isRunning;
        private float timeRemaining;


        void Update()
        {
            if (isRunning)
            {
                if (timeRemaining > 0f)
                {
                    timeRemaining -= Time.deltaTime;
                    DisplayTime();
                }

                if (timeRemaining <= 0f)
                {
                    StopTimer(true);
                    BalloonsGame.instance.OnTimeUp();
                }
            }
        }

        public void InitTimer()
        {
            time = BalloonsGame.instance.roundTime;
            if (MinigamesUI.Timer != null)
            {
                MinigamesUI.Timer.Setup(time);
            }
        }

        public void StartTimer()
        {
            isRunning = true;
            MinigamesUI.Timer.Play();
        }

        public void StopTimer(bool forceCompletion = false)
        {
            isRunning = false;
            if (MinigamesUI.Timer != null)
            {
                if (forceCompletion) MinigamesUI.Timer.Complete();
                else MinigamesUI.Timer.Pause();
            }
            //AudioManager.I.StopSfx(Sfx.DangerClockLong);
        }

        public void ResetTimer()
        {
            if (MinigamesUI.Timer == null)
            {
                return;
            }
            if (!MinigamesUI.Timer.IsSetup)
            {
                InitTimer();
            }
            StopTimer();
            timeRemaining = time;
            MinigamesUI.Timer.Rewind();
        }

        public void DisplayTime()
        {
            //textvar text = Mathf.Floor(timeRemaining).ToString();
            //timerText.text = text;
        }
    }
}