﻿using EA4S.Core;
using System.Linq;
using UnityEngine;

namespace EA4S.Rewards
{
    /// <summary>
    /// Manager for the Play Session Result scene.
    /// Accessed a play session is completed.
    /// </summary>
    public class PlaySessionResultManager : SceneBase
    {
        protected override void Start()
        {
            base.Start();

            // Calculate items to unlock count
            var itemsToUnlock = AppManager.I.NavigationManager.CalculateUnlockItemCount();
            var earnedStars = AppManager.I.NavigationManager.CalculateStarsCount();

            var oldRewards = AppManager.I.Player.RewardsUnlocked
                .Where(ru => ru.GetJourneyPosition().Equals(AppManager.I.Player.CurrentJourneyPosition)).ToList();
            var itemAlreadyUnlocked = oldRewards.Count;
            for (var i = 0; i < itemsToUnlock - itemAlreadyUnlocked; i++) {
                // if necessary add one new random reward unlocked
                var newRewardToUnlock = RewardSystemManager.GetNextRewardPack(true)[0];
                oldRewards.Add(newRewardToUnlock);
                AppManager.I.Player.AddRewardUnlocked(newRewardToUnlock);
            }

            // Show UI result and unlock transform parent where show unlocked items
            var objs = GameResultUI.ShowEndsessionResult(AppManager.I.NavigationManager.UseEndSessionResults(), itemAlreadyUnlocked);

            for (var i = 0; i < objs.Length - oldRewards.Count; i++) {
                // if necessary add one new random reward not to be unlocked!
                oldRewards.Add(RewardSystemManager.GetNextRewardPack(true)[0]);
            }

            LogManager.I.LogPlaySessionScore(AppManager.I.JourneyHelper.GetCurrentPlaySessionData().Id, objs.Length);
            AppManager.I.Teacher.logAI.UnlockVocabularyDataForJourneyPosition(AppManager.I.Player.CurrentJourneyPosition);
            // save max progression (internal check if necessary)
            if (earnedStars > 0) {
                // only if earned at least one star
                AppManager.I.Player.AdvanceMaxJourneyPosition();
            }

            // for any rewards mount them model on parent transform object (objs)
            for (int i = 0; i < oldRewards.Count && i < objs.Length; i++) {
                ModelsManager.MountModel(
                    oldRewards[i].ItemId,
                    objs[i].transform,
                    oldRewards[i].GetMaterialPair()
                );
            }
        }
    }
}