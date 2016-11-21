﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2016/11/20
namespace EA4S
{
    public struct EndsessionResultData
    {
        public int Stars;
        public string MinigameIconResourcesPath;
        public string MinigameBadgeResourcesPath;

        /// <summary>
        /// Data for a minigame played during the session
        /// </summary>
        /// <param name="_stars">Total stars gained</param>
        /// <param name="_minigameIconResourcesPath">Resources path to minigame icon</param>
        /// <param name="_minigameBadgeResourcesPath">Resource path to minigame badge (alphabet/letters/counting/etc)</param>
        public EndsessionResultData(int _stars, string _minigameIconResourcesPath, string _minigameBadgeResourcesPath)
        {
            Stars = _stars;
            MinigameIconResourcesPath = _minigameIconResourcesPath;
            MinigameBadgeResourcesPath = _minigameBadgeResourcesPath;
        }
    }
}