﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using EA4S.Antura;
using EA4S.Core;
using EA4S.Database;
using EA4S.Helpers;
using System.Linq;
using System.Collections;

namespace EA4S.Rewards
{
    public static class RewardSystemManager
    {

        #region CONST
        public const string ANTURA_REWARDS_CONFIG_PATH = "Configs/" + "AnturaRewardsConfig";
        public const string COLOR_PAIRS_CONFIG_PATH = "Configs/" + "ColorPairs";
        public const string ANTURA_REWARDS_PREFABS_PATH = "Prefabs/Rewards/";
        /// <summary>
        /// The maximum rewards unlockable for playsession.
        /// </summary>
        public const int MaxRewardsUnlockableForPlaysession = 2;
        #endregion

        #region Configurations
        /// <summary>
        /// The configuration
        /// </summary>
        static RewardConfig config;

        /// <summary>
        /// GetConfig if not already loaded load it from disk.
        /// </summary>
        /// <returns></returns>
        public static RewardConfig GetConfig()
        {
            if (config.Rewards == null)
                LoadFromConfig();
            return config.GetClone();
        }

        /// <summary>
        /// Gets the total rewars count. Any type with any color variation available in game.
        /// </summary>
        /// <returns></returns>
        public static int GetTotalRewardsCount() {
            int returnInt = 0;
            returnInt += GetConfig().Rewards.Count * GetConfig().RewardsColorPairs.Count;
            returnInt += GetConfig().RewardsDecal.Count * GetConfig().RewardsDecalColor.Count;
            returnInt += GetConfig().RewardsTile.Count * GetConfig().RewardsTileColor.Count;
            return returnInt;
        }

        /// <summary>
        /// Gets the unlocked reward count for actual player. 0 if acutal player is null.
        /// </summary>
        /// <returns></returns>
        public static int GetUnlockedRewardsCount() {
            if (AppManager.I.Player != null)
                return AppManager.I.Player.RewardsUnlocked.Count;
            else
                return 0;
        }

        public static RewardPackUnlockData CurrentReward = new RewardPackUnlockData();

        /// <summary>
        /// Init
        /// </summary>
        public static void Init()
        {
            LoadFromConfig();
        }

        /// <summary>
        /// Loads from configuration.
        /// </summary>
        static void LoadFromConfig()
        {
            TextAsset configData = Resources.Load(ANTURA_REWARDS_CONFIG_PATH) as TextAsset;
            string configString = configData.text;
            config = JsonUtility.FromJson<RewardConfig>(configString);

        }
        #endregion

        #region API

        #region UI Interactions

        /// <summary>
        /// Gets the reward items by rewardType (always 9 items, if not presente item in the return list is null).
        /// </summary>
        /// <param name="_rewardType">Type of the reward.</param>
        /// <param name="_parentsTransForModels">The parents trans for models.</param>
        /// <param name="_categoryRewardId">The category reward identifier.</param>
        /// <returns></returns>
        public static List<RewardItem> GetRewardItemsByRewardType(RewardTypes _rewardType, List<Transform> _parentsTransForModels, string _categoryRewardId = "")
        {
            List<RewardItem> returnList = new List<RewardItem>();
            /// TODO: logic
            /// - Load returnList by type and category checking unlocked and if exist active one
            switch (_rewardType) {
                case RewardTypes.reward:
                    // Filter from unlocked elements (only items with this category and only one for itemID)
                    List<Reward> rewards = GetConfig().GetClone().Rewards;
                    foreach (var item in rewards.FindAll(r => r.Category == _categoryRewardId)) {
                        if (AppManager.I.Player.RewardsUnlocked.FindAll(ur => ur.GetRewardCategory() == _categoryRewardId).Exists(ur => ur.ItemId == item.ID)) {
                            returnList.Add(new RewardItem() {
                                ID = item.ID,
                                IsNew = AppManager.I.Player.RewardItemIsNew(item.ID),
                                IsSelected = AppManager.I.Player.CurrentAnturaCustomizations.Fornitures.Exists(f => f.ItemId == item.ID) });
                        } else {
                            returnList.Add(null);
                        }
                    }
                    /// - Charge models
                    for (int i = 0; i < returnList.Count; i++) {
                        if (returnList[i] != null) {
                            ModelsManager.MountModel(returnList[i].ID, _parentsTransForModels[i]);
                        }
                    }
                    break;
                case RewardTypes.texture:
                    // Filter from unlocked elements (only one for itemID)
                    foreach (var item in GetConfig().RewardsTile) {
                        if (AppManager.I.Player.RewardsUnlocked.FindAll(ur => ur.Type == RewardTypes.texture).Exists(ur => ur.ItemId == item.ID)) {
                            returnList.Add(new RewardItem() {
                                ID = item.ID,
                                IsNew = AppManager.I.Player.RewardItemIsNew(item.ID),
                                IsSelected = AppManager.I.Player.CurrentAnturaCustomizations.TileTexture.ItemId == item.ID
                            });
                        } else {
                            returnList.Add(null);
                        }
                    }
                    /// - Charge texture
                    for (int i = 0; i < returnList.Count; i++) {
                        if (returnList[i] != null) {
                            string texturePath = "AnturaStuff/Textures_and_Materials/";
                            Texture2D inputTexture = Resources.Load<Texture2D>(texturePath + returnList[i].ID);
                            _parentsTransForModels[i].GetComponent<RawImage>().texture = inputTexture;
                        }
                    }
                    break;
                case RewardTypes.decal:
                    // Filter from unlocked elements (only one for itemID)
                    foreach (var item in GetConfig().RewardsDecal) {
                        if (AppManager.I.Player.RewardsUnlocked.FindAll(ur => ur.Type == RewardTypes.decal).Exists(ur => ur.ItemId == item.ID)) {
                            returnList.Add(new RewardItem() {
                                ID = item.ID,
                                IsNew = AppManager.I.Player.RewardItemIsNew(item.ID),
                                IsSelected = AppManager.I.Player.CurrentAnturaCustomizations.DecalTexture.ItemId == item.ID });
                        } else {
                            returnList.Add(null);
                        }
                    }
                    /// - Charge texture
                    for (int i = 0; i < returnList.Count; i++) {
                        if (returnList[i] != null) {
                            string texturePath = "AnturaStuff/Textures_and_Materials/";
                            Texture2D inputTexture = Resources.Load<Texture2D>(texturePath + returnList[i].ID);
                            _parentsTransForModels[i].GetComponent<RawImage>().texture = inputTexture;
                        }
                    }
                    break;
                default:
                    Debug.LogWarningFormat("Reward typology requested {0} not found", _rewardType);
                    break;
            }

            //// add empty results
            //int emptyItemsCount = _parentsTransForModels.Count - returnList.Count;
            //for (int i = 0; i < emptyItemsCount; i++) {
            //    returnList.Add(null);
            //}
            return returnList;
        }

        /// <summary>
        /// Selects the reward item.
        /// </summary>
        /// <param name="_rewardItemId">The reward item identifier.</param>
        /// <returns></returns>
        public static List<RewardColorItem> SelectRewardItem(string _rewardItemId, RewardTypes _rewardType)
        {
            List<RewardColorItem> returnList = new List<RewardColorItem>();
            /// logic
            /// - Trigger selected reward event.
            /// - Load returnList of color for reward checking unlocked and if exist active one


            switch (_rewardType) {
                case RewardTypes.reward:
                    foreach (RewardColor color in GetConfig().GetClone().RewardsColorPairs) {
                        if (AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID)) {
                            RewardColorItem rci = new RewardColorItem(color);
                            rci.IsNew = AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID && ur.IsNew == true);
                            returnList.Add(rci);
                        } else {
                            returnList.Add(null);
                        }
                    }
                    // set current reward in modification
                    CurrentReward = new RewardPackUnlockData() { ItemId = _rewardItemId, Type = RewardTypes.reward };
                    break;
                case RewardTypes.texture:
                    foreach (RewardColor color in GetConfig().RewardsTileColor) {
                        if (AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID)) {
                            RewardColorItem rci = new RewardColorItem(color);
                            rci.IsNew = AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID && ur.IsNew == true);
                            rci.Color2RGB = rci.Color1RGB; // to avoid exadecimal conversion error on ui rgb code conversion.
                            returnList.Add(rci);
                        } else {
                            returnList.Add(null);
                        }
                    }
                    // set current reward in modification
                    CurrentReward = new RewardPackUnlockData() { ItemId = _rewardItemId, Type = RewardTypes.texture };
                    break;
                case RewardTypes.decal:
                    foreach (RewardColor color in GetConfig().RewardsDecalColor) {
                        if (AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID)) {
                            RewardColorItem rci = new RewardColorItem(color);
                            rci.IsNew = AppManager.I.Player.RewardsUnlocked.Exists(ur => ur.ItemId == _rewardItemId && ur.ColorId == color.ID && ur.IsNew == true);
                            rci.Color2RGB = rci.Color1RGB; // to avoid exadecimal conversion error on ui rgb code conversion.
                            returnList.Add(rci);
                        } else {
                            returnList.Add(null);
                        }
                    }
                    //foreach (RewardColor color in config.RewardsDecalColor) {
                    //    RewardColorItem rci = new RewardColorItem(color);
                    //    rci.Color2RGB = rci.Color1RGB; // to avoid exadecimal conversion error on ui rgb code conversion.
                    //    returnList.Add(rci);
                    //}
                    // set current reward in modification
                    CurrentReward = new RewardPackUnlockData() { ItemId = _rewardItemId, Type = RewardTypes.decal };
                    break;
                default:
                    Debug.LogWarningFormat("Reward typology requested {0} not found", _rewardType);
                    break;
            }

            // Color selection
            RewardPackUnlockData alreadySelectedReward = null;
            switch (_rewardType) {
                case RewardTypes.reward:
                    List<RewardPackUnlockData> fornitures = AppManager.I.Player.CurrentAnturaCustomizations.Fornitures;
                    alreadySelectedReward = fornitures.Find(r => r.ItemId == _rewardItemId && r.Type == _rewardType);
                    break;
                case RewardTypes.texture:
                    if (AppManager.I.Player.CurrentAnturaCustomizations.TileTexture.ItemId == _rewardItemId)
                        alreadySelectedReward = AppManager.I.Player.CurrentAnturaCustomizations.TileTexture;
                    break;
                case RewardTypes.decal:
                    if (AppManager.I.Player.CurrentAnturaCustomizations.DecalTexture.ItemId == _rewardItemId)
                        alreadySelectedReward = AppManager.I.Player.CurrentAnturaCustomizations.DecalTexture;
                    break;
                default:
                    Debug.LogErrorFormat("Reward type {0} not found!", _rewardType);
                    return returnList;
            }
            
            if (alreadySelectedReward != null) {
                // if previous selected this reward use previous color...
                returnList.Find(color => color != null && color.ID == alreadySelectedReward.ColorId).IsSelected = true;
            } else {
                // ...else selecting first available color
                foreach (var firstItem in returnList) {
                    if (firstItem != null) {
                        firstItem.IsSelected = true;
                        return returnList;
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Selects the reward color item.
        /// </summary>
        /// <param name="_rewardColorItemId">The reward color item identifier.</param>
        /// <param name="_rewardType">Type of the reward.</param>
        public static void SelectRewardColorItem(string _rewardColorItemId, RewardTypes _rewardType)
        {
            CurrentReward.ColorId = _rewardColorItemId;
            if (OnRewardChanged != null)
                OnRewardChanged(CurrentReward);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_categoryRewardId"></param>
        public static void DeselectAllRewardItemsForCategory(string _categoryRewardId = "")
        {
            AnturaModelManager.I.ClearLoadedRewardInCategory(_categoryRewardId);
        }

        /// <summary>
        /// TODO: public or private?
        /// Gets the reward colors by identifier.
        /// </summary>
        /// <param name="_rewardItemId">The reward item identifier.</param>
        /// <returns></returns>
        static List<RewardColorItem> GetRewardColorsById(string _rewardItemId, RewardTypes _rewardType)
        {
            List<RewardColorItem> returnList = new List<RewardColorItem>();
            // TODO: logic
            return returnList;
        }

        #endregion

        #region General

        /// <summary>
        /// Gets the reward by identifier.
        /// </summary>
        /// <param name="_rewardId">The reward identifier.</param>
        /// <returns></returns>
        public static Reward GetRewardById(string _rewardId)
        {
            Reward reward = GetConfig().Rewards.Find(r => r.ID == _rewardId);
            return reward;
        }

        /// <summary>
        /// Get material pair from Reward id and colors name.
        /// </summary>
        /// <param name="_rewardId"></param>
        /// <param name="_color1"></param>
        /// <param name="_color2"></param>
        /// <returns></returns>
        [Obsolete("...", true)]
        public static MaterialPair GetMaterialPairFromRewardAndColor(string _rewardId, string _color1, string _color2)
        {
            Reward reward = RewardSystemManager.GetRewardById(_rewardId);
            MaterialPair mp = new MaterialPair(_color1, reward.Material1, _color2, reward.Material2);
            return mp;
        }

        /// <summary>
        /// Gets the material pair for standard reward.
        /// </summary>
        /// <param name="_rewardId">The reward identifier.</param>
        /// <param name="_colorId">The color identifier.</param>
        /// <returns></returns>
        public static MaterialPair GetMaterialPairFromRewardIdAndColorId(string _rewardId, string _colorId)
        {
            Reward reward = RewardSystemManager.GetRewardById(_rewardId);
            RewardColor color = GetConfig().GetClone().RewardsColorPairs.Find(c => c.ID == _colorId);
            if (color == null || reward == null)
                return new MaterialPair();
            MaterialPair mp = new MaterialPair(color.Color1Name, reward.Material1, color.Color2Name, reward.Material2);
            return mp;
        }

        /// <summary>
        /// Gets the unlocked reward for specified playsession.
        /// </summary>
        /// <param name="journeyPosition">The playsession identifier (format 1.4.2).</param>
        /// <returns></returns>
        public static int GetUnlockedRewardForPlaysession(JourneyPosition journeyPosition)
        {
            int rCount = AppManager.I.Player.RewardsUnlocked.FindAll(ur => ur.GetJourneyPosition().Equals(journeyPosition)).Count;
            return rCount > 2 ? 2 : rCount;
        }
        #endregion

        #region Helpers

        public static IEnumerator UnlockExtraRewards() {
            JourneyPosition extraRewardJourney = new JourneyPosition(100,100,100);
            List<RewardPackUnlockData> alreadyUnlocked = AppManager.I.Player.RewardsUnlocked;
            for (int i = 0; i < GetConfig().Rewards.Count; i++) {
                for (int y = 0; y < GetConfig().RewardsColorPairs.Count; y++) {
                    if (!alreadyUnlocked.Exists(ur => ur.ItemId == GetConfig().Rewards[i].ID && ur.ColorId == GetConfig().RewardsColorPairs[y].ID)) {
                        AppManager.I.Player.AddRewardUnlocked(
                            new RewardPackUnlockData(
                                AppManager.I.LogManager.AppSession,
                                GetConfig().Rewards[i].ID,
                                GetConfig().RewardsColorPairs[y].ID, RewardTypes.reward, extraRewardJourney));
                        yield return null;
                    }
                }
            }
            for (int i = 0; i < GetConfig().RewardsDecal.Count; i++) {
                for (int y = 0; y < GetConfig().RewardsDecalColor.Count; y++) {
                    if (!alreadyUnlocked.Exists(ur => ur.ItemId == GetConfig().RewardsDecal[i].ID && ur.ColorId == GetConfig().RewardsDecalColor[y].ID)) {
                        AppManager.I.Player.AddRewardUnlocked(
                            new RewardPackUnlockData(
                                AppManager.I.LogManager.AppSession,
                                GetConfig().RewardsDecal[i].ID,
                                GetConfig().RewardsDecalColor[y].ID, RewardTypes.decal, extraRewardJourney));
                        yield return null;
                    }
                }
            }
            for (int i = 0; i < GetConfig().RewardsTile.Count; i++) {
                for (int y = 0; y < GetConfig().RewardsTileColor.Count; y++) {
                    if (!alreadyUnlocked.Exists(ur => ur.ItemId == GetConfig().RewardsTile[i].ID && ur.ColorId == GetConfig().RewardsTileColor[y].ID)) {
                        AppManager.I.Player.AddRewardUnlocked(
                            new RewardPackUnlockData(
                                AppManager.I.LogManager.AppSession,
                                GetConfig().RewardsTile[i].ID,
                                GetConfig().RewardsTileColor[y].ID, RewardTypes.texture, extraRewardJourney));
                        yield return null;
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// Unlocks all rewards.
        /// </summary>
        public static void UnlockAllRewards()
        {

            int RewardCount = 0; int TextureCount = 0; int DecalCount = 0; int OtherCount = 0;

            var actualCurrentJourneyPosition = AppManager.I.Player.CurrentJourneyPosition;
            var allPlaySessionInfos = AppManager.I.ScoreHelper.GetAllPlaySessionInfo();

            

            for (int i = 0; i < allPlaySessionInfos.Count; i++) {
                // Check if already unlocked reward for this playSession.
                JourneyPosition journeyPosition = allPlaySessionInfos[i].data.GetJourneyPosition();
                if (RewardAlreadyUnlocked(journeyPosition))
                    continue;
                List<RewardPackUnlockData> newUnlocked = new List<RewardPackUnlockData>();
                AppManager.I.Player.SetCurrentJourneyPosition(AppManager.I.JourneyHelper.PlaySessionIdToJourneyPosition(allPlaySessionInfos[i].data.Id));
                foreach (RewardPackUnlockData pack in GetNextRewardPack()) {
                    pack.IsLocked = false;
                    //AppManager.I.Player.AddRewardUnlockedAll(pack);
                    
                    newUnlocked.Add(pack);

                    switch (pack.Type) {
                        case RewardTypes.reward:
                            RewardCount++;
                            break;
                        case RewardTypes.texture:
                            TextureCount++;
                            break;
                        case RewardTypes.decal:
                            DecalCount++;
                            break;
                        default:
                            OtherCount++;
                            break;
                    }
                    Debug.LogFormat("Unlocked reward for playsession {0} : {1}", journeyPosition, pack);
                }
                AppManager.I.Player.AddRewardUnlockedRange(newUnlocked);
            }

            
            AppManager.I.Player.SetCurrentJourneyPosition(actualCurrentJourneyPosition);
            Debug.LogFormat("Bulk unlocking rewards result: rewards: {0} | texture: {1} | decal: {2} | other: {3}", RewardCount, TextureCount, DecalCount, OtherCount);

            AppManager.I.StartCoroutine(RewardSystemManager.UnlockExtraRewards());
            Debug.LogFormat("Unlock also all extra rewards!");
            RewardSystemManager.Init();
        }

        /// <summary>
        /// Return true if Reward for this JourneyPosition is already unlocked.
        /// </summary>
        /// <param name="_journeyPosition">The journey position.</param>
        /// <returns></returns>
        public static bool RewardAlreadyUnlocked(JourneyPosition journeyPosition)
        {
            List<RewardPackUnlockData> unlocked = AppManager.I.Player.RewardsUnlocked;
            RewardPackUnlockData rewardPackUnlockData = unlocked.Find(r => r.GetJourneyPosition().Equals(journeyPosition));
            if (rewardPackUnlockData != null)
                return true;
            return false;
        }

        /// <summary>
        /// Rewards the already unlocked.
        /// </summary>
        /// <param name="_itemId">The item identifier.</param>
        /// <param name="_colorId">The color identifier.</param>
        /// <param name="_type">The type.</param>
        /// <returns></returns>
        public static bool RewardAlreadyUnlocked(string _itemId, string _colorId, RewardTypes _type)
        {
            if (AppManager.I.Player.RewardsUnlocked.Find(r => r.ItemId == _itemId && r.ColorId == _colorId && r.Type == _type) != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets the antura rotation angle view for reward category.
        /// </summary>
        /// <param name="_categoryId">The category identifier.</param>
        /// <returns></returns>
        public static float GetAnturaRotationAngleViewForRewardCategory(string _categoryId)
        {
            switch (_categoryId) {
                case "HEAD":
                    return 20;
                case "NOSE":
                    return -20;
                case "BACK":
                    return 200;
                case "NECK":
                    return 80;
                case "JAW":
                    return 30;
                case "TAIL":
                    return 160;
                case "EAR_R":
                    return -40;
                case "EAR_L":
                    return 40;
                default:
                    return 0;
            }
        }
        #endregion

        #endregion

        #region RewardAI 
        /// <summary>
        /// Return list of right rewards for actual playsession if not already.
        /// </summary>
        /// <returns></returns>
        public static List<RewardPackUnlockData> GetNextRewardPack(bool _forceToReturnReward = false)
        {
            List<RewardPackUnlockData> returnList = new List<RewardPackUnlockData>();
            var journeyPosition = AppManager.I.Player.CurrentJourneyPosition;
            // not _forceToReturnReward check if reward is already unlocked for this playsession and if true return empty list
            if (RewardAlreadyUnlocked(journeyPosition) && !_forceToReturnReward)
                return returnList;
            // What kind of reward it is?
            PlaySessionRewardUnlock unlock = GetConfig().PlaySessionRewardsUnlock.Find(r => r.PlaySession == journeyPosition.ToStringId());
            if (unlock == null) {
                Debug.LogErrorFormat("Unable to find reward type for this playsession {0}", journeyPosition);
            }
            // -- check kind
            if (unlock.RewardColor != string.Empty) {
                // Get reward color for reward item already unlocked
                for (int i = 0; i < int.Parse(unlock.RewardColor); i++) {
                    returnList.Add(GetRewardPack(journeyPosition, RewardTypes.reward, false));
                }
            } else if (unlock.Reward != string.Empty) {
                // Get new reward item with random color
                RewardPackUnlockData newItemReward = GetRewardPack(journeyPosition, RewardTypes.reward, true);
                if (OnNewRewardUnlocked != null)
                    OnNewRewardUnlocked(newItemReward);
                returnList.Add(newItemReward);
            } else if (unlock.Texture != string.Empty) {
                // Get new texture
                returnList.Add(GetRewardPack(journeyPosition, RewardTypes.texture, true));
            } else if (unlock.Decal != string.Empty) {
                // Get new decal
                returnList.Add(GetRewardPack(journeyPosition, RewardTypes.decal, true));
            }

            ////////////////////////////////////////////////////
            return returnList;
        }

        /// <summary>
        /// Gets the next reward pack. Contains all logic to create new reward.
        /// </summary>
        /// <param name="_rewardType">Type of the reward.</param>
        /// <returns></returns>
        public static RewardPackUnlockData GetRewardPack(JourneyPosition journeyPosition, RewardTypes _rewardType, bool _random)
        {
            /// TODOs:
            /// - Filter without already unlocked items
            /// - Automatic select reward type by situation
            RewardPackUnlockData rp = new RewardPackUnlockData();
            string itemId = string.Empty;
            RewardColor color = null;
            bool alreadyUnlocked = false;
            List<RewardPackUnlockData> alreadyUnlockedRewardOfType = AppManager.I.Player.RewardsUnlocked.FindAll(r => r.Type == _rewardType);
            switch (_rewardType) {
                case RewardTypes.reward:
                    int countAvoidInfiniteLoop = 300;
                    // If not random take id from list of already unlocked rewards of this type

                    if (_random) { // Need to create new reward and first color pair
                        bool duplicated = false;
                        do {
                            //int count = AppManager.I.Player.GetNotYetUnlockedRewardCountForType(_rewardType);
                            List<Reward> availableItems = GetConfig().Rewards;
                            // Try fix for #421
                            List<Reward> itemsForRandomSelection = availableItems.Where(r => !alreadyUnlockedRewardOfType.Exists(ur => ur.ItemId == r.ID)).ToList();
                            if (itemsForRandomSelection.Count == 0) {
                                duplicated = true;
                                continue;
                            }
                            // ----------------
                            itemId = itemsForRandomSelection.GetRandomAlternative().ID;
                            color = GetConfig().RewardsColorPairs.GetRandomAlternative();
                            List<RewardPackUnlockData> unlocked = AppManager.I.Player.RewardsUnlocked;
                            duplicated = unlocked.Find(r => r.ItemId == itemId) != null;
                            if (duplicated)
                                Debug.LogFormat("Reward {0} already unlocked! Retry!", itemId);
                            countAvoidInfiniteLoop--;
                            if(countAvoidInfiniteLoop == 0)
                                Debug.LogFormat("-------------- Reward {0} infinite loop!!!!", itemId);
                        } while (duplicated && countAvoidInfiniteLoop > 0);
                        //} while (duplicated && AppManager.I.Player.RewardForTypeAvailableYet(_rewardType) || countAvoidInfiniteLoop < 1) ;
                    } else { // need only to create new color pair for one of already unlocked reward
                        color = null;
                        List<RewardPackUnlockData> alreadyUnlockeds = AppManager.I.Player.RewardsUnlocked.Where(r => r.Type == RewardTypes.reward).ToList();
                        List<string> availableRewardIds = new List<string>();
                        foreach (var reward in alreadyUnlockeds) {
                            if (!availableRewardIds.Contains(reward.ItemId)) {
                                int alreadyUnlockedsCount = alreadyUnlockeds.Where(r => r.ItemId == reward.ItemId).ToList().Count;
                                if (alreadyUnlockedsCount < RewardSystemManager.GetConfig().RewardsColorPairs.Count) {// TODO: quick fix
                                    availableRewardIds.Add(reward.ItemId);
                                }
                            }
                        } 
                        if(availableRewardIds.Count < 1) { 
                            Debug.LogWarning("No rewards with lockable color variation. Unable to unlock new color variation.");
                            return null;
                        }
                        itemId = availableRewardIds.GetRandomAlternative();
                        List<RewardColor> availableColors = GetConfig().RewardsColorPairs;
                        //availableColors.Where(r => !alreadyUnlockeds.Exists(ur => ur.ItemId == itemId && ur.ColorId == r.ID));
                        color = availableColors.Where(r => !alreadyUnlockeds.Exists(ur => ur.ItemId == itemId && ur.ColorId == r.ID)).ToList().GetRandomAlternative();
                    }
                    rp = new RewardPackUnlockData(AppManager.I.LogManager.AppSession, itemId, color.ID, _rewardType, journeyPosition);
                    break;
                case RewardTypes.texture:
                    do {
                        itemId = GetConfig().RewardsTile.GetRandomAlternative().ID;
                        color = GetConfig().RewardsTileColor.GetRandomAlternative();
                        alreadyUnlocked = RewardAlreadyUnlocked(itemId, color.ID, _rewardType);
                    } while (alreadyUnlocked);
                    rp = new RewardPackUnlockData(AppManager.I.LogManager.AppSession, itemId, color.ID, _rewardType, journeyPosition);
                    break;
                case RewardTypes.decal:
                    do {
                        itemId = GetConfig().RewardsDecal.GetRandomAlternative().ID;
                        color = GetConfig().RewardsDecalColor.GetRandomAlternative();
                        alreadyUnlocked = RewardAlreadyUnlocked(itemId, color.ID, _rewardType);
                    } while (alreadyUnlocked);
                    rp = new RewardPackUnlockData(AppManager.I.LogManager.AppSession, itemId, color.ID, _rewardType, journeyPosition);
                    break;
                default:
                    break;
            }
            return rp;
        }

        /// <summary>
        /// Unlocks the first set of rewards for current player.
        /// </summary>
        public static void UnlockFirstSetOfRewards(Profile.PlayerProfile _player = null) {
            if (_player == null) {
                if (AppManager.I.Player == null) {
                    Debug.LogError("No current player available!");
                    return;
                }
            }
            _player = AppManager.I.Player;

            _player.ResetRewardsUnlockedData();
            _player.AddRewardUnlocked(GetFirstAnturaReward(RewardTypes.reward));
            // decal
            RewardPackUnlockData defaultDecal = GetFirstAnturaReward(RewardTypes.decal);
            _player.AddRewardUnlocked(defaultDecal);
            // force to to wear decal
            _player.CurrentAnturaCustomizations.DecalTexture = defaultDecal;
            _player.CurrentAnturaCustomizations.DecalTextureId = defaultDecal.GetIdAccordingToDBRules();
            // texture
            RewardPackUnlockData defaultTexture = GetFirstAnturaReward(RewardTypes.texture);
            _player.AddRewardUnlocked(defaultTexture);
            // force to to wear texture
            _player.CurrentAnturaCustomizations.TileTexture = defaultTexture;
            _player.CurrentAnturaCustomizations.TileTexture.Id = defaultTexture.GetIdAccordingToDBRules();
            // Add all 3 rewards
            //_player.AddRewardUnlockedAll();
            // Save actual customization
            _player.SaveCustomization();
        }

        /// <summary>
        /// Gets the first antura reward.
        /// </summary>
        /// <param name="_rewardType">Type of the reward.</param>
        /// <returns></returns>
        public static RewardPackUnlockData GetFirstAnturaReward(RewardTypes _rewardType)
        {
            RewardPackUnlockData rp = new RewardPackUnlockData();
            switch (_rewardType) {
                case RewardTypes.reward:
                    rp = GetRewardPack(new JourneyPosition(0, 0, 0), _rewardType, true);
                    break;
                case RewardTypes.texture:
                    rp = new RewardPackUnlockData(AppManager.I.LogManager.AppSession, "Antura_wool_tilemat", "color1", _rewardType, new JourneyPosition(0, 0, 0));
                    rp.IsNew = false; // Because is automatically selected
                    break;
                case RewardTypes.decal:
                    rp = new RewardPackUnlockData(AppManager.I.LogManager.AppSession, "Antura_decalmap01", "color1", _rewardType, new JourneyPosition(0, 0, 0));
                    rp.IsNew = false; // Because is automatically selected
                    break;
                default:
                    break;
            }
            return rp;
        }
        #endregion

        #region Events

        public delegate void RewardSystemEventHandler(RewardPackUnlockData rewardPackUnlockData);

        /// <summary>
        /// Occurs when [on reward item changed].
        /// </summary>
        public static event RewardSystemEventHandler OnRewardChanged;

        public static event RewardSystemEventHandler OnNewRewardUnlocked;
        #endregion
    }


    #region rewards data structures

    #region static DB

    [Serializable]
    public class RewardConfig
    {
        public List<Reward> Rewards;
        public List<RewardColor> RewardsColorPairs;
        public List<RewardDecal> RewardsDecal;
        public List<RewardColor> RewardsDecalColor;
        public List<RewardTile> RewardsTile;
        public List<RewardColor> RewardsTileColor;
        public List<PlaySessionRewardUnlock> PlaySessionRewardsUnlock;

        public RewardConfig GetClone() {
            return MemberwiseClone() as RewardConfig;
        }
    }

    [Serializable]
    public class PlaySessionRewardUnlock
    {
        public string PlaySession;
        public string RewardColor;
        public string Reward;
        public string Texture;
        public string Decal;
    }

    [Serializable]
    public class Reward
    {
        public string ID;
        public string RewardName;
        public string BoneAttach;
        public string Material1;
        public string Material2;
        public string Category;
        public string RemTongue;
    }

    [Serializable]
    public class RewardColor
    {
        public string ID;
        public string Color1Name;
        public string Color2Name;
        public string Color1RGB; // "rrggbbaa"
        public string Color2RGB; // "rrggbbaa"
    }

    [Serializable]
    public class RewardDecal
    {
        public string ID;
    }

    [Serializable]
    public class RewardTile
    {
        public string ID;
    }

    #endregion

    #region Dynamic DB



    /*
    /// <summary>
    /// Class structure to identify reward pack used as price in game.
    /// </summary>
    [Serializable]
    public class RewardPackUnlockData
    {
        public string ItemId;
        public string ColorId;
        public RewardTypes Type;
        /// <summary>
        /// The play session id where this reward is assigned.
        /// </summary>
        public string JourneyPosition;
        /// <summary>
        /// The order of playsession rewards in case of multi reward for same playsession.
        /// </summary>
        public int Order = 0;
        /// <summary>
        /// True if nevere used by player.
        /// </summary>
        public bool IsNew = true;

        public MaterialPair GetMaterialPair() {
            return RewardSystemManager.GetMaterialPairFromRewardIdAndColorId(ItemId, ColorId);
        }

        public Reward GetReward() {
            if (Type != RewardTypes.reward)
                return null;
            return RewardSystemManager.GetConfig().Rewards.Find(r => r.ID == ItemId);
        }

        public string GetRewardCategory() {
            if (Type != RewardTypes.reward)
                return string.Empty;
            Reward reward = RewardSystemManager.GetConfig().Rewards.Find(r => r.ID == ItemId);
            if (reward != null)
                return reward.Category;
            return string.Empty;
        }

        public override string ToString() {
            return string.Format("{0} : {1} [{2}] [{3}]", ItemId, ColorId, Type, JourneyPosition);
        }
    }*/

    #endregion

    #region reward UI data structures

    /// <summary>
    /// Structure focused to comunicate about items from e to UI.
    /// </summary>
    public class RewardItem
    {
        public string ID;
        public bool IsSelected;
        public bool IsNew;
    }

    /// <summary>
    /// Structure focused to comunicate about colors from e to UI.
    /// </summary>
    /// <seealso cref="RewardColor" />
    public class RewardColorItem : RewardColor
    {
        public bool IsSelected;
        public bool IsNew = true;
        public RewardColorItem() { }
        public RewardColorItem(RewardColor _color)
        {
            ID = _color.ID;
            Color1Name = _color.Color1Name;
            Color1RGB = _color.Color1RGB;
            Color2Name = _color.Color2Name;
            Color2RGB = _color.Color2RGB;
        }

    }

    #endregion

    #endregion
}