﻿using UnityEngine;
using System;
using System.Collections;
using TMPro;
using ArabicSupport;
using DG.Tweening;
using EA4S;

namespace EA4S
{
    public class WidgetSubtitles : MonoBehaviour
    {
        public static WidgetSubtitles I;
        public GameObject Background;
        public TextMeshProUGUI TextUI;
        public WalkieTalkie WalkieTalkie;

        System.Action currentCallback;

        int index;
        Tween showTween, textTween;

        void Awake()
        {
            I = this;
            WalkieTalkie.gameObject.SetActive(true);
            WalkieTalkie.Setup();

            TextUI.isRightToLeftText = true;

            showTween = DOTween.Sequence().SetUpdate(true).SetAutoKill(false).Pause()
                .Append(Background.GetComponent<RectTransform>().DOAnchorPosY(170, 0.4f).From())
                .OnPlay(() => this.gameObject.SetActive(true))
                .OnRewind(() => {
                    TextUI.text = "";
                    this.gameObject.SetActive(false);
                });

            DisplayText("");

            this.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            I = null;
            this.StopAllCoroutines();
            showTween.Kill();
            textTween.Kill();
        }


        public void DisplayDebug(string sentence)
        {
            this.StopAllCoroutines();
            currentCallback = null;
            showTween.PlayForward();
            TextUI.text = sentence;
        }

        /// <summary>
        /// Activate view elements if SentenceId != "" and display sentence.
        /// </summary>
        public void DisplaySentence(string SentenceId, float duration = 2, bool isKeeper = false, System.Action callback = null)
        {
            GlobalUI.Init();
            this.StopAllCoroutines();
            currentCallback = callback;
            showTween.PlayForward();
            WalkieTalkie.Show(isKeeper);
            DisplayText(SentenceId, duration);

        }
        // Overload
        public void DisplaySentence(string[] SentenceIdList, float duration = 2, bool isKeeper = false, System.Action callback = null)
        {
            index = 0;
            DisplaySentence(SentenceIdList[index], duration, isKeeper, callback);
        }

        public void Close(bool _immediate = false)
        {
            this.StopAllCoroutines();
            if (_immediate)
                showTween.Rewind();
            else
                showTween.PlayBackwards();
            WalkieTalkie.Show(false, _immediate);
        }

        public void ShowNext()
        {
            // TODO Don't know how to deal with this (note by Daniele)
        }

        void DisplayText(string textID, float duration = 3)
        {
            bool isContinue = !string.IsNullOrEmpty(TextUI.text);
            this.StopAllCoroutines();
            textTween.Kill();
            TextUI.text = "";
            if (string.IsNullOrEmpty(textID)) {
                this.gameObject.SetActive(false);
                return;
            }

            this.gameObject.SetActive(true);
            if (WalkieTalkie.isShown)
                WalkieTalkie.StartPulsing(isContinue);
            Db.LocalizationData row = LocalizationManager.GetLocalizationData(textID);
            //Debug.Log("DisplayText " + textID + " " + row.GetStringData("Arabic") + " " + ArabicFixer.Fix(row.GetStringData("Arabic")));
            TextUI.text = row == null ? textID : ReverseText(ArabicFixer.Fix(row.Arabic));
            this.StartCoroutine(DisplayTextCoroutine(duration));

            AudioManager.I.PlayDialog(textID, currentCallback);
            Debug.Log("DisplayText() " + textID + " - " + row.English);
        }

        IEnumerator DisplayTextCoroutine(float duration)
        {
            yield return null; // Wait 1 frame otherwise TMP doesn't update characterCount

            TextUI.maxVisibleCharacters = TextUI.textInfo.characterCount;
            textTween = DOTween.To(() => TextUI.maxVisibleCharacters, x => TextUI.maxVisibleCharacters = x, 0, duration)
                .From().SetUpdate(true).SetEase(Ease.Linear)
                .OnComplete(WalkieTalkie.StopPulsing);
        }

        string ReverseText(string text)
        {
            char[] cArray = text.ToCharArray();
            string reverse = String.Empty;
            for (int i = cArray.Length - 1; i > -1; i--) {
                reverse += cArray[i];
            }
            return reverse;
        }
    }
}