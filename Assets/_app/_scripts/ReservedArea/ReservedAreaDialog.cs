﻿using UnityEngine;
using System.Collections.Generic;
using EA4S.Audio;
using EA4S.Core;
using EA4S.Database;
using EA4S.UI;
using EA4S.Helpers;

namespace EA4S.ReservedArea
{
    /// <summary>
    /// Pop-up that allows access to the reserved area with a parental lock.
    /// </summary>
    public class ReservedAreaDialog : MonoBehaviour
    {
        public TextRender englishTextUI;
        public TextRender arabicTextUI;

        private int firstButtonClickCounter;

        private const int nButtons = 4;

        private int firstButtonIndex;
        private int secondButtonIndex;
        private int firstButtonClicksTarget;

        void OnEnable()
        {
            firstButtonClickCounter = 0;

            // Selecting two buttons at random
            var availableIndices = new List<int>();
            for (var i = 0; i < nButtons; i++)
                availableIndices.Add(i);
            var selectedIndices = availableIndices.RandomSelect(2);
            firstButtonIndex = selectedIndices[0];
            secondButtonIndex = selectedIndices[1];

            // Number of clicks at random
            const int min_number = 4;
            const int max_number = 8;
            firstButtonClicksTarget = Random.Range(min_number, max_number);

            // Update text
            string[] numberWords = { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
            string[] colorsWords = { "green", "red", "blue", "yellow" };
            string[] colorsWordsArabic = { "الأخضر", "الأحمر", "الأزرق", "الأصفر" };

            string numberWord = numberWords[firstButtonClicksTarget - 1];
            string firstColorWord = colorsWords[firstButtonIndex];
            string secondColorWord = colorsWords[secondButtonIndex];

            var titleLoc = LocalizationManager.GetLocalizationData("ReservedArea_Title");
            var sectionIntroLoc = LocalizationManager.GetLocalizationData("ReservedArea_SectionDescription_Intro");
            var sectionErrorLoc= LocalizationManager.GetLocalizationData("ReservedArea_SectionDescription_Error");

            englishTextUI.text =
                "<b>" + titleLoc.English + "</b>" +
                "\n" + sectionIntroLoc.English +  //"This section is reserved for parents and guardians." +
                "\n\nPress " + numberWord + " times the " + firstColorWord + " button, then press the " + secondColorWord + " one once." +
                "\n\n" + sectionErrorLoc.English; //"If you make an error, retry by re - accessing this panel");

            string numberWordArabic = AppManager.I.DB.GetWordDataById("number_0" + firstButtonClicksTarget).Arabic;
            string firstColorWordArabic = colorsWordsArabic[firstButtonIndex];
            string secondColorWordArabic = colorsWordsArabic[secondButtonIndex];

            string arabicIntroduction = "";
            arabicIntroduction += titleLoc.Arabic + "\n\n"; 
            arabicIntroduction += sectionIntroLoc.Arabic + "\n\n"; 
            arabicIntroduction += string.Format("لفتح القفل، اضغط الزر {0} {2} مرات، ثم الزر {1} مرة واحدة", firstColorWordArabic, secondColorWordArabic, numberWordArabic);
            arabicIntroduction += "\n\n" + sectionErrorLoc.Arabic; // "\n\n في حال أخطأت، أعد المحاولة باستعمال هذه اللوحة";

            //Debug.Log(arabicIntroduction);
            arabicTextUI.text = arabicIntroduction;
        }

        public void OnButtonClick(int buttonIndex)
        {
            AudioManager.I.PlaySound(Sfx.UIButtonClick);
            if (buttonIndex == firstButtonIndex) {
                firstButtonClickCounter++;
            } else if (buttonIndex == secondButtonIndex) {
                if (firstButtonClickCounter == firstButtonClicksTarget) {
                    UnlockReservedArea();
                } else {
                    firstButtonClickCounter = firstButtonClicksTarget + 1; // disabling
                }
            }
        }

        void UnlockReservedArea()
        {
            AppManager.I.NavigationManager.GoToReservedArea();
        }
    }
}