﻿using System;
using System.Collections.Generic;
using EA4S.Helpers;
using SQLite;
using UnityEngine;

namespace EA4S.Database
{
    /// <summary>
    /// Data defining a Minigame.
    /// One entry should exist for each minigame (or minigame variation).
    /// A Play Session contains one or more minigames that can be selected to play when reaching that play session.
    /// <seealso cref="PlaySessionData"/>
    /// </summary>
    [Serializable]
    public class MiniGameData : IData
    {

        public string Title_En
        {
            get { return _Title_En; }
            set { _Title_En = value; }
        }
        [SerializeField]
        private string _Title_En;

        public string Title_Ar
        {
            get { return _Title_Ar; }
            set { _Title_Ar = value; }
        }
        [SerializeField]
        private string _Title_Ar;

        [PrimaryKey]
        public MiniGameCode Code
        {
            get { return _Code; }
            set { _Code = value; }
        }
        [SerializeField]
        private MiniGameCode _Code;

        public bool Available
        {
            get { return _Available; }
            set { _Available = value; }
        }
        [SerializeField]
        private bool _Available;

        /// <summary>
        /// a Minigame can be a normal game or an assessment
        /// </summary>
        /// <value>The type.</value>
        public MiniGameDataType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }
        [SerializeField]
        private MiniGameDataType _Type;

        /// <summary>
        /// the main is the game name
        /// </summary>
        /// <value>The main.</value>
        public string Main
        {
            get { return _Main; }
            set { _Main = value; }
        }
        [SerializeField]
        private string _Main;

        public string Variation
        {
            get { return _Variation; }
            set { _Variation = value; }
        }
        [SerializeField]
        private string _Variation;

        public string Badge
        {
            get { return _Badge; }
            set { _Badge = value; }
        }
        [SerializeField]
        private string _Badge;

        public string Scene
        {
            get { return _Scene; }
            set { _Scene = value; }
        }
        [SerializeField]
        private string _Scene;

        [Ignore]
        public WeightedPlaySkill[] AffectedPlaySkills
        {
            get { return _AffectedPlaySkills; }
            set { _AffectedPlaySkills = value; }
        }
        [SerializeField]
        private WeightedPlaySkill[] _AffectedPlaySkills;
        public string AffectedPlaySkills_list
        {
            get { return _AffectedPlaySkills.ToJoinedString(); }
            set { }
        }

        public string GetId()
        {
            return Code.ToString();
        }

        public override string ToString()
        {
            return string.Format("[Minigame: id={0}, type={4}, available={1},  title_en={2}, title_ar={3}]", GetId(), Available, Title_En, Title_Ar, Type.ToString());
        }

        public string GetTitleSoundFilename()
        {
            return GetId() + "_Title";
        }

        public string GetIconResourcePath()
        {
            return "Images/GameIcons/minigame_Ico_" + Main;
        }

        public string GetBadgeIconResourcePath()
        {
            return "Images/GameIcons/minigame_BadgeIco_" + Badge;
        }
    }

    [Serializable]
    public struct WeightedPlaySkill
    {
        public PlaySkill Skill;
        public float Weight;

        public WeightedPlaySkill(PlaySkill skill, float weight)
        {
            Skill = skill;
            Weight = weight;
        }

        override public string ToString()
        {
            return Skill.ToString() + ":" + Weight;
        }
    }

}