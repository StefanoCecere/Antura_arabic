﻿using System.Collections.Generic;
using EA4S.Core;
using EA4S.Database;
using EA4S.Rewards;
using UnityEngine;

namespace EA4S.Profile
{
    /// <summary>
    /// Handles the creation, selection, and deletion of player profiles.
    /// </summary>
    public class PlayerProfileManager
    {
        #region Current Player

        private PlayerProfile _currentPlayer;

        /// <summary>
        /// The player that is currently playing.
        /// </summary>
        public PlayerProfile CurrentPlayer {
            get { return _currentPlayer; }
            set {
                if (_currentPlayer != value)
                {
                    AppManager.I.Player = _currentPlayer = value;
                    AppManager.I.Teacher.SetPlayerProfile(value);
                    // TODO refactor: make this part more clear, better create a SetCurrentPlayer() method for this!
                    if (AppManager.I.DB.HasLoadedPlayerProfile()) {
                        LogManager.I.LogInfo(InfoEvent.AppSessionEnd, "{\"AppSession\":\"" + LogManager.I.AppSession + "\"}");
                    }
                    AppManager.I.AppSettings.LastActivePlayerUUID = value.Uuid;
                    AppManager.I.AppSettingsManager.SaveSettings();
                    LogManager.I.LogInfo(InfoEvent.AppSessionStart, "{\"AppSession\":\"" + LogManager.I.AppSession + "\"}");
                    AppManager.I.NavigationManager.InitPlayerNavigationData(_currentPlayer);

                    _currentPlayer.LoadRewardsUnlockedFromDB(); // refresh list of unlocked rewards
                    if (OnProfileChanged != null)
                        OnProfileChanged();
                }
                _currentPlayer = value;
            }
        }

        #endregion

        #region Player UUID

        /// <summary>
        /// Sets the player as current player profile loading from db by UUID.
        /// </summary>
        /// <param name="playerUUID">The player UUID.</param>
        /// <returns></returns>
        public PlayerProfile SetPlayerAsCurrentByUUID(string playerUUID)
        {
            PlayerProfile returnProfile = GetPlayerProfileByUUID(playerUUID);
            AppManager.I.PlayerProfileManager.CurrentPlayer = returnProfile;
            return returnProfile;
        }

        /// <summary>
        /// Gets the player profile from db by UUID.
        /// </summary>
        /// <param name="playerUUID">The player UUID.</param>
        /// <returns></returns>
        public PlayerProfile GetPlayerProfileByUUID(string playerUUID)
        {
            PlayerProfileData profileFromDB = AppManager.I.DB.LoadDatabaseForPlayer(playerUUID);

            // If null, the player does not exist.
            // The DB got desynced. Remove this player!
            if (profileFromDB == null)
            {
                Debug.LogError("ERROR: no profile data for player UUID " + playerUUID);
            }

            return new PlayerProfile().FromData(profileFromDB);
        }
        #endregion

        #region Settings        

        /// <summary>
        /// Reloads all the settings and, optionally, the current player
        /// TODO: rebuild database only for desynchronized profile
        /// </summary>
        public void LoadSettings(bool alsoLoadCurrentPlayerProfile = true)
        {
            AppManager.I.AppSettingsManager.LoadSettings();

            if (alsoLoadCurrentPlayerProfile)
            {
                // No last active? Get the first one.
                if (AppManager.I.AppSettings.LastActivePlayerUUID == string.Empty) {
                    if (AppManager.I.AppSettings.SavedPlayers.Count > 0) {
                        //UnityEngine.Debug.Log("No last! Get the first.");
                        AppManager.I.AppSettings.LastActivePlayerUUID = AppManager.I.AppSettings.SavedPlayers[0].Uuid;
                    } else {
                        AppManager.I.Player = null;
                        Debug.Log("Actual Player == null!!");
                    }
                } else {
                    string playerUUID = AppManager.I.AppSettings.LastActivePlayerUUID;

                    // Check whether the SQL DB is in-sync first
                    PlayerProfileData profileFromDB = AppManager.I.DB.LoadDatabaseForPlayer(playerUUID);

                    // If null, the player does not actually exist.
                    // The DB got desyinced. Do not load it!
                    if (profileFromDB != null) {
                        //UnityEngine.Debug.Log("DB in sync! OK!");
                        SetPlayerAsCurrentByUUID(playerUUID);
                    } else {
                        //UnityEngine.Debug.Log("DB OUT OF SYNC. RESET");
                        ResetEverything();
                        LoadSettings();
                    }
                }
            }
        }

        #endregion

        #region Players Icon Data

        /// <summary>
        /// Return the list of existing player profiles.
        /// </summary>
        /// <returns></returns>
        public List<PlayerIconData> GetPlayersIconData()
        {
            return AppManager.I.AppSettings.SavedPlayers;
        }

        /// <summary>
        /// Updates the PlayerIconData for current player in list of PlayersIconData in GameSettings.
        /// </summary>
        public void UpdateCurrentPlayerIconDataInSettings()
        {
            for (int i = 0; i < AppManager.I.AppSettings.SavedPlayers.Count; i++)
            {
                if (AppManager.I.AppSettings.SavedPlayers[i].Uuid == _currentPlayer.Uuid)
                {
                    AppManager.I.AppSettings.SavedPlayers[i] = CurrentPlayer.GetPlayerIconData();
                }
            }
            AppManager.I.AppSettingsManager.SaveSettings();
        }

        #endregion

        #region Player Profile Creation

        /// <summary>
        /// Creates the player profile.
        /// </summary>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="avatarID">The avatar identifier.</param>
        /// <param name="tint">The color.</param>
        /// <returns></returns>
        public string CreatePlayerProfile(int age, PlayerGender gender, int avatarID, PlayerTint tint, bool isDemoUser = false)
        {
            PlayerProfile returnProfile = new PlayerProfile();
            // Data
            returnProfile.Uuid = System.Guid.NewGuid().ToString();
            returnProfile.Age = age;
            returnProfile.Gender = gender;
            returnProfile.AvatarId = avatarID;
            returnProfile.Tint = tint;
            returnProfile.IsDemoUser = isDemoUser;
            returnProfile.ProfileCompletion = (isDemoUser ? ProfileCompletionState.GameCompletedAndFinalShowed : ProfileCompletionState.New);

            // DB Creation
            AppManager.I.DB.CreateDatabaseForPlayer(returnProfile.ToData());
            // Added to list
            AppManager.I.AppSettings.SavedPlayers.Add(returnProfile.GetPlayerIconData());
            // Set player profile as current player
            AppManager.I.PlayerProfileManager.CurrentPlayer = returnProfile;
            // Create new Antura skin
            RewardSystemManager.UnlockFirstSetOfRewards();

            // Call Event Profile creation
            if (OnNewProfileCreated != null)
                OnNewProfileCreated();

            return returnProfile.Uuid;
        }

        #endregion

        #region Player Profile Save/Load

        /// <summary>
        /// Saves the player profile.
        /// </summary>
        /// <param name="_playerProfile">The player profile.</param>
        public void SavePlayerProfile(PlayerProfile _playerProfile)
        {
            AppManager.I.DB.UpdatePlayerProfileData(_playerProfile.ToData());
        }

        #endregion

        #region Player Profile Deletion

        /// <summary>
        /// Deletes the player profile.
        /// </summary>
        /// <param name="playerUUID">The player UUID.</param>
        /// <returns></returns>
        public PlayerProfile DeletePlayerProfile(string playerUUID)
        {
            PlayerProfile returnProfile = new PlayerProfile();
            // it prevents errors if rewards unlock coroutine is still running
            AppManager.I.StopAllCoroutines();
            // TODO: check if is necessary to hard delete DB
            PlayerIconData playerIconData = GetPlayersIconData().Find(p => p.Uuid == playerUUID);
            if (playerIconData.Uuid == string.Empty)
                return null;
            // if setted as active player in gamesettings remove from it
            if (playerIconData.Uuid == AppManager.I.AppSettings.LastActivePlayerUUID)
            {
                // if possible set the first available player...
                PlayerIconData newActivePlayerIcon = GetPlayersIconData().Find(p => p.Uuid != playerUUID);
                if (newActivePlayerIcon.Uuid != null)
                {
                    AppManager.I.PlayerProfileManager.SetPlayerAsCurrentByUUID(newActivePlayerIcon.Uuid);
                }
                else {
                    // ...else set to null
                    AppManager.I.PlayerProfileManager._currentPlayer = null;
                }
            }
            AppManager.I.AppSettings.SavedPlayers.Remove(playerIconData);

            AppManager.I.AppSettingsManager.SaveSettings();
            return returnProfile;
        }

        /// <summary>
        /// Resets everything.
        /// </summary>
        public void ResetEverything()
        {
            // Reset all the Databases
            if (AppManager.I.AppSettings.SavedPlayers != null)
            {
                foreach (PlayerIconData pp in AppManager.I.AppSettings.SavedPlayers)
                {
                    AppManager.I.DB.LoadDatabaseForPlayer(pp.Uuid);
                    AppManager.I.DB.DropProfile();
                }
            }
            AppManager.I.DB.UnloadCurrentProfile();

            // Reset all settings too
            AppManager.I.AppSettingsManager.DeleteAllSettings();
            LoadSettings(alsoLoadCurrentPlayerProfile: false);
        }

        #endregion

        #region Import

        public void ImportAllPlayerProfiles()
        {
            string[] importFilePaths = AppManager.I.DB.GetImportFilePaths();
            foreach (var filePath in importFilePaths)
            {
                // Check whether that is a DB and load it
                if (filePath.Contains(".sqlite3"))
                {
                    ImportPlayerProfile(filePath);
                }
            }
            AppManager.I.AppSettingsManager.SaveSettings();
        }

        public void ImportPlayerProfile(string filePath)
        {
            PlayerProfileData importedPlayerProfileData = AppManager.I.DB.ImportDynamicDatabase(filePath);
            if (importedPlayerProfileData != null)
            {
                PlayerProfile importedPlayerProfile = new PlayerProfile().FromData(importedPlayerProfileData);
                AppManager.I.AppSettings.SavedPlayers.Add(importedPlayerProfile.GetPlayerIconData());
            }
        }

        #endregion

        #region Events
        public delegate void ProfileEventHandler();

        /// <summary>
        /// Occurs when [on profile changed].
        /// </summary>
        public static event ProfileEventHandler OnProfileChanged;
        public static event ProfileEventHandler OnNewProfileCreated;
        #endregion

        #region Checks

        public bool IsDemoUserExisting()
        {
            bool demoUserExists = false;
            var playerList = GetPlayersIconData();
            foreach (var player in playerList)
            {
                if (player.IsDemoUser)
                {
                    demoUserExists = true;
                }
            }
            return demoUserExists;
        }

        #endregion
    }
}