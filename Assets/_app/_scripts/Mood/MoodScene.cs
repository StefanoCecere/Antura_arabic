﻿using EA4S.Audio;
using EA4S.Core;
using EA4S.UI;
using UnityEngine;

namespace EA4S.Scenes
{
    /// <summary>
    /// Manager for the Mood scene.
    /// </summary>
    public class MoodScene : SceneBase
    {

        protected override void Start()
        {
            base.Start();
            GlobalUI.ShowPauseMenu(false);

            if ((AppManager.I.Player.CurrentJourneyPosition.PlaySession) < 2) {
                KeeperManager.I.PlayDialog(Database.LocalizationDataId.Mood_Question_2);
            } else {
                int rnd = Random.Range(1, 3);
                switch (rnd) {
                    case 1:
                        KeeperManager.I.PlayDialog(Database.LocalizationDataId.Mood_Question_1);
                        break;
                    case 3:
                        KeeperManager.I.PlayDialog(Database.LocalizationDataId.Mood_Question_3);
                        break;
                    default:
                        KeeperManager.I.PlayDialog(Database.LocalizationDataId.Mood_Question_2);
                        break;
                }
            }
        }

        /// <summary> 
        /// Mood selected. Values 0,1,2,3,4.
        /// </summary>
        /// <param name="_mood"></param>
        public void MoodSelected(int _mood)
        {
            LogManager.I.LogMood(_mood);
            AudioManager.I.PlaySound(Sfx.UIButtonClick);
            Invoke("exitScene", 0.5f);
        }

        void exitScene()
        {
            AppManager.I.Player.Save();
            AppManager.I.NavigationManager.GoToNextScene();
        }
    }
}